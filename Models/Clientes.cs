namespace AfReparosAutomotivos.Models
{
    public class Clientes
    {
        // Dados do Cliente
        public int id { get; set; }
        public string nome { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string telefone { get; set; } = string.Empty;
        public string endereco { get; set; } = string.Empty;

        public char? tipo_doc { get; set; }
    }
}
