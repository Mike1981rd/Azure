using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebsiteBuilderAPI.DTOs.PaymentProvider
{
    public class ConfigureProviderFormDto
    {
        [Required]
        public string Provider { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public bool IsTestMode { get; set; }
        
        public decimal TransactionFee { get; set; }
        
        public string? StoreId { get; set; }
        public string? Auth1 { get; set; }
        public string? Auth2 { get; set; }
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? WebhookSecret { get; set; }
        
        public IFormFile? CertificateFile { get; set; }
        public IFormFile? PrivateKeyFile { get; set; }
    }
}