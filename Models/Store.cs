using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Store
    {
        [Required(ErrorMessage = "Id is required.")]
        [MaxLength(64, ErrorMessage = "Id cannot exceed 64 characters.")]
        [IdValidation]
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(64, ErrorMessage = "Name cannot exceed 64 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(64, ErrorMessage = "Address cannot exceed 64 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(64, ErrorMessage = "City cannot exceed 64 characters.")]
        public string City { get; set; }

        [StringLength(16, ErrorMessage = "PostalCode cannot exceed 16 characters.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(64, ErrorMessage = "Country cannot exceed 64 characters.")]
        public string Country { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(128, ErrorMessage = "Email cannot exceed 128 characters.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        [StringLength(16, ErrorMessage = "Phone cannot exceed 16 characters.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public Location Location { get; set; }

        [Url(ErrorMessage = "Invalid Image URL.")]
        [StringLength(1024, ErrorMessage = "Image Url cannot exceed 1024 characters.")]
        public string ImageUrl { get; set; }

        [Url(ErrorMessage = "Invalid Website URL.")]
        [StringLength(1024, ErrorMessage = "Website URL cannot exceed 1024 characters.")]
        public string WebUrl { get; set; }

        [Required(ErrorMessage = "Opening Hours are required.")]
        public List<OpeningHour> OpeningHours { get; set; }

        public List<string> Tags { get; set; }

        public string Note { get; set; }
    }
}