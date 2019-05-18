using FurCoNZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class CheckoutViewModel
    {
        public int OrderId { get; set; }

        public OrderViewModel Order { get; set; }

        public decimal AmnountDue => Order.AmountTotal;

        public ICollection<PaymentProviderViewmodel> PaymentProviders { get; set; }

        public string SelectedPaymentProvider { get; set; }
    }
}
