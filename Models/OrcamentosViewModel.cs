using System.ComponentModel.DataAnnotations;

namespace AfReparosAutomotivos.Models
{
    public class OrcamentosViewModel
    {
        // Dados do Cliente
        [Display(Name = "Nome do Cliente")]
        public string NomeCli { get; set; }

        [Display(Name = "Documento do Cliente (CPF/CNPJ)")]
        public string DocumentoCli { get; set; }

        [Display(Name = "Telefone")]
        public string TelefoneCli { get; set; }

        [Display(Name = "Endereço")]
        public string EnderecoCli { get; set; }


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
