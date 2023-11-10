using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Web.Code.Attributes.Data;

/// <summary>
/// Validation attribute for boolean == true
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
{
    public string GetErrorMessage() => ErrorMessage ?? "This field is required.";

    public bool DisableClientSideValidation { get; set; }

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

        throw new ArgumentException(null, nameof(value));
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
        ArgumentNullException.ThrowIfNull(context);

        MergeAttribute(context.Attributes, "data-val", "true");

        if (!DisableClientSideValidation)
        {
            MergeAttribute(context.Attributes, "data-val-mustbetrue", GetErrorMessage());
        }
    }
}
