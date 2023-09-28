using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Location
    {
        public Location()
        {
            Type = "Point";
        }

        public string Type { get; private set; }

        [Required(ErrorMessage = "Coordinates are required.")]
        [CoordinateValidation]
        public List<double> Coordinates { get; set; }
    }
}