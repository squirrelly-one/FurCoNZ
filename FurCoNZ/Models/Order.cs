﻿using System.Collections.Generic;
using System.Linq;

namespace FurCoNZ.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int OrderedById { get; set; }
        public virtual User OrderedBy { get; set; }

        public int TotalAmountCents
        {
            get
            {
                return TicketsPurchased.Sum(x => x.TicketType.PriceCents);
            }
        }

        public int AmountPaidCents { get; set; }
        public int AmountOwingCents
        {
            get
            {
                return TotalAmountCents - AmountPaidCents;
            }
        }

        public virtual ICollection<Ticket> TicketsPurchased { get; set; }
    }
}