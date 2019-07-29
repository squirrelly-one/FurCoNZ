using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Models
{
    public class OrderAudit
    {
        public int Id { get; set; }

        [Required]
        public AuditType Type { get; set; }

        /// <summary>
        /// Amount in cents that affects the sum of the order. 
        /// </summary>
        /// <remarks>
        /// A refund must be in negative value. so that a sum operation will net zero.
        /// </remarks>
        [Required]
        public int AmountCents { get; set; }

        [Required]
        public DateTimeOffset When { get; set; }

        [Required]
        public string PaymentProvider { get; set; }

        /// <summary>
        /// A reference spedcific to the payment provider.
        /// </summary>
        [Required]
        public string PaymentProviderReference { get; set; }

        [Required]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
    }

    public enum AuditType
    {
        Unknown = 0,
        Received = 1,
        Refunded = 2,
    }
}
