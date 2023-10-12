using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Store
    {
        public Store()
        {
            Id = Guid.NewGuid().ToString();
            Address = new Address();
            Location = new Location();
            OpeningHours = new List<OpeningHour>();
            Features = new List<string>();
        }

        [Required(ErrorMessage = "Id is required.")]
        [MaxLength(64, ErrorMessage = "Id cannot exceed 64 characters.")]
        [IdValidation]
        public string Id { get; set; }

        [MaxLength(64, ErrorMessage = "Store Number cannot exceed 64 characters.")]
        public string StoreNumber { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(64, ErrorMessage = "Name cannot exceed 64 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public Address Address { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(128, ErrorMessage = "Email cannot exceed 128 characters.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        [StringLength(16, ErrorMessage = "Phone cannot exceed 16 characters.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public Location Location { get; set; }

        [Url(ErrorMessage = "Invalid Image URL.")]
        [StringLength(1024, ErrorMessage = "Image Url cannot exceed 1024 characters.")]
        public string ImageUrl { get; set; }

        [Url(ErrorMessage = "Invalid Website URL.")]
        [StringLength(1024, ErrorMessage = "Website URL cannot exceed 1024 characters.")]
        public string WebUrl { get; set; }

        [Required(ErrorMessage = "Time Zone UTC is required.")]
        public int TimeZoneUtc { get; set; }

        [Required(ErrorMessage = "Opening Hours are required.")]
        public List<OpeningHour> OpeningHours { get; set; }

        public List<string> Features { get; set; }

        public string Note { get; set; }
    }
}