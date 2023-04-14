using System;

namespace VaquinhaAnimal.Api.ViewModels
{
    public class ContaDepositoViewModel
    {
        public Guid id { get; set; }
        public string banco { get; set; } 
        public int tipo_conta { get; set; } 
        public string agencia { get; set; } 
        public string agencia_digito { get; set; } 
        public string conta { get; set; } 
        public string conta_digito { get; set; } 
        public int tipo_pessoa { get; set; } 
        public string documento { get; set; } 
        public Guid campanha_id { get; set; } 
    }
}
