using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IDomainService
    {
        // CRUD Operations
        Task<DomainOperationResult> CreateDomainAsync(int companyId, CreateDomainDto dto, string userId);
        Task<DomainResponseDto?> GetDomainByIdAsync(int id, int companyId);
        Task<List<DomainResponseDto>> GetDomainsByCompanyIdAsync(int companyId);
        Task<DomainOperationResult> UpdateDomainAsync(int id, int companyId, UpdateDomainDto dto, string userId);
        Task<DomainOperationResult> DeleteDomainAsync(int id, int companyId, string userId);
        
        // Domain-specific operations
        Task<DomainOperationResult> SetPrimaryDomainAsync(int id, int companyId, string userId);
        Task<DomainOperationResult> VerifyDomainAsync(int id, int companyId, string userId);
        Task<DomainOperationResult> RegenerateDomainVerificationTokenAsync(int id, int companyId, string userId);
        
        // DNS Record operations
        Task<DomainOperationResult> AddDnsRecordAsync(int domainId, int companyId, CreateDnsRecordDto dto, string userId);
        Task<DomainOperationResult> UpdateDnsRecordAsync(int recordId, int companyId, UpdateDnsRecordDto dto, string userId);
        Task<DomainOperationResult> DeleteDnsRecordAsync(int recordId, int companyId, string userId);
        Task<List<DnsRecordResponseDto>> GetDnsRecordsByDomainIdAsync(int domainId, int companyId);
        
        // Validation operations
        Task<bool> IsDomainNameAvailableAsync(string domainName, int companyId, int? excludeDomainId = null);
        Task<Dictionary<string, List<string>>> ValidateDomainConfigurationAsync(int domainId, int companyId);
        Task<List<string>> GetDnsConfigurationSuggestionsAsync(string domainName);
        
        // Status operations
        Task<DomainOperationResult> UpdateDomainStatusAsync(int id, int companyId, DomainStatus status, string userId);
        Task<List<DomainResponseDto>> GetExpiringDomainsAsync(int companyId, int daysThreshold = 30);
        Task<List<DomainResponseDto>> GetExpiringSslCertificatesAsync(int companyId, int daysThreshold = 30);
    }
}