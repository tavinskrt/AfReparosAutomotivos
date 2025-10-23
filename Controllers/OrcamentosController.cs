using Microsoft.AspNetCore.Mvc;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace AfReparosAutomotivos.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Login")]
public class OrcamentosController : Controller
{
    private readonly IConfiguration _configuration;
    public OrcamentosController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        List<Orcamentos> orcamentos = new List<Orcamentos>();
        string? connectionString = _configuration.GetConnectionString("default");

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Orcamento";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orcamentos.Add(new Orcamentos
                    {
                        idFuncionario = reader.GetInt32(1),
                        idCliente = reader.GetInt32(2),
                        dataCriacao = reader.GetDateTime(3),
                        dataEntrega = reader.IsDBNull(4) ? (DateTime?)null: reader.GetDateTime(4),
                        status = reader.GetInt32(5),
                        total = reader.GetDecimal(6),
                        formaPagamento = reader.GetString(7),
                        parcelas = reader.GetInt32(8)
                    });
                }
            }
        }
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
        if (ModelState.IsValid)
        {
            string? connectionString = _configuration.GetConnectionString("default");
            if (string.IsNullOrEmpty(connectionString))
            {
                return Problem("Erro de configuração: a string de conexão não é válida.");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Orcamento (idFuncionario, idCliente, data_criacao, data_entrega, status, total, forma_pgto, parcelas) VALUES (@funcionario, @cliente, @data_criacao, @data_entrega, @status, @total, @forma_pgto, @parcelas)";
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@cliente", orcamento.idCliente);
                command.Parameters.AddWithValue("@data_criacao", orcamento.dataCriacao);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);

                await command.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Index", "Orcamentos");
        }
        return View(orcamento);
    }
}