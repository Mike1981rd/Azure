using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.PaymentProvider
{
    public class UpdatePaymentProviderRequestDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsManual { get; set; }

        [Range(0, 100)]
        public decimal? TransactionFee { get; set; }

        public bool? IsTestMode { get; set; }

        // Credenciales generales (se encriptarán)
        [StringLength(255)]
        public string? ApiKey { get; set; }

        [StringLength(255)]
        public string? SecretKey { get; set; }

        [StringLength(255)]
        public string? ClientId { get; set; }

        [StringLength(255)]
        public string? ClientSecret { get; set; }

        [StringLength(255)]
        public string? WebhookSecret { get; set; }

        // Específicos para Azul Dominicana
        [StringLength(50)]
        public string? StoreId { get; set; }

        [StringLength(255)]
        public string? Auth1 { get; set; }

        [StringLength(255)]
        public string? Auth2 { get; set; }
    }
}