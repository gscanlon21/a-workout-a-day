using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Core.Code.Attributes;

/// <summary>
/// Provides conditional validation based on related property value.
/// </summary>
/// <param name="otherProperty">The other property.</param>
/// <param name="otherPropertyValue">The other property value.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class RequiredIfAttribute(string otherProperty, object otherPropertyValue) : ValidationAttribute("'{0}' is required because '{1}' has a value {3}'{2}'.")
{
    /// <summary>
    /// The other property name that will be used during validation.
    /// </summary>
    public string OtherProperty { get; private set; } = otherProperty;

    /// <summary>
    /// The display name of the other property.
    /// </summary>
    public string? OtherPropertyDisplayName { get; set; }

    /// <summary>
    /// The other property value that will be relevant for validation.
    /// </summary>
    public object OtherPropertyValue { get; private set; } = otherPropertyValue;

    /// <summary>
    /// Gets or sets a value indicating whether other property's value should match or differ from provided other property's value (default is <c>false</c>).
    /// 
    /// How this works
    /// - true: validated property is required when other property doesn't equal provided value
    /// - false: validated property is required when other property matches provided value
    /// </summary>
    public bool IsInverted { get; set; } = false;

    /// <summary>
    /// Gets a value that indicates whether the attribute requires validation context.
    /// </summary>
    public override bool RequiresValidationContext
    {
        get { return true; }
    }

    /// <summary>
    /// Applies formatting to an error message, based on the data field where the error occurred.
    /// </summary>
    /// <param name="name">The name to include in the formatted message.</param>
    public override string FormatErrorMessage(string name)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            base.ErrorMessageString,
            name,
            this.OtherPropertyDisplayName ?? this.OtherProperty,
            this.OtherPropertyValue,
            this.IsInverted ? "other than " : "of ");
    }

    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        var otherProperty = validationContext.ObjectType.GetProperty(OtherProperty);
        if (otherProperty == null)
        {
            return new ValidationResult(
                string.Format(CultureInfo.CurrentCulture, "Could not find a property named '{0}'.", OtherProperty));
        }

        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        // Check if this value is actually required and validate it
        if (!IsInverted && Equals(otherValue, OtherPropertyValue) ||
            IsInverted && !Equals(otherValue, OtherPropertyValue))
        {
            if (value == null)
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }

            // Additional check for strings so they're not empty
            if (value is string val && val.Trim().Length == 0)
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
        }

        return ValidationResult.Success;
    }
}