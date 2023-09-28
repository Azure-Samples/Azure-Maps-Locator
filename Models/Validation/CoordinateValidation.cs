using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CoordinateValidation : ValidationAttribute
    {
        public CoordinateValidation()
        {
            ErrorMessage = "Invalid coordinates. Coordinates should contain exactly two elements: longitude and latitude.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is List<double> coordinates && coordinates.Count == 2)
            {
                double longitude = coordinates[0];
                double latitude = coordinates[1];

                // Check longitude and latitude ranges (modify as needed)
                if (longitude >= -180 && longitude <= 180 && latitude >= -90 && latitude <= 90)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}