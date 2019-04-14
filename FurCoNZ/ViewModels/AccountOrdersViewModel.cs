using FurCoNZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class AccountOrdersViewModel : AccountViewModel
    {
        public AccountOrdersViewModel() { }

        public AccountOrdersViewModel(User user)
            : base(user) { }

        public ICollection<Order> Orders { get; set; }
    }
}
