namespace WebsiteBuilderAPI.DTOs.Payment
{
    public class ProcessPaymentDto
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardExpiry { get; set; } = string.Empty; // MM/YY
        public string CardCVC { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ReturnUrl { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}