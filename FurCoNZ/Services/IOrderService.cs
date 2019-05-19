using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Models;

namespace FurCoNZ.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<TicketType>> GetTicketTypesAsync(bool IncludeUnavailableTickets = true, CancellationToken cancellationToken = default);
        Task<DateTimeOffset> ReserveTicketsForPurchaseAsync(IDictionary<int, int> ticketsToReserveById, CancellationToken cancellationToken = default);
        Task<Order> CreateOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default);


        Task<IEnumerable<Order>> GetUserOrders(User user, CancellationToken cancellationToken = default);

        Task<Order> GetUserOrderAsync(User user, int orderId, CancellationToken cancellationToken = default);

        Task<Order> GetUserPendingOrderAsync(User user, CancellationToken cancellationToken = default);

        Task AddReceivedFundsForOrder(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default);

        Task RefundFundsForOrder(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default);
    }
}
