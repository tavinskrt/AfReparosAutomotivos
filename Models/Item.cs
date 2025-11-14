using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Item
    {
        // Indicador para sinalizar que o item é chave primária no banco
        [Key]
        public int idItem {get; set;}

        // Indicador para sinalizar que o item é obrigatório
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

        public int qtd {get; set;}

        // Indicador para definir o tamanho do atributo
        [StringLength(50)]
        public string? descricao {get; set;}

        public decimal? taxa {get; set;}

        public decimal? desconto {get; set;}

        public Orcamentos? Orcamentos {get; set;}
        public Veiculos? Veiculos {get; set;}
        public Servicos? Servicos {get; set;}
    }
}