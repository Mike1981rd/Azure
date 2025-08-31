using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public int CompanyId { get; set; }
        public int CustomerId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        // Estados principales
        [MaxLength(20)]
        public string OrderStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Cancelled
        
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Cancelled, Refunded
        
        [MaxLength(20)]
        public string DeliveryStatus { get; set; } = "Pending"; // Pending, ReadyToPickup, Dispatched, OutForDelivery, Delivered
        
        // Información de pago
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Mastercard, Visa, PayPal, Cash
        
        [MaxLength(4)]
        public string? PaymentMethodLast4 { get; set; }
        
        [MaxLength(100)]
        public string? PaymentMethodIcon { get; set; }
        
        // País para la bandera en la UI
        [MaxLength(2)]
        public string? CountryCode { get; set; }
        
        // Montos
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingAmount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";
        
        // Snapshots del cliente al momento de la orden
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? CustomerPhone { get; set; }
        
        [MaxLength(255)]
        public string? CustomerAvatar { get; set; }
        
        // Direcciones
        public int? ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
        
        // Snapshots de direcciones en JSON
        public string? ShippingAddressSnapshot { get; set; }
        public string? BillingAddressSnapshot { get; set; }
        
        // Información de envío
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
        
        [MaxLength(255)]
        public string? TrackingUrl { get; set; }
        
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        
        // Información adicional
        public string? Notes { get; set; }
        
        [MaxLength(45)]
        public string? CustomerIP { get; set; }
        
        [MaxLength(20)]
        public string? OrderSource { get; set; } // Web, Mobile, POS
        
        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        
        // Propiedades de control
        public bool CanDelete { get; set; } = true;
        public bool CanEdit { get; set; } = true;
        public bool IsExportable { get; set; } = true;
        
        // Navegación
        public virtual Company Company { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual CustomerAddress? ShippingAddress { get; set; }
        public virtual CustomerAddress? BillingAddress { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        
        // Colecciones
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();
        public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
        
        // Propiedades calculadas
        [NotMapped]
        public int ItemCount => OrderItems?.Sum(x => x.Quantity) ?? 0;
        
        [NotMapped]
        public bool IsPaid => PaymentStatus == "Paid";
        
        [NotMapped]
        public bool IsShipped => DeliveryStatus == "Dispatched" || DeliveryStatus == "OutForDelivery" || DeliveryStatus == "Delivered";
        
        [NotMapped]
        public bool CanCancel => OrderStatus != "Completed" && OrderStatus != "Cancelled" && PaymentStatus != "Refunded";
        
        [NotMapped]
        public bool CanRefund => PaymentStatus == "Paid" && (DateTime.UtcNow - OrderDate).TotalDays <= 30;
        
        [NotMapped]
        public string FormattedOrderNumber => $"#{OrderNumber}";
    }
}