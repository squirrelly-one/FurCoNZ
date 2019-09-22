using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IReminderService
    {
        Task NotifyOfPendingOrderAsync(CancellationToken cancellationToken = default);
        Task NotifyOfCancelledOrderAsync(CancellationToken cancellationToken = default);
    }
}