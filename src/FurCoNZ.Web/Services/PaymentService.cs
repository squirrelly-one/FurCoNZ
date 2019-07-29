using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ICollection<IPaymentProvider> _paymentServices;

        public PaymentService(IEnumerable<IPaymentProvider> paymentServices)        {
            _paymentServices = paymentServices.ToList();
        }

        public IEnumerable<IPaymentProvider> PaymentServicees => _paymentServices;


        public IPaymentProvider GetPaymentService(string name)
        {
            return _paymentServices.SingleOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
