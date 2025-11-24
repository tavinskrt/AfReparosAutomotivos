using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Servicos
    {
        [Display(Name = "ID do Serviço")]
        public int IdServico { get; set; }

        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;
        
        [Display(Name = "Preço Base")]
        [Range(0, 99999999.99, ErrorMessage = "O preço deve estar entre 0 e 99.999.999,99")]
        [RegularExpression(@"^\d{1,8}(\,\d{1,2}|\.\d{1,2})?$", ErrorMessage = "Use no máximo 8 dígitos antes e 2 depois da vírgula.")]
        public decimal PrecoBase { get; set; }
    }
}