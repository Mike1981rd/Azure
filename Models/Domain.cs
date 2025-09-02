using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class Domain
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign key para Company (single-tenant)
        [Required]
        public int CompanyId { get; set; }
        
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
        
        // Información básica del dominio
        [Required]
        [StringLength(255)]
        [RegularExpression(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", 
            ErrorMessage = "El formato del dominio no es válido")]
        public string DomainName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? SubDomain { get; set; }
        
        // Estado del dominio
        [Required]
        public DomainStatus Status { get; set; } = DomainStatus.Pending;
        
        public bool IsActive { get; set; } = false;
        
        public bool IsPrimary { get; set; } = false;
        
        // SSL Certificate
        public bool HasSsl { get; set; } = false;
        
        public DateTime? SslExpiryDate { get; set; }
        
        [StringLength(100)]
        public string? SslProvider { get; set; }
        
        // Verificación
        public bool IsVerified { get; set; } = false;
        
        [StringLength(255)]
        public string? VerificationToken { get; set; }
        
        public DateTime? VerifiedAt { get; set; }
        
        // Información adicional
        [StringLength(100)]
        public string? Provider { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
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
        
        [StringLength(100)]
        public string? DeletedBy { get; set; }
        
        // Navegación
        public virtual ICollection<DnsRecord> DnsRecords { get; set; } = new List<DnsRecord>();
    }
    
    // Enum para el estado del dominio
    public enum DomainStatus
    {
        [Display(Name = "Pendiente")]
        Pending = 0,
        
        [Display(Name = "Activo")]
        Active = 1,
        
        [Display(Name = "Inactivo")]
        Inactive = 2,
        
        [Display(Name = "Suspendido")]
        Suspended = 3,
        
        [Display(Name = "Expirado")]
        Expired = 4,
        
        [Display(Name = "Transfiriendo")]
        Transferring = 5,
        
        [Display(Name = "Fallido")]
        Failed = 6
    }
}