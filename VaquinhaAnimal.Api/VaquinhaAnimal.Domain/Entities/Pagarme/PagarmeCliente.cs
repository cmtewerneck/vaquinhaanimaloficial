using System;
using System.Text.Json.Serialization;

namespace VaquinhaAnimal.Domain.Entities.Pagarme
{
    public class PagarmeCliente
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("document")]
        public string Document { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("document_type")]
        public string Document_Type { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("birthdate")]
        public DateTime Birthdate { get; set; }

        [JsonPropertyName("address")]
        public PagarmeCliente_Endereco Address { get; set; }

        [JsonPropertyName("phones")]
        public PagarmeCliente_Telefone Phones { get; set; }
    }

    public class PagarmeCliente_Endereco
    {
        public string line_1 { get; set; }
        public string line_2 { get; set; }
        public string zip_code { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }

    public class PagarmeCliente_Telefone
    {
        public PagarmeCliente_Telefone_Home home_phone { get; set; }
        public PagarmeCliente_Telefone_Mobile mobile_phone { get; set; }
    }

    public class PagarmeCliente_Telefone_Home
    {
        public string country_code { get; set; }
        public string area_code { get; set; }
        public string number { get; set; }
    }

    public class PagarmeCliente_Telefone_Mobile
    {
        public string country_code { get; set; }
        public string area_code { get; set; }
        public string number { get; set; }
    }
}