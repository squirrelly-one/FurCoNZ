using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IReminderService
    {
        Task Send30DayRemainingPendingOrdersAsync(CancellationToken cancellationToken = default);
        Task SendCancelledOrdersAsync(CancellationToken cancellationToken = default);
    }
}