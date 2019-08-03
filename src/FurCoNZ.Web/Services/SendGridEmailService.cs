using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Options;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FurCoNZ.Web.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridEmailServiceOptions _options;

        public SendGridEmailService(ISendGridClient sendGridClient, IOptions<SendGridEmailServiceOptions> options)
        {
            _sendGridClient = sendGridClient;
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

            await sendGridMessage.AddAttachmentsAsync(attachments, cancellationToken);

            await _sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
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
