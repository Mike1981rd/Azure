using System;
using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerDeviceDto
    {
        public int Id { get; set; }
        public string Browser { get; set; }
        public string DeviceType { get; set; }
        public string DeviceName { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime FirstSeen { get; set; }
        public bool IsTrusted { get; set; }
        public bool IsActive { get; set; }
        
        // Display properties
        [JsonIgnore]
        public string ActivityDisplay => GetActivityDisplay();
        
        [JsonIgnore]
        public string DeviceDisplay => $"{DeviceName} - {OperatingSystem}";
        
        [JsonIgnore]
        public string BrowserDisplay => GetBrowserWithIcon();
        
        private string GetActivityDisplay()
        {
            var timeDiff = DateTime.UtcNow - LastActivity;
            
            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes} minutes ago";
            if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours} hours ago";
            if (timeDiff.TotalDays < 30)
                return $"{(int)timeDiff.TotalDays} days ago";
            
            return LastActivity.ToString("dd MMM yyyy HH:mm");
        }
        
        private string GetBrowserWithIcon()
        {
            // This can be enhanced with actual icon mapping on frontend
            return $"{Browser} on {OperatingSystem}";
        }
    }
}