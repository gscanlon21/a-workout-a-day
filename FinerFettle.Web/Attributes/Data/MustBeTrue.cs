using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Attributes.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MustBeTrue : ValidationAttribute, IClientModelValidator
    {
        public string GetErrorMessage() => ErrorMessage == null ? "This field is required" : ErrorMessage;

        public override bool IsValid(object? value)
        {
            if (value == null) 
            {
                // Leave required to the RequiredAttribute
                return true;
            }

            if (value is bool b) 
            { 
                return b; 
            }

            throw new ArgumentException(nameof(value));
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (IsValid(value))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(GetErrorMessage());
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }

            attributes.Add(key, value);
            return true;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-mustbetrue", GetErrorMessage());
        }
    }
}
