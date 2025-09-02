using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.DTOs
{
    public class CreateDomainDto
    {
        [Required(ErrorMessage = "El nombre del dominio es requerido")]
        [StringLength(255, ErrorMessage = "El nombre del dominio no puede exceder 255 caracteres")]
        [RegularExpression(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", 
            ErrorMessage = "El formato del dominio no es válido. Ejemplo: miempresa.com")]
        public string DomainName { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "El subdominio no puede exceder 100 caracteres")]
        public string? SubDomain { get; set; }
        
        public DomainStatus Status { get; set; } = DomainStatus.Pending;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsPrimary { get; set; } = false;
        
        // SSL Information
        public bool HasSsl { get; set; } = false;
        
        public DateTime? SslExpiryDate { get; set; }
        
        [StringLength(100)]
        public string? SslProvider { get; set; }
        
        // Provider Information
        [StringLength(100)]
        public string? Provider { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // DNS Records to create
        public List<CreateDnsRecordDto>? DnsRecords { get; set; }
    }
    
    public class CreateDnsRecordDto
    {
        [Required(ErrorMessage = "El tipo de registro DNS es requerido")]
        public DnsRecordType Type { get; set; }
        
        [Required(ErrorMessage = "El host es requerido")]
        [StringLength(100, ErrorMessage = "El host no puede exceder 100 caracteres")]
        public string Host { get; set; } = "@";
        
        [Required(ErrorMessage = "El valor del registro es requerido")]
        [StringLength(500, ErrorMessage = "El valor no puede exceder 500 caracteres")]
        public string Value { get; set; } = string.Empty;
        
        [Range(300, 86400, ErrorMessage = "TTL debe estar entre 300 y 86400 segundos")]
        public int TTL { get; set; } = 3600;
        
        // Para registros MX
        [Range(0, 65535, ErrorMessage = "La prioridad debe estar entre 0 y 65535")]
        public int? Priority { get; set; }
        
        // Para registros SRV
        [Range(0, 65535, ErrorMessage = "El puerto debe estar entre 0 y 65535")]
        public int? Port { get; set; }
        
        [Range(0, 65535, ErrorMessage = "El peso debe estar entre 0 y 65535")]
        public int? Weight { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}