using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class IdValidation : ValidationAttribute
{
    private readonly char[] RestrictedCharacters = { '/', '\\', '?', '#', ' ' };

    public IdValidation()
    {
        ErrorMessage = "Id cannot contain spaces or the characters '/', '\\', '?' and '#'.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string id && id.Intersect(RestrictedCharacters).Any())
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}