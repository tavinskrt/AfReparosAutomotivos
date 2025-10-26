using AfReparosAutomotivos.Models;

namespace AfReparosAutomotivos.Interfaces
{
    public interface IOrcamentoRepository
    {
        Task<List<Orcamentos>> Get();

        /// <summary>
        /// Adiciona orçamentos.
        /// </summary>
        Task Add(Orcamentos orcamento);

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