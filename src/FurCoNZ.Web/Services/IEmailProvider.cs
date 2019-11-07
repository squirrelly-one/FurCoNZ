using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IEmailProvider
    {
        Task SendEmailAsync(MailAddressCollection to, string subject, string htmlBody, IEnumerable<Attachment> attachments = null, CancellationToken cancellationToken = default);
    }
}
