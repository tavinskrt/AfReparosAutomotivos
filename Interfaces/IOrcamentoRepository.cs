using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<List<Orcamentos>> Get();

        Task<OrcamentosViewModel?> GetId(int id);

        Task<List<Orcamentos>> GetFilter(OrcamentosFilterViewModel filtros);

        /// <summary>
        /// Adiciona orçamentos.
        /// </summary>
        Task<int> Add(Orcamentos orcamento);

        /// <summary>
        /// Atualiza o orçamento.
        /// </summary>
        Task<Orcamentos?> Update(int id);

        Task Update(OrcamentosViewModel orcamento);

        /// <summary>
        /// Exclui orçamentos.
        /// </summary>
        Task Delete(int id);
    }
}