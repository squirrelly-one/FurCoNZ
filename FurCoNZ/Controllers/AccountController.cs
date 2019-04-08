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

        private T PrefillAccountViewModel<T>(T account) where T : AccountViewModel
        {
            account.CalledName = User.FindFirst("name").Value;

            return account;
        }

        public IActionResult Index()
        {
            return View(PrefillAccountViewModel(new AccountViewModel()));
        }

        public async Task<IActionResult> Orders(CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserAsync(User.FindFirst("sub").Value, cancellationToken);
            if (user == null)
                throw new Exception("We are unable to find your user details within our database. Which may indicate that you did not log in properly");

            cancellationToken.ThrowIfCancellationRequested();

            var orders = await _orderService.GetUserOrders(user, cancellationToken);

            return View("Index", PrefillAccountViewModel(new AccountOrdersViewModel {
                Orders = orders.ToList()
            }));
        }
    }
}
