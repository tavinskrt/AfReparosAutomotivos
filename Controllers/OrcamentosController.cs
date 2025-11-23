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

    public async Task<IActionResult> GerarPdf(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        
        // **Sugestão de melhoria**: Buscar os Itens para incluir no PDF
        // var itens = await _itemRepository.GetByOrcamentoIdAsync(id);

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
        var orcamentoViewModel = new OrcamentosViewModel 
        {
            // CRÍTICO: Inicializa a coleção de Itens com pelo menos 1 item vazio para o Model Binding funcionar na View
            Veiculos = new List<VeiculoItemViewModel> { new VeiculoItemViewModel() } 
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
    // Obtenção do ID do funcionário logado (ou fallback para 1)
    var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    var idFuncionario = idClaim != null && int.TryParse(idClaim.Value, out int id) ? id : 1;

    // 1. ANTES DE TUDO: Verificação de Model State
    if (!ModelState.IsValid)
    {
        Console.WriteLine("[DIAGNÓSTICO CRÍTICO] Falha no ModelState. Recarregando dados e retornando a View...");
        foreach (var state in ModelState)
        {
            if (state.Value.Errors.Any())
            {
                Console.WriteLine($"[ERRO DE MODEL BINDING] Campo: {state.Key}, Erro: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }
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
        // 2. Persistência do Cliente (Feita apenas uma vez)
        Clientes cliente = new Clientes
        {
            nome = orcamentoViewModel.nome,
            telefone = orcamentoViewModel.TelefoneCli,
            endereco = orcamentoViewModel.EnderecoCli,
            documento = orcamentoViewModel.DocumentoCli
        };
        // Aqui, deve-se verificar se o cliente já existe antes de adicionar.
        // Assumindo que Add lida com a inserção/retorno do ID:
        int idCliente = await _clienteRepository.Add(cliente);
        Console.WriteLine($"[DIAGNÓSTICO] Cliente persistido com ID: {idCliente}");

        // Lista para coletar todos os itens de serviço de TODOS os veículos, após o cálculo
        var todosOsItensCalculados = new List<Tuple<int, ItemViewModel>>(); // Item1: idVeiculo, Item2: ItemViewModel
        decimal totalGeral = 0;

        // 3. LAÇO PRINCIPAL: Processa CÁLCULO, PERSISTÊNCIA do VEÍCULO e ITENS
        if (orcamentoViewModel.Veiculos != null)
        {
            foreach (var veiculoViewModel in orcamentoViewModel.Veiculos)
            {
                if (veiculoViewModel == null || veiculoViewModel.ServicosAssociados == null || !veiculoViewModel.ServicosAssociados.Any())
                {
                    continue; // Pula veículos sem serviços
                }

                // 3.1. Persistência do Veículo Atual
                Veiculos veiculoEntidade = new Veiculos
                {
                    placa = veiculoViewModel.Placa, 
                    marca = veiculoViewModel.Marca,
                    modelo = veiculoViewModel.Modelo
                };
                
                // Nota: Assumindo que Add retorna o idVeiculo (novo ou existente, se a lógica de repositório for inteligente)
                int idVeiculo = await _veiculoRepository.Add(veiculoEntidade);
                Console.WriteLine($"[DIAGNÓSTICO] Veículo '{veiculoEntidade.placa}' persistido com ID: {idVeiculo}");


                // 3.2. CÁLCULO e COLECIONA ITENS DE SERVIÇO para este veículo
                var itensDoVeiculo = veiculoViewModel.ServicosAssociados
                    .Where(item => item != null && item.idServico > 0 && item.qtd > 0)
                    .ToList();
                
                foreach (var item in itensDoVeiculo)
                {
                    // Busca o preço base do serviço no repositório.
                    // **IMPORTANTE**: Este método deve estar implementado no ServicoRepository.cs
                    var precoBase = await _servicoRepository.GetPrecoBaseByIdAsync(item.idServico); 
                    
                    // Mapeia o precoBase para o ItemViewModel.preco.
                    item.preco = precoBase;

                    // Fórmula: (Preço * Quantidade) * (1 + Taxa) - Desconto
                    // Assumindo que a Taxa no front-end é DECIMAL (e.g., 0.10 para 10%), se for porcentagem, use item.taxa / 100
                    // Se for 0.10, mantenha a fórmula anterior:
                    var custoTotal = (item.preco * item.qtd) * (1 + item.taxa); 
                    // Se a taxa for inserida como porcentagem (e.g., 10 para 10%), use:
                    // var custoTotal = (item.preco * item.qtd) * (1 + (item.taxa / 100)); 

                    var valorItemFinal = custoTotal - item.desconto;

                    totalGeral += valorItemFinal;

                    // Adiciona o item calculado junto com o ID do veículo à lista consolidada
                    todosOsItensCalculados.Add(Tuple.Create(idVeiculo, item));
                    
                    Console.WriteLine($"[DIAGNÓSTICO] Item (Veículo {idVeiculo}): Serviço={item.idServico}, Final={valorItemFinal:N2}");
                }
            }
        }
        
        // 4. Validação final de itens
        if (!todosOsItensCalculados.Any())
        {
            throw new InvalidOperationException("Nenhum item de serviço válido foi fornecido para o orçamento.");
        }

        // Atribui o total calculado ao OrcamentosViewModel
        orcamentoViewModel.total = totalGeral;
        Console.WriteLine($"[DIAGNÓSTICO] Total Geral Calculado: {totalGeral:N2}");

        // 5. Persistência do Orçamento (Header)
        Orcamentos orcamento = new Orcamentos
        {
            idFuncionario = idFuncionario,
            idCliente = idCliente, // Usa o ID do cliente persistido
            dataCriacao = DateTime.Now,
            dataEntrega = orcamentoViewModel.dataEntrega,
            status = orcamentoViewModel.status,
            total = orcamentoViewModel.total, // USA O VALOR CALCULADO!
            formaPagamento = orcamentoViewModel.formaPagamento,
            parcelas = orcamentoViewModel.parcelas
        };
        int idOrcamento = await _orcamentoRepository.Add(orcamento);
        Console.WriteLine($"[DIAGNÓSTICO] Orçamento persistido com ID: {idOrcamento}");

        // 6. Persistência dos Itens (Detalhes da Tabela Dinâmica)
        if (todosOsItensCalculados.Any()) 
        {
            var listaEntidadesItens = new List<Item>();
            
            // Mapeia cada ItemViewModel calculado para a entidade Itens (BD)
            foreach (var itemTuple in todosOsItensCalculados)
            {
                int idVeiculoItem = itemTuple.Item1;
                ItemViewModel itemViewModel = itemTuple.Item2;

                // CRIAÇÃO E MAPEAMENTO DA ENTIDADE 'Item'
                var itemEntidade = new Item
                {
                    idOrcamento = idOrcamento, 
                    idVeiculo = idVeiculoItem, // Vincula o Item ao Veículo correto

                    // Mapeando as propriedades obrigatórias para a persistência:
                    idServico = itemViewModel.idServico,
                    qtd = itemViewModel.qtd,
                    
                    // O itemViewModel.preco JÁ CONTÉM o preço base do BD (passo 3.2)
                    data_entrega = itemViewModel.data_entrega, 
                    preco = itemViewModel.preco,
                    descricao = itemViewModel.descricao,
                    taxa = itemViewModel.taxa,
                    desconto = itemViewModel.desconto
                };
                
                listaEntidadesItens.Add(itemEntidade);
                Console.WriteLine($"[DIAGNÓSTICO] Item Mapeado: VeiculoID={itemEntidade.idVeiculo}, ServicoID={itemEntidade.idServico}");
            }
            
            Console.WriteLine($"[DIAGNÓSTICO] Total de itens a serem inseridos: {listaEntidadesItens.Count}");

            // Chama o repositório para inserir a lista de entidades 'Itens'.
            await _itemRepository.Add(listaEntidadesItens);
            Console.WriteLine("[DIAGNÓSTICO] Itens adicionados ao repositório com sucesso.");
        }
        
        // Sucesso
        return RedirectToAction("Index", "Orcamentos");
    }
    catch (Exception ex)
    {
        // Tratamento de Erro
        Console.WriteLine($"[ERRO DE PERSISTÊNCIA CRÍTICA] Mensagem: {ex.Message}");
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
            return Json(null);
        }

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