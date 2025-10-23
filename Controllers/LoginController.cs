using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AFReparosAutomotivos.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Json(new {message = "Usuário já logado."});
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            string? connectionString = _configuration.GetConnectionString("default");

            string sql = "SELECT * FROM Usuario WHERE Nome = @username AND Senha = @senha";

            using (var connection = new SqlConnection(connectionString))
            {
                using (var sqlCommand = new SqlCommand(sql, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@username", username);
                    sqlCommand.Parameters.AddWithValue("@senha", senha);

                    await connection.OpenAsync();

                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int usuarioId = reader.GetInt32(0);
                            string nome = reader.GetString(1);

                            List<Claim> direitosAcesso = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                                new Claim(ClaimTypes.Name, nome)
                            };

                            var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                            var userPrincipal = new ClaimsPrincipal(new[] { identity });

                            await HttpContext.SignInAsync(userPrincipal,
                            new AuthenticationProperties
                            {
                                IsPersistent = false
                            });

                            return RedirectToAction("Index", "Orcamentos");
                        }
                    }
                }
            }
            return Json(new {message = "Usuário não encontrado."});
        }
        
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