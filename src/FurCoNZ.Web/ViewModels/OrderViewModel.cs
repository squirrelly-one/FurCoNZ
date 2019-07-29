using FurCoNZ.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.ViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel() { }

        public OrderViewModel(Order order)
        {
            Id = order.Id;
            OrderedBy = order.OrderedBy;
            Tickets = order.TicketsPurchased;
            AmountTotalCents = order.TotalAmountCents;
            AmountOwingCents = order.AmountOwingCents;
            AmountPaidCents = order.AmountPaidCents;
        }

        public int Id { get; set; }

        public User OrderedBy { get; set; }

        public decimal AmountTotal => (decimal)AmountTotalCents / 100;
        public decimal AmountPaid => (decimal)AmountPaidCents / 100;
        public decimal AmountOwing => (decimal)AmountOwingCents / 100;

        public ICollection<Ticket> Tickets { get; set; }
        public int AmountTotalCents { get; set; }
        public int AmountOwingCents { get; set; }
        public int AmountPaidCents { get; set; }
    }
}
