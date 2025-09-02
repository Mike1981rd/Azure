using System;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerPaymentMethodDto
    {
        public int Id { get; set; }
        public string CardType { get; set; } // Mastercard/Visa/AmericanExpress
        public string CardholderName { get; set; }
        public string Last4Digits { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string? BillingAddress { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Computed properties for UI
        [JsonIgnore]
        public string DisplayName => $"{CardType} ending in {Last4Digits}";
        
        [JsonIgnore]
        public string ExpiryDate => $"{ExpiryMonth}/{ExpiryYear}";
        
        [JsonIgnore]
        public bool IsExpired => DateTime.Now > new DateTime(int.Parse(ExpiryYear), int.Parse(ExpiryMonth), 1).AddMonths(1);
        
        [JsonIgnore]
        public string ExpiryDisplay => $"Expires {GetMonthName(ExpiryMonth)} {ExpiryYear}";
        
        private string GetMonthName(string month)
        {
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            if (int.TryParse(month, out int monthNum) && monthNum >= 1 && monthNum <= 12)
                return monthNames[monthNum - 1];
            return month;
        }
    }
}