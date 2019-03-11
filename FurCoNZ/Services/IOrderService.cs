using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Models;

namespace FurCoNZ.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<TicketType>> GetTicketTypes(bool IncludeUnavailableTickets = true, CancellationToken cancellationToken = default);
        Task<DateTimeOffset> ReserveTicketsForPurchase(IDictionary<int, int> ticketsToReserveById, CancellationToken cancellationToken = default);
        Task<Order> CreatePendingOrder(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default);
    }
}
