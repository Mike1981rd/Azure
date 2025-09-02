using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Checkout
{
    public class ProcessRoomReservationDto
    {
        // ========== Customer Information ==========
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; }

        // ========== Account Creation Options ==========
        public bool CreateAccount { get; set; } = false;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string? Password { get; set; } // Only required if CreateAccount is true

        public bool SubscribeToNewsletter { get; set; } = false;

        // ========== Billing Address (Optional based on settings) ==========
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "Apartment info cannot exceed 100 characters")]
        public string? Apartment { get; set; }

        // Indicates whether user provided a different billing address than contact
        public bool BillingDifferent { get; set; } = false;

        // ========== Contact Address (Primary address used for contact/shipping) ==========
        [StringLength(255, ErrorMessage = "Contact address cannot exceed 255 characters")]
        public string? ContactAddress { get; set; }

        [StringLength(100, ErrorMessage = "Contact city cannot exceed 100 characters")]
        public string? ContactCity { get; set; }

        [StringLength(100, ErrorMessage = "Contact state cannot exceed 100 characters")]
        public string? ContactState { get; set; }

        [StringLength(20, ErrorMessage = "Contact postal code cannot exceed 20 characters")]
        public string? ContactPostalCode { get; set; }

        [StringLength(100, ErrorMessage = "Contact apartment info cannot exceed 100 characters")]
        public string? ContactApartment { get; set; }

        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string? CompanyName { get; set; }

        // ========== Tax Document Type (for Latin America) ==========
        [StringLength(50, ErrorMessage = "Tax document type cannot exceed 50 characters")]
        public string TaxDocumentType { get; set; } = "consumidor_final";

        [StringLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string? TaxId { get; set; }

        // ========== Reservation Details ==========
        [Required(ErrorMessage = "Room ID is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }

        [StringLength(1000, ErrorMessage = "Special requests cannot exceed 1000 characters")]
        public string? SpecialRequests { get; set; }

        [StringLength(50, ErrorMessage = "Arrival time cannot exceed 50 characters")]
        public string? ArrivalTime { get; set; } // morning, afternoon, evening, late

        // ========== Pricing Information ==========
        [Required(ErrorMessage = "Room rate is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Room rate must be between 0.01 and 999999.99")]
        public decimal RoomRate { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Total amount must be between 0.01 and 999999.99")]
        public decimal TotalAmount { get; set; }

        public decimal? CleaningFee { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? Taxes { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string Currency { get; set; } = "USD";

        // ========== Payment Information ==========
        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string PaymentMethod { get; set; } = "credit_card"; // credit_card, debit_card, paypal, etc.

        // Card details - should be encrypted in transit
        [Required(ErrorMessage = "Card number is required")]
        [CreditCard(ErrorMessage = "Invalid card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiry date must be in MM/YY format")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Cardholder name is required")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; }

        // ========== Additional Options ==========
        public bool AcceptTermsAndConditions { get; set; } = true;

        [StringLength(50, ErrorMessage = "Discount code cannot exceed 50 characters")]
        public string? DiscountCode { get; set; }

        // ========== Metadata ==========
        public string? SessionId { get; set; } // For tracking purposes
        public string? IpAddress { get; set; } // For fraud detection
        public string? UserAgent { get; set; } // Browser info
    }

    public class ProcessRoomReservationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ReservationId { get; set; }
        public int? CustomerId { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public bool AccountCreated { get; set; }
        public bool PasswordEmailSent { get; set; }
        public ProcessReservationErrorDto? Error { get; set; }
    }

    public class ProcessReservationErrorDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Field { get; set; }
    }
}