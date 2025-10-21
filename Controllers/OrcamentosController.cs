using Microsoft.AspNetCore.Mvc;

namespace AFReparosAutomotivos.Controllers;

public class OrcamentosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}