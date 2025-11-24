using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IServicoRepository
    {
        Task<List<Servicos>> Get();

        Task<Servicos?> GetId(int id);

        Task<decimal> GetPrecoBaseByIdAsync(int id);

        Task Add(Servicos servico);

        Task<Servicos?> Update(int id);

        Task Update(Servicos servico);

        Task Delete(int id);
    }
}