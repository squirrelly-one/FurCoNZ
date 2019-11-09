using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.ViewModels
{
    public class BankTransferViewModel
    {
        public OrderViewModel Order { get; set; }

        public string PaymentReference { get; set; }

        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }

        public int TotalCents { get; set; }

        public decimal Total => (decimal)TotalCents / 100;
    }
}
