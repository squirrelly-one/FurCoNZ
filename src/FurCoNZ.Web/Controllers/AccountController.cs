using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Web.Helpers;
using FurCoNZ.Web.Services;
using FurCoNZ.Web.Models;
using FurCoNZ.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public static readonly string RedirectAfterDetailsSessionKey = "RedirectAfterDetails";

        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public AccountController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var hasRedirect = HttpContext.Session.Keys.Contains(RedirectAfterDetailsSessionKey);
            var user = await _userService.GetCurrentUserAsync(cancellationToken);
            return View(new AccountViewModel(user)
                {
                    IsConfirmingDetails = hasRedirect,
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AccountViewModel account, CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid) {
                var user = await _userService.GetCurrentUserAsync(cancellationToken);

                user.Name = account.Name;
                // TODO: Require email validation of this change
                user.Email = account.Email;

                await _userService.UpdateUserAsync(user, cancellationToken);

                if (HttpContext.Session.Keys.Contains(RedirectAfterDetailsSessionKey))
                {
                    var redirectTo = HttpContext.Session.Get<string>(RedirectAfterDetailsSessionKey);
                    HttpContext.Session.Remove(RedirectAfterDetailsSessionKey);
                    return Redirect(redirectTo);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Orders(CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUserAsync(cancellationToken);
            if (user == null)
            {
                throw new Exception("We are unable to find your user details within our database. Which may indicate that you did not log in properly");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var orders = await _orderService.GetUserOrdersAsync(user, cancellationToken);

            return View(new AccountOrdersViewModel (await _userService.GetCurrentUserAsync(cancellationToken))
                {
                    Orders = orders.Select(o => new OrderViewModel(o)).ToList()
                });
        }

        public async Task<IActionResult> Tickets()
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext.RequestAborted);
            if (user == null)
            {
                throw new Exception("We are unable to find your user details within our database. Which may indicate that you did not log in properly");
            }

            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            var orders = await _orderService.GetUserOrdersAsync(user, HttpContext.RequestAborted);

            return View(new AccountTicketsViewModel
            {
                Tickets = orders
                    // Filter out refunded tickets
                    .Where(o => o.Audits.All(a => a.Type != AuditType.Refunded))
                    .SelectMany(o => o.TicketsPurchased)
                    .Select(t => new TicketDetailViewModel(t))
                    .OrderBy(t => t.Id).ToList(),
            });
        }

        public async Task<IActionResult> Logout()
        {
            var callbackUrl = Url.Action("Index", "Home", values: null, protocol: Request.Scheme);
            var signOut = SignOut(new AuthenticationProperties {
                    RedirectUri = callbackUrl
                }, CookieAuthenticationDefaults.AuthenticationScheme, "oidc");
            return signOut;
        }
    }
}
