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
        public PagarmeClienteEndereco Address { get; set; }

        [JsonPropertyName("phones")]
        public PagarmeClienteTelefone Phones { get; set; }

    }
}