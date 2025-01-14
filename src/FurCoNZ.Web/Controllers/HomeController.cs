﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FurCoNZ.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace FurCoNZ.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetNotified()
        {
            return View();
        }

        public IActionResult News()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Tickets()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult ParentalConsent()
        {
            return View();
        }


        public IActionResult FAQ()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
