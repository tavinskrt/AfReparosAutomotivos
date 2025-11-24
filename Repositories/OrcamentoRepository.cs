using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Text;
using AfReparosAutomotivos.Models.ViewModels; 

namespace AfReparosAutomotivos.Repositories
{
    /// <summary>
    /// Repositório para operações de Orçamentos.
    /// </summary>
    public class OrcamentoRepository : IOrcamentoRepository
    {
        /// <summary>
        /// Reserva espaço para a string de conexão com o banco de dados.
        /// </summary>
        private readonly string? _connectionString;

        public OrcamentoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");

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
            List<Orcamentos> orcamentos = new List<Orcamentos>();

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
                try
                {
                    await connection.OpenAsync();
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
                                dataEntrega = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                status = reader.GetInt32(5),
                                total = reader.GetDecimal(6),
                                formaPagamento = reader.GetString(7),
                                parcelas = reader.GetInt32(8)
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em Get(): {ex.Message}");
                    throw;
                }
            }
            return orcamentos;
        }

        public async Task<Orcamentos?> GetId(int id)
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
                                parcelas,
                                Pessoa.nome,
                                Funcionario.nome
                                 FROM Orcamento
                                 JOIN Pessoa ON Pessoa.idPessoa = Orcamento.idCliente
                                 JOIN Pessoa AS Funcionario ON Funcionario.idPessoa = Orcamento.idFuncionario
                                 WHERE idOrcamento = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                try
                {
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
                                dataEntrega = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
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
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em GetId(): {ex.Message}");
                    throw;
                }
            }
            return orcamento;
        }

        public async Task<List<Orcamentos>> GetFilter(OrcamentosFilterViewModel filtros)
        {
            List<Orcamentos> orcamentos = new List<Orcamentos>();

            string sql = @" SELECT
                             O.idOrcamento,
                             O.idFuncionario,
                             O.idCliente,
                             O.data_criacao,
                             O.data_entrega,
                             O.status,
                             O.total,
                             O.forma_pgto,
                             O.parcelas,
                             P.nome,
                             F.nome,
                             P.documento
                             FROM Orcamento O
                             JOIN Pessoa P ON P.idPessoa = O.idCliente
                             JOIN Pessoa F ON F.idPessoa = O.idFuncionario
                             WHERE 1 = 1
                             ";
            var where = new StringBuilder();
            var parametros = new List<SqlParameter>();

            if (filtros.statusId.HasValue && filtros.statusId.Value > 0)
            {
                where.Append(" AND O.status = @statusId ");
                parametros.Add(new SqlParameter("@statusId", filtros.statusId.Value));
            }

            if (!string.IsNullOrWhiteSpace(filtros.cpf))
            {
                string busca = $"%{filtros.cpf.Trim()}%";
                where.Append(" AND P.documento LIKE @cpf ");
                parametros.Add(new SqlParameter("@cpf", busca));
            }

            if (!string.IsNullOrWhiteSpace(filtros.nome))
            {
                string busca = $"%{filtros.nome.Trim()}%";
                where.Append(" AND P.nome LIKE @nome ");
                parametros.Add(new SqlParameter("@nome", busca));
            }

            if (filtros.dataCriacao.HasValue)
            {
                where.Append(" AND CONVERT(DATE, O.data_criacao) = @dataCriacao "); 
                parametros.Add(new SqlParameter("@dataCriacao", filtros.dataCriacao.Value.Date));
            }

            if (filtros.dataEntrega.HasValue)
            {
                where.Append(" AND CONVERT(DATE, O.data_entrega) = @dataEntrega "); 
                parametros.Add(new SqlParameter("@dataEntrega", filtros.dataEntrega.Value.Date));
            }

            if (!string.IsNullOrWhiteSpace(filtros.formaPagamento))
            {
                where.Append(" AND O.forma_pgto = @metodoPagamento ");
                parametros.Add(new SqlParameter("@metodoPagamento", filtros.formaPagamento));
            }

            if (filtros.parcelas.HasValue && filtros.parcelas > 0)
            {
                where.Append(" AND O.parcelas = @parcelas ");
                parametros.Add(new SqlParameter("@parcelas", filtros.parcelas.Value));
            }

            if (filtros.preco.HasValue && filtros.preco > 0)
            {
                where.Append(" AND O.total <= @preco ");
                parametros.Add(new SqlParameter("@preco", filtros.preco.Value));
            }

            string consulta = sql + where.ToString() + " ORDER BY O.data_criacao DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(consulta, connection))
            {
                command.Parameters.AddRange(parametros.ToArray());
                try
                {
                    await connection.OpenAsync();

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
                                dataEntrega = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                status = reader.GetInt32(5),
                                total = reader.GetDecimal(6),
                                formaPagamento = reader.GetString(7),
                                parcelas = reader.GetInt32(8),
                                nome = reader.GetString(9),
                                nomeFunc = reader.GetString(10),
                                documento = reader.GetString(11)
                            });
                        }
                    }
                    return orcamentos;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em GetFilter(): {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Cria um novo orçamento (apenas o cabeçalho) e retorna o ID gerado automaticamente.
        /// </summary>
        /// <returns>O ID (int) do orçamento recém-criado.</returns>
        public async Task<int> Add(Orcamentos orcamento)
        {
            string sqlOrcamento = @"INSERT INTO 
                                        Orcamento (idFuncionario, idCliente, data_criacao, data_entrega, status, total, forma_pgto, parcelas)
                                        VALUES (@funcionario, @cliente, @data_criacao, @data_entrega, @status, @total, @forma_pgto, @parcelas);
                                    SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(_connectionString))
            using (var commandOrcamento = new SqlCommand(sqlOrcamento, connection))
            {
                commandOrcamento.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                commandOrcamento.Parameters.AddWithValue("@cliente", orcamento.idCliente);
                commandOrcamento.Parameters.AddWithValue("@data_criacao", orcamento.dataCriacao);
                commandOrcamento.Parameters.AddWithValue("@data_entrega", (object?)orcamento.dataEntrega ?? DBNull.Value);
                commandOrcamento.Parameters.AddWithValue("@status", orcamento.status);
                commandOrcamento.Parameters.AddWithValue("@total", orcamento.total);
                commandOrcamento.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);
                commandOrcamento.Parameters.AddWithValue("@parcelas", orcamento.parcelas);

                try
                {
                    await connection.OpenAsync();
                    
                    var newId = await commandOrcamento.ExecuteScalarAsync();

                    if (newId == null || newId == DBNull.Value)
                    {
                        throw new InvalidOperationException("Falha ao obter o ID do orçamento recém-criado.");
                    }
                    
                    int idOrcamento = Convert.ToInt32(newId);
                    return idOrcamento;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em Add(): {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Retorna um orçamento para edição.
        /// </summary>
        public async Task<Orcamentos?> Update(int id)
        {
            Orcamentos? orcamento = null;
            string sql = @" SELECT
                             O.idOrcamento,
                             O.idFuncionario,
                             O.idCliente,
                             O.data_criacao,
                             O.data_entrega,
                             O.status,
                             O.total,
                             O.forma_pgto,
                             O.parcelas,
                             P.nome,
                             F.nome,
                             P.documento
                             FROM Orcamento O
                             JOIN Pessoa P ON P.idPessoa = O.idCliente
                             JOIN Pessoa F ON F.idPessoa = O.idFuncionario
                             WHERE idOrcamento = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                try
                {
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
                                dataEntrega = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                status = reader.GetInt32(5),
                                total = reader.GetDecimal(6),
                                formaPagamento = reader.GetString(7),
                                parcelas = reader.GetInt32(8),
                                nome = reader.GetString(9),
                                nomeFunc = reader.GetString(10),
                                documento = reader.GetString(11)
                            };
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em Update(int id) - SELECT: {ex.Message}");
                    throw;
                }
            }
            return orcamento;
        }

        /// <summary>
        /// Atualiza um orçamento existente.
        /// </summary>
        public async Task Update(Orcamentos orcamento)
        {
            string sql = @"UPDATE Orcamento
                                     SET idFuncionario = @funcionario,
                                         idCliente = @cliente,
                                         data_entrega = @data_entrega,
                                         status = @status,
                                         total = @total,
                                         forma_pgto = @forma_pgto,
                                         parcelas = @parcelas
                                     WHERE idOrcamento = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", orcamento.idOrcamento);
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@cliente", orcamento.idCliente);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega.HasValue ? (object)orcamento.dataEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);

                try
                {
                    await connection.OpenAsync();

                    await command.ExecuteNonQueryAsync();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em Update(Orcamentos): {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Deleta o orçamento e, primeiramente, seus itens relacionados (exclusão em cascata manual).
        /// </summary>
        public async Task Delete(int id)
        {
            string sql = @" DELETE FROM ITENS
                               WHERE idOrcamento = @id;
                               DELETE FROM Orcamento
                               WHERE idOrcamento = @id;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    await connection.OpenAsync();

                    await command.ExecuteNonQueryAsync();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Erro SQL em Delete(): {ex.Message}");
                    throw;
                }
            }
        }
    }
}