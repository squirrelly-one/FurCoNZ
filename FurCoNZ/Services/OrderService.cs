using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.DAL;
using FurCoNZ.Models;
using Microsoft.EntityFrameworkCore;

namespace FurCoNZ.Services
{
    public class OrderService : IOrderService
    {
        private readonly FurCoNZDbContext _db;

        public OrderService(FurCoNZDbContext db)
        {
            _db = db;
        }

        public Task<Order> CreatePendingOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync(bool IncludeUnavailableTickets = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = _db.TicketTypes.OrderByDescending(tt => tt.PriceCents);

            if (!IncludeUnavailableTickets)
            {
                query.Where(tt => tt.TotalAvailable > 0);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public Task<DateTimeOffset> ReserveTicketsForPurchaseAsync(IDictionary<int, int> ticketsToReserveById, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Order>> GetUserOrders(User user, CancellationToken cancellationToken = default)
        {
            // TODO: Should we support pagination?
            return await _db.Orders.Where(o => o.OrderedBy == user).ToListAsync(cancellationToken);
        }
    }
}
