using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class OrderPayment
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int? PaymentProviderId { get; set; }
        public int? CustomerPaymentMethodId { get; set; }
        
        [MaxLength(100)]
        public string? TransactionId { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";
        
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded, PartialRefund
        
        [MaxLength(50)]
        public string? CardType { get; set; } // Mastercard, Visa, Amex
        
        [MaxLength(4)]
        public string? CardLast4 { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        public DateTime? RefundDate { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? RefundAmount { get; set; }
        
        [MaxLength(100)]
        public string? RefundReason { get; set; }
        
        // Respuesta del gateway de pago en JSON
        public string? GatewayResponse { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Para pagos parciales o múltiples
        public bool IsPartialPayment { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Order Order { get; set; } = null!;
        public virtual PaymentProvider? PaymentProvider { get; set; }
        public virtual CustomerPaymentMethod? CustomerPaymentMethod { get; set; }
    }
}