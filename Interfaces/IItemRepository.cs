using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IItemRepository
    {
        Task Add(IEnumerable<Item> itens);
        Task Delete(int idOrcamento);
    }
}