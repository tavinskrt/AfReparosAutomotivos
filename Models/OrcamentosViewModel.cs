using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class OrcamentosViewModel
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

        // Dados do Cliente
        public int? idCli { get; set; }

        [Display(Name = "Documento do Cliente (CPF/CNPJ)")]
        public string DocumentoCli { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string TelefoneCli { get; set; } = string.Empty;

        [Display(Name = "Endereço")]
        public string EnderecoCli { get; set; } = string.Empty;


        // Dados do Veículo
        [Display(Name = "Placa do Veículo")]
        public string? Placa { get; set; }

        [Display(Name = "Marca")]
        public string? Marca { get; set; }

        [Display(Name = "Modelo")]
        public string? Modelo { get; set; }


        // Dados do Serviço
        [Display(Name = "Descrição do Serviço")]
        public string? Descricao { get; set; }
    }
}
