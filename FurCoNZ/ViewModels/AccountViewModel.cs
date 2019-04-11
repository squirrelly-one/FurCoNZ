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
            Pronouns = user.Pronouns;
            Allergies = user.Allergies;
            DietryRequirements = user.DietryRequirements;
            DateOfBirth = user.DateOfBirth;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Pronouns { get; set; }

        public string Allergies { get; set; }

        public string DietryRequirements { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}
