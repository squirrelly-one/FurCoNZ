using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Web.Areas.Admin.ViewModels;
using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Models;
using FurCoNZ.Web.Services;
using FurCoNZ.Web.Services.Payment;
using FurCoNZ.Web.ViewModels;

namespace FurCoNZ.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrdersController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(int id = 0)
        {
            if(id != 0)
            {
                var order = await _orderService.GetOrderById(id, HttpContext.RequestAborted);
                if (order == null)
                    return NotFound();
                return View("Order", new OrderViewModel(order));
            }

            var orders = await _orderService.GetAllOrdersAsync(HttpContext.RequestAborted);
            return View(new OrdersViewModel
            {
                Orders = orders.Select(o => new OrderViewModel(o)).AsEnumerable(),
                ReceivedPayment = new ReceivedPayment()
            });
        }

        public async Task<IActionResult> Payments()
        {
            var orders = await _orderService.GetAllOrdersAsync(HttpContext.RequestAborted);

            var payments = orders
                .SelectMany(o => o.Audits)
                .OrderByDescending(a => a.When)
                .Select(a => new OrderAuditViewModel(a));

            return View(new PaymentsViewModel
            {
                Payments = payments.ToList(),
                ReceivedPayment = new ReceivedPayment(),
            });
        }

        /// <summary>
        /// Refunds entire order.
        /// </summary>
        /// <param name="orderViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(OrderViewModel orderViewModel)
        {
            // TODO: Refund a select payment
            var order = await _orderService.GetOrderById(orderViewModel.Id, HttpContext.RequestAborted);
            if (order == null)
            {
                return NotFound();
            }


            var payments = order.Audits.GroupBy(a => a.PaymentProviderReference, (paymentReference, orders) => new OrderAudit
            {
                // Note that we're not populating all fields. 
                PaymentProvider = orders.First().PaymentProvider,
                PaymentProviderReference = paymentReference,
                Type = orders.Any(o => o.Type == AuditType.Refunded) ? AuditType.Refunded : AuditType.Received,
            });

            if (order.Audits.All(a => a.Type == AuditType.Refunded))
            {
                return BadRequest("Order has already been refunded");
            }

            foreach(var payment in payments.Where(p => p.Type != AuditType.Refunded))
            {
                var provider = _paymentService.GetPaymentService(payment.PaymentProvider);

                // Try to perform a refund with the payment provider
                await provider.RefundAsync(order.Id, payment.PaymentProviderReference, HttpContext.RequestAborted);
            }

            return RedirectToAction(nameof(Index), new { id = order.Id });
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
            {
                return Json("No order found with this id");
            }

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
                {
                    return NotFound();
                }

                await _orderService.AddReceivedFundsForOrderAsync(
                    order.Id,
                    receivedPayment.AmountCents,
                    BankPaymentProvider.NAME, receivedPayment.OrderReference,
                    receivedPayment.When,
                    HttpContext.RequestAborted);

                if (HttpContext.Request.Headers.ContainsKey("Referer"))
                {
                    return Redirect(HttpContext.Request.Headers["Referer"].ToString());
                }

                return RedirectToAction("Index");
            }

            if (HttpContext.Request.Headers.ContainsKey("Referer"))
            {
                return Redirect(HttpContext.Request.Headers["Referer"].ToString());
            }

            return RedirectToAction("Index");
        }
    }
}