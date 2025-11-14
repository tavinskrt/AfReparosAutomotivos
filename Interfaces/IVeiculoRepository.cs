using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<List<Veiculos>> GetAllAsync();
        Task<int> Add(Veiculos veiculo);
    }
}