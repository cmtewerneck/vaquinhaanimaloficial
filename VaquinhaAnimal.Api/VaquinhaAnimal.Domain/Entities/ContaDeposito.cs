using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VaquinhaAnimal.Domain.Entities
{
    public class ContaDeposito : BaseEntity
    {
        [JsonPropertyName("banco")]
        [Required]
        public string Banco { get; set; } 

        [JsonPropertyName("tipo_conta")]
        [Required]
        public TipoContaDepositoEnum TipoConta { get; set; } 

        [JsonPropertyName("agencia")]
        [Required]
        [MaxLength(10)]
        public string Agencia { get; set; } 

        [JsonPropertyName("agencia_digito")]
        [MaxLength(1)]
        public string AgenciaDigito { get; set; }

        [JsonPropertyName("conta")]
        [Required]
        [MaxLength(13)]
        public string Conta { get; set; } 

        [JsonPropertyName("conta_digito")]
        [Required]
        [MaxLength(2)]
        public string ContaDigito { get; set; }

        [JsonPropertyName("tipo_pessoa")]
        [Required]
        public TipoPessoaEnum TipoPessoa { get; set; }

        [JsonPropertyName("documento")]
        [Required]
        [MaxLength(14)]
        [MinLength(11)]
        public string Documento { get; set; }

        [JsonPropertyName("campanha_id")]
        [Required]
        public Guid Campanha_Id { get; set; } 
        public Campanha Campanha { get; set; }
    }
}
