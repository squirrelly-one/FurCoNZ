﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.Helpers; // Required for SessionExtensions
using FurCoNZ.Web.Services;
using FurCoNZ.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string ActiveOrderSessionKey = "ActiveOrder";

        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
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
                var ticketTypes = await _orderService.GetTicketTypesAsync(false, false, cancellationToken);

                var ticketIndex = 0;
                foreach (var ticketTypeOrdered in orderIndexViewModel.AvailableTicketTypes.Where(x => x.Value.QuantityOrdered > 0))
                {
                    var ticketTypeId = ticketTypeOrdered.Key;
                    var quantityOrderedForTicketType = ticketTypeOrdered.Value.QuantityOrdered;

                    for (var i = 0; i < quantityOrderedForTicketType; i++)
                    {
                        var ticketType = ticketTypes.FirstOrDefault(x => x.Id == ticketTypeId);
                        viewModel.Add(new TicketDetailViewModel
                        {
                            Id = ++ticketIndex,
                            TicketType = ticketType != null 
                                ? new TicketTypeViewModel(ticketType) 
                                : null,
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
            // Repopulate ticket type as that information isn't submitted back to us.
            var ticketTypes = await _orderService.GetTicketTypesAsync(cancellationToken: cancellationToken);
            foreach (var ticket in model)
            {
                var ticketType = ticketTypes.FirstOrDefault(x => x.Id == ticket.TicketType.Id);
                ticket.TicketType = ticketType != null
                    ? new TicketTypeViewModel(ticketType)
                    : null;
            }

            if (ModelState.IsValid)
            {
                HttpContext.Session.Set(ActiveOrderSessionKey, model);

                var viewModel = new ValidateOrderViewModel
                {
                    TicketDetails = model,
                    TicketOrderHashBase64 = Convert.ToBase64String(HttpContext.Session.GetValueHash(ActiveOrderSessionKey)),
                };

                return View(viewModel);
            }

            // If we get here, something has gone wrong...
            return View("TicketDetail", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ValidateOrderViewModel model, CancellationToken cancellationToken)
        {
            // Get the ticket details (we'll want them for the validation page if something goes wrong)
            model.TicketDetails = HttpContext.Session.Get<IList<TicketDetailViewModel>>(ActiveOrderSessionKey);

            if (ModelState.IsValid)
            {
                // secureValidation = false since there is no risk to a malicous user correctly determining the hash through timing attacks
                var sessionHasNotChanged = HttpContext.Session.ValidateValueHash(ActiveOrderSessionKey, Convert.FromBase64String(model.TicketOrderHashBase64), secureValidation: false);

                if (sessionHasNotChanged)
                {
                    var user = await _userService.GetCurrentUserAsync(cancellationToken);

                    var tickets = new List<Models.Ticket>();

                    foreach (var ticketViewModel in model.TicketDetails)
                    {
                        tickets.Add(GetTicketEntityFromViewModel(ticketViewModel));
                    }

                    var order = await _orderService.CreateOrderAsync(user, tickets, cancellationToken: cancellationToken);

                    return RedirectToAction("Index", "Checkout", new { orderId = order.Id });
                }
            }

            // If we get here, something has gone wrong...
            return View("Validate", model);
        }

        // TODO: Lots of things missing here
        private Models.Ticket GetTicketEntityFromViewModel(TicketDetailViewModel ticketViewModel)
        {
            return new Models.Ticket
            {
                // AttendeeAccountId // Set this if the user claims this as theirs, and leave null if it's for someone else

                TicketTypeId = ticketViewModel.TicketType.Id,

                TicketName = ticketViewModel.BadgeName,

                PreferredName = ticketViewModel.PreferredFullName,
                // PreferredPronouns


                LegalName = String.IsNullOrWhiteSpace(ticketViewModel.IdentificationFullName) ? ticketViewModel.PreferredFullName : ticketViewModel.IdentificationFullName,
                DateOfBirth = ticketViewModel.DateOfBirth,

                EmailAddress = ticketViewModel.EmailAddress,

                // MealRequirements
                MealRequirements = ticketViewModel.DietryRequirements.Any() 
                    ? ticketViewModel.DietryRequirements.Select(f => (Models.FoodMenu)f).Aggregate((a,b) => a | b)
                    : Models.FoodMenu.Regular,
                KnownAllergens = ticketViewModel.KnownAllergies,
                CabinGrouping = ticketViewModel.CabinPreferences,

                AdditionalNotes = ticketViewModel.OtherNotes,
            };
        }
    }
}
