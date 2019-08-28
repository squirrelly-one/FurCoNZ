using FurCoNZ.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Areas.Admin.ViewModels
{
    public class PaymentsViewModel
    {
        public ICollection<OrderAuditViewModel> Payments { get; set; }
        public ReceivedPayment ReceivedPayment { get; set; }
    }
}
