
using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using AfReparosAutomotivos.Interfaces;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class OrcamentosController : Controller
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    public OrcamentosController(IOrcamentoRepository orcamentoRepository)
    {
        _orcamentoRepository = orcamentoRepository;
    }

    public async Task<IActionResult> Index()
    {
        var orcamentos = await _orcamentoRepository.Get();
        return View(orcamentos);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Orcamentos());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Orcamentos orcamento)
    {
        await _orcamentoRepository.Add(orcamento);
        return RedirectToAction("Index", "Orcamentos");
    }

     [HttpGet, ActionName("Edit")]
     public async Task<IActionResult> Update(int id)
     {
        var orcamento = await _orcamentoRepository.Update(id);
        return View(orcamento);
     }


    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> Update(Orcamentos orcamento)
    {
        await _orcamentoRepository.Update(orcamento);
        return RedirectToAction("Index", "Orcamentos");
    }
    
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _orcamentoRepository.Delete(id);
        return RedirectToAction("Index", "Orcamentos");
    }
}