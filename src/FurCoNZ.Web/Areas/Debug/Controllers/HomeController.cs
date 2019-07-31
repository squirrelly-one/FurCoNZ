using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Models;
using FurCoNZ.Web.Services;

namespace FurCoNZ.Web.Areas.Debug.Controllers
{
    [Area("Debug")]
    public class ToolsController : Controller
    {
        private readonly FurCoNZDbContext _dbContext;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;

        public ToolsController(FurCoNZDbContext dbContext, IHostingEnvironment hostingEnvironment, IUserService userService)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

#if DEBUG
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MakeAdmin(CancellationToken cancellationToken)
        {
            if (!_hostingEnvironment.IsDevelopment()) throw new InvalidOperationException("This is not a development environment, operation is not allowed.");
            if (await _dbContext.Users.AnyAsync(u => u.IsAdmin, cancellationToken)) throw new InvalidOperationException("An administrator is already present in this database.");

            var currentUser = await _userService.GetCurrentUserAsync(cancellationToken);

            currentUser.IsAdmin = true;

            await _userService.UpdateUserAsync(currentUser, cancellationToken);

            await HttpContext.SignOutAsync("oidc");

            return RedirectToAction("Index", "Home", new { Area = "" });
        }
#endif

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SeedData(CancellationToken cancellationToken)
        {
            if (!_hostingEnvironment.IsDevelopment()) throw new InvalidOperationException("This is not a development environment, operation is not allowed.");
            if (await _dbContext.TicketTypes.AnyAsync(cancellationToken)) throw new InvalidOperationException("Ticket Types already exist in the database.");

            var sampleTicketTypes = new List<TicketType>
            {
                new TicketType
                {
                    Name = "Super Sponsor",
                    Description = "Absolute Luxury for furs with a the cash!",
                    PriceCents = 2000000,
                    TotalAvailable = 2,
                },
                new TicketType
                {
                    Name = "Sponsor",
                    Description = "Deluxe cabins and extra perks!",
                    PriceCents = 200000,
                    TotalAvailable = 20,
                },
                new TicketType
                {
                    Name = "Standard",
                    Description = "Standard tickets for an awesome time!",
                    PriceCents = 20000,
                    TotalAvailable = 200,
                },
                new TicketType
                {
                    Name = "Day Pass",
                    Description = "Come along for just the day!",
                    PriceCents = 2000,
                    TotalAvailable = 2000,
                },
            };

            _dbContext.TicketTypes.AddRange(sampleTicketTypes);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index", "Home", new { Area = "" });
        }
    }
}