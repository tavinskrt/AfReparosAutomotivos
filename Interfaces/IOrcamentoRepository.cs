using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<List<Orcamentos>> Get();
        /// <summary>
        /// Adiciona orçamentos.
        /// </summary>
        /// <param name="orcamento">O orçamento a ser adicionado.</param>
        /// <returns>O orçamento.</returns>
        Task Add(Orcamentos orcamento);
        /// <summary>
        /// Atualiza o orçamento.
        /// </summary>
        /// <param name="orcamento">O orçamento a ser atualizado.</param>
        /// <returns>O orçamento atualizado.</returns>
        Task<Orcamentos?> Update(int id);
        Task Update(Orcamentos orcamento);
        /// <summary>
        /// Exclui orçamentos.
        /// </summary>
        /// <param name="id">O id do orçamento a ser excluído.</param>
        Task Delete(int id);
    }
}