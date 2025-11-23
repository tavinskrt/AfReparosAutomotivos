using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class ItemViewModel
    {
        [Required]
        [StringLength(7)]
        public string Placa { get; set; } = string.Empty;
        
        [Required]
        public string Marca { get; set; } = string.Empty;
        
        [Required]
        public string Modelo { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Serviço")]
        public int idServico { get; set; }
        
        [Required]
        [Display(Name = "Quantidade")]
        [Range(1, int.MaxValue)]
        public int qtd { get; set; }

        public DateTime? data_entrega { get; set; } 

        public decimal preco { get; set; }

        public int idVeiculo { get; set; }

        public string? descricao { get; set; }

        public decimal taxa { get; set; }

        public decimal desconto { get; set; }
    }
}