using System.Runtime.InteropServices;
using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            string sql = @"SELECT   Veiculo.idVeiculo,
                                    Veiculo.marca,
                                    Veiculo.placa
                                    Veiculo.modelo
                             FROM   Veiculo";

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

        public async Task Add(Veiculos veiculo)
        {
            string sql = @"
                            INSERT INTO Veiculo (marca, placa, modelo)
                            VALUES (@marca, @placa, @modelo)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@marca", veiculo.marca);
                command.Parameters.AddWithValue("@placa", veiculo.placa);
                command.Parameters.AddWithValue("@modelo", veiculo.modelo);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}