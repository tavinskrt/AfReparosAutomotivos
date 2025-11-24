using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IItemRepository
    {
        Task Add(IEnumerable<Item> itens);
        Task Update(IEnumerable<Item> itens);
        Task Delete(int idOrcamento);
        Task<IEnumerable<Item>> GetByOrcamento(int idOrcamento);
    }
}