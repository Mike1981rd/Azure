using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.PaymentProvider
{
    public class CreatePaymentProviderRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Provider { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Logo { get; set; }

        public bool IsActive { get; set; } = false;
        public bool IsManual { get; set; } = false;

        [Range(0, 100)]
        public decimal TransactionFee { get; set; } = 0;

        public bool IsTestMode { get; set; } = true;

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