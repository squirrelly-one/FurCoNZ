using FurCoNZ.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Components
{
    public class StripeViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public StripeViewComponent(IUserService userService, IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.RequestAborted);
            if (user == null)
                return View(); // Default view displays nothing to the end user.
            var order = await _orderService.GetUserOrderAsync(user, orderId, HttpContext.RequestAborted);
            if (order == null)
                return View(); // Default view displays nothing to the end user.

            // Will display the strip payment form that submits to the StripeController.
            return View("Input");
        }
    }
}
