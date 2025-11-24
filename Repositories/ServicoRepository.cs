using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    [Authorize(AuthenticationSchemes = "Identity.Login")]

    public class ServicoRepository : IServicoRepository
    {
        private readonly string? _connectionString;

        public ServicoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
            }
        }

        public async Task<List<Servicos>> Get()
        {
            List<Servicos> servicos = new List<Servicos>();

            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                               FROM Servico";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        servicos.Add(new Servicos
                        {
                            IdServico = reader.GetInt32(0),
                            Descricao = reader.GetString(1),
                            PrecoBase = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return servicos;
        }
        public async Task<decimal> GetPrecoBaseByIdAsync(int id)
        {
            decimal precoBase = 0.00M;

            string sql = "SELECT preco_base FROM Servico WHERE idServico = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    precoBase = Convert.ToDecimal(result);
                }
                else
                {
                    Console.WriteLine($"[AVISO NO REPOSITÓRIO] Serviço com ID {id} não encontrado ou PrecoBase nulo.");
                }
            }
            return precoBase;
        }
        public async Task<Servicos?> GetId(int id)
        {
            Servicos? servico = null;

            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                               FROM Servico
                               WHERE idServico = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        servico = new Servicos
                        {
                            IdServico = reader.GetInt32(0),
                            Descricao = reader.GetString(1),
                            PrecoBase = reader.GetDecimal(2)
                        };
                    }
                }
            }
            return servico;
        }

        public async Task Add(Servicos servico)
        {
            string sql = @"INSERT INTO 
                           Servico (descricao, preco_base)
                           VALUES(@descricao, @preco_base)";
 
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@descricao", servico.Descricao);
                command.Parameters.AddWithValue("@preco_base", servico.PrecoBase);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Servicos?> Update(int id)
        {
            Servicos? servico = null;
            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                               FROM Servico
                               WHERE idServico = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        servico = new Servicos
                        {
                            IdServico = reader.GetInt32(0),
                            Descricao = reader.GetString(1),
                            PrecoBase = reader.GetDecimal(2)
                        };
                    }
                }
            }
            return servico;
        }

        [HttpPost]
        public async Task Update(Servicos servico)
        {
            string sql = @"UPDATE Servico
                              SET descricao = @descricao,
                                  preco_base = @preco_base
                            WHERE idServico = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", servico.IdServico);
                command.Parameters.AddWithValue("@descricao", servico.Descricao);
                command.Parameters.AddWithValue("@preco_base", servico.PrecoBase);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            string sql = @" DELETE FROM ITENS
                             WHERE idServico = @id
                             DELETE FROM Servico
                             WHERE idServico = @id";

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