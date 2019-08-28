using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services.Payment
{
    public class BankPaymentProvider : IPaymentProvider
    {
        public const string NAME = "BankTransfer";

        public string Name => NAME;

        public string SupportedMethods => "Direct Bank Transfer";

        public string Description => "Transactions may take up to 3 business days";

        public async Task<bool> RefundAsync(string paymentReference, CancellationToken cancellationToken = default)
        {
            return true;
        }
    }
}
