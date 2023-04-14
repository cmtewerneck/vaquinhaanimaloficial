using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VaquinhaAnimal.Domain.Entities
{
    public class RedeSocial : BaseEntity
    {
        [JsonPropertyName("tipo")]
        [Required]
        public TipoRedeSocialEnum Tipo { get; set; } 

        [JsonPropertyName("url")]
        [Required]
        [MaxLength(200)]
        public string Url { get; set; } // (200)

        [JsonPropertyName("campanha_id")]
        [Required]
        public Guid Campanha_Id { get; set; }
        public Campanha Campanha { get; set; } 

    }
}
