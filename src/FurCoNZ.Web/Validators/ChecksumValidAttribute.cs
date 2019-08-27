using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using FurCoNZ.Web.Helpers;

namespace FurCoNZ.Web.Validators
{
    public class ChecksumValidAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is int intValue)
            {
                return DammAlgorithm.IsValid(intValue);
            }
            else if (value is string stringValue && int.TryParse(stringValue, out intValue))
            {
                return DammAlgorithm.IsValid(intValue);
            }
            return false;

        }
    }
}
