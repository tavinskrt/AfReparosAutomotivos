
using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;
using System.Linq;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class OrcamentosController : Controller
{
    /// <summary>
    /// Reserva espaço para, no construtor, receber e guardar uma instância do repositório de orcamento.
    /// </summary>
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IClienteRepository _clienteRepository;

    /// <summary>
    /// Atribui a instância do repositório de orcamento ao espaço reservado.
    /// </summary>
    public OrcamentosController(IOrcamentoRepository orcamentoRepository, IClienteRepository clienteRepository)
    {
        _orcamentoRepository = orcamentoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<IActionResult> Index()
    {
        /// Busca a lista de orçamentos no repositório e a passa para a view.
        var orcamentos = await _orcamentoRepository.Get();
        return View(orcamentos);
    }
    public async Task<IActionResult> Index(
        string cpf,
        string nome,
        DateTime? dataCriacao,
        DateTime? dataEntrega,
        string metodoPagamento,
        string status,
        int? parcelas,
        decimal? preco)
    {
        /// Busca a lista de orçamentos no repositório e a passa para a view.
        var orcamentos = await _orcamentoRepository.Get();
        return View(orcamentos);
    }


    /// <summary>
    /// Retorna os detalhes do orçamento do ID.
    /// </summary>
    /// <param name="id">O ID do orçamento</param>
    /// <returns>Uma view do orçamento.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var orcamento = await _orcamentoRepository.GetId(id);
        return View(orcamento);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new OrcamentosViewModel());
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