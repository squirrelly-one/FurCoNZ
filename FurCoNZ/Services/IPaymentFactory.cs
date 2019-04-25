using FurCoNZ.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FurCoNZ.Services
{
    public interface IPaymentService
    {
        IPaymentProvider GetPaymentService(string name);

        IEnumerable<IPaymentProvider> PaymentServicees { get; }
    }
}
