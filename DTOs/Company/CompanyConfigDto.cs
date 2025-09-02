namespace WebsiteBuilderAPI.DTOs.Company
{
    public class CompanyConfigDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int LogoSize { get; set; } = 120;
        public string PrimaryColor { get; set; } = "#22c55e";
        public string SecondaryColor { get; set; } = "#64748b";
        public string Currency { get; set; } = "USD";
        public string? TimeZone { get; set; }
        public string OrderIdPrefix { get; set; } = "#";
        public string OrderIdSuffix { get; set; } = "";
    }
}