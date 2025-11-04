
using System.Data;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    /// <summary>
    /// Somente usuários autenticados podem acessar os métodos deste repositório.
    /// </summary>
    [Authorize(AuthenticationSchemes = "Identity.Login")]

    public class ServicoRepository : IServicoRepository
    {
        /// <summary>
        /// Reserva espaço para a string de conexão com o banco de dados.
        /// </summary>
        private readonly string? _connectionString;

        public ServicoRepository(IConfiguration configuration)
        {
            /// Armazena a string de conexão vinda do arquivo de configuração.
            _connectionString = configuration.GetConnectionString("default");

            /// Retorna um erro se a string de conexão não for encontrada.
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
            }
        }

        public async Task<List<Servicos>> Get()
        {
            /// Cria a lista de orçamentos.
            List<Servicos> servicos = new List<Servicos>();

            /// Comando SQL a ser executado.
            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                             FROM Servico";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                /// Abre a conexão e executa o comando.
                await connection.OpenAsync();
                /// Armazena em orcamentos os resultados da consulta.
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

        public async Task<Servicos?> GetId(int id)
        {
            Servicos? servico = null;

            /// Comando SQL a ser executado.
            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                             FROM Servico
                             WHERE idServico = @id";
            
                        /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                /// Abre a conexão e executa o comando.
                await connection.OpenAsync();
                /// Armazena em orcamentos os resultados da consulta.
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

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        public async Task Add(Servicos servico)
        {
            /// Comando SQL a ser executado
            string sql = @"INSERT INTO 
                           Servico (descricao, preco_base)
                           VALUES(@descricao, @preco_base)";

            /// Cria a conexão e o comando SQL.  
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@descricao", servico.Descricao);
                command.Parameters.AddWithValue("@preco_base", servico.PrecoBase);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Executa a query SQL que não retorna resultados.
                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Retorna um orçamento para edição.
        /// </summary>
        public async Task<Servicos?> Update(int id)
        {
            Servicos? servico = null;
            string sql = @"SELECT idServico,
                                  descricao,
                                  preco_base
                             FROM Servico
                            WHERE idServico = @id";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                /// Abre a conexão e executa o comando.
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

        /// <summary>
        /// Garante que somente requisições POST possam acessar este método.
        /// </summary>
        [HttpPost]
        public async Task Update(Servicos servico)
        {
            string sql = @"UPDATE Servico
                              SET descricao = @descricao,
                                  preco_base = @preco_base
                            WHERE idServico = @id";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", servico.IdServico);
                command.Parameters.AddWithValue("@descricao", servico.Descricao);
                command.Parameters.AddWithValue("@preco_base", servico.PrecoBase);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Executa a query SQL que não retorna resultados.
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            string sql = @" DELETE FROM ITENS
                            WHERE idServico = @id
                            DELETE FROM Servico
                            WHERE idServico = @id";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Executa a query SQL que não retorna resultados.
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}