using Microsoft.AspNetCore.Mvc;
using AfRepartosAutomotivos.Models;

namespace AfRepartosAutomotivos.Controllers;

public class OrcamentoController : Controller
{

    [HttpGet]
    public ActionResult Criar()
    {
        return View();
    }

    /*[HttpPost]
    public ActionResult Criar(OrcamentoViewModel orcamento)
    {
        
        return View("Index");
    }*/

}