namespace WebsiteBuilderAPI.DTOs.ConfigOptions
{
    public class ConfigOptionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string LabelEs { get; set; } = string.Empty;
        public string LabelEn { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? IconType { get; set; }
        public string? Category { get; set; }
        public int SortOrder { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsCustom { get; set; }
        public bool IsDefault { get; set; }
    }
}