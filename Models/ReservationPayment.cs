using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class ReservationPayment
    {
        public int Id { get; set; }
        
        [Required]
        public int ReservationId { get; set; }
        
        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Card"; // Card, Cash, Transfer, Other
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        
        [Required]
        public DateTime PaymentDate { get; set; }
        
        [StringLength(100)]
        public string? TransactionId { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Metadatos
        public DateTime CreatedAt { get; set; }
        public int? ProcessedByUserId { get; set; }
        
        // Navegación
        public virtual Reservation Reservation { get; set; } = null!;
        public virtual User? ProcessedByUser { get; set; }
        
        public ReservationPayment()
        {
            Status = "Pending";
            PaymentDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }
    }
}