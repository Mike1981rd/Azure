using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class UpdateCustomerDto
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }
        
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }
        
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string? Username { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }
        
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }
        
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string? Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        public string? Avatar { get; set; }
        
        // Update financial metrics (admin only)
        public decimal? AccountBalance { get; set; }
        public int? LoyaltyPoints { get; set; }
        
        // Security settings
        public bool? TwoFactorEnabled { get; set; }
        public string? TwoFactorPhone { get; set; }
        
        // Additional optional fields
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }
        public string? LoyaltyTier { get; set; }
    }
}