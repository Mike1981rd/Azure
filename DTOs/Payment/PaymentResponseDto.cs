namespace WebsiteBuilderAPI.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? AuthorizationCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? CardLastFour { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
}