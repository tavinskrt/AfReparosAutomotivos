using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    [Authorize(AuthenticationSchemes = "Identity.Login")]
    public class OrcamentoRepository : IOrcamentoRepository
    {
        private readonly string? _connectionString;
        /// <summary>
        /// Conecta ao banco de dados com a string fornecida.
        /// </summary>
        /// <param name="configuration">Parâmetro de configuração.</param>
        public OrcamentoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");
            if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
        }
        /// <summary>
        /// Retorna a lista de orçamentos.
        /// </summary>
        /// <returns>A lista de orçamentos.</returns>
        public async Task<List<Orcamentos>> Get()
        {
            /// Criar lista de orçamentos
            List<Orcamentos> orcamentos = new List<Orcamentos>();

            /// Comando SQL a ser executado
            string sql = @"SELECT idOrcamento,
                                  idFuncionario,
                                  idCliente,
                                  data_criacao,
                                  data_entrega,
                                  status,
                                  total,
                                  forma_pgto,
                                  parcelas
                             FROM Orcamento";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                /// Conectando
                await connection.OpenAsync();
                /// Utilizando reader para comandos SELECT
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orcamentos.Add(new Orcamentos
                        {
                            idOrcamento = reader.GetInt32(0),
                            idFuncionario = reader.GetInt32(1),
                            idCliente = reader.GetInt32(2),
                            dataCriacao = reader.GetDateTime(3),
                            dataEntrega = reader.GetDateTime(4),
                            status = reader.GetInt32(5),
                            total = reader.GetDecimal(6),
                            formaPagamento = reader.GetString(7),
                            parcelas = reader.GetInt32(8)
                        });
                    }
                }
            }
            return orcamentos;
        }
        /// <summary>
        /// Cria um novo orçamento.
        /// </summary>
        /// <param name="orcamento">O orçamento a ser criado.</param>
        /// <returns>Um novo orçamento.</returns>
        public async Task Add(Orcamentos orcamento)
        {
            /// Comando SQL a ser executado
            string sql = @"INSERT INTO 
                           Orcamento (idFuncionario, idCliente, data_criacao, data_entrega, status, total, forma_pgto, parcelas)
                           VALUES (@funcionario, @cliente, @data_criacao, @data_entrega, @status, @total, @forma_pgto, @parcelas)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                    command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                    command.Parameters.AddWithValue("@cliente", orcamento.idCliente);
                    command.Parameters.AddWithValue("@data_criacao", orcamento.dataCriacao);
                    command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega);
                    command.Parameters.AddWithValue("@status", orcamento.status);
                    command.Parameters.AddWithValue("@total", orcamento.total);
                    command.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);
                    command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Orcamentos?> Update(int id)
        {
            Orcamentos? orcamento = null;
            string sql = @"SELECT idOrcamento,
                                  idFuncionario,
                                  idCliente,
                                  data_criacao,
                                  data_entrega,
                                  status,
                                  total,
                                  forma_pgto,
                                  parcelas
                             FROM Orcamento
                            WHERE idOrcamento = @id";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                /// Conectando
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        orcamento = new Orcamentos
                        {
                            idOrcamento = reader.GetInt32(0),
                            idFuncionario = reader.GetInt32(1),
                            idCliente = reader.GetInt32(2),
                            dataCriacao = reader.GetDateTime(3),
                            dataEntrega = reader.GetDateTime(4),
                            status = reader.GetInt32(5),
                            total = reader.GetDecimal(6),
                            formaPagamento = reader.GetString(7),
                            parcelas = reader.GetInt32(8)
                        };
                    }
                }
            }
            return orcamento;
        }

        [HttpPost]
        public async Task Update(Orcamentos orcamento)
        {
            string sql = @"UPDATE Orcamento
                              SET idFuncionario = @funcionario,
                                  data_entrega = @data_entrega,
                                  status = @status,
                                  total = @total,
                                  parcelas = @parcelas
                            WHERE idOrcamento = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", orcamento.idOrcamento);
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            string sql = @"DELETE FROM Orcamento
                            WHERE idOrcamento = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}