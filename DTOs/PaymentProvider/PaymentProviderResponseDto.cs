namespace WebsiteBuilderAPI.DTOs.PaymentProvider
{
    public class PaymentProviderResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public bool IsActive { get; set; }
        public bool IsManual { get; set; }
        public decimal TransactionFee { get; set; }
        public bool IsTestMode { get; set; }

        // Campos no sensibles que se pueden mostrar
        public string? StoreId { get; set; }
        public bool HasCertificate { get; set; }
        public bool HasPrivateKey { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}