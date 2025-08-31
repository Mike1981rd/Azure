using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerPaymentMethod
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CardType { get; set; } // Mastercard/Visa/AmericanExpress
        
        [Required]
        [StringLength(100)]
        public string CardholderName { get; set; }
        
        [Required]
        [StringLength(4)]
        public string Last4Digits { get; set; }
        
        [Required]
        [StringLength(2)]
        public string ExpiryMonth { get; set; }
        
        [Required]
        [StringLength(4)]
        public string ExpiryYear { get; set; }
        
        [StringLength(255)]
        public string? BillingAddress { get; set; }
        
        public bool IsPrimary { get; set; }
        
        // Encrypted token for payment processing (never expose)
        public string? TokenizedCard { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        
        public CustomerPaymentMethod()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Helper property for display
        public string DisplayName => $"{CardType} ending in {Last4Digits}";
        public string ExpiryDate => $"{ExpiryMonth}/{ExpiryYear}";
        public bool IsExpired => DateTime.Now > new DateTime(int.Parse(ExpiryYear), int.Parse(ExpiryMonth), 1).AddMonths(1);
    }
}