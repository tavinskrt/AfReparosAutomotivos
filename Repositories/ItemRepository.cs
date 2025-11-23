using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly string? _connectionString;

        public ItemRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");
        }

        /// <summary>
        /// Insere uma coleção de itens em uma única transação SQL.
        /// </summary>
        public async Task Add(IEnumerable<Item> itens)
        {
            string sql = @" INSERT INTO Itens
                            (idOrcamento, idVeiculo, idServico, data_entrega, qtd, preco, descricao, taxa, desconto)
                            VALUES
                            (@idOrcamento, @idVeiculo, @idServico, @data_entrega, @qtd, @preco, @descricao, @taxa, @desconto)";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // Inicia a transação para garantir que todos os itens sejam inseridos
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    foreach (var item in itens)
                    {
                        using (var command = new SqlCommand(sql, connection, transaction)) // Usa a transação
                        {
                            command.Parameters.AddWithValue("@idOrcamento", item.idOrcamento);
                            command.Parameters.AddWithValue("@idVeiculo", item.idVeiculo);
                            command.Parameters.AddWithValue("@idServico", item.idServico);
                            command.Parameters.AddWithValue("@data_entrega", item.data_entrega);
                            command.Parameters.AddWithValue("@qtd", item.qtd);
                            command.Parameters.AddWithValue("@preco", item.preco);
                            command.Parameters.AddWithValue("@descricao", (object?)item.descricao ?? DBNull.Value);
                            command.Parameters.AddWithValue("@taxa", (object?)item.taxa ?? DBNull.Value);
                            command.Parameters.AddWithValue("@desconto", (object?)item.desconto ?? DBNull.Value);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    // Confirma a transação
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    // Em caso de erro, desfaz (rollback) a transação
                    transaction.Rollback();
                    Console.WriteLine($"Erro SQL em Add(IEnumerable<Item>): {ex.Message}");
                    throw;
                }
            }
        }

        public async Task Delete(int idOrcamento)
        {
            string sql = @"DELETE From Itens
                            WHERE idOrcamento = @idOrcamento";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@idOrcamento", idOrcamento);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Puxar as informações através do id do Orçsamento
        /// </summary>
        public async Task<IEnumerable<Item>> GetByOrcamento(int idOrcamento)
        {
            var itens = new List<Item>();

            string sql = @"
                SELECT 
                    I.idItem,
                    I.idOrcamento,
                    I.idVeiculo,
                    I.idServico,
                    I.data_entrega,
                    I.qtd,
                    I.preco,
                    I.descricao,
                    I.taxa,
                    I.desconto
                FROM Itens I
                WHERE I.idOrcamento = @idOrcamento;
            ";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@idOrcamento", idOrcamento);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        itens.Add(new Item
                        {
                            idItem = reader.GetInt32(reader.GetOrdinal("idItem")),
                            idOrcamento = reader.GetInt32(reader.GetOrdinal("idOrcamento")),
                            idVeiculo = reader.GetInt32(reader.GetOrdinal("idVeiculo")),
                            idServico = reader.GetInt32(reader.GetOrdinal("idServico")),
                            data_entrega = reader["data_entrega"] == DBNull.Value ? null : reader.GetDateTime(reader.GetOrdinal("data_entrega")),
                            qtd = reader.GetInt32(reader.GetOrdinal("qtd")),
                            preco = reader.GetDecimal(reader.GetOrdinal("preco")),
                            descricao = reader["descricao"]?.ToString(),
                            taxa = reader["taxa"] == DBNull.Value ? null : reader.GetDecimal(reader.GetOrdinal("taxa")),
                            desconto = reader["desconto"] == DBNull.Value ? null : reader.GetDecimal(reader.GetOrdinal("desconto")),
                        });
                    }
                }
            }

            return itens;
        }
    }
}