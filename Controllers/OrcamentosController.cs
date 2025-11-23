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
    public async Task<IActionResult> GerarPdf(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        if (orcamento == null)
            return NotFound("Orçamento não encontrado.");

        var cliente = await _clienteRepository.GetId(orcamento.idCliente);

        var itens = await _itemRepository.GetByOrcamento(id);

        var idVeiculo = itens.FirstOrDefault()?.idVeiculo;
        var veiculo = await _veiculoRepository.GetId(idVeiculo ?? 0);

        var document = new OrcamentoPdfDocument(orcamento, cliente, veiculo, itens);
        var pdf = document.GeneratePdf();

        return File(pdf, "application/pdf", $"orcamento_{id}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var orcamentoViewModel = new OrcamentosViewModel 
        {
            // CRÍTICO: Inicializa a coleção de Itens com pelo menos 1 item vazio para o Model Binding funcionar na View
            ItensServicos = new List<ItemViewModel> { new ItemViewModel() } 
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

    // Validação de formato de documento (mantida)
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
        // 1. CALCULA O TOTAL DO ORÇAMENTO E INCLUI O PREÇO DO SERVIÇO NA VIEWMODEL
        
        decimal totalGeral = 0;
        
        // Filtramos apenas itens válidos (com ID de Serviço e Qtd > 0)
        var itensValidos = orcamentoViewModel.ItensServicos?
            .Where(item => item != null && item.idServico > 0 && item.qtd > 0)
            .ToList();

        if (itensValidos == null || !itensValidos.Any())
        {
             throw new InvalidOperationException("Nenhum item de serviço válido foi fornecido para o orçamento.");
        }
        
        // --- LÓGICA DE CÁLCULO CRÍTICA ---
        foreach (var item in itensValidos)
        {
            // Busca o preço base do serviço no repositório.
            // **IMPORTANTE**: Este método deve estar implementado no ServicoRepository.cs
            var precoBase = await _servicoRepository.GetPrecoBaseByIdAsync(item.idServico); 
            
            // Mapeia o precoBase para o ItemViewModel.preco. Isso garante que o preço base
            // correto (vindo do BD) seja salvo no item.
            item.preco = precoBase;

            // Fórmula: (Preço * Quantidade) * (1 + Taxa) - Desconto
            // A taxa é dividida por 100 assumindo que o usuário insere "5" para 5%.
            var custoTotal = (item.preco * item.qtd) * (1 + (item.taxa / 100)); 
            var valorItemFinal = custoTotal - item.desconto;

            totalGeral += valorItemFinal;
            
            Console.WriteLine($"[DIAGNÓSTICO] Item {item.idServico}: PrecoBase={precoBase:N2}, Qtd={item.qtd}, Taxa={item.taxa}%, Desc={item.desconto:N2}, Final={valorItemFinal:N2}");
        }
        
        // Atribui o total calculado ao OrcamentosViewModel
        orcamentoViewModel.total = totalGeral;
        Console.WriteLine($"[DIAGNÓSTICO] Total Geral Calculado: {totalGeral:N2}");
        // ----------------------------------------

        // 2. Persistência do Cliente
        Clientes cliente = new Clientes
        {
            nome = orcamentoViewModel.nome,
            telefone = orcamentoViewModel.TelefoneCli,
            endereco = orcamentoViewModel.EnderecoCli,
            documento = orcamentoViewModel.DocumentoCli
        };
        int clienteId = await _clienteRepository.Add(cliente);

        // 3. Persistência do Veículo (Considerando o primeiro item para os dados de Veículo)
        var primeiroItem = orcamentoViewModel.ItensServicos?.FirstOrDefault();
        
        var veiculo = new Veiculos
        {
            placa = primeiroItem?.Placa, 
            marca = primeiroItem?.Marca,
            modelo = primeiroItem?.Modelo
        };

        int idVeiculo = await _veiculoRepository.Add(veiculo);
        Console.WriteLine($"[DIAGNÓSTICO] Veículo persistido com ID: {idVeiculo}");

        // 4. Persistência do Orçamento (Header)
        Orcamentos orcamento = new Orcamentos
        {
            idFuncionario = idFuncionario,
            idCliente = clienteId,
            dataCriacao = DateTime.Now,
            dataEntrega = orcamentoViewModel.dataEntrega,
            status = orcamentoViewModel.status,
            total = orcamentoViewModel.total, // USA O VALOR CALCULADO!
            formaPagamento = orcamentoViewModel.formaPagamento,
            parcelas = orcamentoViewModel.parcelas
        };
        int idOrcamento = await _orcamentoRepository.Add(orcamento);
        Console.WriteLine($"[DIAGNÓSTICO] Orçamento persistido com ID: {idOrcamento}");

        // 5. Persistência dos Itens (Detalhes da Tabela Dinâmica)
        if (itensValidos != null) 
        {
            var listaEntidadesItens = new List<Item>();
            
            // Mapeia cada ItemViewModel para a entidade Itens (BD)
            foreach (var itemViewModel in itensValidos)
            {
                // CRIAÇÃO E MAPEAMENTO DA ENTIDADE 'Itens'
                var itemEntidade = new Item
                {
                    idOrcamento = idOrcamento, 
                    idVeiculo = idVeiculo, 

                    // Mapeando as propriedades obrigatórias para a persistência:
                    idServico = itemViewModel.idServico,
                    qtd = itemViewModel.qtd,
                    
                    // itemViewModel.preco AGORA CONTÉM O PREÇO BASE CORRETO.
                    data_entrega = itemViewModel.data_entrega, 
                    preco = itemViewModel.preco,
                    descricao = itemViewModel.descricao,
                    taxa = itemViewModel.taxa,
                    desconto = itemViewModel.desconto
                };
                
                listaEntidadesItens.Add(itemEntidade);
                Console.WriteLine($"[DIAGNÓSTICO] Item Mapeado: ServicoID={itemEntidade.idServico}, PreçoBase={itemEntidade.preco:N2}");
            }
            
            Console.WriteLine($"[DIAGNÓSTICO] Total de itens a serem inseridos: {listaEntidadesItens.Count}");

            if (listaEntidadesItens.Any())
            {
                // Chama o repositório para inserir a lista de entidades 'Itens'.
                await _itemRepository.Add(listaEntidadesItens);
                Console.WriteLine("[DIAGNÓSTICO] Itens adicionados ao repositório com sucesso.");
            }
        }
        // FIM da Persistência dos Itens
        
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