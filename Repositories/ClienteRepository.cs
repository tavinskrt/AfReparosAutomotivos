using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        /// <summary>
        /// Reserva espaço para a string de conexão com o banco de dados.
        /// </summary>
        private readonly string? _connectionString;

        public ClienteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
            }
        }

        /// <summary>
        /// Retorna a lista de clientes.
        /// </summary>
        public async Task<List<Clientes>> GetAllAsync()
        {
            List<Clientes> clientes = new List<Clientes>();

            string sql = @"SELECT Cliente.idCliente,
                                  Pessoa.nome,
                                  Pessoa.telefone,
                                  Pessoa.endereco,
                                  Pessoa.documento,
                                  Pessoa.tipo_doc
                             FROM Cliente
                             JOIN Pessoa ON Pessoa.idPessoa = Cliente.idCliente
                             ORDER BY Pessoa.nome";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        clientes.Add(new Clientes
                        {
                            id = reader.GetInt32(0),
                            nome = reader.GetString(1),
                            telefone = reader.GetString(2),
                            endereco = reader.GetString(3),
                            documento = reader.GetString(4)
                        });
                    }
                }
            }
            return clientes;
        }
        public async Task<Clientes?> GetId(int id)
        {  
            Clientes? cliente = null;
            string sql = @"SELECT Cliente.idCliente,
                                  Pessoa.nome,
                                  Pessoa.telefone,
                                  Pessoa.endereco,
                                  Pessoa.documento,
                                  Pessoa.tipo_doc
                             FROM Cliente
                             JOIN Pessoa ON Pessoa.idPessoa = Cliente.idCliente
                             WHERE Cliente.idCliente = @id";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cliente = new Clientes
                        {
                            id = reader.GetInt32(0),
                            nome = reader.GetString(1),
                            telefone = reader.GetString(2),
                            endereco = reader.GetString(3),
                            documento = reader.GetString(4)
                        };
                    }
                } 
            }
            return cliente;
        }

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        public async Task<int> Add(Clientes cliente)
        {
            Clientes? clienteExistente = await GetByDocumento(cliente.documento);
            if (clienteExistente != null)
            {
                return clienteExistente.id;
            }
            if (cliente.documento.Length == 11)
            {
                cliente.tipo_doc = 'F';
            }
            else if (cliente.documento.Length == 14)
            {
                cliente.tipo_doc = 'J';
            }
            else
            {
                throw new ArgumentException("Documento inválido. Deve ser CPF (11 dígitos) ou CNPJ (14 dígitos).");
            }

            string sql = @"
                            INSERT INTO Pessoa (nome, telefone, endereco, documento, tipo_doc)
                            VALUES (@nome, @telefone, @endereco, @documento, @tipo_doc)
                            
                            DECLARE @id_pessoa INT = SCOPE_IDENTITY()
                            
                            INSERT INTO CLIENTE (idCliente)
                            VALUES (@id_pessoa)
                            
                            SELECT @id_pessoa";
 
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@nome", cliente.nome);
                command.Parameters.AddWithValue("@telefone", cliente.telefone);
                command.Parameters.AddWithValue("@documento", cliente.documento);
                command.Parameters.AddWithValue("@endereco", (object)cliente.endereco ?? DBNull.Value);
                command.Parameters.AddWithValue("@tipo_doc", cliente.tipo_doc);

                await connection.OpenAsync();

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                throw new InvalidOperationException("Falha ao obter o ID do cliente recém criado.");
            }
        }

        public async Task Update(Clientes cliente)
        {
            string sql = @"UPDATE Pessoa
                              SET nome = @nome,
                                  documento = @documento,
                                  telefone = @telefone,
                                  endereco = @endereco
                            WHERE idPessoa = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", cliente.id);
                command.Parameters.AddWithValue("@nome", cliente.nome);
                command.Parameters.AddWithValue("@documento", cliente.documento);
                command.Parameters.AddWithValue("@telefone", cliente.telefone);
                command.Parameters.AddWithValue("@endereco", cliente.endereco);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Clientes?> GetByDocumento(string documento)
        {
            Clientes? cliente = null;
            string sql = @"SELECT Cliente.idCliente,
                                  Pessoa.nome,
                                  Pessoa.telefone,
                                  Pessoa.endereco,
                                  Pessoa.documento
                             FROM Cliente
                             JOIN Pessoa ON Pessoa.idPessoa = Cliente.idCliente
                             WHERE Pessoa.documento = @documento";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@documento", documento);
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cliente = new Clientes
                        {
                            id = reader.GetInt32(0),
                            nome = reader.GetString(1),
                            telefone = reader.GetString(2),
                            endereco = reader.GetString(3),
                            documento = reader.GetString(4)
                        };
                    }
                } 
            }
            return cliente;
        }
    }
}