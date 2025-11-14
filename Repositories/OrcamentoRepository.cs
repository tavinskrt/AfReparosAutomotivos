using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;

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
                            // Se o campo data_entrega do banco for nulo, guarda null em dataEntrega. Caso contrário, lê a data e guarda normalmente.
                            dataEntrega = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
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
                            JOIN Cliente C ON O.idCliente = C.idCliente
                            JOIN Pessoa F ON F.idPessoa = O.idFuncionario
                            JOIN Pessoa P ON P.idPessoa = C.idCliente
                            WHERE 1 = 1
            ";
            var where = new StringBuilder();
            var parametros = new List<SqlParameter>();

            if (filtros.statusId.HasValue && filtros.statusId.Value > 0)
            {
                where.Append(" AND O.status = @statusId ");
                parametros.Add(new SqlParameter("@statusId", filtros.statusId.Value));
            }

            if(!string.IsNullOrWhiteSpace(filtros.cpf))
            {
                string busca = $"%{filtros.cpf.Trim()}%";
                where.Append(" AND P.documento LIKE @cpf ");
                parametros.Add(new SqlParameter("@cpf", busca));
            }

            if(!string.IsNullOrWhiteSpace(filtros.nome))
            {
                string busca = $"%{filtros.nome.Trim()}%";
                where.Append(" AND P.nome LIKE @nome ");
                parametros.Add(new SqlParameter("@nome", busca));
            }

            if(filtros.dataCriacao.HasValue)
            {
                where.Append(" AND O.data_criacao = @dataCriacao ");
                parametros.Add(new SqlParameter("@dataCriacao", filtros.dataCriacao.Value.Date));
            }

            if(filtros.dataEntrega.HasValue)
            {
                where.Append(" AND O.data_entrega = @dataEntrega ");
                parametros.Add(new SqlParameter("@dataEntrega", filtros.dataEntrega.Value.Date));
            }

            if(!string.IsNullOrWhiteSpace(filtros.formaPagamento))
            {
                where.Append(" AND O.forma_pgto = @metodoPagamento ");
                parametros.Add(new SqlParameter("@metodoPagamento", filtros.formaPagamento));
            }

            if(filtros.parcelas.HasValue && filtros.parcelas > 0)
            {
                where.Append(" AND O.parcelas = @parcelas ");
                parametros.Add(new SqlParameter("@parcelas", filtros.parcelas.Value));
            }

            if(filtros.preco.HasValue && filtros.preco > 0)
            {
                where.Append(" AND O.total <= @preco ");
                parametros.Add(new SqlParameter("@preco", filtros.preco.Value));
            }

            string consulta = sql + where.ToString() + " ORDER BY O.data_criacao DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(consulta, connection))
            {
                command.Parameters.AddRange(parametros.ToArray());
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
                // Verifica se orcamento.dataEntrega tem valor, se tiver, envia para o banco. Se for nulo, envia DBNull.Value para evitar erro.
                command.Parameters.AddWithValue("@data_entrega", (object?)orcamento.dataEntrega ?? DBNull.Value);
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
                            JOIN Cliente C ON O.idCliente = C.idCliente
                            JOIN Pessoa F ON F.idPessoa = O.idFuncionario
                            JOIN Pessoa P ON P.idPessoa = C.idCliente
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
                            // // Se o campo data_entrega do banco for nulo, guarda null em dataEntrega. Caso contrário, lê a data e guarda normalmente.
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
            return orcamento;
        }

        /// <summary>
        /// Garante que somente requisições POST possam acessar este método.
        /// </summary>
        [HttpPost]
        public async Task Update(Orcamentos orcamento)
        {
            string sql = @"UPDATE Orcamento
                              SET
                                  data_entrega = @data_entrega,
                                  status = @status,
                                  total = @total,
                                  parcelas = @parcelas,
                                  forma_pgto = @forma_pgto
                            WHERE idOrcamento = @id";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", orcamento.idOrcamento);
                command.Parameters.AddWithValue("@funcionario", orcamento.idFuncionario);
                command.Parameters.AddWithValue("@data_entrega", orcamento.dataEntrega.HasValue ? (object)orcamento.dataEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@status", orcamento.status);
                command.Parameters.AddWithValue("@total", orcamento.total);
                command.Parameters.AddWithValue("@parcelas", orcamento.parcelas);
                command.Parameters.AddWithValue("@forma_pgto", orcamento.formaPagamento);

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