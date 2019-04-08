using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Services;
using FurCoNZ.Models;
using FurCoNZ.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public AccountController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        private async Task<T> PrefillAccountViewModel<T>(T account, CancellationToken cancellationToken = default) where T : AccountViewModel
        {
            var user = await _userService.GetUserAsync(User.FindFirst("sub").Value, cancellationToken);
            account.Name = user.Name;
            account.Email = user.Email;

            return account;
        }

        public async Task<IActionResult> Index()
        {
            return View(await PrefillAccountViewModel(new AccountViewModel()));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccount(AccountViewModel account, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserAsync(User.FindFirst("sub").Value, cancellationToken);

            user.Name = account.Name;
            user.Email = account.Email;

            await _userService.UpdateUserAsync(user, cancellationToken);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Orders(CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserAsync(User.FindFirst("sub").Value, cancellationToken);
            if (user == null)
                throw new Exception("We are unable to find your user details within our database. Which may indicate that you did not log in properly");

            cancellationToken.ThrowIfCancellationRequested();

            var orders = await _orderService.GetUserOrders(user, cancellationToken);

            return View("Index", await PrefillAccountViewModel(new AccountOrdersViewModel {
                Orders = orders.ToList()
            }));
        }
    }
}
