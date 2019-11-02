using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Services;
using FurCoNZ.Web.ViewModels;
using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentProvider;

        public CheckoutController(IUserService userService, IOrderService orderService, IPaymentService paymentProvider)
        {
            _userService = userService;
            _orderService = orderService;
            _paymentProvider = paymentProvider;
        }

        [HttpGet("/Checkout/{orderId}")]
        public async Task<IActionResult> Index(int orderId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUserAsync(cancellationToken);

            // TODO: Verify the order is pending payment
            var order = await _orderService.GetUserOrderAsync(user, orderId, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.Audits.Any(a => a.Type == AuditType.Refunded))
                return RedirectToAction(nameof(AccountController.Orders), "Account");

            if (order.AmountOwingCents == 0)
                return RedirectToAction(nameof(AccountController.Orders), "Account");

            return View(GetCheckoutViewModelFromOrder(order));
        }

        private CheckoutViewModel GetCheckoutViewModelFromOrder(Order order)
        {
            return new CheckoutViewModel
            {
                OrderId = order.Id,
                Order = new OrderViewModel(order),
                PaymentProviders = _paymentProvider.PaymentServicees.Select(p => new PaymentProviderViewmodel
                {
                    Name = p.Name,
                    Methods = p.SupportedMethods,
                    Description = p.Description,
                }).ToList(),
            };
        }
    }
}
