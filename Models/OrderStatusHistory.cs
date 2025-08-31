using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string StatusType { get; set; } = string.Empty;
        // OrderPlaced, PickupScheduled, Dispatched, PackageArrived, DispatchedForDelivery, Delivered
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; } // "Amazon facility, NY"
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public DateTime? EstimatedDate { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        // Usuario que realizó el cambio (opcional)
        public int? UserId { get; set; }
        
        // Metadata adicional en JSON
        public string? Metadata { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Order Order { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}