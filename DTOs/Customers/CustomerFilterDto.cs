using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerFilterDto
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public string? Search { get; set; }
        public string? Status { get; set; } = "Active";
        public string? Country { get; set; }
        public string? LoyaltyTier { get; set; }
        public decimal? MinSpent { get; set; }
        public decimal? MaxSpent { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        
        // Helper properties
        [JsonIgnore]
        public int Skip => (Page - 1) * Size;
        
        [JsonIgnore]
        public int Take => Size;
    }
}