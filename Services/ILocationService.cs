using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs.Location;

namespace WebsiteBuilderAPI.Services
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetLocationsByCompanyIdAsync(int companyId);
        Task<LocationDto> GetLocationByIdAsync(int id, int companyId);
        Task<LocationDto> CreateLocationAsync(int companyId, CreateLocationDto dto);
        Task<LocationDto> UpdateLocationAsync(int id, int companyId, UpdateLocationDto dto);
        Task<bool> DeleteLocationAsync(int id, int companyId);
        Task<bool> SetDefaultLocationAsync(int locationId, int companyId);
        Task<LocationDto> GetDefaultLocationAsync(int companyId);
    }
}