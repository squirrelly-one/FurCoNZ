using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IPaymentProvider
    {
        string Name { get; }
        string DisplayName { get; }
        string SupportedMethods { get; }
        string Description { get; }

        Task<bool> RefundAsync(int orderId, string paymentReference, CancellationToken cancellationToken = default);
    }
}
