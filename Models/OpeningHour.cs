using System.ComponentModel.DataAnnotations;

namespace Locator.Models
{
    public class OpeningHour
    {
        [Required]
        public string DayOfWeek { get; set; }

        public string Open { get; set; } // 24h format

        public string Close { get; set; } // 24h format
    }
}