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
        private readonly ILoginRepository _loginRepository;

        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Json(new {message = "Usuário já logado."});
            }
            return View();
        }

        /// <summary>
        /// Quando autenticado, cria o cookie de autenticação para o usuário e retorna para o painel de orçamentos.
        /// Quando a inserção de login é inválida, retorna para a página inicial novamente.
        /// </summary>
        /// <param name="username">O usuário.</param>
        /// <param name="senha">A senha.</param>
        /// <returns>Painel de orçamentos.</returns>
        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            var funcionario = await _loginRepository.GetFuncionarioByCredentialsAsync(username, senha);

            if (funcionario != null)
            {
                List<Claim> direitosAcesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, funcionario.idFuncionario.ToString()),
                    new Claim(ClaimTypes.Name, funcionario.Nome)
                };

                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var user = new ClaimsPrincipal(new[] {identity});

                await HttpContext.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = false
                });
                return RedirectToAction("Index", "Orcamentos");
            }
            var erro = new Modal
            {
                Title = "Credenciais inválidas",
                Mensagem = "O usuário ou a senha fornecidos são inválidos."
            };
            TempData["Mensagem"] = JsonSerializer.Serialize(erro);
            return View("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}