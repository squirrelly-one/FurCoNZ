using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FurCoNZ.Web.Models;

namespace FurCoNZ.Web.ViewModels
{
    public class AccountTicketsViewModel : AccountViewModel
    {
        public AccountTicketsViewModel() { }

        public AccountTicketsViewModel(User user)
            : base(user) { }

        public ICollection<TicketDetailViewModel> Tickets { get; set; } = new List<TicketDetailViewModel>();
    }
}
