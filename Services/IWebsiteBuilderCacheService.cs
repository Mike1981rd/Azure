using System.Threading.Tasks;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Cache service interface for website builder
    /// </summary>
    public interface IWebsiteBuilderCacheService
    {
        // Page cache operations
        Task<string?> GetPagePreviewAsync(int pageId);
        Task SetPagePreviewAsync(int pageId, string content);
        Task<string?> GetPageProductionAsync(int pageId);
        Task<string?> GetPageProductionBySlugAsync(int companyId, string slug);
        Task SetPageProductionAsync(int pageId, string content);
        Task InvalidatePageCacheAsync(int pageId);
        Task WarmProductionCacheAsync(int pageId);

        // Theme cache operations
        Task<T?> GetThemeConfigAsync<T>(int companyId, string configType) where T : class;
        Task SetThemeConfigAsync<T>(int companyId, string configType, T config) where T : class;
        Task InvalidateThemeConfigAsync(int companyId, string? configType = null);

        // General cache operations
        Task InvalidateCompanyCacheAsync(int companyId);
        Task ClearAllCacheAsync();
        Task<CacheStatistics> GetCacheStatisticsAsync();
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public long TotalItems { get; set; }
        public long TotalSize { get; set; }
        public long PreviewCacheItems { get; set; }
        public long ProductionCacheItems { get; set; }
        public long ThemeCacheItems { get; set; }
        public double HitRate { get; set; }
        public double MissRate { get; set; }
    }
}