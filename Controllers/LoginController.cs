using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models.ViewModels;
using System.Text.Json;

namespace AfReparosAutomotivos.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>
        /// Reserva espaço para, no construtor, receber e guardar uma instância do repositório de login.
        /// </summary>
        private readonly ILoginRepository _loginRepository;

        /// <summary>
        /// Atribui a instância do repositório de login ao espaço reservado.
        /// </summary>
        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public IActionResult Index()
        {
            /// Se o usuário já estiver autenticado, retorna uma mensagem JSON informando que o usuário já está logado.
            if (User.Identity?.IsAuthenticated == true)
            {
                return Json(new { message = "Usuário já logado." });
            }
            return View();
        }

        public IActionResult Erro()
        {
            return View();
        }

        /// <summary>
        /// Garante que somente requisições POST possam acessar este método.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            /// Retorna null se as credenciais forem inválidas.
            var funcionario = await _loginRepository.GetFuncionarioByCredentialsAsync(username, senha);

            if (funcionario != null)
            {
                List<Claim> direitosAcesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, funcionario.idFuncionario.ToString()),
                    new Claim(ClaimTypes.Name, funcionario.Nome)
                };

                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var user = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = false
                });
                return RedirectToAction("Index", "Orcamentos");
            }
            var erro = new Modal
            {
                Title = "Credenciais inválidas",
                Mensagem = "O usuário ou senha fornecidos são inválidos."
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return View("Index");
        }
        
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                /// Remove o cookie de autenticação.
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}