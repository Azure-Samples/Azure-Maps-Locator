using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Location
    {
        [Required]
        public string Type { get; set; }

        [Required]
        [MaxLength(2)]
        public List<double> Coordinates { get; set; }
    }
}
