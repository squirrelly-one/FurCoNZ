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
        private readonly IOrderService _orderService;

        public string Name => NAME;

        public string SupportedMethods => "Direct Bank Transfer";

        public string Description => "Transactions may take up to 3 business days";

        public BankPaymentProvider(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<bool> RefundAsync(int orderId, string paymentReference, CancellationToken cancellationToken = default)
        {
            var order = await _orderService.GetOrderById(orderId, cancellationToken);
            if (order == null)
            {
                return false;
            }

            // Calculate the amount paid for the order.
            var paid = order.Audits
                .Where(a => a.PaymentProvider == NAME && a.PaymentProviderReference == paymentReference)
                .Select(a => a.AmountCents)
                .Sum();

            if (paid <= 0)
            {
                return true;
            }

            await _orderService.RefundFundsForOrderAsync(
                        order.Id, paid,
                        NAME, paymentReference, DateTimeOffset.Now,
                        cancellationToken);

            return true;
        }
    }
}
