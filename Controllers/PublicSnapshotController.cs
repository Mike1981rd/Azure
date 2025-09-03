using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Public controller for serving published snapshots with aggressive caching
    /// Used by production domains to serve stable content
    /// </summary>
    [ApiController]
    [Route("api/website/{companyId}/snapshot")]
    [AllowAnonymous]
    public class PublicSnapshotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebsiteBuilderCacheService _cacheService;
        private readonly ILogger<PublicSnapshotController> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _requiresVerifiedDomain;

        public PublicSnapshotController(
            ApplicationDbContext context,
            IWebsiteBuilderCacheService cacheService,
            ILogger<PublicSnapshotController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
            _configuration = configuration;
            _requiresVerifiedDomain = _configuration.GetValue<bool>("WebsiteBuilder:Snapshot:RequiresVerifiedDomain", true);
        }

        /// <summary>
        /// Get published snapshot for a specific page by slug
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="slug">Page slug</param>
        /// <returns>Snapshot data with CDN cache headers</returns>
        [HttpGet("{slug}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // 24 hours
        public async Task<IActionResult> GetSnapshotBySlug(int companyId, string slug)
        {
            try
            {
                // Check domain verification if required
                if (_requiresVerifiedDomain)
                {
                    var hostHeader = Request.Headers["Host"].ToString();
                    if (!string.IsNullOrEmpty(hostHeader))
                    {
                        // Check if domain is verified for this company
                        var isVerified = await IsDomainVerifiedAsync(companyId, hostHeader);
                        if (!isVerified)
                        {
                            _logger.LogWarning("Unverified domain {Domain} tried to access snapshot for company {CompanyId}", 
                                hostHeader, companyId);
                            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                            return NotFound(new { error = "Domain not verified", domain = hostHeader });
                        }
                    }
                }
                
                // Normalize slug
                slug = slug?.ToLowerInvariant()?.Trim() ?? "home";
                
                // Try to get from cache service first (includes snapshot lookup)
                var cachedSnapshot = await _cacheService.GetPageProductionBySlugAsync(companyId, slug);
                if (!string.IsNullOrEmpty(cachedSnapshot))
                {
                    // Set aggressive cache headers for CDN
                    Response.Headers["Cache-Control"] = "public, max-age=86400, s-maxage=86400";
                    Response.Headers["Vary"] = "Accept-Encoding";
                    Response.Headers["X-Snapshot-Hit"] = "true";
                    
                    return Content(cachedSnapshot, "application/json");
                }
                
                // Fallback: Direct database query (should rarely happen)
                var snapshot = await _context.PublishedSnapshots
                    .Where(s => s.CompanyId == companyId && s.PageSlug == slug && !s.IsStale)
                    .OrderByDescending(s => s.Version)
                    .FirstOrDefaultAsync();
                    
                if (snapshot == null)
                {
                    _logger.LogWarning("No snapshot found for company {CompanyId} slug {Slug}", companyId, slug);
                    
                    // Return 404 with cache headers to prevent repeated lookups
                    Response.Headers["Cache-Control"] = "public, max-age=300"; // Cache 404s for 5 minutes
                    return NotFound(new { error = "Page not found", companyId, slug });
                }
                
                // Set cache headers
                Response.Headers["Cache-Control"] = "public, max-age=86400, s-maxage=86400";
                Response.Headers["Vary"] = "Accept-Encoding";
                Response.Headers["X-Snapshot-Version"] = snapshot.Version.ToString();
                Response.Headers["X-Snapshot-Published"] = snapshot.PublishedAt.ToString("O");
                
                _logger.LogInformation("Served snapshot v{Version} for company {CompanyId} slug {Slug}",
                    snapshot.Version, companyId, slug);
                
                return Content(snapshot.SnapshotData, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving snapshot for company {CompanyId} slug {Slug}", 
                    companyId, slug);
                    
                // Don't cache errors
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Get list of all published pages for a company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of published page metadata</returns>
        [HttpGet("pages")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // 1 hour
        public async Task<IActionResult> GetPublishedPages(int companyId)
        {
            try
            {
                var pages = await _context.PublishedSnapshots
                    .Where(s => s.CompanyId == companyId && !s.IsStale)
                    .GroupBy(s => new { s.PageId, s.PageSlug, s.PageType })
                    .Select(g => new
                    {
                        pageId = g.Key.PageId,
                        slug = g.Key.PageSlug,
                        pageType = g.Key.PageType,
                        latestVersion = g.Max(s => s.Version),
                        lastPublished = g.Max(s => s.PublishedAt)
                    })
                    .OrderBy(p => p.pageType)
                    .ThenBy(p => p.slug)
                    .ToListAsync();
                    
                // Set cache headers
                Response.Headers["Cache-Control"] = "public, max-age=3600, s-maxage=3600";
                Response.Headers["Vary"] = "Accept-Encoding";
                
                return Ok(new
                {
                    companyId,
                    pageCount = pages.Count,
                    pages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving published pages for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Get specific version of a snapshot (for rollback scenarios)
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="pageId">Page ID</param>
        /// <param name="version">Version number</param>
        /// <returns>Specific snapshot version</returns>
        [HttpGet("page/{pageId}/version/{version}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // 24 hours
        public async Task<IActionResult> GetSnapshotByVersion(int companyId, int pageId, int version)
        {
            try
            {
                var snapshot = await _context.PublishedSnapshots
                    .Where(s => s.CompanyId == companyId && s.PageId == pageId && s.Version == version)
                    .FirstOrDefaultAsync();
                    
                if (snapshot == null)
                {
                    Response.Headers["Cache-Control"] = "public, max-age=300"; // Cache 404s for 5 minutes
                    return NotFound(new { error = "Snapshot version not found", companyId, pageId, version });
                }
                
                // Set cache headers (versions are immutable)
                Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable"; // 1 year
                Response.Headers["Vary"] = "Accept-Encoding";
                Response.Headers["X-Snapshot-Version"] = snapshot.Version.ToString();
                Response.Headers["X-Snapshot-Stale"] = snapshot.IsStale.ToString();
                
                return Content(snapshot.SnapshotData, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving snapshot version {Version} for page {PageId}", 
                    version, pageId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Check if a domain is verified for a specific company
        /// </summary>
        private async Task<bool> IsDomainVerifiedAsync(int companyId, string domain)
        {
            try
            {
                // Remove port if present
                var domainWithoutPort = domain.Split(':')[0].ToLowerInvariant();
                
                // Check CustomDomains table for verified domain
                var verifiedDomain = await _context.CustomDomains
                    .Where(d => d.CompanyId == companyId && 
                           d.Domain.ToLower() == domainWithoutPort && 
                           d.IsVerified)
                    .FirstOrDefaultAsync();
                    
                if (verifiedDomain != null)
                {
                    _logger.LogInformation("Domain {Domain} is verified for company {CompanyId}", 
                        domainWithoutPort, companyId);
                    return true;
                }
                
                // Also allow staging/development domains without verification
                if (domainWithoutPort.Contains("onrender.com") || 
                    domainWithoutPort.Contains("localhost") ||
                    domainWithoutPort.Contains("vercel.app"))
                {
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking domain verification for {Domain}", domain);
                return false;
            }
        }
    }
}