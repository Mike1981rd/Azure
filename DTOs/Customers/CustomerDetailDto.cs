using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs.Customers
{
    public class CustomerDetailDto : CustomerDto
    {
        // Name fields separated for editing
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        // Additional personal details
        public string? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }
        
        // Billing Address fields
        public string? BillingAddress { get; set; }
        public string? BillingApartment { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        
        // Collections
        public List<CustomerAddressDto> Addresses { get; set; }
        public List<CustomerPaymentMethodDto> PaymentMethods { get; set; }
        public CustomerNotificationPreferenceDto NotificationPreferences { get; set; }
        public List<CustomerDeviceDto> RecentDevices { get; set; }
        public List<CustomerWishlistItemDto> WishlistItems { get; set; }
        public List<CustomerCouponDto> Coupons { get; set; }
        
        // Additional details for the detail view
        public string? TwoFactorPhone { get; set; }
        
        // Security settings
        public string? RecoveryEmail { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public List<CustomerSecurityQuestionDto> SecurityQuestions { get; set; }
        
        public CustomerDetailDto()
        {
            Addresses = new List<CustomerAddressDto>();
            PaymentMethods = new List<CustomerPaymentMethodDto>();
            NotificationPreferences = new CustomerNotificationPreferenceDto();
            RecentDevices = new List<CustomerDeviceDto>();
            WishlistItems = new List<CustomerWishlistItemDto>();
            Coupons = new List<CustomerCouponDto>();
            SecurityQuestions = new List<CustomerSecurityQuestionDto>();
            SessionTimeoutMinutes = 30; // Default value
        }
    }
}