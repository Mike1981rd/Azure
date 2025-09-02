using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class AddAddressDto
    {
        [Required(ErrorMessage = "Address type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } // Home/Office/Family/Other
        
        [Required(ErrorMessage = "Label is required")]
        [StringLength(100, ErrorMessage = "Label cannot exceed 100 characters")]
        public string Label { get; set; }
        
        [Required(ErrorMessage = "Street address is required")]
        [StringLength(255, ErrorMessage = "Street cannot exceed 255 characters")]
        public string Street { get; set; }
        
        [StringLength(100, ErrorMessage = "Apartment cannot exceed 100 characters")]
        public string? Apartment { get; set; }
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }
        
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; }
        
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }
        
        public bool IsDefault { get; set; }
    }
}