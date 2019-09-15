using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.Web.Validators
{
    public class MinValueAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly double _minValue;

        public MinValueAttribute(double minValue)
        {
            _minValue = minValue;
            ErrorMessage = "Enter a value greater than or equal to " + _minValue;
        }

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
            ErrorMessage = "Enter a value greater than or equal to " + _minValue;
        }

        public override bool IsValid(object value)
        {
            return Convert.ToDouble(value) >= _minValue;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-range", ErrorMessage);
            MergeAttribute(context.Attributes, "data-val-range-max", Double.MaxValue.ToString(CultureInfo.InvariantCulture));
            MergeAttribute(context.Attributes, "data-val-range-min", _minValue.ToString(CultureInfo.InvariantCulture));
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
    }
}