using WebsiteBuilderAPI.DTOs.PaymentProvider;

namespace WebsiteBuilderAPI.Services
{
    public interface IPaymentProviderService
    {
        Task<IEnumerable<PaymentProviderResponseDto>> GetAllAsync();
        Task<PaymentProviderResponseDto?> GetByIdAsync(int id);
        Task<PaymentProviderResponseDto> CreateAsync(CreatePaymentProviderRequestDto request);
        Task<PaymentProviderResponseDto?> UpdateAsync(int id, UpdatePaymentProviderRequestDto request);
        Task<bool> DeleteAsync(int id);
        Task<PaymentProviderResponseDto?> ToggleActiveAsync(int id);
        Task<bool> UploadCertificateAsync(int id, IFormFile file);
        Task<bool> UploadPrivateKeyAsync(int id, IFormFile file);
        Task<IEnumerable<AvailableProviderDto>> GetAvailableProvidersAsync();
        Task InitializeDefaultProvidersAsync(int hotelId);
    }
}