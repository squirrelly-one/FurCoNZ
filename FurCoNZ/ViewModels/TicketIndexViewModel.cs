using System.Collections.Generic;
using FurCoNZ.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.ViewModels
{
    public class TicketIndexViewModel
    {
        public IEnumerable<TicketType> AvailableTicketTypes { get; set; }
    }
}
