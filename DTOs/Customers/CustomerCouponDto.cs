using System;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerCouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountType { get; set; }
        public decimal? MinimumPurchase { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public int? MaxUsageCount { get; set; }
        
        // Computed properties
        [JsonIgnore]
        public bool IsValid => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidUntil;
        
        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow > ValidUntil;
        
        [JsonIgnore]
        public bool CanBeUsed => IsValid && (MaxUsageCount == null || UsageCount < MaxUsageCount);
        
        [JsonIgnore]
        public string Status => GetStatus();
        
        [JsonIgnore]
        public string DiscountDisplay => GetDiscountDisplay();
        
        [JsonIgnore]
        public int DaysLeft => Math.Max(0, (ValidUntil - DateTime.UtcNow).Days);
        
        private string GetStatus()
        {
            if (IsExpired) return "Expired";
            if (!IsActive) return "Inactive";
            if (MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value) return "Used";
            if (DateTime.UtcNow < ValidFrom) return "Not Yet Valid";
            return "Active";
        }
        
        private string GetDiscountDisplay()
        {
            if (DiscountType == "Percentage")
                return $"{DiscountAmount}% OFF";
            else
                return $"${DiscountAmount} OFF";
        }
    }
}