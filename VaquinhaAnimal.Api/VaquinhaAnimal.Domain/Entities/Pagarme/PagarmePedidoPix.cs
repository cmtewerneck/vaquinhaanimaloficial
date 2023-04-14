using System.Collections.Generic;

namespace VaquinhaAnimal.Domain.Entities.Pagarme
{
    public class PagarmePedidoPix
    {
        public List<PagarmePedidoItensPix> items { get; set; }
        public string customer_id { get; set; }
        public List<PagarmePedidoPagamentosPix> payments { get; set; }
    }

    public class PagarmePedidoItensPix
    {
        public double amount { get; set; }
        //public int amount { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public string code { get; set; }
    }

    public class PagarmePedidoPagamentosPix
    {
        public string payment_method { get; set; }
        public PagarmePedidoPixJson pix { get; set; }
        public List<PagarmePedidoPixSplit> split { get; set; }
    }

    public class PagarmePedidoPixSplit
    {
        public int amount { get; set; }
        public string recipient_id { get; set; }
        public string type { get; set; }
        public PagarmeSplitPixOptions options { get; set; }
    }

    public class PagarmeSplitPixOptions
    {
        public bool charge_processing_fee { get; set; }
        public bool charge_remainder_fee { get; set; }
        public bool liable { get; set; }
    }

    public class PagarmePedidoPixJson
    {
        public int expires_in { get; set; }
        public List<PagarmePedidoPixAdditional> additional_information { get; set; }
    }

    public class PagarmePedidoPixAdditional
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}