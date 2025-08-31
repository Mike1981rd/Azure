using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.NewsletterSubscribers
{
    public class ConvertToCustomerDto
    {
        [Required(ErrorMessage = "First name is required when converting to customer")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required when converting to customer")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; }
        
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; }
        
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }
        
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; }
        
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string Gender { get; set; }
        
        // Password for new customer account
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }
}