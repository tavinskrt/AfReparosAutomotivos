using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Item
    {
        [Key]
        public int idItem {get; set;}

        [Required]
        public int idOrcamento {get; set;}

        [Required]
        public int idVeiculo {get; set;}

        [Required]
        public int idServico {get; set;}

        [Required]
        [Display(Name = "Data de Entrega")]
        public DateTime? data_entrega {get; set;}

        [Required]
        [Display(Name = "Preço Unitário")]
        public decimal preco {get; set;}

        [Required]
        public int qtd {get; set;}

        [StringLength(50)]
        public string? descricao {get; set;}

        public decimal? taxa {get; set;}

        public decimal? desconto {get; set;}

        public Orcamentos? Orcamentos {get; set;}
        public Veiculos? Veiculos {get; set;}
        public Servicos? Servicos {get; set;}
    }
}