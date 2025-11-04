using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Controllers;

/// <summary>
/// Classe padrão do Framework.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Reserva espaço para armazenar o logger.
    /// </summary>
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Atribui o logger ao espaço reservado.
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
  
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Armazena em cache por 0 segundos, e em nenhum local, não armazenando nada.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Retorna a view de erro, passando o Id da suboperação atual ou o identificador de rastreamento HTTP.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
