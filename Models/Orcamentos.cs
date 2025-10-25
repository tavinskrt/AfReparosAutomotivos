using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Orcamentos
    {
        [Display(Name = "ID do Orçamento")]
        public int idOrcamento { get; set; }
        [Display(Name = "ID do Funcionário")]
        public int idFuncionario { get; set;}
        [Display(Name = "ID do Cliente")]
        public int idCliente { get; set; }
        [Display(Name = "Data de Criação")]
        public DateTime dataCriacao { get; set; }
        [Display(Name = "Data de Entrega")]
        public DateTime? dataEntrega { get; set; }
        [Display(Name = "Status")]
        public int status { get; set; }
        [Display(Name = "Total")]
        public decimal total { get; set; }
        [Display(Name = "Forma de Pagamento")]
        public string formaPagamento { get; set; } = string.Empty;
        [Display(Name = "Parcelas")]
        public int parcelas { get; set; }
    }
}