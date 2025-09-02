using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs
{
    // DTO para listar reservaciones
    public class ReservationListDto
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string? GuestAvatar { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumberOfNights { get; set; }
    }

    // DTO para crear reservación
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "El cliente es requerido")]
        public int CustomerId { get; set; }
        
        [Required(ErrorMessage = "La habitación es requerida")]
        public int RoomId { get; set; }
        
        [Required(ErrorMessage = "La fecha de check-in es requerida")]
        public DateTime CheckInDate { get; set; }
        
        [Required(ErrorMessage = "La fecha de check-out es requerida")]
        public DateTime CheckOutDate { get; set; }
        
        [Required(ErrorMessage = "El número de huéspedes es requerido")]
        [Range(1, 20, ErrorMessage = "El número de huéspedes debe estar entre 1 y 20")]
        public int NumberOfGuests { get; set; }
        
        public string? SpecialRequests { get; set; }
        public string? InternalNotes { get; set; }
        
        // Facturación opcional
        public int? BillingAddressId { get; set; }
        public string? CustomBillingAddress { get; set; }
        public string? CustomBillingCity { get; set; }
        public string? CustomBillingState { get; set; }
        public string? CustomBillingPostalCode { get; set; }
        public string? CustomBillingCountry { get; set; }
        
        // Validate dates
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOutDate <= CheckInDate)
            {
                yield return new ValidationResult(
                    "La fecha de check-out debe ser posterior a la fecha de check-in",
                    new[] { nameof(CheckOutDate) });
            }
            
            if (CheckInDate.Date < DateTime.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "La fecha de check-in no puede ser en el pasado",
                    new[] { nameof(CheckInDate) });
            }
        }
    }

    // DTO para actualizar reservación
    public class UpdateReservationDto
    {
        public int? CustomerId { get; set; }
        public int? RoomId { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? Status { get; set; }
        public string? SpecialRequests { get; set; }
        public string? InternalNotes { get; set; }
        
        // Facturación
        public int? BillingAddressId { get; set; }
        public string? CustomBillingAddress { get; set; }
        public string? CustomBillingCity { get; set; }
        public string? CustomBillingState { get; set; }
        public string? CustomBillingPostalCode { get; set; }
        public string? CustomBillingCountry { get; set; }
    }

    // DTO para detalles completos de reservación
    public class ReservationDetailsDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Información del huésped
        public GuestInfoDto GuestInfo { get; set; } = new();
        
        // Información de facturación
        public BillingInfoDto BillingInfo { get; set; } = new();
        
        // Detalles de la reserva
        public ReservationInfoDto ReservationInfo { get; set; } = new();
        
        // Resumen de pago
        public PaymentSummaryDto PaymentSummary { get; set; } = new();
        
        // Pagos realizados
        public List<ReservationPaymentDto> Payments { get; set; } = new();
    }

    // Sub-DTOs para detalles
    public class GuestInfoDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class BillingInfoDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool IsDefaultAddress { get; set; }
    }

    public class ReservationInfoDto
    {
        public string RoomName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public int NumberOfNights { get; set; }
        public string? SpecialRequests { get; set; }
        public string? InternalNotes { get; set; }
    }

    public class PaymentSummaryDto
    {
        public decimal RoomRate { get; set; }
        public int NumberOfNights { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
    }

    // DTO para pagos
    public class ReservationPaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }

    // DTO para crear pago
    public class CreatePaymentDto
    {
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "El método de pago es requerido")]
        public string PaymentMethod { get; set; } = "Card";
        
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }

    // Response DTOs
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public ReservationDetailsDto? Data { get; set; }
    }
}