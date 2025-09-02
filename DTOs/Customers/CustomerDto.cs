using System;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } // #895280
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public string? Phone { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        
        // Financial metrics
        public decimal AccountBalance { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        
        // Loyalty program
        public int LoyaltyPoints { get; set; }
        public string LoyaltyTier { get; set; }
        
        // Security
        public bool TwoFactorEnabled { get; set; }
        
        // Counts
        public int WishlistCount { get; set; }
        public int CouponsCount { get; set; }
        
        // Dates
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Computed properties for UI
        [JsonIgnore]
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;
        
        [JsonIgnore]
        public string Initials => GetInitials();
        
        private string GetInitials()
        {
            if (string.IsNullOrEmpty(FullName))
                return Username?.Substring(0, 2).ToUpper() ?? "??";
            
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        }
    }
}