using System.Collections.Generic;
using FurCoNZ.Web.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Web.ViewModels
{
    public class OrderIndexViewModel
    {
        public Dictionary<int, OrderTicketTypeViewModel> AvailableTicketTypes { get; set; }
    }
}
