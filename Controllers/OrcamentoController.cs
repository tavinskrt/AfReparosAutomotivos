using Microsoft.AspNetCore.Mvc;
using TaskWeb.Models;

namespace TaskWeb.Controllers;

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
        
    }*/

}