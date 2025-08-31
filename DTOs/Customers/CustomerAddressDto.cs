using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerAddressDto
    {
        public int Id { get; set; }
        public string Type { get; set; } // Home/Office/Family/Other
        public string Label { get; set; } // "23 Shatinon Mekalan"
        public string Street { get; set; }
        public string? Apartment { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string Country { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Helper property for display
        [JsonIgnore]
        public string FullAddress => GetFullAddress();
        
        private string GetFullAddress()
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(Street))
                parts.Add(Street);
            if (!string.IsNullOrEmpty(Apartment))
                parts.Add(Apartment);
            if (!string.IsNullOrEmpty(City))
                parts.Add(City);
            if (!string.IsNullOrEmpty(State))
                parts.Add(State);
            if (!string.IsNullOrEmpty(Country))
                parts.Add(Country);
            if (!string.IsNullOrEmpty(PostalCode))
                parts.Add(PostalCode);
            
            return string.Join(", ", parts);
        }
    }
}