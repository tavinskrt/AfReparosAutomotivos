using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Clientes>> GetAllAsync();

        /// <summary>
        /// Adiciona Clientes.
        /// </summary>
        Task<int> Add(Clientes cliente);

        /// <summary>
        /// Atualiza o cliente.
        /// </summary>
    }
}