using AfReparosAutomotivos.Interfaces;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        /// <summary>
        /// Reserva espaço para a string de conexão com o banco de dados.
        /// </summary>
        private readonly string? _connectionString;

        public LoginRepository(IConfiguration configuration)
        {
            /// Armazena a string de conexão vinda do arquivo de configuração.
            _connectionString = configuration.GetConnectionString("default");

            /// Verifica se a string de conexão foi encontrada.
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Erro de configuração: string de conexão não encontrada.");
            }
        }
        
        /// <summary>
        /// Obtém um funcionário com base nas credenciais fornecidas.
        /// </summary>
        public async Task<Funcionario?> GetFuncionarioByCredentialsAsync(string username, string senha)
        {
            string sql = @"SELECT Funcionario.idFuncionario,
                                  Pessoa.nome
                             FROM Funcionario
                             JOIN Pessoa ON Pessoa.idPessoa = Funcionario.idFuncionario
                            WHERE
                                  usuario = @usuario
                              AND senha = @senha";

            /// Cria a conexão e o comando SQL.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                /// Adiciona os parâmetros ao comando SQL.
                command.Parameters.AddWithValue("@usuario", username);
                command.Parameters.AddWithValue("@senha", senha);

                /// Abre a conexão e executa o comando.
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    /// Retorna o funcionário se encontrado.
                    if (await reader.ReadAsync())
                    {
                        var funcionario = new Funcionario
                        {
                            idFuncionario = reader.GetInt32(0),
                            Nome = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                        };
                        return funcionario;
                    }
                    return null;
                }
            }
        }
    }
}