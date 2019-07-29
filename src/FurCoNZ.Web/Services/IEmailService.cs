using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailAddressCollection to, string subject, string htmlBody, AttachmentCollection attachments = null, CancellationToken cancellationToken = default);
    }
}
