using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfReparosAutomotivos.Models
{
    public class ItemViewModel
    {
        [Required]
        [StringLength(7)]
        public string Placa { get; set; }
        
        [Required]
        public string Marca { get; set; }
        
        [Required]
        public string Modelo { get; set; }
        
        [Required]
        [Display(Name = "Serviço")]
        public int idServico { get; set; }
        
        [Required]
        [Display(Name = "Quantidade")]
        [Range(1, int.MaxValue)]
        public int qtd { get; set; } 
    }
}