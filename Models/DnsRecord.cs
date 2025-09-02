using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class DnsRecord
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign key para Domain
        [Required]
        public int DomainId { get; set; }
        
        [ForeignKey("DomainId")]
        public virtual Domain? Domain { get; set; }
        
        // Tipo de registro DNS
        [Required]
        public DnsRecordType Type { get; set; }
        
        // Host/Name (@ para root, www, mail, etc.)
        [Required]
        [StringLength(100)]
        public string Host { get; set; } = "@";
        
        // Valor del registro (IP, CNAME target, etc.)
        [Required]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;
        
        // Time To Live en segundos (300 - 86400)
        [Range(300, 86400, ErrorMessage = "TTL debe estar entre 300 y 86400 segundos")]
        public int TTL { get; set; } = 3600;
        
        // Prioridad para registros MX (0-65535)
        [Range(0, 65535, ErrorMessage = "La prioridad debe estar entre 0 y 65535")]
        public int? Priority { get; set; }
        
        // Puerto para registros SRV
        [Range(0, 65535, ErrorMessage = "El puerto debe estar entre 0 y 65535")]
        public int? Port { get; set; }
        
        // Peso para registros SRV
        [Range(0, 65535, ErrorMessage = "El peso debe estar entre 0 y 65535")]
        public int? Weight { get; set; }
        
        // Estado del registro
        public bool IsActive { get; set; } = true;
        
        // Auditoría
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }
        
        // Soft delete
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
    }
    
    // Enum para tipos de registros DNS
    public enum DnsRecordType
    {
        [Display(Name = "A Record")]
        A = 0,
        
        [Display(Name = "AAAA Record")]
        AAAA = 1,
        
        [Display(Name = "CNAME Record")]
        CNAME = 2,
        
        [Display(Name = "MX Record")]
        MX = 3,
        
        [Display(Name = "TXT Record")]
        TXT = 4,
        
        [Display(Name = "NS Record")]
        NS = 5,
        
        [Display(Name = "SOA Record")]
        SOA = 6,
        
        [Display(Name = "PTR Record")]
        PTR = 7,
        
        [Display(Name = "SRV Record")]
        SRV = 8,
        
        [Display(Name = "CAA Record")]
        CAA = 9
    }
}