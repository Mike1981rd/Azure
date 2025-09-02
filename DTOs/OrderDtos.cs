using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs
{
    // DTO para crear una orden
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
        
        public int? ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
        
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }
        
        [MaxLength(20)]
        public string? OrderSource { get; set; } = "Web";
        
        public string? Notes { get; set; }
        
        public decimal? DiscountAmount { get; set; }
        
        [MaxLength(100)]
        public string? CouponCode { get; set; }
    }
    
    // DTO para items de la orden al crear
    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }
        
        public int? ProductVariantId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        public string? Notes { get; set; }
    }
    
    // DTO para actualizar una orden
    public class UpdateOrderDto
    {
        [MaxLength(20)]
        public string? OrderStatus { get; set; }
        
        [MaxLength(20)]
        public string? PaymentStatus { get; set; }
        
        [MaxLength(20)]
        public string? DeliveryStatus { get; set; }
        
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
        
        [MaxLength(255)]
        public string? TrackingUrl { get; set; }
        
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        
        public string? Notes { get; set; }
        
        public int? ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
    }
    
    // DTO de respuesta para una orden
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string FormattedOrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerAvatar { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? PaymentMethodLast4 { get; set; }
        public string? CountryCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? TrackingUrl { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public AddressDto? BillingAddress { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
        public List<OrderPaymentResponseDto> Payments { get; set; } = new List<OrderPaymentResponseDto>();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
        public int ItemCount { get; set; }
        public bool CanCancel { get; set; }
        public bool CanRefund { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    // DTO para items de la orden en respuesta
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? ProductSku { get; set; }
        public string? ProductAttributes { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsSelected { get; set; }
        public string? Notes { get; set; }
    }
    
    // DTO para historial de estados
    public class OrderStatusHistoryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EstimatedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string? UserName { get; set; }
    }
    
    // DTO para pagos
    public class OrderPaymentResponseDto
    {
        public int Id { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CardType { get; set; }
        public string? CardLast4 { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? RefundDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
    }
    
    // DTO para direcciones
    public class AddressDto
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
    
    // DTO para filtros de búsqueda
    public class OrderFilterDto
    {
        public string? Search { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? DeliveryStatus { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    
    // DTO para métricas del dashboard
    public class OrderMetricsDto
    {
        public int PendingPayment { get; set; }
        public int Completed { get; set; }
        public int Refunded { get; set; }
        public int Failed { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TodayOrders { get; set; }
        public int ThisMonthOrders { get; set; }
    }
    
    // DTO para actualizar estado de pago
    public class UpdatePaymentStatusDto
    {
        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? TransactionId { get; set; }
        
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }
        
        [MaxLength(4)]
        public string? CardLast4 { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    
    // DTO para actualizar estado de envío
    public class UpdateShippingStatusDto
    {
        [Required]
        [MaxLength(20)]
        public string DeliveryStatus { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
        
        [MaxLength(255)]
        public string? TrackingUrl { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    
    // DTO para procesar reembolso
    public class ProcessRefundDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal RefundAmount { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string RefundReason { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public bool NotifyCustomer { get; set; } = true;
    }
}