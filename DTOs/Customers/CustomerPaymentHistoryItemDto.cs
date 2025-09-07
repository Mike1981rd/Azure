using System;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerPaymentHistoryItemDto
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
    }
}

