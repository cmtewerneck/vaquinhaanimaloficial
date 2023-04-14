using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VaquinhaAnimal.Domain.Entities.Base
{
    public class ApplicationUser : IdentityUser
    {
        // Obrigatório --> 64 caracteres
        [JsonPropertyName("name")]
        public string Name { get; set; }

        // Obrigatório --> 64 caracteres
        [JsonPropertyName("email")]
        [MaxLength(64)]
        public override string Email { get => base.Email; set => base.Email = value; }

        // Opcional --> deixar em branco
        [JsonPropertyName("code")]
        public string Code { get; set; }

        // Obrigatório --> 16 caracteres
        [JsonPropertyName("document")]
        public string Document { get; set; }

        // Obrigatório --> Individual para PF / Company para PJ
        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Obrigatório --> CPF / CNPJ / PASSPORT
        [JsonPropertyName("document_type")]
        public string Document_Type { get; set; }

        // Obrigatório --> Male ou Female
        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        // Obrigatório --> Menor que a atual
        [JsonPropertyName("birthdate")]
        public DateTime Birthdate { get; set; }

        // Obrigatório --> 150 caracteres
        [JsonPropertyName("line_1")]
        public string Line_1 { get; set; }

        // Opcional --> 150 caracteres
        [JsonPropertyName("line_2")]
        public string Line_2 { get; set; }

        // Obrigatório --> 8 caracteres (sem traço)
        [JsonPropertyName("zip_code")]
        public string Zip_Code { get; set; }

        // Obrigatório --> 50 caracteres
        [JsonPropertyName("city")]
        public string City { get; set; }

        // Obrigatório --> ISO - somente 2 caracteres: RJ, SP....
        [JsonPropertyName("state")]
        public string State { get; set; }

        // Obrigatório --> ISO - até 5 caracteres: BR, USA...
        [JsonPropertyName("country")]
        public string Country { get; set; }

        // HOME PHONE
        // Opcional --> ISO - até 5 caracteres: 55, 301...
        [JsonPropertyName("home_phone_country_code")]
        public string Home_phone_country_code { get; set; }

        // Opcional --> ISO - até 5 caracteres: 21, 31...
        [JsonPropertyName("home_phone_area_code")]
        public string Home_phone_area_code { get; set; }

        // Opcional --> até 15 caracteres
        [JsonPropertyName("home_phone_number")]
        public string Home_phone_number { get; set; }

        // MOBILE PHONE
        // Obrigatório --> ISO - até 5 caracteres: 55, 301...
        [JsonPropertyName("mobile_phone_country_code")]
        public string Mobile_phone_country_code { get; set; }

        // Obrigatório --> ISO - até 5 caracteres: 21, 31...
        [JsonPropertyName("mobile_phone_area_code")]
        public string Mobile_phone_area_code { get; set; }

        // Obrigatório --> até 15 caracteres
        [JsonPropertyName("mobile_phone_number")]
        public string Mobile_phone_number { get; set; }

        // Opcional
        [JsonPropertyName("codigo_pagarme")]
        public string Codigo_Pagarme { get; set; }

        // Opcional
        [JsonPropertyName("foto")]
        public string Foto { get; set; }
    }
}
