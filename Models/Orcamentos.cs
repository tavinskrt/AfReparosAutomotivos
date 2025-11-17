using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class Orcamentos
    {
        [Display(Name = "ID do Orçamento")]
        public int idOrcamento { get; set; }

        [Display(Name = "ID do Funcionário")]
        public int idFuncionario { get; set; }
        
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

        [Display(Name = "Nome do Cliente")]
        public string nome { get; set; }  = string.Empty;

        [Display(Name = "Nome do Funcionário")]
        public string nomeFunc { get; set; } = string.Empty;
        
        [Display(Name = "Documento")]
        public string documento { get; set; } = string.Empty;

        [Display(Name = "ID do Serviço")]
        public int? idServico { get; set; }

        [Display(Name = "ID do Veículo")]
        public int? idVeiculo { get; set; }

        [Display(Name = "Placa")]
        public string placa { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public string marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        public string modelo { get; set; } = string.Empty;

        /// <summary>
        /// Cada orçamento possui uma coleção de itens (1 ou mais itens)
        /// </summary>
        public ICollection<Item> Itens {get; set;} = new List<Item>();
    }
}