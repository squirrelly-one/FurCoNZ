using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class StripeChargeViewModel
    {
        public string CheckoutSessionId { get; set; }
        public OrderViewModel Order { get; set; }

        public int FeeCents { get; set; }

        public decimal Fee => (decimal)FeeCents / 100;

        public int TotalCents { get; set; }

        public decimal Total => (decimal)TotalCents / 100;
    }
}
