using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Helpers; // Required for SessionExtensions
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

        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(OrderIndexViewModel orderIndexViewModel, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var viewModel = new List<TicketDetailViewModel>();
                var ticketTypes = await _orderService.GetTicketTypesAsync(false, cancellationToken);

                var ticketIndex = 0;
                foreach (var ticketTypeOrdered in orderIndexViewModel.AvailableTicketTypes.Where(x => x.Value.QuantityOrdered > 0))
                {
                    var ticketTypeId = ticketTypeOrdered.Key;
                    var quantityOrderedForTicketType = ticketTypeOrdered.Value.QuantityOrdered;

                    for (var i = 0; i < quantityOrderedForTicketType; i++)
                    {
                        viewModel.Add(new TicketDetailViewModel
                        {
                            Id = ++ticketIndex,
                            TicketTypeId = ticketTypeId,
                            TicketTypeName = ticketTypes.FirstOrDefault(x => x.Id == ticketTypeId).Name,
                        });
                    }
                }

                return View("TicketDetail", viewModel);
            }

            return View(orderIndexViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate(IList<TicketDetailViewModel> model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.Set("ActiveOrder", model);

                var ticketTypes = await _orderService.GetTicketTypesAsync(cancellationToken: cancellationToken);
                foreach (var ticket in model)
                {
                    ticket.TicketTypeName = ticketTypes.FirstOrDefault(x => x.Id == ticket.TicketTypeId)?.Name;
                }

                return View(model);
            }

            // If we get here, something has done wrong...
            return View("TicketDetail", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();

            var model = HttpContext.Session.Get<IList<TicketDetailViewModel>>("ActiveOrder");
        }
    }
}
