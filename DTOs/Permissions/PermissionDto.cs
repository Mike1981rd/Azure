using System.Text.Json.Serialization;

namespace WebsiteBuilderAPI.DTOs.Permissions
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string Code => $"{Resource}.{Action}";
    }
}