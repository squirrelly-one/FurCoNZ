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

        public Task<Order> CreateOrderAsync(User purchasingAccount, IEnumerable<Ticket> ticketsInBasket, CancellationToken cancellationToken = default)
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
