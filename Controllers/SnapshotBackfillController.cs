using System;
using System.Collections.Generic;
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
    /// Internal controller for backfilling snapshots
    /// Protected endpoint for administrative use only
    /// </summary>
    [ApiController]
    [Route("api/internal/snapshot")]
    [Authorize] // Requires authentication
    public class SnapshotBackfillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebsiteBuilderService _websiteService;
        private readonly ILogger<SnapshotBackfillController> _logger;
        private readonly IConfiguration _configuration;

        public SnapshotBackfillController(
            ApplicationDbContext context,
            IWebsiteBuilderService websiteService,
            ILogger<SnapshotBackfillController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _websiteService = websiteService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Generate snapshots for all pages of a specific company
        /// </summary>
        /// <param name="companyId">Company ID to backfill</param>
        /// <param name="secret">Secret key for additional protection</param>
        /// <returns>Backfill status</returns>
        [HttpPost("backfill/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin")] // Extra role protection
        public async Task<IActionResult> BackfillCompanySnapshots(int companyId, [FromQuery] string secret)
        {
            try
            {
                // Verify secret for extra protection
                var configuredSecret = _configuration["WebsiteBuilder:BackfillSecret"] ?? "default-backfill-secret-2025";
                if (secret != configuredSecret)
                {
                    _logger.LogWarning("Invalid backfill secret attempt for company {CompanyId}", companyId);
                    return Unauthorized(new { error = "Invalid secret" });
                }

                // Get all pages for the company
                var pages = await _context.WebsitePages
                    .Where(p => p.CompanyId == companyId)
                    .Select(p => new { p.Id, p.Name, p.Slug, p.PageType })
                    .ToListAsync();

                if (!pages.Any())
                {
                    return NotFound(new { error = "No pages found for company", companyId });
                }

                var results = new List<object>();
                var successCount = 0;
                var errorCount = 0;

                foreach (var page in pages)
                {
                    try
                    {
                        // Generate snapshot using the service's method via reflection
                        // (since GenerateSnapshotAsync is private, we'll create snapshots directly)
                        await GenerateSnapshotForPageAsync(page.Id, companyId, page.Slug);
                        
                        successCount++;
                        results.Add(new { 
                            pageId = page.Id, 
                            slug = page.Slug, 
                            status = "success",
                            message = $"Snapshot generated for {page.Name}"
                        });
                        
                        _logger.LogInformation("Generated snapshot for page {PageId} ({Slug}) in company {CompanyId}", 
                            page.Id, page.Slug, companyId);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        results.Add(new { 
                            pageId = page.Id, 
                            slug = page.Slug, 
                            status = "error",
                            message = ex.Message
                        });
                        
                        _logger.LogError(ex, "Failed to generate snapshot for page {PageId} in company {CompanyId}", 
                            page.Id, companyId);
                    }

                    // Small delay to avoid overwhelming the system
                    await Task.Delay(100);
                }

                return Ok(new
                {
                    companyId,
                    totalPages = pages.Count,
                    successCount,
                    errorCount,
                    timestamp = DateTime.UtcNow,
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backfill for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "Backfill failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Generate snapshot for a specific page
        /// </summary>
        private async Task GenerateSnapshotForPageAsync(int pageId, int companyId, string slug)
        {
            // Load complete page data
            var page = await _context.WebsitePages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Children.OrderBy(c => c.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == pageId);
                
            if (page == null) 
            {
                throw new InvalidOperationException($"Page {pageId} not found");
            }
            
            // Check for existing snapshot
            var existingSnapshot = await _context.PublishedSnapshots
                .Where(s => s.PageId == pageId && !s.IsStale)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();
                
            // Mark old snapshots as stale
            if (existingSnapshot != null)
            {
                existingSnapshot.IsStale = true;
            }
            
            // Create complete snapshot data
            var snapshotData = new
            {
                pageId = page.Id,
                companyId = page.CompanyId,
                pageType = page.PageType,
                name = page.Name,
                slug = page.Slug,
                metaTitle = page.MetaTitle,
                metaDescription = page.MetaDescription,
                isActive = page.IsActive,
                isPublished = page.IsPublished,
                sections = page.Sections.Select(s => new
                {
                    id = s.Id,
                    sectionType = s.SectionType,
                    config = s.Config,
                    themeOverrides = s.ThemeOverrides,
                    sortOrder = s.SortOrder,
                    isActive = s.IsActive,
                    children = s.Children?.Select(c => new
                    {
                        id = c.Id,
                        sortOrder = c.SortOrder,
                        isActive = c.IsActive
                    })
                }),
                timestamp = DateTime.UtcNow
            };
            
            var newVersion = (existingSnapshot?.Version ?? 0) + 1;
            
            // Create new snapshot
            var snapshot = new Models.PublishedSnapshot
            {
                CompanyId = companyId,
                PageId = pageId,
                PageSlug = slug,
                PageType = page.PageType,
                SnapshotData = System.Text.Json.JsonSerializer.Serialize(snapshotData),
                Version = newVersion,
                IsStale = false,
                PublishedAt = DateTime.UtcNow
            };
            
            _context.PublishedSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Generated snapshot v{Version} for page {PageId} (backfill)", 
                newVersion, pageId);
        }

        /// <summary>
        /// Get backfill status for a company
        /// </summary>
        [HttpGet("status/{companyId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetBackfillStatus(int companyId)
        {
            try
            {
                var totalPages = await _context.WebsitePages
                    .CountAsync(p => p.CompanyId == companyId);
                    
                var snapshotCount = await _context.PublishedSnapshots
                    .Where(s => s.CompanyId == companyId && !s.IsStale)
                    .Select(s => s.PageId)
                    .Distinct()
                    .CountAsync();
                    
                var latestSnapshot = await _context.PublishedSnapshots
                    .Where(s => s.CompanyId == companyId)
                    .OrderByDescending(s => s.PublishedAt)
                    .Select(s => s.PublishedAt)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    companyId,
                    totalPages,
                    pagesWithSnapshots = snapshotCount,
                    coverage = totalPages > 0 ? (decimal)snapshotCount / totalPages * 100 : 0,
                    lastSnapshotAt = latestSnapshot,
                    status = snapshotCount == totalPages ? "complete" : "partial"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backfill status for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "Failed to get status" });
            }
        }
    }
}