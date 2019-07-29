using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.ViewComponents
{
    public class BankTransferViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            // TODO: Generate a payment uniquie reference number for the order id
            return View();
        }

    }
}
