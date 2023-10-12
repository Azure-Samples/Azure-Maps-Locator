using StoreLocator.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models
{
    public class Country
    {
        [Required(ErrorMessage = "Id is required.")]
        [MaxLength(2, ErrorMessage = "Id cannot exceed 2 characters.")]
        [IdValidation]
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(128, ErrorMessage = "Name cannot exceed 128 characters.")]
        public string Name { get; set; }
    }
}
