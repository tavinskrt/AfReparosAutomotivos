using AfReparosAutomotivos.Interfaces;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly string? _connectionString;
        public LoginRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("default");
            if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Erro de conexão: string de conexão não configurada.");
        }
        
        public async Task<Funcionario?> GetFuncionarioByCredentialsAsync(string username, string senha)
        {

            string sql = @"SELECT Funcionario.idFuncionario,
                                  Pessoa.nome
                             FROM Funcionario
                             JOIN Pessoa ON Pessoa.idPessoa = Funcionario.idFuncionario
                            WHERE
                                  usuario = @usuario
                              AND senha = @senha";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@usuario", username);
                command.Parameters.AddWithValue("@senha", senha);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
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