namespace WebsiteBuilderAPI.DTOs.PaymentProvider
{
    public class AvailableProviderDto
    {
        public string Provider { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public decimal TransactionFee { get; set; }
        public bool IsManual { get; set; }
        public string? Description { get; set; }
    }
}