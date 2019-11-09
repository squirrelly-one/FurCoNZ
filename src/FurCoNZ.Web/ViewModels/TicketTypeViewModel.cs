using FurCoNZ.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.ViewModels
{
    public class TicketTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceCents { get; set; }
        public int TotalAvailable { get; set; }
        public DateTimeOffset SoldOutAt { get; set; }

        public decimal Price => (decimal)PriceCents / 100;


        public TicketTypeViewModel()
        {

        }

        public TicketTypeViewModel(TicketType ticketType)
        {
            if (ticketType == null)
                return;

            Id = ticketType.Id;
            Name = ticketType.Name;
            Description = ticketType.Description;
            PriceCents = ticketType.PriceCents;
            TotalAvailable = ticketType.TotalAvailable;
            SoldOutAt = ticketType.SoldOutAt;
        }
    }
}
