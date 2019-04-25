using FurCoNZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class CheckoutViewModel
    {
        public OrderViewModel Order { get; set; }

        public decimal AmnountDue => Order.TotalAmount;

        public ICollection<PaymentProviderViewmodel> PaymentProviders { get; set; }
    }
}
