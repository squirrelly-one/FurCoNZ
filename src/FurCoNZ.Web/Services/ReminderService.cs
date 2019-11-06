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

        public ReminderService(
            FurCoNZDbContext dbContext,
            IEmailService emailService,
            ILogger<ReminderService> logger,
            IOptions<ReminderServiceOptions> options
        )
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task NotifyOfPendingOrderAsync(CancellationToken cancellationToken = default)
        {
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            // TODO: Support multiple reminders before expiry
            var unpaidOrdersDueForReminder = await _dbContext.Orders
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.TicketsPurchased.Min(t => t.TicketType.SoldOutAt) >= currentRun && // Order has not yet expired
                    EF.Functions.DateDiffDay(currentRun, o.LastReminderSent.GetValueOrDefault(o.CreatedAt)) >= _options.RemindUserOfUnpaidOrderEveryXDays // Last notification was >= 14 days ago OR no notification has been sent AND >= 14 days since the order was created
                ).ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var order in unpaidOrdersDueForReminder)
            {
                await _emailService.SendPendingOrderNotificationAsync(order, cancellationToken);

                // Update last run time so we don't resend messages
                order.LastReminderSent = currentRun;
            }

            // Save last run times
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task NotifyOfCancelledOrderAsync(CancellationToken cancellationToken = default)
        {
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            var expiredOrdersSinceLastSend = await _dbContext.Orders
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.TicketsPurchased.Min(t => t.TicketType.SoldOutAt) <= currentRun && // Order has expired
                    o.ExpiredNotificationSent != null // Reminder has not been sent
                )
                .ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var order in expiredOrdersSinceLastSend)
            {
                await _emailService.SendOrderCancelledNotificationAsync(order, cancellationToken);

                // Update last run time so we don't resend messages
                order.LastReminderSent = currentRun;
            }

            // Save last run times
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
