using System.Collections.Generic;

namespace VaquinhaAnimal.Domain.Entities.Pagarme
{
    public class PagarmePedidoRecorrencia
    {
        public string payment_method { get; set; }
        public string currency { get; set; }
        public string interval { get; set; }
        public int interval_count { get; set; }
        public string billing_type { get; set; }
        public int billing_day { get; set; }
        public int installments { get; set; }
        public string card_id { get; set; }
        public string customer_id { get; set; }
        public List<RecorrenciaItem> items { get; set; }
    }

    public class RecorrenciaItem
    {
        public string description { get; set; }
        public int quantity { get; set; }
        public RecorrenciaScheme pricing_scheme { get; set; }
    }

    public class RecorrenciaScheme
    {
        public int price { get; set; }
    }
}