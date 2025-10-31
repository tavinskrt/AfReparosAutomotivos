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

    public class OrcamentoRepository : IOrcamentoRepository
    {
        /// <summary>
        /// Reserva espaço para a string de conexão com o banco de dados.
        /// </summary>
        private readonly string? _connectionString;

        public OrcamentoRepository(IConfiguration configuration)
        {
            /// Armazena a string de conexão vinda do arquivo de configuração.
            _connectionString = configuration.GetConnectionString("default");

            /// Retorna um erro se a string de conexão não for encontrada.
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
            }
        }

        /// <summary>
        /// Retorna a lista de orçamentos.
        /// </summary>
        public async Task<List<Orcamentos>> Get()
        {
            /// Cria a lista de orçamentos.
            List<Orcamentos> orcamentos = new List<Orcamentos>();

            /// Comando SQL a ser executado.
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
                        orcamentos.Add(new Orcamentos
                        {
                            idOrcamento = reader.GetInt32(0),
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
            return orcamentos;
        }

        public async Task<Orcamentos?> GetId(int id)
        {
            Orcamentos? orcamento = null;

            /// Comando SQL a ser executado.
            string sql = @"SELECT idOrcamento,
                                  idFuncionario,
                                  idCliente,
                                  data_criacao,
                                  data_entrega,
                                  status,
                                  total,
                                  forma_pgto,
                                  parcelas,
                                  Pessoa.nome,
                                  Funcionario.nome
                             FROM Orcamento
                             JOIN Pessoa ON idPessoa = Orcamento.idCliente
                             JOIN Pessoa AS Funcionario ON Funcionario.idPessoa = Orcamento.idFuncionario
                             WHERE idOrcamento = @id";
            
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
                        orcamento = new Orcamentos
                        {
                            idOrcamento = reader.GetInt32(0),
                            idFuncionario = reader.GetInt32(1),
                            idCliente = reader.GetInt32(2),
                            dataCriacao = reader.GetDateTime(3),
                            dataEntrega = reader.IsDBNull(4) ? (DateTime?)null: reader.GetDateTime(4),
                            status = reader.GetInt32(5),
                            total = reader.GetDecimal(6),
                            formaPagamento = reader.GetString(7),
                            parcelas = reader.GetInt32(8),
                            nome = reader.GetString(9),
                            nomeFunc = reader.GetString(10)
                        };
                    }
                }
            }
            return orcamento;
        }

        /// <summary>
        /// Cria um novo orçamento.
        /// </summary>
        public async Task Add(Orcamentos orcamento)
        {
            /// Comando SQL a ser executado
            string sql = @"INSERT INTO 
                           Orcamento (idFuncionario, idCliente, data_criacao, data_entrega, status, total, forma_pgto, parcelas)
                           VALUES (@funcionario, @cliente, @data_criacao, @data_entrega, @status, @total, @forma_pgto, @parcelas)";

            /// Cria a conexão e o comando SQL.  
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@cliente", orcamento.idCliente);
                command.Parameters.AddWithValue("@data_criacao", orcamento.dataCriacao);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega.HasValue ? (object)orcamento.dataEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Executa a query SQL que não retorna resultados.
                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Retorna um orçamento para edição.
        /// </summary>
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
                        orcamento = new Orcamentos
                        {
                            idOrcamento = reader.GetInt32(0),
                            idFuncionario = reader.GetInt32(1),
                            idCliente = reader.GetInt32(2),
                            dataCriacao = reader.GetDateTime(3),
                            dataEntrega = reader.IsDBNull(4) ? (DateTime?)null: reader.GetDateTime(4),
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

        /// <summary>
        /// Garante que somente requisições POST possam acessar este método.
        /// </summary>
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

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", orcamento.idOrcamento);
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Executa a query SQL que não retorna resultados.
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(int id)
        {
            string sql = @" DELETE FROM ITENS
                            WHERE idOrcamento = @id
                            DELETE FROM Orcamento
                            WHERE idOrcamento = @id";

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