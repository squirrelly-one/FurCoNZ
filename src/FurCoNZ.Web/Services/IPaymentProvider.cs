using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Services
{
    public interface IPaymentProvider
    {
        string Name { get; }
        string SupportedMethods { get; }
        string Description { get; }

        Task<bool> RefundAsync(string paymentReference, CancellationToken cancellationToken = default);
    }
}