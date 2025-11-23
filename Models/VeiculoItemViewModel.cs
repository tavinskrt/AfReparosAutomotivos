using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class VeiculoItemViewModel
    {
        public int idVeiculo { get; set; }

        [Required]
        [StringLength(7)]
        public string Placa { get; set; } = string.Empty;
        
        [Required]
        public string Marca { get; set; } = string.Empty;
        
        [Required]
        public string Modelo { get; set; } = string.Empty;
        
        public List<ItemViewModel> ServicosAssociados { get; set; } = new List<ItemViewModel>();
    }
}