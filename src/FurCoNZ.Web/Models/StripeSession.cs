using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Models
{
    public class StripeSession
    {
        /// <summary>
        /// Session ID is generated through Stripe API
        /// </summary>
        public string Id { get; set; }

        public StripeSessionState State { get; set; }

        public string PaymentIntent { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
    }

    public enum StripeSessionState
    {
        None = 0,
        Processing = 1,
        Cancelled = 2,
        Completed = 3,
        Refunding = 4,
        Refunded = 5,
    }
}
