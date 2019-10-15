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

using FurCoNZ.Web.Configuration;
using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.Services.Payment
{
    public class StripeService : IPaymentProvider
    {
        public const string NAME = "Stripe";

        private readonly FurCoNZDbContext _dbContext;
        private readonly IOptions<StripeSettings> _options;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly SessionService _checkoutService;
        private readonly ChargeService _chargeService;
        private readonly RefundService _refundService;

        public string Name => NAME;

        public string SupportedMethods => "Credit Card";

        public string Description => "Online payment processor for credit cards.";

        public StripeService(FurCoNZDbContext dbContext, IOptions<StripeSettings> options, IOrderService orderService, ILogger<StripeService> logger)
        {
            _dbContext = dbContext;
            _options = options;
            _orderService = orderService;
            _logger = logger;
            _paymentIntentService = new PaymentIntentService();

            _checkoutService = new SessionService();
            _chargeService = new ChargeService();
            _refundService = new RefundService();
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

                await _orderService.AddReceivedFundsForOrderAsync(
                    stripeSession.OrderId, amountReceived,
                    NAME, session.PaymentIntentId,
                    session.PaymentIntent.Created.Value,
                    cancellationToken);

                stripeSession.State = StripeSessionState.Completed;
                // TODO: Record audit log of payment received.
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task ProcessChargeAsync(Charge charge, CancellationToken cancellationToken)
        {
            var session = await _dbContext.StripeSessions
                .FirstOrDefaultAsync(ss => ss.PaymentIntent == charge.PaymentIntentId, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (session == null)
            {
                _logger.LogCritical($"Could not find stripe session to process refund for payment intent ({charge.PaymentIntentId})");
                return;
            }

            if (charge.Refunds.Any())
            {

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

                    await _orderService.RefundFundsForOrderAsync(
                        session.OrderId, amountReceived,
                        NAME, charge.PaymentIntentId,
                        charge.Created,
                        cancellationToken);

                    session.State = StripeSessionState.Refunded;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

        }

        public async Task<bool> RefundAsync(int orderId, string paymentReference, CancellationToken cancellationToken = default)
        {
            var session = await _dbContext.StripeSessions
                    .FirstOrDefaultAsync(ss => ss.PaymentIntent == paymentReference, cancellationToken);

            if (session == null)
            {
                _logger.LogCritical($"Could not find stripe session to process refund for payment intent ({paymentReference})");
                return false;
            }

            var paymentIntent = await _paymentIntentService.GetAsync(session.PaymentIntent, cancellationToken: cancellationToken);

            var charge = paymentIntent.Charges.SingleOrDefault(c => c.Paid);
            if (charge == null)
            {
                _logger.LogCritical($"Could not find identify a paid charge for payment intent ({paymentReference})");
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var refund = await _refundService.CreateAsync(new RefundCreateOptions
            {
                Charge = charge.Id,
                Amount = charge.Amount,
                Reason = RefundReasons.RequestedByCustomer,
            }, cancellationToken: cancellationToken);

            // The database will be updated with the refund occurs through the poll/web-hook handler
            session.State = StripeSessionState.Refunding;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
