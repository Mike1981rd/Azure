using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(10)]
        public string CustomerId { get; set; } // Format: #895280
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [StringLength(500)]
        public string? Avatar { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Country { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active/Inactive/Pending
        
        // Additional profile fields
        public DateTime? BirthDate { get; set; }
        
        [StringLength(20)]
        public string? Gender { get; set; } // Male/Female/Other/PreferNotToSay
        
        [StringLength(100)]
        public string? CompanyName { get; set; }
        
        [StringLength(50)]
        public string? TaxId { get; set; }
        
        // Billing Address Fields
        [StringLength(255)]
        public string? BillingAddress { get; set; }
        
        [StringLength(100)]
        public string? BillingApartment { get; set; }
        
        [StringLength(100)]
        public string? BillingCity { get; set; }
        
        [StringLength(100)]
        public string? BillingState { get; set; }
        
        [StringLength(20)]
        public string? BillingPostalCode { get; set; }
        
        [StringLength(100)]
        public string? BillingCountry { get; set; }
        
        // Preferences
        [StringLength(50)]
        public string PreferredLanguage { get; set; } = "English";
        
        [StringLength(10)]
        public string PreferredCurrency { get; set; } = "USD";
        
        // Financial metrics
        public decimal AccountBalance { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        
        // Loyalty program
        public int LoyaltyPoints { get; set; }
        
        [StringLength(20)]
        public string LoyaltyTier { get; set; } = "Silver"; // Platinum/Gold/Silver
        
        // Security
        public bool TwoFactorEnabled { get; set; }
        
        [StringLength(20)]
        public string? TwoFactorPhone { get; set; }
        
        public string? TwoFactorSecret { get; set; }
        
        [EmailAddress]
        [StringLength(255)]
        public string? RecoveryEmail { get; set; }
        
        public int SessionTimeoutMinutes { get; set; } = 30;
        
        public bool ForcePasswordChange { get; set; } = false;
        
        // Wishlist and coupons count (computed)
        public int WishlistCount { get; set; }
        public int CouponsCount { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Soft delete
        
        // Navigation properties
        public virtual Company Company { get; set; }
        public virtual ICollection<CustomerAddress> Addresses { get; set; }
        public virtual ICollection<CustomerPaymentMethod> PaymentMethods { get; set; }
        public virtual CustomerNotificationPreference NotificationPreference { get; set; }
        public virtual ICollection<CustomerDevice> Devices { get; set; }
        public virtual ICollection<CustomerWishlistItem> WishlistItems { get; set; }
        public virtual ICollection<CustomerCoupon> Coupons { get; set; }
        public virtual ICollection<CustomerSecurityQuestion> SecurityQuestions { get; set; }
        
        public Customer()
        {
            Addresses = new HashSet<CustomerAddress>();
            PaymentMethods = new HashSet<CustomerPaymentMethod>();
            Devices = new HashSet<CustomerDevice>();
            WishlistItems = new HashSet<CustomerWishlistItem>();
            Coupons = new HashSet<CustomerCoupon>();
            SecurityQuestions = new HashSet<CustomerSecurityQuestion>();
            Status = "Active";
            LoyaltyTier = "Silver";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";
        
        // Method to update loyalty tier based on points
        public void UpdateLoyaltyTier()
        {
            if (LoyaltyPoints >= 3000)
                LoyaltyTier = "Platinum";
            else if (LoyaltyPoints >= 1000)
                LoyaltyTier = "Gold";
            else
                LoyaltyTier = "Silver";
        }
    }
}