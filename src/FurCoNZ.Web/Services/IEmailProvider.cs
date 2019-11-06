using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.Services
{
    public interface IEmailProvider
    {
        Task SendEmailAsync(MailAddressCollection to, string subject, string htmlBody, AttachmentCollection attachments = null, CancellationToken cancellationToken = default);
    }
}
