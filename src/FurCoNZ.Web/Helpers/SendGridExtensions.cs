using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace FurCoNZ.Web.Helpers
{
    internal static class SendGridExtensions
    {
        internal static EmailAddress ToSendGridAddress(this MailAddress mailAddress)
        {
            return new EmailAddress(mailAddress.Address, mailAddress.DisplayName);
        }

        internal static List<EmailAddress> ToSendGridAddresses(this IEnumerable<MailAddress> mailAddresses)
        {
            return mailAddresses.Select(mailAddress => mailAddress.ToSendGridAddress()).ToList();
        }

        internal async static Task AddAttachmentAsync(this SendGridMessage sendGridMessage, System.Net.Mail.Attachment attachment, CancellationToken cancellationToken = default)
        {
            await sendGridMessage.AddAttachmentAsync(attachment.ContentDisposition.FileName, attachment.ContentStream, attachment.ContentType.Name, attachment.ContentDisposition.Inline ? "inline" : "attachment", attachment.ContentId, cancellationToken);
        }

        internal static async Task AddAttachmentsAsync(this SendGridMessage sendGridMessage, IEnumerable<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(attachments.Select(a => sendGridMessage.AddAttachmentAsync(a, cancellationToken)));
        }
    }
}
