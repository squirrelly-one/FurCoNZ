using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

using FurCoNZ.Configuration;
using FurCoNZ.DAL;
using FurCoNZ.Models;

namespace FurCoNZ.Services.Payment
{
    public class StripeService : IPaymentProvider
    {
        private readonly FurCoNZDbContext _dbContext;
        private readonly IOptions<StripeSettings> _options;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly PaymentIntentService _paymentIntentService;

        public string Name => "Stripe";

        public string SupportedMethods => "Credit Card";

        public string Description => "Online payment processor for credit cards.";

        public StripeService(FurCoNZDbContext dbContext, IOptions<StripeSettings> options, IOrderService orderService, ILogger<StripeService> logger)
        {
            _dbContext = dbContext;
            _options = options;
            _orderService = orderService;
            _logger = logger;
            _paymentIntentService = new PaymentIntentService();
        }

        public async Task AddStripeSessionToOrderAsync(int orderId, string sessionId, CancellationToken cancellationToken = default)
        {
            var stripeSession = new StripeSession
            {
                Id = sessionId,
                State = StripeSessionState.None,
                OrderId = orderId,
            };

            _dbContext.StripeSessions.Add(stripeSession);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task FufillOrderFromCompletedSessionAsync(Session session, CancellationToken cancellationToken = default)
        {
            var stripeSession = await _dbContext.StripeSessions
                .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

            if (stripeSession == null)
            {
                _logger.LogError($"session does not exist. Has data been lost? ({session.Id})");
                return;
            }


            if (string.IsNullOrEmpty(session.PaymentIntentId))
            {
                _logger.LogCritical($"No Payment Intent associated with Checkout Session ({session.Id})");
                return;
            }

            if (session.PaymentIntent == null)
                session.PaymentIntent = await _paymentIntentService.GetAsync(session.PaymentIntentId, cancellationToken: cancellationToken);

            // Record the patyment intent for future refunds.
            stripeSession.PaymentIntent = session.PaymentIntentId;

            if ((stripeSession.State == StripeSessionState.None || stripeSession.State == StripeSessionState.Processing) &&
                session.PaymentIntent.Status == "succeeded")
            {
                var amountReceived = (int)session.PaymentIntent.AmountReceived.Value;
                // Assume the fee was included in the price during checkout when IncludeFee is enabled.
                if (_options.Value.IncludeFee)
                    amountReceived = amountReceived - (amountReceived * 29 / 1000) - 30;

                await _orderService.AddReceivedFundsForOrder(
                    stripeSession.OrderId, amountReceived,
                    "stripe", session.PaymentIntentId,
                    session.PaymentIntent.Created.Value,
                    cancellationToken);

                stripeSession.State = StripeSessionState.Completed;
                // TODO: Record audit log of payment received.
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task ProcessChargeAsync(Charge charge, CancellationToken cancellationToken)
        {
            // For now, only process refunds.
            if (charge.Refunds.Any())
            {
                var session = await _dbContext.StripeSessions
                    .FirstOrDefaultAsync(ss => ss.PaymentIntent == charge.PaymentIntentId, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (session == null)
                {
                    _logger.LogCritical($"Could not find stripe session to process refund for payment intent ({charge.PaymentIntentId})");
                    return;
                }

                // If partial refund, mark as refund in process 
                if (!charge.Refunded)
                {
                    session.State = StripeSessionState.Refunding;
                }
                else if (session.State != StripeSessionState.Refunded)
                {
                    var amountReceived = (int)charge.Refunds.Sum(r => r.Amount);
                    // Assume the fee was included in the price during checkout when IncludeFee is enabled.
                    if (_options.Value.IncludeFee)
                        amountReceived = amountReceived - (amountReceived * 29 / 1000) - 30;

                    await _orderService.RefundFundsForOrder(
                        session.OrderId, amountReceived,
                        "stripe", charge.PaymentIntentId,
                        charge.Created,
                        cancellationToken);

                    session.State = StripeSessionState.Refunded;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
