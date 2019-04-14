using FurCoNZ.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.ViewModels
{
    public class AccountViewModel
    {
        public AccountViewModel() { }

        public AccountViewModel(User user)
        {
            Name = user.Name;
            Email = user.Email;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
