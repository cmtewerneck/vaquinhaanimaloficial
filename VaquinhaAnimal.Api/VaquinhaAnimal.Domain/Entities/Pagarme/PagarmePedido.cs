using System.Collections.Generic;

namespace VaquinhaAnimal.Domain.Entities.Pagarme
{
    public class PagarmePedido
    {
        public List<PagarmePedidoItens> items { get; set; }
        public string customer_id { get; set; }
        public List<PagarmePedidoPagamentos> payments { get; set; }
    }

    public class PagarmePedidoItens
    {
        public double amount { get; set; }
        //public int amount { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public string code { get; set; }
    }

    public class PagarmePedidoPagamentos
    {
        public string payment_method { get; set; }
        public PagarmePedidoCartaoCredito credit_card { get; set; }
        public List<PagarmePedidoSplit> split { get; set; }
    }

    public class PagarmePedidoCartaoCredito
    {
        public bool recurrence { get; set; }
        public int installments { get; set; }
        public string statement_descriptor { get; set; }
        public string card_id { get; set; }
        public PagarmePedidoCartaoCreditoUsado card { get; set; }
    }

    public class PagarmePedidoSplit
    {
        public int amount { get; set; }
        public string recipient_id { get; set; }
        public string type { get; set; }
        public PagarmeSplitOptions options { get; set; }
    }

    public class PagarmeSplitOptions
    {
        public bool charge_processing_fee { get; set; }
        public bool charge_remainder_fee { get; set; }
        public bool liable { get; set; }
    }

    public class PagarmePedidoCartaoCreditoUsado
    {
        public string cvv { get; set; }
    }
}