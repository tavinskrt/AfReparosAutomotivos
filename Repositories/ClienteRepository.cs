using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.AspNetCore.Authorization;
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
            /// Armazena a string de conexão vinda do arquivo de configuração.
            _connectionString = configuration.GetConnectionString("default");

            /// Retorna um erro se a string de conexão não for encontrada.
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
            /// Cria a lista de orçamentos.
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

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        public async Task<int> Add(Clientes cliente)
        {
            if (cliente.documento.Length == 11)
            {
                cliente.tipo_doc = 'F';
            }
            else if (cliente.documento.Length == 14)
            {
                cliente.tipo_doc = 'J';
            }

            /// Comando SQL a ser executado
            string sql = @"
                            INSERT INTO Pessoa (nome, telefone, endereco, documento, tipo_doc)
                            VALUES (@nome, @telefone, @endereco, @documento, @tipo_doc)
                            
                            DECLARE @id_pessoa INT = SCOPE_IDENTITY()
                            
                            INSERT INTO CLIENTE (idCliente)
                            VALUES (@id_pessoa)
                            
                            SELECT @id_pessoa";

            /// Cria a conexão e o comando SQL.  
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@nome", cliente.nome);
                command.Parameters.AddWithValue("@telefone", cliente.telefone);
                command.Parameters.AddWithValue("@documento", cliente.documento);
                command.Parameters.AddWithValue("@endereco", (object)cliente.endereco ?? DBNull.Value);
                command.Parameters.AddWithValue("@tipo_doc", cliente.tipo_doc);

                /// Abre a conexão.
                await connection.OpenAsync();

                /// Retornando o ID do cliente na variável result
                var result = await command.ExecuteScalarAsync();

                /// Convertendo o ID do cliente para int
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                throw new InvalidOperationException("Falha ao obter o ID do cliente recém criado.");
            }
        }
    }
}