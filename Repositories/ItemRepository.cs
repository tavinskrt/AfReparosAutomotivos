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
                SqlTransaction transaction = connection.BeginTransaction(); 

                try
                {
                    foreach (var item in itens)
                    {
                        using (var command = new SqlCommand(sql, connection, transaction))
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
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
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
    }
}