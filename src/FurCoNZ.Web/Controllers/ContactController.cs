using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FurCoNZ.Web.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ActivityOrTalk()
        {
            return View();
        }

        public IActionResult DealersDen()
        {
            return View();
        }

        public IActionResult Volunteer()
        {
            return View();
        }
    }
}