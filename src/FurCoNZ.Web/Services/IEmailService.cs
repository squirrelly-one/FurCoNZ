using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default);
        Task SendPaymentReceivedAsync(Order order, CancellationToken cancellationToken = default);
        Task SendPaymentRefundedAsync(Order order, CancellationToken cancellationToken = default);
        Task SendPendingOrderNotificationAsync(Order order, CancellationToken cancellationToken = default);
        Task SendOrderCancelledNotificationAsync(Order order, CancellationToken cancellationToken = default);
        Task SendOrderPaidAsync(Order order, CancellationToken cancellationToken = default);
    }
}
