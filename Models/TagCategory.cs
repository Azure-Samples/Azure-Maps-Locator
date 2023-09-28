using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class TagCategory
    {
        [Required(ErrorMessage = "Id is required.")]
        [MaxLength(64, ErrorMessage = "Id cannot exceed 64 characters.")]
        [IdValidation]
        public string Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(128, ErrorMessage = "Title cannot exceed 128 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Tags are required.")]
        public List<Tag> Tags { get; set; }
    }
}
