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
        Task<Order> CreatePendingOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default);

        Task<IEnumerable<Order>> GetUserOrders(User user, CancellationToken cancellationToken = default);
    }
}
