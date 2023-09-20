using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Store
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        [Required]
        [StringLength(64)]
        public string Address { get; set; }

        [Required]
        [StringLength(64)]
        public string City { get; set; }

        [StringLength(16)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(64)]
        public string Country { get; set; }

        [EmailAddress]
        [StringLength(128)]
        public string Email { get; set; }

        [Phone]
        [StringLength(16)]
        public string Phone { get; set; }

        [Required]
        public Location Location { get; set; }

        [Url]
        [StringLength(1024)]
        public string ImageUrl { get; set; }

        [Url]
        [StringLength(1024)]
        public string WebUrl { get; set; }

        [Required]
        public List<OpeningHour> OpeningHours { get; set; }

        public List<string> Tags { get; set; }

        public string Note { get; set; }
    }
}