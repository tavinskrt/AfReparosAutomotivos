using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Servicos
    {
        [Display(Name = "ID do Serviço")]
        public int IdServico { get; set; }

        [Display(Name = "Descrição")]
        public string Descricao { get; set; }
        
        [Display(Name = "Preço Base")]
        public decimal PrecoBase { get; set; }
    }
}