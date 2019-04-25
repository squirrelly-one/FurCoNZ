using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Services.Payment
{
    public class StripePaymentProvider : IPaymentProvider
    {
        public string Name => "Stripe";

        public string SupportedMethods => "Credit Card";

        public string Description => "Online payment processor for credit cards.";
    }
}
