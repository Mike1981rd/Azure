using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Cache service implementation for website builder
    /// Implements a two-tier cache strategy: Memory cache for hot data, Distributed cache for shared data
    /// </summary>
    public class WebsiteBuilderCacheService : IWebsiteBuilderCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<WebsiteBuilderCacheService> _logger;

        // Cache key prefixes
        private const string PREVIEW_PREFIX = "preview:";
        private const string PRODUCTION_PREFIX = "production:";
        private const string THEME_PREFIX = "theme:";
        
        // Cache durations
        private readonly TimeSpan _previewCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _productionCacheDuration = TimeSpan.FromHours(24);
        private readonly TimeSpan _themeCacheDuration = TimeSpan.FromMinutes(30);

        // Statistics tracking
        private long _cacheHits = 0;
        private long _cacheMisses = 0;

        public WebsiteBuilderCacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<WebsiteBuilderCacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        #region Page Cache Operations

        public async Task<string?> GetPagePreviewAsync(int pageId)
        {
            var key = $"{PREVIEW_PREFIX}{pageId}";
            
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out string? cachedContent))
            {
                _cacheHits++;
                _logger.LogDebug("Preview cache hit for page {PageId}", pageId);
                return cachedContent;
            }

            // Try distributed cache
            var distributedContent = await _distributedCache.GetStringAsync(key);
            if (distributedContent != null)
            {
                _cacheHits++;
                // Populate memory cache
                _memoryCache.Set(key, distributedContent, _previewCacheDuration);
                _logger.LogDebug("Preview distributed cache hit for page {PageId}", pageId);
                return distributedContent;
            }

            _cacheMisses++;
            _logger.LogDebug("Preview cache miss for page {PageId}", pageId);
            return null;
        }

        public async Task SetPagePreviewAsync(int pageId, string content)
        {
            var key = $"{PREVIEW_PREFIX}{pageId}";
            
            // Set in both caches
            _memoryCache.Set(key, content, _previewCacheDuration);
            
            await _distributedCache.SetStringAsync(
                key, 
                content,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = _previewCacheDuration
                });

            _logger.LogDebug("Set preview cache for page {PageId}", pageId);
        }

        public async Task<string?> GetPageProductionAsync(int pageId)
        {
            var key = $"{PRODUCTION_PREFIX}{pageId}";
            
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out string? cachedContent))
            {
                _cacheHits++;
                _logger.LogDebug("Production cache hit for page {PageId}", pageId);
                return cachedContent;
            }

            // Try distributed cache
            var distributedContent = await _distributedCache.GetStringAsync(key);
            if (distributedContent != null)
            {
                _cacheHits++;
                // Populate memory cache
                _memoryCache.Set(key, distributedContent, _productionCacheDuration);
                _logger.LogDebug("Production distributed cache hit for page {PageId}", pageId);
                return distributedContent;
            }

            _cacheMisses++;
            _logger.LogDebug("Production cache miss for page {PageId}", pageId);
            return null;
        }

        public async Task SetPageProductionAsync(int pageId, string content)
        {
            var key = $"{PRODUCTION_PREFIX}{pageId}";
            
            // Set in both caches
            _memoryCache.Set(key, content, _productionCacheDuration);
            
            await _distributedCache.SetStringAsync(
                key, 
                content,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = _productionCacheDuration
                });

            _logger.LogInformation("Set production cache for page {PageId}", pageId);
        }

        public async Task InvalidatePageCacheAsync(int pageId)
        {
            var previewKey = $"{PREVIEW_PREFIX}{pageId}";
            var productionKey = $"{PRODUCTION_PREFIX}{pageId}";
            
            // Remove from memory cache
            _memoryCache.Remove(previewKey);
            _memoryCache.Remove(productionKey);
            
            // Remove from distributed cache
            await _distributedCache.RemoveAsync(previewKey);
            await _distributedCache.RemoveAsync(productionKey);
            
            _logger.LogInformation("Invalidated all cache for page {PageId}", pageId);
        }

        public async Task WarmProductionCacheAsync(int pageId)
        {
            // This would typically call a service to generate the page content
            // For now, we just log the intent
            _logger.LogInformation("Warming production cache for page {PageId}", pageId);
            
            // In a real implementation, you would:
            // 1. Generate the page content
            // 2. Call SetPageProductionAsync with the content
            
            await Task.CompletedTask;
        }

        #endregion

        #region Theme Cache Operations

        public async Task<T?> GetThemeConfigAsync<T>(int companyId, string configType) where T : class
        {
            var key = $"{THEME_PREFIX}{companyId}:{configType}";
            
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out T? cachedConfig))
            {
                _cacheHits++;
                _logger.LogDebug("Theme cache hit for company {CompanyId}, type {ConfigType}", 
                    companyId, configType);
                return cachedConfig;
            }

            // Try distributed cache
            var distributedJson = await _distributedCache.GetStringAsync(key);
            if (distributedJson != null)
            {
                try
                {
                    var config = JsonSerializer.Deserialize<T>(distributedJson);
                    if (config != null)
                    {
                        _cacheHits++;
                        // Populate memory cache
                        _memoryCache.Set(key, config, _themeCacheDuration);
                        _logger.LogDebug("Theme distributed cache hit for company {CompanyId}, type {ConfigType}", 
                            companyId, configType);
                        return config;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize theme config from cache");
                }
            }

            _cacheMisses++;
            _logger.LogDebug("Theme cache miss for company {CompanyId}, type {ConfigType}", 
                companyId, configType);
            return null;
        }

        public async Task SetThemeConfigAsync<T>(int companyId, string configType, T config) where T : class
        {
            var key = $"{THEME_PREFIX}{companyId}:{configType}";
            
            // Set in memory cache
            _memoryCache.Set(key, config, _themeCacheDuration);
            
            // Set in distributed cache
            try
            {
                var json = JsonSerializer.Serialize(config);
                await _distributedCache.SetStringAsync(
                    key, 
                    json,
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = _themeCacheDuration
                    });
                
                _logger.LogDebug("Set theme cache for company {CompanyId}, type {ConfigType}", 
                    companyId, configType);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize theme config for cache");
            }
        }

        public async Task InvalidateThemeConfigAsync(int companyId, string? configType = null)
        {
            if (configType != null)
            {
                var key = $"{THEME_PREFIX}{companyId}:{configType}";
                _memoryCache.Remove(key);
                await _distributedCache.RemoveAsync(key);
                _logger.LogInformation("Invalidated theme cache for company {CompanyId}, type {ConfigType}", 
                    companyId, configType);
            }
            else
            {
                // Invalidate all theme configs for the company
                // This is a simplified implementation - in production you'd want to track keys
                var commonTypes = new[] { "appearance", "typography", "colorSchemes", "productCards", 
                    "productBadges", "cart", "favicon", "navigation", "socialMedia", "swatches" };
                
                foreach (var type in commonTypes)
                {
                    var key = $"{THEME_PREFIX}{companyId}:{type}";
                    _memoryCache.Remove(key);
                    await _distributedCache.RemoveAsync(key);
                }
                
                _logger.LogInformation("Invalidated all theme cache for company {CompanyId}", companyId);
            }
        }

        #endregion

        #region General Cache Operations

        public async Task InvalidateCompanyCacheAsync(int companyId)
        {
            // Invalidate theme configs
            await InvalidateThemeConfigAsync(companyId);
            
            // In a real implementation, you would also:
            // - Invalidate all pages for the company
            // - Invalidate any other company-specific cache entries
            
            _logger.LogInformation("Invalidated all cache for company {CompanyId}", companyId);
        }

        public async Task ClearAllCacheAsync()
        {
            // Clear memory cache
            if (_memoryCache is MemoryCache memCache)
            {
                memCache.Compact(1.0);
            }
            
            // Note: IDistributedCache doesn't have a clear all method
            // In production, you'd need to implement this based on your cache provider
            // For Redis: await ((RedisCache)_distributedCache).ExecuteAsync("FLUSHDB");
            
            _logger.LogWarning("Cleared all cache entries");
            
            await Task.CompletedTask;
        }

        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            var totalRequests = _cacheHits + _cacheMisses;
            var hitRate = totalRequests > 0 ? (double)_cacheHits / totalRequests : 0;
            
            var stats = new CacheStatistics
            {
                TotalItems = 0, // Would need to track this
                TotalSize = 0, // Would need to track this
                PreviewCacheItems = 0, // Would need to track this
                ProductionCacheItems = 0, // Would need to track this
                ThemeCacheItems = 0, // Would need to track this
                HitRate = hitRate,
                MissRate = 1 - hitRate
            };
            
            _logger.LogInformation("Cache statistics - Hit rate: {HitRate:P2}, Total requests: {TotalRequests}", 
                hitRate, totalRequests);
            
            return await Task.FromResult(stats);
        }

        #endregion
    }
}