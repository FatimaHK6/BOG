using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BOG.DTO.Validation;

/// <summary>
/// Validates Saudi mobile number format (05XXXXXXXX).
/// </summary>
public class SaudiMobileAttribute : ValidationAttribute
{
    private static readonly Regex MobileRegex = new(@"^05\d{8}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success; // Optional field

        var mobile = value.ToString()!.Trim();

        if (!MobileRegex.IsMatch(mobile))
        {
            return new ValidationResult(
                ErrorMessage ?? "رقم الجوال يجب أن يكون 10 أرقام ويبدأ بـ 05"
            );
        }

        return ValidationResult.Success;
    }
}
