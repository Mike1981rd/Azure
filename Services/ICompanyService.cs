using WebsiteBuilderAPI.DTOs.Company;
using WebsiteBuilderAPI.DTOs.CheckoutSettings;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto?> GetCurrentCompanyAsync();
        Task<CompanyResponseDto?> GetCompanyByIdAsync(int companyId);
        Task<Company?> GetCompanyEntityByIdAsync(int companyId);
        Task<CompanyResponseDto?> UpdateCurrentCompanyAsync(UpdateCompanyRequestDto request);
        Task<string> UploadLogoAsync(IFormFile file);
        Task UpdateLogoSizeAsync(int size);
        Task<CompanyConfigDto?> GetCompanyConfigAsync();
        Task UpdateLogoAsync(string logoUrl);
        
        // Checkout Settings methods
        Task<CheckoutSettingsDto?> GetCheckoutSettingsAsync();
        Task<CheckoutSettingsDto> CreateDefaultCheckoutSettingsAsync();
        Task<CheckoutSettingsDto> UpdateCheckoutSettingsAsync(UpdateCheckoutSettingsDto request);
        Task<CheckoutSettingsDto> ResetCheckoutSettingsAsync();

        // Checkout Branding
        Task<string> UploadCheckoutLogoAsync(IFormFile file);

        // Currency settings (manual)
        Task<CurrencySettingsDto> GetCurrencySettingsAsync(int companyId);
        Task UpdateCurrencySettingsAsync(int companyId, CurrencySettingsDto settings);
    }
}