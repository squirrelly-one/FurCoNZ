using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Web.Areas.Admin.ViewModels;
using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Services;
using FurCoNZ.Web.ViewModels;

namespace FurCoNZ.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;

        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync(HttpContext.RequestAborted);
            return View(new OrdersViewModel
            {
                Orders = orders.Select(o => new OrderViewModel(o)).AsEnumerable(),
                ReceivedPayment = new ReceivedPayment()
            });
        }

        public async Task<IActionResult> VerifyOrderRef(string orderReference)
        {
            if (!int.TryParse(orderReference, out var orderRefAsInt))
            {
                return Json("Order reference must only contain digits");
            }

            if(!DammAlgorithm.IsValid(orderRefAsInt))
            {
                return Json("Invalid reference number");
            }

            var order = await _orderService.GetOrderByRef(orderRefAsInt, HttpContext.RequestAborted);
            if (order == null)
                return Json("No order found with this id");

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(ReceivedPayment receivedPayment)
        {
            if (ModelState.IsValid)
            {
                if (!int.TryParse(receivedPayment.OrderReference, out var orderRefAsInt))
                    return ValidationProblem();

                var order = await _orderService.GetOrderByRef(orderRefAsInt, HttpContext.RequestAborted);
                if (order == null)
                    return NotFound();

                await _orderService.AddReceivedFundsForOrderAsync(
                    order.Id,
                    receivedPayment.AmountCents,
                    "bank", receivedPayment.OrderReference,
                    receivedPayment.When,
                    HttpContext.RequestAborted);

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}