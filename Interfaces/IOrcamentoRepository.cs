using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<List<Orcamentos>> Get();

        Task<Orcamentos?> GetId(int id);

        Task<List<Orcamentos>> GetFilter(OrcamentosFilterViewModel filtros);

        /// <summary>
        /// Adiciona orçamentos.
        /// </summary>
        Task<int> Add(Orcamentos orcamento);

        /// <summary>
        /// Atualiza o orçamento.
        /// </summary>
        Task<Orcamentos?> Update(int id);

        Task Update(Orcamentos orcamento);

        /// <summary>
        /// Exclui orçamentos.
        /// </summary>
        Task Delete(int id);
    }
}