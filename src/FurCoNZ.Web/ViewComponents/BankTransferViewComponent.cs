using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using FurCoNZ.Web.Configuration;
using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Services;
using FurCoNZ.Web.ViewModels;

namespace FurCoNZ.Web.ViewComponents
{
    public class BankTransferViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IOptions<BankSettings> _bankOptions;

        public BankTransferViewComponent(IUserService userService, IOrderService orderService, IOptions<BankSettings> bankOptions)
        {
            _userService = userService;
            _orderService = orderService;
            _bankOptions = bankOptions;
        }

        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.RequestAborted);
            if (user == null)
                return View(); // Default view displays nothing to the end user.

            var order = await _orderService.GetUserOrderAsync(user, orderId, HttpContext.RequestAborted);
            if (order == null)
                return View(); // Default view displays nothing to the end user.

            if (order.AmountPaidCents >= order.TotalAmountCents)
                return View(); // Default view displays nothing to the end user.

            // TODO: Generate a payment uniquie reference number for the order id
            return View("Input", new BankTransferViewModel
            {
                Order = new OrderViewModel(order),
                PaymentReference = $"{order.Id}{DammAlgorithm.GetCheck(order.Id)}",
                TotalCents = order.AmountOwingCents,
                BankAccountName = _bankOptions.Value.AccountName,
                BankAccountNumber = _bankOptions.Value.AccountNumber,
            });
        }

    }
}
