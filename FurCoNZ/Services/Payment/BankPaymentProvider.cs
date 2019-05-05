using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Services.Payment
{
    public class BankPaymentProvider : IPaymentProvider
    {
        public string Name => "BankTransfer";

        public string SupportedMethods => "Direct Bank Transfer";

        public string Description => "Transactions may take up to 3 business days";
    }
}
