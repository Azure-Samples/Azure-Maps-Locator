using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Feature
    {
        [Required(ErrorMessage = "Id is required.")]
        [MaxLength(64, ErrorMessage = "Id cannot exceed 64 characters.")]
        [IdValidation]
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(128, ErrorMessage = "Name cannot exceed 128 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Tags are required.")]
        public List<Tag> Tags { get; set; }
    }
}
