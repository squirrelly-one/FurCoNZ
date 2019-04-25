using FurCoNZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel() { }

        public OrderViewModel(Order order)
        {
            Id = order.Id;
            OrderedBy = order.OrderedBy;
            Tickets = order.TicketsPurchased;
        }

        public int Id { get; set; }

        public User OrderedBy { get; set; }

        public decimal TotalAmount => Tickets.Sum(t => t.TicketType.PriceCents) / 100;

        public ICollection<Ticket> Tickets { get; set; }
    }
}
