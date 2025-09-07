using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Customers;

namespace WebsiteBuilderAPI.Services
{
    public interface ICustomerService
    {
        // Main CRUD operations
        Task<PagedResult<CustomerDto>> GetCustomersAsync(int companyId, CustomerFilterDto filter);
        Task<CustomerDetailDto> GetCustomerByIdAsync(int companyId, int id);
        Task<CustomerDto> CreateCustomerAsync(int companyId, CreateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int companyId, int id, UpdateCustomerDto dto);
        Task DeleteCustomerAsync(int companyId, int id);
        Task<CustomerDto> UpdateAvatarAsync(int companyId, int id, string avatarUrl);
        
        // Authentication
        Task<bool> EnableTwoFactorAsync(int companyId, int id, string phoneNumber);
        Task<bool> DisableTwoFactorAsync(int companyId, int id);
        
        // Address management
        Task<List<CustomerAddressDto>> GetAddressesAsync(int customerId);
        Task<CustomerAddressDto> AddAddressAsync(int customerId, AddAddressDto dto);
        Task<CustomerAddressDto> UpdateAddressAsync(int customerId, int addressId, AddAddressDto dto);
        Task DeleteAddressAsync(int customerId, int addressId);
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);
        
        // Payment methods
        Task<List<CustomerPaymentMethodDto>> GetPaymentMethodsAsync(int customerId);
        Task<CustomerPaymentMethodDto> AddPaymentMethodAsync(int customerId, AddPaymentMethodDto dto);
        Task DeletePaymentMethodAsync(int customerId, int methodId);
        Task<bool> SetPrimaryPaymentMethodAsync(int customerId, int methodId);
        
        // Notification preferences
        Task<List<CustomerNotificationPreferenceDto>> GetNotificationPreferencesAsync(int customerId);
        Task UpdateNotificationPreferencesAsync(int customerId, UpdateNotificationPreferencesDto dto);
        
        // Device management
        Task<List<CustomerDeviceDto>> GetDevicesAsync(int customerId);
        Task DeleteDeviceAsync(int customerId, int deviceId);
        Task<bool> TrustDeviceAsync(int customerId, int deviceId);
        
        // Wishlist
        Task<List<CustomerWishlistItemDto>> GetWishlistAsync(int customerId);
        Task<CustomerWishlistItemDto> AddToWishlistAsync(int customerId, int productId, int? variantId = null);
        Task RemoveFromWishlistAsync(int customerId, int wishlistItemId);
        
        // Coupons
        Task<List<CustomerCouponDto>> GetCouponsAsync(int customerId);
        Task<CustomerCouponDto> AssignCouponAsync(int customerId, string couponCode);
        Task<bool> UseCouponAsync(int customerId, int couponId);
        
        // Metrics
        Task UpdateCustomerMetricsAsync(int customerId);
        Task<string> GenerateCustomerIdAsync();
        
        // Security settings
        Task UpdateSecuritySettingsAsync(int customerId, UpdateSecuritySettingsDto dto);
        Task<bool> ChangePasswordAsync(int customerId, CustomerPasswordChangeDto dto);
        Task<List<CustomerSecurityQuestionDto>> GetSecurityQuestionsAsync(int customerId);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(int customerId, ResetPasswordDto dto);

        // Payments history (reservations -> payments)
        Task<List<CustomerPaymentHistoryItemDto>> GetPaymentsHistoryAsync(int companyId, int customerId);
    }
}
