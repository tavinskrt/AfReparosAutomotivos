namespace AfReparosAutomotivos.Interfaces
{
    public interface ILoginRepository
    {
        Task<Funcionario?> GetFuncionarioByCredentialsAsync(string username, string senha);
    }

    public class Funcionario()
    {
        public int idFuncionario { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}