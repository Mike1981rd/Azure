using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ApplicationDbContext context, ILogger<DebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("connection-test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var connectionString = _context.Database.GetConnectionString();
                
                // Mask the password for security
                var maskedConnection = connectionString;
                if (!string.IsNullOrEmpty(maskedConnection) && maskedConnection.Contains("Password="))
                {
                    var start = maskedConnection.IndexOf("Password=") + 9;
                    var end = maskedConnection.IndexOf(';', start);
                    if (end == -1) end = maskedConnection.Length;
                    var password = maskedConnection.Substring(start, end - start);
                    maskedConnection = maskedConnection.Replace(password, "***MASKED***");
                }
                
                return Ok(new
                {
                    CanConnect = canConnect,
                    ConnectionString = maskedConnection,
                    Provider = _context.Database.ProviderName,
                    Database = _context.Database.GetDbConnection().Database,
                    DataSource = _context.Database.GetDbConnection().DataSource
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return Ok(new
                {
                    CanConnect = false,
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.ToString()
                });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new 
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.IsActive,
                        u.EmailConfirmed,
                        HasPassword = !string.IsNullOrEmpty(u.PasswordHash)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalUsers = users.Count,
                    Users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { Error = ex.Message, Details = ex.ToString() });
            }
        }

        [HttpGet("tables-raw")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var sql = @"
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    ORDER BY table_name";
                
                var tables = await _context.Database
                    .SqlQueryRaw<string>(sql)
                    .ToListAsync();

                return Ok(new { Tables = tables });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
