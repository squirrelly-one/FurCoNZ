using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using FurCoNZ.Web.Models;
using FurCoNZ.Web.ViewModels;

namespace FurCoNZ.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IViewRenderService _viewRenderService;

        public EmailService(IEmailProvider emailProvider, IViewRenderService viewRenderService)
        {
            _emailProvider = emailProvider;
            _viewRenderService = viewRenderService;
        }

        public async Task SendOrderCancelledNotificationAsync(Order order, CancellationToken cancellationToken = default)
        {
            var viewModel = new OrderExpiredNotificationViewModel
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Name = order.OrderedBy.Name,
                Email = order.OrderedBy.Email,
            };

            // Gather metadata
            var toAddresses = new MailAddressCollection
                {
                    new MailAddress (viewModel.Email, viewModel.Name),
                };

            // Prepare template
            var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/ExpiredOrder", viewModel, cancellationToken: cancellationToken);

            var subject = message.ViewData.ContainsKey("Subject")
                ? message.ViewData["Subject"] as string
                : $"Order #{viewModel.Id} has expired";

            // Send message
            await _emailProvider.SendEmailAsync(toAddresses, subject, message.Output, cancellationToken: cancellationToken);
        }

        public async Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
        {
            // Generate email to send to purchasing account
            var toAddresses = new MailAddressCollection
            {
                new MailAddress(order.OrderedBy.Email, order.OrderedBy.Name),
            };

            // Prepare template
            var message = await _viewRenderService.RenderToStringAsync("EmailTemplates/OrderConfirmed", new OrderViewModel(order), cancellationToken: cancellationToken);

            var subject = message.ViewData.ContainsKey("Subject")
                ? message.ViewData["Subject"] as string
                : $"Order #{order.Id} has been confirmed";

            AttachmentCollection attachments = null;
            if (message.ViewData.TryGetValue("Attachments", out var vdAattachments))
                attachments = vdAattachments as AttachmentCollection;

            // Send message
            await _emailProvider.SendEmailAsync(toAddresses, subject, message.Output, attachments: attachments, cancellationToken: cancellationToken);
        }

        public async Task SendPaymentReceivedAsync(Order order, CancellationToken cancellationToken)
        {
            // Generate email to send to purchasing account
            var toAddresses = new MailAddressCollection
            {
                new MailAddress (order.OrderedBy.Email, order.OrderedBy.Name),
            };

            // Prepare template
            var message = await _viewRenderService.RenderToStringAsync("EmailTemplates/PaymentReceived", new OrderViewModel(order), cancellationToken: cancellationToken);

            var subject = message.ViewData.ContainsKey("Subject")
                ? message.ViewData["Subject"] as string
                : $"Payment received for order #{order.Id}";

            AttachmentCollection attachments = null;
            if (message.ViewData.TryGetValue("Attachments", out var vdAattachments))
                attachments = vdAattachments as AttachmentCollection;

            // Send message
            await _emailProvider.SendEmailAsync(toAddresses, subject, message.Output, attachments: attachments, cancellationToken: cancellationToken);
        }

        public async Task SendPaymentRefundedAsync(Order order, CancellationToken cancellationToken)
        {
            // Generate email to send to purchasing account
            var toAddresses = new MailAddressCollection
            {
                new MailAddress (order.OrderedBy.Email, order.OrderedBy.Name),
            };

            // Prepare template
            var message = await _viewRenderService.RenderToStringAsync("EmailTemplates/OrderRefunded", new OrderViewModel(order), cancellationToken: cancellationToken);

            var subject = message.ViewData.ContainsKey("Subject")
                ? message.ViewData["Subject"] as string
                : $"Your order #{order.Id} has been refunded";

            AttachmentCollection attachments = null;
            if (message.ViewData.TryGetValue("Attachments", out var vdAattachments))
                attachments = vdAattachments as AttachmentCollection;

            // Send message
            await _emailProvider.SendEmailAsync(toAddresses, subject, message.Output, attachments: attachments, cancellationToken: cancellationToken);
        }

        public async Task SendPendingOrderNotificationAsync(Order order, CancellationToken cancellationToken)
        {
            var viewModel = new UnpaidReminderViewModel
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Name = order.OrderedBy.Name,
                Email = order.OrderedBy.Email,
                ExpiresAt = order.TicketsPurchased.Min(t => t.TicketType.SoldOutAt),
            };

            // Gather metadata
            var toAddresses = new MailAddressCollection
            {
                new MailAddress (viewModel.Email, viewModel.Name),
            };

            // Prepare template
            var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/UnpaidReminder", viewModel, cancellationToken: cancellationToken);

            var subject = message.ViewData.ContainsKey("Subject")
                ? message.ViewData["Subject"] as string
                : $"Order #{viewModel.Id} is still pending payment";

            AttachmentCollection attachments = null;
            if (message.ViewData.TryGetValue("Attachments", out var vdAattachments))
                attachments = vdAattachments as AttachmentCollection;

            // Send message
            await _emailProvider.SendEmailAsync(toAddresses, subject, message.Output, attachments: attachments, cancellationToken: cancellationToken);
        }
    }
}
