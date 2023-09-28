using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class OpeningHour
    {
        [Required(ErrorMessage = "Day of the week is required.")]
        public string DayOfWeek { get; set; }

        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Open time must be in 24-hour format (HH:mm).")]
        public string Open { get; set; }

        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Close time must be in 24-hour format (HH:mm).")]
        public string Close { get; set; }
    }
}