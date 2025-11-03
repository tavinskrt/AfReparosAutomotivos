using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Clientes>> Get();

        Task<Clientes?> GetId(int id);

        /// <summary>
        /// Adiciona Clientes.
        /// </summary>
        Task Add(Clientes cliente);

        /// <summary>
        /// Atualiza o cliente.
        /// </summary>
        Task<Clientes?> Update(int id);

        Task Update(Clientes cliente);

        /// <summary>
        /// Exclui clientes.
        /// </summary>
        Task Delete(int id);
    }
}