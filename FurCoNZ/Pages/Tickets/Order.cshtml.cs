using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FurCoNZ.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FurCoNZ.Pages.Tickets
{
    public class OrderModel : PageModel
    {
        public IEnumerable<TicketType> AvailableTicketTypes { get; set; }

        public void OnGet()
        {
            // TODO: Wire up EF
            AvailableTicketTypes = new List<TicketType>
            {
                new TicketType { Name = "Standard", PriceCents = 9500, TotalAvailable = 80},
                new TicketType { Name = "Sponsor", PriceCents = 15000, TotalAvailable = 20},
                new TicketType { Name = "Super Sponsor", PriceCents = 35000, TotalAvailable = 2},
                new TicketType { Name = "Day Pass", PriceCents = 4000, TotalAvailable = 200},
            };
        }
    }
}
