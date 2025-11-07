using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    /// <summary>
    /// Atribui a instância do repositório de orcamento ao espaço reservado.
    /// </summary>
    public OrcamentosController
    (
        IOrcamentoRepository orcamentoRepository,
        IClienteRepository clienteRepository,
        IServicoRepository servicoRepository
    )
    {
        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
        _servicoRepository = servicoRepository;
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

    public async Task<IActionResult> Index()
    {
        /// Busca a lista de orçamentos no repositório e a passa para a view.
        var orcamentos = await _orcamentoRepository.Get();
        return View(orcamentos);
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