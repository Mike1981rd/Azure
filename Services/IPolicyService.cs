using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs.Policies;

namespace WebsiteBuilderAPI.Services
{
    public interface IPolicyService
    {
        Task<List<PolicyDto>> GetAllByCompanyAsync(int companyId);
        Task<PolicyDto> GetByTypeAsync(int companyId, string type);
        Task<PolicyDto> UpdateAsync(int companyId, string type, UpdatePolicyDto dto);
        Task InitializePoliciesForCompanyAsync(int companyId);
        Task<bool> PolicyExistsAsync(int companyId, string type);
    }
}