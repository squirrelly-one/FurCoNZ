using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Services;
using FurCoNZ.ViewModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var availableTicketTypes = await _orderService.GetTicketTypesAsync(cancellationToken: cancellationToken);

            var viewModel = new OrderIndexViewModel
            {
                AvailableTicketTypes = availableTicketTypes.Select(a => new KeyValuePair<int, OrderTicketTypeViewModel>
                (
                    a.Id,
                    new OrderTicketTypeViewModel
                    {
                        Name = a.Name,
                        Description = a.Description,
                        PriceCents = a.PriceCents,
                        TotalAvailable = a.TotalAvailable,
                        QuantityOrdered = 0, // default to zero tickets
                    }
                )).ToDictionary(a => a.Key, a => a.Value)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(OrderIndexViewModel orderIndexViewModel, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var viewModel = new List<TicketDetailViewModel>();

                return View("TicketDetail", viewModel);
            }

            return View(orderIndexViewModel);
        }
    }
}
