using System;

using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.ViewModels
{
    public class OrderAuditViewModel
    {

        public OrderAuditViewModel(OrderAudit audit)
        {
            Id = audit.Id;
            Type = audit.Type.ToString();
            AmountCents = audit.AmountCents;
            When = audit.When;
            PaymentProvider = audit.PaymentProvider;
            PaymentProviderReference = audit.PaymentProviderReference;
            OrderId = audit.OrderId;
        }
        public int Id { get; set; }

        public string Type { get; set; }

        public int AmountCents { get; set; }

        public decimal Amount => (decimal)AmountCents / 100;

        public DateTimeOffset When { get; set; }

        public string PaymentProvider { get; set; }

        public string PaymentProviderReference { get; set; }

        public int OrderId { get; set; }
    }
}