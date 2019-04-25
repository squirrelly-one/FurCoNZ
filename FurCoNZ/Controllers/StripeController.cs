using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Controllers
{
    public class StripeController : Controller
    {
        [HttpPost]
        public IActionResult Charge(string token)
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
