using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public List<LinkedAccount> LinkedAccounts { get; set; }
    }
}
