namespace VaquinhaAnimal.Domain.Entities.Pagarme
{
    public class PagarmeClienteEndereco
    {
        public string line_1 { get; set; }
        public string line_2 { get; set; }
        public string zip_code { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }
}