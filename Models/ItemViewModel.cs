using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class ItemViewModel
    {
        [Required]
        [Display(Name = "Serviço")]
        public int idServico { get; set; }
        
        [Required]
        [Display(Name = "Quantidade")]
        [Range(1, int.MaxValue)]
        public int qtd { get; set; }

        public DateTime? data_entrega { get; set; } 

        public decimal preco { get; set; }


        public string? descricao { get; set; }

        public decimal taxa { get; set; }

        public decimal desconto { get; set; }
    }
}