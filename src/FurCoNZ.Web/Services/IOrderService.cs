using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.Services
{
    public interface IOrderService
    {
        Task AddReceivedFundsForOrderAsync(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default);
        Task<Order> CreateOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default);
        Task<IEnumerable<TicketType>> GetTicketTypesAsync(bool IncludeUnavailableTickets = true, CancellationToken cancellationToken = default);
        Task<Order> GetUserOrderAsync(User user, int orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetUserOrdersAsync(User user, CancellationToken cancellationToken = default);
        Task<Order> GetUserPendingOrderAsync(User user, CancellationToken cancellationToken = default);
        Task RefundFundsForOrderAsync(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default);
        Task<DateTimeOffset> ReserveTicketsForPurchaseAsync(IDictionary<int, int> ticketsToReserveById, CancellationToken cancellationToken = default);
        Task<Order> GetOrderByRef(int orderRef, CancellationToken cancellationToken = default);
        Task<Order> GetOrderById(int orderId, CancellationToken cancellationToken = default);
    }
}
