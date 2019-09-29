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

        public async Task NotifyOfPendingOrderAsync(CancellationToken cancellationToken = default)
        {
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            // TODO: Support multiple reminders before expiry
            var unpaidOrdersDueForReminder = await _dbContext.Orders.AsNoTracking()
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.TicketsPurchased.Min(t => t.TicketType.SoldOutAt) >= currentRun && // Order has not yet expired
                    EF.Functions.DateDiffDay(currentRun, o.LastReminderSent.GetValueOrDefault(o.CreatedAt)) >= _options.RemindUserOfUnpaidOrderEveryXDays // Last notification was >= 14 days ago OR no notification has been sent AND >= 14 days since the order was created
                )
                .Select(o => new UnpaidReminderViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Name = o.OrderedBy.Name,
                    Email = o.OrderedBy.Email,
                    ExpiresAt = o.TicketsPurchased.Min(t => t.TicketType.SoldOutAt),
                })
                .ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var viewModel in unpaidOrdersDueForReminder)
            {
                // Gather metadata
                var toAddresses = new MailAddressCollection
                {
                    new MailAddress (viewModel.Email, viewModel.Name),
                };
                var subject = $"Order #{viewModel.Id} is still pending payment";

                // Prepare template
                var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/UnpaidReminder", viewModel, cancellationToken: cancellationToken);

                // Send message
                await _emailService.SendEmailAsync(toAddresses, subject, message, cancellationToken: cancellationToken);

                // Update last run time so we don't resend messages
                (await _dbContext.Orders.FirstAsync(o => o.Id == viewModel.Id, cancellationToken)).LastReminderSent = currentRun;
            }

            // Save last run times
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task NotifyOfCancelledOrderAsync(CancellationToken cancellationToken = default)
        {
            var currentRun = DateTimeOffset.Now;

            // Get the orders that we will send reminders for
            var expiredOrdersSinceLastSend = await _dbContext.Orders.AsNoTracking()
                .Where(o =>
                    !o.Audits.Any() && // No payments have been made
                    o.TicketsPurchased.Min(t => t.TicketType.SoldOutAt) <= currentRun && // Order has expired
                    o.ExpiredNotificationSent != null // Reminder has not been sent
                )
                .Select(o => new OrderExpiredNotificationViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Name = o.OrderedBy.Name,
                    Email = o.OrderedBy.Email,
                })
                .ToListAsync(cancellationToken);

            // Send emails for each order
            foreach (var viewModel in expiredOrdersSinceLastSend)
            {
                // Gather metadata
                var toAddresses = new MailAddressCollection
                {
                    new MailAddress (viewModel.Email, viewModel.Name),
                };
                var subject = $"Order #{viewModel.Id} has expired";

                // Prepare template
                var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/ExpiredOrder", viewModel, cancellationToken: cancellationToken);

                // Send message
                await _emailService.SendEmailAsync(toAddresses, subject, message, cancellationToken: cancellationToken);

                // Update last run time so we don't resend messages
                (await _dbContext.Orders.FirstAsync(o => o.Id == viewModel.Id, cancellationToken)).LastReminderSent = currentRun;
            }

            // Save last run times
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
