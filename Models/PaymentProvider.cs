using System;

namespace WebsiteBuilderAPI.Models
{
    public class PaymentProvider
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        // Información Básica
        public string Name { get; set; } = string.Empty;        // "PayPal", "Azul Dominicana"
        public string Provider { get; set; } = string.Empty;    // "paypal", "azul", "stripe"
        public string? Logo { get; set; }
        
        // Estado y Configuración
        public bool IsActive { get; set; } = false;
        public bool IsManual { get; set; } = false;      // Manual vs Gateway
        public decimal TransactionFee { get; set; } = 0; // 2.99
        public bool IsTestMode { get; set; } = true;     // Modo prueba
        
        // Credenciales Generales (encriptadas)
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? WebhookSecret { get; set; }
        
        // Específicos para Azul Dominicana
        public string? StoreId { get; set; }         // 39035544035
        public string? Auth1 { get; set; }           // Usuario (encriptado)
        public string? Auth2 { get; set; }           // Contraseña (encriptado)
        public string? CertificatePath { get; set; } // Ruta .pem
        public string? PrivateKeyPath { get; set; }  // Ruta .key
        
        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navegación
        public Company Company { get; set; } = null!;
    }
}