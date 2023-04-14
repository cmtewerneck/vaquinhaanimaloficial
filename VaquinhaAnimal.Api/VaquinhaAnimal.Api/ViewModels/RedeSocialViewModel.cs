using System;

namespace VaquinhaAnimal.Api.ViewModels
{
    public class RedeSocialViewModel
    {
        public Guid id { get; set; }
        public int tipo { get; set; }
        public string url { get; set; } 
        public Guid campanha_id { get; set; } 
    }
}
