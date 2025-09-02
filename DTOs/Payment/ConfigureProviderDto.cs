namespace WebsiteBuilderAPI.DTOs.Payment
{
    public class ConfigureProviderDto
    {
        public string Provider { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        // Credenciales generales
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? WebhookSecret { get; set; }
        
        // Específicos para Azul
        public string? StoreId { get; set; }
        public string? Auth1 { get; set; }
        public string? Auth2 { get; set; }
        
        // Archivos para upload
        public IFormFile? CertificateFile { get; set; }
        public IFormFile? PrivateKeyFile { get; set; }
        
        // Configuración
        public bool IsTestMode { get; set; } = true;
        public decimal TransactionFee { get; set; } = 0;
        public bool IsActive { get; set; } = false;
    }
}