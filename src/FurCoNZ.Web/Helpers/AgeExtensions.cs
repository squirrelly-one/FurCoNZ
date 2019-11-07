using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Helpers
{
    public static class AgeExtensions
    {
        public static int GetAge(this DateTime dateOfBirth)
        {
            return GetAgeAtDate(dateOfBirth, DateTime.Now);
        }

        public static int GetAgeAtDate(this DateTime dateOfBirth, DateTime when)
        {
            int age = when.Year - dateOfBirth.Year;
            return dateOfBirth > when.AddYears(-age)
                ? age - 1
                : age;
        }
    }
}
