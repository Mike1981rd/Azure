using System;
using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Company
{
    public class CurrencySettingsDto
    {
        public string CurrencyBase { get; set; } = "USD"; // ISO code
        public List<string> EnabledCurrencies { get; set; } = new List<string>();
        public Dictionary<string, decimal> ManualRates { get; set; } = new Dictionary<string, decimal>(); // relative to base
        public DateTime? LockedUntil { get; set; }
        public Dictionary<string, int>? RoundingRule { get; set; }
        
        public CurrencySettingsDto()
        {
            EnabledCurrencies = new List<string>();
            ManualRates = new Dictionary<string, decimal>();
            RoundingRule = new Dictionary<string, int>();
        }
    }
}
