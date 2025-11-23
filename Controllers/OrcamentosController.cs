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
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    /// <summary>
    /// Construtor injetando todos os repositórios necessários.
    /// </summary>
    public OrcamentosController
    (
        IOrcamentoRepository orcamentoRepository,
        IClienteRepository clienteRepository,
        IItemRepository itemRepository,
        IServicoRepository servicoRepository,
        IVeiculoRepository veiculoRepository
    )
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
        _itemRepository = itemRepository;
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

    /// <summary>
    /// Gera o PDF (O Layout ficou no "OrcamentoPdfDocument.cs")
    /// </summary>
    public async Task<IActionResult> GerarPdf(int idOrcamento)
    {
        var orcamento = await _orcamentoRepository.GetId(idOrcamento);
        if (orcamento == null)
            return NotFound("Orçamento não encontrado.");

        var cliente = await _clienteRepository.GetId(orcamento.idCliente);
        var veiculo = await _veiculoRepository.GetId(orcamento.idVeiculo ?? 0);
        var itens = await _itemRepository.GetByOrcamento(idOrcamento);

        var document = new OrcamentoPdfDocument(orcamento, cliente, veiculo, itens);
        var pdf = document.GeneratePdf();

        return File(pdf, "application/pdf", $"orcamento_{idOrcamento}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var orcamentoViewModel = new OrcamentosViewModel 
        {
            // Inicializa a coleção de Itens com 1 item vazio para o Model Binding funcionar na View
            Itens = new List<Item> { new Item() } 
        };
        await CarregarServicosNoViewModel(orcamentoViewModel); 
        return View(orcamentoViewModel);
    }

/// <summary>
    /// Ação post para criação de Orçamento e seus Itens relacionados.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrcamentosViewModel orcamentoViewModel)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var idFuncionario = idClaim != null && int.TryParse(idClaim.Value, out int id) ? id : 1;

        // Adicionando validação básica do ModelState (útil para campos obrigatórios)
        if (!ModelState.IsValid)
        {
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }

        if (orcamentoViewModel.DocumentoCli != null && orcamentoViewModel.DocumentoCli.Length != 11 && orcamentoViewModel.DocumentoCli.Length != 14)
        {
            var erro = new Modal
            {
                Title = "Formato de documento inválido",
                Mensagem = "O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos)."
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }

        try
        {
            Clientes cliente = new Clientes
            {
                nome = orcamentoViewModel.nome,
                telefone = orcamentoViewModel.TelefoneCli,
                endereco = orcamentoViewModel.EnderecoCli,
                documento = orcamentoViewModel.DocumentoCli
            };
            int clienteId = await _clienteRepository.Add(cliente);

            var veiculo = new Veiculos
            {
                placa = orcamentoViewModel.Placa,
                marca = orcamentoViewModel.Marca,
                modelo = orcamentoViewModel.Modelo
            };

            int idVeiculo = await _veiculoRepository.Add(veiculo);

            Orcamentos orcamento = new Orcamentos
            {
                idFuncionario = idFuncionario,
                idCliente = clienteId,
                dataCriacao = DateTime.Now,
                dataEntrega = orcamentoViewModel.dataEntrega,
                status = orcamentoViewModel.status,
                total = orcamentoViewModel.total,
                formaPagamento = orcamentoViewModel.formaPagamento,
                parcelas = orcamentoViewModel.parcelas
            };
            int idOrcamento = await _orcamentoRepository.Add(orcamento);

            if (orcamentoViewModel.Itens != null && orcamentoViewModel.Itens.Any())
            {
                // Filtra os itens vazios antes de processar. 
                // Um item é considerado válido se tiver um idServico selecionado E uma quantidade > 0.
                var itensParaInserir = orcamentoViewModel.Itens
                    .Where(item => item.idServico > 0 && item.qtd > 0)
                    .ToList();

                if (itensParaInserir.Any())
                {
                    foreach (var item in itensParaInserir)
                    {
                        item.idOrcamento = idOrcamento; 
                        item.idVeiculo = idVeiculo;
                    }
                    // Chama o repositório de itens para inserir APENAS a lista filtrada.
                    await _itemRepository.Add(itensParaInserir);
                }
            }

            return RedirectToAction("Index", "Orcamentos");
        }
        catch (Exception ex)
        {
            var erro = new Modal
            {
                Title = "Erro na criação",
                Mensagem = $"Ocorreu um erro ao salvar: {ex.Message}"
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            await CarregarServicosNoViewModel(orcamentoViewModel); 
            return View(orcamentoViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PesquisarCliente(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            return Json(null);
        }
        
        var cliente = await _clienteRepository.GetByDocumento(documento); 
        
        if (cliente == null)
        {
            // Cliente não encontrado
            return Json(null);
        }

        // Retorna os dados do cliente como JSON
        return Json(new { 
            id = cliente.id, 
            nome = cliente.nome, 
            telefone = cliente.telefone, 
            endereco = cliente.endereco 
        });
    }

    public async Task<IActionResult> PesquisarVeiculo(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return Json(null);
        }
        
        var veiculo = await _veiculoRepository.GetByPlaca(placa); 
        
        if (veiculo == null)
        {
            return Json(null);
        }

        return Json(new { 
            id = veiculo.id, 
            marca = veiculo.marca, 
            modelo = veiculo.modelo
        });
    }

    /// <summary>
    /// Retorna o orçamento para edição.
    /// </summary>
    [HttpGet, ActionName("Edit")]
    public async Task<IActionResult> Update(int id)
    {
        var orcamento = await _orcamentoRepository.Update(id);
        return View(orcamento);
    }

    /// <summary>
    /// Realiza a atualização do orçamento.
    /// </summary>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Orcamentos orcamento)
    {
        await _orcamentoRepository.Update(orcamento);
        return RedirectToAction("Index", "Orcamentos");
    }

    /// <summary>
    /// Deleta o orçamento. O repositório já se encarrega de deletar os itens primeiro.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _orcamentoRepository.Delete(id);
            
            return RedirectToAction("Index", "Orcamentos");
        }
        catch (Exception ex)
        {
            var erro = new Modal
            {
                Title = "Erro na exclusão",
                Mensagem = $"Não foi possível excluir o orçamento. Detalhes: {ex.Message}"
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return RedirectToAction("Index", "Orcamentos");
        }
    }
}