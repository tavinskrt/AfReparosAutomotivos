using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IClienteRepository
    {
        Task<int> Add(Clientes cliente);
        Task<List<Clientes>> GetAllAsync();
        Task<Clientes?> GetId(int id);
        Task<Clientes?> GetByDocumento(string documento);
        Task Update(Clientes cliente);
    }
}