using FurCoNZ.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.ViewModels
{
    public class AccountOrdersViewModel : AccountViewModel
    {
        public AccountOrdersViewModel() { }

        public AccountOrdersViewModel(User user)
            : base(user) { }

        public ICollection<OrderViewModel> Orders { get; set; }
    }
}
