using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Options;
using FurCoNZ.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FurCoNZ.Web.Services
{
    public class ReminderService : IReminderService
    {
        private readonly FurCoNZDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReminderService> _logger;
        private readonly ReminderServiceOptions _options;
        private readonly IViewRenderService _viewRenderService;

        public ReminderService(
            FurCoNZDbContext dbContext,
            IEmailService emailService,
            ILogger<ReminderService> logger,
            IOptions<ReminderServiceOptions> options,
            IViewRenderService viewRenderService
        )
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
            _options = options.Value;
            _viewRenderService = viewRenderService;
        }

        public async Task Send30DayRemainingPendingOrdersAsync(CancellationToken cancellationToken = default)
        {
            // Get the last time the reminders were sent
            var lastSend = await _dbContext.RemindersLastRuns.FindAsync(new[] { "SendUnpaidReminderForOrders" }, cancellationToken);
            if (lastSend == null)
            {
                lastSend = new Models.RemindersLastRun
                {
                    ReminderService = "SendUnpaidReminderForOrders",
                    LastRun = DateTimeOffset.MinValue,
                };
                _dbContext.RemindersLastRuns.Add(lastSend);
            }
            var lastRun = lastSend.LastRun;
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            // TODO: Support multiple reminders before expiry
            var expiredOrdersSinceLastSend = await _dbContext.Orders.AsNoTracking()
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.CreatedAt.AddDays(_options.UnpaidOrderReminderDays.First()) <= currentRun && // Order has expired
                    o.CreatedAt.AddDays(_options.UnpaidOrderReminderDays.First()) >= lastRun // Reminder has not been sent
                )
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.OrderedBy.Name,
                    o.OrderedBy.Email
                })
                .ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var order in expiredOrdersSinceLastSend)
            {
                // Gather metadata
                var toAddresses = new MailAddressCollection
                {
                    new MailAddress (order.Email, order.Name),
                };
                var subject = $"Order #{order.Id} has expired";

                // Prepare template
                var model = new OrderExpiredNotificationViewModel();
                var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/ThirtyDayReminder", model, cancellationToken: cancellationToken);

                // Send message
                await _emailService.SendEmailAsync(toAddresses, subject, message, cancellationToken: cancellationToken);
            }

            // Update last run time so we don't resend messages
            // TODO: Track individual reminders against orders so we don't resend all if one message in a batch is faulty
            lastSend.LastRun = currentRun;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SendCancelledOrdersAsync(CancellationToken cancellationToken = default)
        {
            // Get the last time the reminders were sent
            var lastSend = await _dbContext.RemindersLastRuns.FindAsync(new[] { "SendCancelledOrders" }, cancellationToken);
            if (lastSend == null)
            {
                lastSend = new Models.RemindersLastRun
                {
                    ReminderService = "SendCancelledOrders",
                    LastRun = DateTimeOffset.MinValue,
                };
                _dbContext.RemindersLastRuns.Add(lastSend);
            }
            var lastRun = lastSend.LastRun;
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            var expiredOrdersSinceLastSend = await _dbContext.Orders.AsNoTracking()
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.CreatedAt.AddDays(_options.UnpaidOrderExpiryDays) <= currentRun && // Order has expired
                    o.CreatedAt.AddDays(_options.UnpaidOrderExpiryDays) >= lastRun // Reminder has not been sent
                )
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.OrderedBy.Name,
                    o.OrderedBy.Email
                })
                .ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var order in expiredOrdersSinceLastSend)
            {
                // Gather metadata
                var toAddresses = new MailAddressCollection
                {
                    new MailAddress (order.Email, order.Name),
                };
                var subject = $"Order #{order.Id} has expired";

                // Prepare template
                var model = new OrderExpiredNotificationViewModel();
                var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/ExpiredOrder", model, cancellationToken: cancellationToken);

                // Send message
                await _emailService.SendEmailAsync(toAddresses, subject, message, cancellationToken: cancellationToken);
            }

            // Update last run time so we don't resend messages
            // TODO: Track individual reminders against orders so we don't resend all if one message in a batch is faulty
            lastSend.LastRun = currentRun;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
