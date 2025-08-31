using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class AddPaymentMethodDto
    {
        [Required(ErrorMessage = "Card type is required")]
        [StringLength(50, ErrorMessage = "Card type cannot exceed 50 characters")]
        public string CardType { get; set; } // Mastercard/Visa/AmericanExpress
        
        [Required(ErrorMessage = "Cardholder name is required")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; }
        
        [Required(ErrorMessage = "Card number is required")]
        [CreditCard(ErrorMessage = "Invalid card number")]
        public string CardNumber { get; set; } // Will be tokenized, only last 4 saved
        
        [Required(ErrorMessage = "Expiry month is required")]
        [RegularExpression("^(0[1-9]|1[0-2])$", ErrorMessage = "Invalid month format (MM)")]
        public string ExpiryMonth { get; set; }
        
        [Required(ErrorMessage = "Expiry year is required")]
        [RegularExpression("^20[0-9]{2}$", ErrorMessage = "Invalid year format (YYYY)")]
        public string ExpiryYear { get; set; }
        
        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression("^[0-9]{3,4}$", ErrorMessage = "Invalid CVV")]
        public string Cvv { get; set; } // Will not be stored
        
        [StringLength(255, ErrorMessage = "Billing address cannot exceed 255 characters")]
        public string? BillingAddress { get; set; }
        
        public bool IsPrimary { get; set; }
    }
}