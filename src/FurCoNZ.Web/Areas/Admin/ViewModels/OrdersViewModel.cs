using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FurCoNZ.Web.ViewModels;

namespace FurCoNZ.Web.Areas.Admin.ViewModels
{
    public class OrdersViewModel
    {
        public IEnumerable<OrderViewModel> Orders { get; set; }

        public ReceivedPayment ReceivedPayment { get; set; }
    }
}
