using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        // Fechas y horarios
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
        
        public TimeSpan CheckInTime { get; set; } = new TimeSpan(15, 0, 0); // 3:00 PM default
        public TimeSpan CheckOutTime { get; set; } = new TimeSpan(12, 0, 0); // 12:00 PM default
        
        // Detalles de la reserva
        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
        
        // Dirección de facturación (opcional, si es diferente)
        public int? BillingAddressId { get; set; }
        
        // Campos personalizados de facturación (si no usa una dirección guardada)
        [StringLength(255)]
        public string? CustomBillingAddress { get; set; }
        
        [StringLength(100)]
        public string? CustomBillingCity { get; set; }
        
        [StringLength(100)]
        public string? CustomBillingState { get; set; }
        
        [StringLength(20)]
        public string? CustomBillingPostalCode { get; set; }
        
        [StringLength(100)]
        public string? CustomBillingCountry { get; set; }
        
        // Precios
        [Required]
        [Range(0, 999999.99)]
        public decimal RoomRate { get; set; } // Precio por noche
        
        [Required]
        [Range(0, 999999.99)]
        public decimal TotalAmount { get; set; }
        
        public int NumberOfNights { get; set; } // Calculado automáticamente
        
        // Adicionales
        [StringLength(1000)]
        public string? SpecialRequests { get; set; }
        
        [StringLength(1000)]
        public string? InternalNotes { get; set; }
        
        // Metadatos
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        
        // Navegación
        public virtual Company Company { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual CustomerAddress? BillingAddress { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual ICollection<ReservationPayment> Payments { get; set; }
        
        public Reservation()
        {
            Payments = new HashSet<ReservationPayment>();
            Status = "Pending";
            CreatedAt = DateTime.UtcNow;
            CheckInTime = new TimeSpan(15, 0, 0);
            CheckOutTime = new TimeSpan(12, 0, 0);
        }
        
        // Método para calcular noches automáticamente
        public void CalculateNights()
        {
            if (CheckOutDate > CheckInDate)
            {
                NumberOfNights = (CheckOutDate - CheckInDate).Days;
            }
        }
        
        // Método para calcular total
        public void CalculateTotal()
        {
            CalculateNights();
            TotalAmount = RoomRate * NumberOfNights;
        }
    }
}