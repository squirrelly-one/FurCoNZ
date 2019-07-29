using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace FurCoNZ.Web.ViewModels
{
    public class ValidateOrderViewModel
    {
        public IList<TicketDetailViewModel> TicketDetails { get; set; }
        [HiddenInput]
        public string TicketOrderHashBase64 { get; set; }
    }
}
