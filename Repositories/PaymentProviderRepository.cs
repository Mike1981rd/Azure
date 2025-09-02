using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Repositories
{
    public interface IPaymentProviderRepository
    {
        Task<PaymentProvider?> GetByIdAsync(int id);
        Task<PaymentProvider?> GetActiveProviderAsync(int companyId, string provider);
        Task<List<PaymentProvider>> GetProvidersByCompanyAsync(int companyId);
        Task<PaymentProvider> CreateAsync(PaymentProvider provider);
        Task<PaymentProvider> UpdateAsync(PaymentProvider provider);
        Task DeleteAsync(int id);
        Task DeactivateAllProvidersAsync(int companyId);
    }

    public class PaymentProviderRepository : IPaymentProviderRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentProviderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentProvider?> GetByIdAsync(int id)
        {
            return await _context.PaymentProviders
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PaymentProvider?> GetActiveProviderAsync(int companyId, string provider)
        {
            return await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.CompanyId == companyId && 
                                         p.Provider.ToLower() == provider.ToLower() && 
                                         p.IsActive);
        }

        public async Task<List<PaymentProvider>> GetProvidersByCompanyAsync(int companyId)
        {
            return await _context.PaymentProviders
                .Where(p => p.CompanyId == companyId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<PaymentProvider> CreateAsync(PaymentProvider provider)
        {
            provider.CreatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;
            
            _context.PaymentProviders.Add(provider);
            await _context.SaveChangesAsync();
            
            return provider;
        }

        public async Task<PaymentProvider> UpdateAsync(PaymentProvider provider)
        {
            provider.UpdatedAt = DateTime.UtcNow;
            
            _context.PaymentProviders.Update(provider);
            await _context.SaveChangesAsync();
            
            return provider;
        }

        public async Task DeleteAsync(int id)
        {
            var provider = await GetByIdAsync(id);
            if (provider != null)
            {
                _context.PaymentProviders.Remove(provider);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeactivateAllProvidersAsync(int companyId)
        {
            var providers = await GetProvidersByCompanyAsync(companyId);
            foreach (var provider in providers)
            {
                provider.IsActive = false;
                provider.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
        }
    }
}