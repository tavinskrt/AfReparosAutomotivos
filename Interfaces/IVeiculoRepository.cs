using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<List<Veiculos>> GetAllAsync();
        Task<int> Add(Veiculos veiculo);
        Task<Veiculos?> GetByPlaca(string placa);
        Task<Veiculos?> GetId(int id);
    }
}