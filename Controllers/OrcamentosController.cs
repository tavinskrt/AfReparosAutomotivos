using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using AfReparosAutomotivos.Models.ViewModels;
using System.Text.Json;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class OrcamentosController : Controller
{
    /// <summary>
    /// Reserva espaço para, no construtor, receber e guardar uma instância do repositório de orcamento.
    /// </summary>
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IVeiculoRepository _veiculoRepository;

    /// <summary>
    /// Atribui a instância do repositório de orcamento ao espaço reservado.
    /// </summary>
    public OrcamentosController
    (
        IOrcamentoRepository orcamentoRepository,
        IClienteRepository clienteRepository,
        IServicoRepository servicoRepository,
        IVeiculoRepository veiculoRepository
    )
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
        _servicoRepository = servicoRepository;
        _veiculoRepository = veiculoRepository;
    }

    /// <summary>
    /// Lista os serviços disponíveis e os adiciona ao ViewModel para preenchimento do dropdown na view.
    /// </summary>
    private async Task CarregarServicosNoViewModel(OrcamentosViewModel orcamentoViewModel)
    {
        var servicos = await _servicoRepository.Get();
        orcamentoViewModel.ServicosDisponiveis = servicos.Select(s => new SelectListItem
        {
            Value = s.IdServico.ToString(),
            Text = $"{s.Descricao} (R$ {s.PrecoBase:N2})"
        }).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] OrcamentosFilterViewModel filtros)
    {
        /// Busca a lista de orçamentos no repositório e a passa para a view.
        var orcamentos = await _orcamentoRepository.GetFilter(filtros);
        ViewBag.filtrosAplicados = filtros;
        return View(orcamentos);
    }

    /// <summary>
    /// Retorna os detalhes do orçamento do ID.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        return View(orcamento);
    }

    public async Task<IActionResult> GerarPdf(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));
                
                // Cabeçalho
                page.Header().Height(80).Background("#3b2e1a").AlignCenter().AlignMiddle().Text("AF Reparos Automotivos")
                    .FontColor(Colors.White).FontSize(20).Bold();

                // Corpo
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text($"Orçamento Nº {orcamento?.idOrcamento}").FontSize(16).Bold().FontColor("#3b2e1a");

                    col.Item().Text($"Data de Criação: {orcamento?.dataCriacao:dd/MM/yyyy HH:mm}");
                    if (orcamento?.dataEntrega != null)
                        col.Item().Text($"Data de Entrega: {orcamento.dataEntrega:dd/MM/yyyy}");

                    col.Item().Text($"Funcionário: {orcamento?.nomeFunc}");
                    col.Item().Text($"Cliente: {orcamento?.nome}");
                    col.Item().Text($"Forma de Pagamento: {orcamento?.formaPagamento}");
                    col.Item().Text($"Parcelas: {(orcamento?.parcelas > 1 ? orcamento.parcelas + "x" : "À vista")}");

                    string statusTexto = orcamento?.status switch
                    {
                        1 => "Aberto",
                        2 => "Em andamento",
                        3 => "Concluído",
                        _ => $"Desconhecido ({orcamento?.status})"
                    };
                    col.Item().Text($"Status: {statusTexto}");

                    col.Item().Text($"Total: {orcamento?.total:C2}").FontSize(14).Bold();
                });

                // Rodapé
                page.Footer().AlignCenter().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        });

        // Gera o PDF em memória
        var pdfBytes = documento.GeneratePdf();

        // Retorna para download
        return File(pdfBytes, "application/pdf", $"Orcamento_{orcamento?.idOrcamento}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var orcamentoViewModel = new OrcamentosViewModel();
        await CarregarServicosNoViewModel(orcamentoViewModel); 
        return View(orcamentoViewModel);
    }

    /// <summary>
    /// Garante que somente requisições POST possam acessar este método.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(OrcamentosViewModel orcamentoViewModel)
    {
        int clienteId;

        Clientes cliente = new Clientes
        {
            nome = orcamentoViewModel.nome,
            telefone = orcamentoViewModel.TelefoneCli,
            endereco = orcamentoViewModel.EnderecoCli,
            documento = orcamentoViewModel.DocumentoCli
        };
        if (orcamentoViewModel.DocumentoCli.Length != 11 && orcamentoViewModel.DocumentoCli.Length != 14)
        {
            var erro = new Modal
            {
                Title = "Formato de documento inválido",
                Mensagem = "O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos)."
            };
            /// TempData é uum dicionário temporário para armazenar dados entre requisições. JsonSerializer converte o objeto em string JSON. A view pode acessar TempData["Mensagem"] e desserializar o JSON de volta para um objeto Modal.
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return RedirectToAction("Create", "Orcamentos");
        } 

        clienteId = await _clienteRepository.Add(cliente);

        Orcamentos orcamento = new Orcamentos
        {
            idFuncionario = orcamentoViewModel.idFuncionario,
            idCliente = clienteId,
            dataCriacao = DateTime.Now,
            dataEntrega = orcamentoViewModel.dataEntrega,
            status = orcamentoViewModel.status,
            total = orcamentoViewModel.total,
            formaPagamento = orcamentoViewModel.formaPagamento,
            parcelas = orcamentoViewModel.parcelas
        };
        await _orcamentoRepository.Add(orcamento);

        Veiculos veiculo = new Veiculos
        {
            id = orcamentoViewModel.idVeiculo,
            marca = orcamentoViewModel.Marca,
            placa = orcamentoViewModel.Placa,
            modelo = orcamentoViewModel.Modelo
        };
        await _veiculoRepository.Add(veiculo);

        return RedirectToAction("Index", "Orcamentos");
    }

    /// <summary>
    /// O método na URL aparece como Edit, mas o nome do método no código é Update.
    /// </summary>
    [HttpGet, ActionName("Edit")]
    public async Task<IActionResult> Update(int id)
    {
        var orcamento = await _orcamentoRepository.Update(id);
        return View(orcamento);
    }

    /// <summary>
    /// O método na URL aparece como Edit, mas o nome do método no código é Update.
    /// </summary>
    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> Update(Orcamentos orcamento)
    {
        await _orcamentoRepository.Update(orcamento);
        return RedirectToAction("Index", "Orcamentos");
    }

    /// <summary>
    /// Garante que somente requisições POST possam acessar este método.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _orcamentoRepository.Delete(id);
        return RedirectToAction("Index", "Orcamentos");
    }
}