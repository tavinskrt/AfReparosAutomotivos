namespace AfRepartosAutomotivos.Models
{
    public class OrcamentoViewModel
    {
        // Dados do Cliente
        public string NomeCli { get; set; }
        public string TelefoneCli { get; set; }
        public string CelularCli { get; set; }
        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Numero { get; set; }
        public string Cidade { get; set; }
        public string UF { get; set; }

        // Dados do Veículo
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Cor { get; set; }
        public string Ano { get; set; }

        // Dados do Serviço
        public string Descricao { get; set; }
    }
}
