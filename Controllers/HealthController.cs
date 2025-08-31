using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<HealthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint for Railway
        /// </summary>
        [HttpGet]
        [Route("/api/health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                
                var version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? "1.0.0";

                var response = new
                {
                    status = canConnect ? "healthy" : "degraded",
                    version = version,
                    timestamp = DateTime.UtcNow,
                    environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
                    database = new
                    {
                        connected = canConnect,
                        provider = _context.Database.ProviderName
                    }
                };

                if (!canConnect)
                {
                    _logger.LogWarning("Health check: Database connection failed");
                    return StatusCode(503, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Detailed health check with more information
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> DetailedHealth()
        {
            var checks = new Dictionary<string, object>();

            // Database check
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                checks["database"] = new
                {
                    status = canConnect ? "ok" : "error",
                    provider = _context.Database.ProviderName,
                    pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).Any()
                };
            }
            catch (Exception ex)
            {
                checks["database"] = new
                {
                    status = "error",
                    error = ex.Message
                };
            }

            // Memory check
            var gcInfo = GC.GetGCMemoryInfo();
            checks["memory"] = new
            {
                totalMemoryMB = gcInfo.TotalAvailableMemoryBytes / (1024 * 1024),
                highMemoryLoadThresholdMB = gcInfo.HighMemoryLoadThresholdBytes / (1024 * 1024),
                memoryLoadPercent = (double)gcInfo.MemoryLoadBytes / gcInfo.HighMemoryLoadThresholdBytes * 100
            };

            // Application info
            checks["application"] = new
            {
                version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? "1.0.0",
                environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
                uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()
            };

            return Ok(new
            {
                status = "detailed",
                timestamp = DateTime.UtcNow,
                checks
            });
        }

        /// <summary>
        /// Simple ping endpoint for quick checks
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
        }
    }
}