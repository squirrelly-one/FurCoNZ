using System;
using System.Collections.Generic;
using System.Linq;

namespace FurCoNZ.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int OrderedById { get; set; }
        public virtual User OrderedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

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

        public virtual ICollection<StripeSession> StripeSessions { get; set; }

        public virtual ICollection<OrderAudit> Audits { get; set; }

        /// <summary>
        /// The date that the latest notification was sent informing the user that their order has not yet been paid for.
        /// </summary>
        public DateTimeOffset? LastReminderSent { get; set; }

        /// <summary>
        /// The date that the latest notification was sent informing the user that their order has expired.
        /// </summary>
        /// <remarks>Note: This does not incidate if the order has expired, only if a notification was sent to the user informing them it has.</remarks>
        public DateTimeOffset? ExpiredNotificationSent { get; set; }
    }
}