using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.DAL;
using FurCoNZ.Services;
using FurCoNZ.ViewModels;
using FurCoNZ.Models;

namespace FurCoNZ.Controllers
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

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUserAsync(cancellationToken);

            // TODO: Verify the order is pending payment
            var order = await _orderService.GetUserPendingOrderAsync(user, cancellationToken);
            if (order == null)
                return RedirectToAction("Index","Account");

            return View(GetCheckoutViewModelFromOrder(order));
        }

        [Route("/Checkout/Order/{orderId}")]
        public async Task<IActionResult> Order(int orderId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUserAsync(cancellationToken);

            // TODO: Verify the order is pending payment
            var order = await _orderService.GetUserOrderAsync(user, orderId, cancellationToken);
            if (order == null)
                return NotFound();

            return View("Index", GetCheckoutViewModelFromOrder(order));
        }

        private CheckoutViewModel GetCheckoutViewModelFromOrder(Order order)
        {
            return new CheckoutViewModel
            {
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
