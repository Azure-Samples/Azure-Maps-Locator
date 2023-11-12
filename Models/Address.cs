using System.ComponentModel.DataAnnotations;

namespace StoreLocator.Models;

public class Address
{
    public Address()
    {
        CountryCode = "";
    }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(64, ErrorMessage = "Address cannot exceed 64 characters.")]
    public string StreetAddressLine1 { get; set; }
    public string StreetAddressLine2 { get; set; }
    public string StreetAddressLine3 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(64, ErrorMessage = "City cannot exceed 64 characters.")]
    public string City { get; set; }

    [StringLength(16, ErrorMessage = "Postcode cannot exceed 16 characters.")]
    public string PostalCode { get; set; }

    [Required(ErrorMessage = "Country name is required.")]
    [StringLength(64, ErrorMessage = "Country name cannot exceed 64 characters.")]
    public string CountryName { get; set; }

    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, ErrorMessage = "Country code cannot exceed 2 characters.")]
    public string CountryCode { get; set; }

    [StringLength(2, ErrorMessage = "Country Subdivision code cannot exceed 2 characters.")]
    public string CountrySubdivisionCode { get; set; }
}
