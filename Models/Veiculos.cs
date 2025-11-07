namespace AfReparosAutomotivos.Models
{
    public class Veiculos
    {
        public int? id { get; set; }
        public string placa { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
    }
}