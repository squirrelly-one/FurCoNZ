using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using FurCoNZ.Web.Validators;

namespace FurCoNZ.Web.Areas.Admin.ViewModels
{
    public class ReceivedPayment
    {
        [Required]
        [Remote(action: "VerifyOrderRef", controller: "Orders", areaName:"Admin")]
        [ChecksumValid]
        [Display(Name = "Order Ref")]
        public string OrderReference { get; set; }

        [Required]
        [MinValue(0)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Received At")]
        public DateTime When { get; set; } = DateTime.Now;

        public int AmountCents => (int)(Amount * 100);
    }
}
