using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FurCoNZ.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly FurCoNZDbContext _db;

        public OrderService(FurCoNZDbContext db)
        {
            _db = db;
        }

        public async Task<Order> CreateOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default)
        {
            if (purchasingAccount == null)
                throw new ArgumentNullException(nameof(purchasingAccount), "Must supply a user when creating an order");
            if (ticketsInBasket == null || !ticketsInBasket.Any())
                throw new ArgumentNullException(nameof(ticketsInBasket), "No tickets are being purchased in the creation of this order");
            if (ticketsInBasket.Any(t => t.Id != default))
                throw new InvalidOperationException("Some of the tickets in this order already exist, and have been assigned an id.");

            // Setup
            var ticketList = ticketsInBasket.ToList();
            var ticketTypeIdsInOrder = ticketList.Select(t => t.TicketTypeId).Distinct();
            var ticketTypesInOrder = _db.TicketTypes.Where(tt => ticketTypeIdsInOrder.Contains(tt.Id));

            // Check tickets are still available
            foreach (var ticketType in ticketTypesInOrder)
            {
                var ticketsOfTypeAvailable = ticketType.TotalAvailable;
                var ticketsOfTypeOrdered = ticketList.Count(t => t.TicketTypeId == ticketType.Id);

                if (ticketsOfTypeOrdered > ticketsOfTypeAvailable)
                {
                    throw new InvalidOperationException($"There are not enough {ticketType.Name} tickets available for this order to be created.");
                }

                // Remove the appropriate number of tickets from the available pool
                ticketType.TotalAvailable -= ticketsOfTypeOrdered;
            }

            // Set up tickets for tracking
            _db.Tickets.AddRange(ticketList);

            // Set up order for tracking
            var order = new Order
            {
                OrderedById = purchasingAccount.Id,
                TicketsPurchased = ticketList,
            };
            _db.Orders.Add(order);

            // Commit to DB
            await _db.SaveChangesAsync();

            return order;
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

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(User user, CancellationToken cancellationToken = default)
        {
            // TODO: Should we support pagination?
            return await _db.Orders
                .Include(o => o.TicketsPurchased)
                .ThenInclude(t => t.TicketType)
                .Where(o => o.OrderedBy == user)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order> GetUserOrderAsync(User user, int orderId, CancellationToken cancellationToken = default)
        {
            return await _db.Orders
                .Include(o => o.TicketsPurchased)
                .ThenInclude(t => t.TicketType)
                .SingleOrDefaultAsync(o => o.OrderedById == user.Id && o.Id == orderId, cancellationToken);
        }

        public async Task<Order> GetUserPendingOrderAsync(User user, CancellationToken cancellationToken = default)
        {
            return await _db.Orders
                .Include(o => o.TicketsPurchased)
                .ThenInclude(t => t.TicketType)
                .SingleOrDefaultAsync(o => o.OrderedById == user.Id && o.AmountPaidCents == 0, cancellationToken);
        }

        public async Task AddReceivedFundsForOrderAsync(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default)
        {
            var order = await _db.Orders
                .Include(o => o.Audits)
                .SingleAsync(o => o.Id == orderId, cancellationToken);

            if (order.Audits.Any(a => a.PaymentProvider == paymentProvider && a.PaymentProviderReference == paymentReference && a.Type == AuditType.Received))
                throw new InvalidOperationException($"Received funds for order {orderId} has already been applied for {paymentProvider}: {paymentReference}");

            var audit = new OrderAudit
            {
                OrderId = orderId,
                PaymentProvider = paymentProvider,
                PaymentProviderReference = paymentReference,
                Type = AuditType.Received,
                When = when,
                AmountCents = amountCents,
            };

            // Recalculate amount paid
            order.AmountPaidCents = order.Audits.Sum(a => a.AmountCents) + audit.AmountCents;

            await _db.OrderAudits.AddAsync(audit, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RefundFundsForOrderAsync(int orderId, int amountCents, string paymentProvider, string paymentReference, DateTimeOffset when, CancellationToken cancellationToken = default)
        {
            var order = await _db.Orders
                .Include(o => o.Audits)
                .SingleAsync(o => o.Id == orderId, cancellationToken);

            if(order.Audits.Any(a => a.PaymentProvider == paymentProvider && a.PaymentProviderReference == paymentReference && a.Type == AuditType.Refunded))
                throw new InvalidOperationException($"Refund for order {orderId} has already been applied for {paymentProvider}: {paymentReference}");
            

            var audit = new OrderAudit
            {
                OrderId = orderId,
                PaymentProvider = paymentProvider,
                PaymentProviderReference = paymentReference,
                Type = AuditType.Refunded,
                When = when,
                AmountCents = -amountCents,
            };

            // Recalculate amount paid
            order.AmountPaidCents = order.Audits.Sum(a => a.AmountCents) + audit.AmountCents;

            _db.OrderAudits.Add(audit);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
