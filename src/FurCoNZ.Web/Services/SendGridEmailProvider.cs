using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Options;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FurCoNZ.Web.Services
{
    public class SendGridEmailProvider : IEmailProvider
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger _logger;
        private readonly SendGridEmailServiceOptions _options;

        public SendGridEmailProvider(ISendGridClient sendGridClient, IOptions<SendGridEmailServiceOptions> options, ILogger<SendGridEmailProvider> logger)
        {
            _sendGridClient = sendGridClient;
            _logger = logger;
            _options = options.Value;
        }

        public async Task SendEmailAsync(MailAddressCollection to, string subject, string htmlBody, AttachmentCollection attachments = null, CancellationToken cancellationToken = default)
        {
            var sendGridMessage = MailHelper.CreateSingleEmailToMultipleRecipients(
                from: new EmailAddress(_options.FromAddress, _options.FromName),
                tos: to.ToSendGridAddresses(),
                subject: subject,
                plainTextContent: HtmlToPlainText(htmlBody),
                htmlContent: htmlBody
            );

            if (attachments != null && attachments.Any())
            {
                await sendGridMessage.AddAttachmentsAsync(attachments, cancellationToken);
            }

            try
            {

                var response = await _sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
                if(response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError($"Failed to send email to {to}. Sendgrid response: {response.Body}");
                }
            }
            catch (HttpRequestException ex) {
                _logger.LogError(ex, $"Failed to send email to {to}.");
            }
        }

        /// <summary>
        /// Convert the HTML content to plain text
        /// </summary>
        /// <param name="html">The html content which is going to be converted</param>
        /// <returns>A string</returns>
        private static string HtmlToPlainText(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            if (document.DocumentNode == null)
            {
                return string.Empty;
            }
            return document.DocumentNode.InnerText;
        }
    }
}
