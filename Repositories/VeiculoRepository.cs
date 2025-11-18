using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly string? _connectionString;

        public VeiculoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
            }
        }

        public async Task<List<Veiculos>> GetAllAsync()
        {
            List<Veiculos> veiculos = new List<Veiculos>();

            string sql = @"SELECT 	 Veiculo.idVeiculo,
                                     Veiculo.marca,
                                     Veiculo.placa, 
                                     Veiculo.modelo
                           FROM 	 Veiculo";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        veiculos.Add(new Veiculos{
                            id = reader.GetInt32(0),
                            marca = reader.GetString(1),
                            placa = reader.GetString(2),
                            modelo = reader.GetString(3)
                        });
                    }
                }
            }
            return veiculos;
        }

        public async Task<int> Add(Veiculos veiculo)
        {
            Veiculos? veiculoExistente = await GetByPlaca(veiculo.placa);
            if (veiculoExistente != null)
            {
                return Convert.ToInt32(veiculoExistente.id);
            }
            string sql = @"
                            INSERT INTO Veiculo (marca, placa, modelo)
                            VALUES (@marca, @placa, @modelo);
                            SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@marca", veiculo.marca);
                command.Parameters.AddWithValue("@placa", veiculo.placa);
                command.Parameters.AddWithValue("@modelo", veiculo.modelo);

                await connection.OpenAsync();
                
                var newId = await command.ExecuteScalarAsync();

                if (newId == null || newId == DBNull.Value)
                {
                    throw new InvalidOperationException("Falha ao obter o ID do veículo recém-criado.");
                }

                return Convert.ToInt32(newId);
            }
        }

        public async Task<Veiculos?> GetByPlaca(string placa)
        {
            Veiculos? veiculo = null;
            string sql = @"SELECT Veiculo.idVeiculo,
                                  Veiculo.marca,
                                  Veiculo.placa, 
                                  Veiculo.modelo
                            FROM Veiculo 
                            WHERE Veiculo.placa = @placa";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@placa", placa);
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        veiculo = new Veiculos
                        {
                            id = reader.GetInt32(0),
                            marca = reader.GetString(1),
                            placa = reader.GetString(2),
                            modelo = reader.GetString(3)
                        };
                    }
                } 
            }
            return veiculo;
        }
    }
}