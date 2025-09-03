using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Temporary diagnostic controller for debugging snapshot generation
    /// </summary>
    [ApiController]
    [Route("api/diagnostic")]
    [AllowAnonymous] // Temporary for debugging
    public class DiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DiagnosticController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get diagnostic information about snapshot configuration and status
        /// </summary>
        [HttpGet("snapshot-status")]
        public async Task<IActionResult> GetSnapshotStatus()
        {
            try
            {
                // Get configuration values
                var autoGenerate = _configuration.GetValue<bool>("WebsiteBuilder:Snapshot:AutoGenerate", false);
                var requiresVerifiedDomain = _configuration.GetValue<bool>("WebsiteBuilder:Snapshot:RequiresVerifiedDomain", false);
                
                // Get snapshot statistics
                var totalSnapshots = await _context.PublishedSnapshots.CountAsync();
                var uniquePages = await _context.PublishedSnapshots
                    .Select(s => s.PageId)
                    .Distinct()
                    .CountAsync();
                var latestSnapshot = await _context.PublishedSnapshots
                    .OrderByDescending(s => s.PublishedAt)
                    .Select(s => new { s.PublishedAt, s.PageId, s.CompanyId })
                    .FirstOrDefaultAsync();
                
                // Get page statistics
                var totalPages = await _context.WebsitePages.CountAsync();
                var publishedPages = await _context.WebsitePages
                    .Where(p => p.IsPublished)
                    .CountAsync();
                
                // Log for debugging
                _logger.LogInformation("[DIAGNOSTIC] Snapshot status requested - AutoGenerate: {Auto}, Snapshots: {Count}", 
                    autoGenerate, totalSnapshots);
                
                return Ok(new
                {
                    configuration = new
                    {
                        autoGenerate,
                        requiresVerifiedDomain,
                        configSource = "appsettings.json"
                    },
                    database = new
                    {
                        totalSnapshots,
                        uniquePagesWithSnapshots = uniquePages,
                        latestSnapshot,
                        totalPages,
                        publishedPages
                    },
                    environment = new
                    {
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                        machineName = Environment.MachineName,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DIAGNOSTIC] Error getting snapshot status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test snapshot generation for a specific page
        /// </summary>
        [HttpPost("test-snapshot/{pageId}")]
        public async Task<IActionResult> TestSnapshotGeneration(int pageId)
        {
            try
            {
                var page = await _context.WebsitePages
                    .Include(p => p.Sections)
                    .FirstOrDefaultAsync(p => p.Id == pageId);
                    
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }
                
                _logger.LogInformation("[DIAGNOSTIC] Testing snapshot generation for page {PageId}", pageId);
                
                // Try to create a snapshot directly
                var snapshot = new Models.PublishedSnapshot
                {
                    CompanyId = page.CompanyId,
                    PageId = pageId,
                    PageSlug = page.Slug,
                    PageType = page.PageType,
                    SnapshotData = System.Text.Json.JsonSerializer.Serialize(new { test = true, pageId, timestamp = DateTime.UtcNow }),
                    Version = 999, // Test version
                    IsStale = false,
                    PublishedAt = DateTime.UtcNow
                };
                
                _context.PublishedSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("[DIAGNOSTIC] Test snapshot created successfully for page {PageId}", pageId);
                
                return Ok(new
                {
                    success = true,
                    snapshotId = snapshot.Id,
                    pageId,
                    message = "Test snapshot created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DIAGNOSTIC] Error creating test snapshot for page {PageId}", pageId);
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}