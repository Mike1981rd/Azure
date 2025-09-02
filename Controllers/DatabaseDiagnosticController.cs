using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DatabaseDiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseDiagnosticController> _logger;

        public DatabaseDiagnosticController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DatabaseDiagnosticController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("ping-db")]
        [AllowAnonymous]
        public async Task<IActionResult> PingDatabase()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // 1. Log connection string (sin credenciales)
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                    _logger.LogInformation("DB Config - Host: {Host}, Port: {Port}, Database: {Database}, Username: {Username}",
                        builder.Host, builder.Port, builder.Database, builder.Username);
                }

                // 2. Test CanConnect
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation("Database.CanConnect = {CanConnect}", canConnect);

                if (!canConnect)
                {
                    return Ok(new
                    {
                        ok = false,
                        message = "Cannot connect to database",
                        elapsedMs = stopwatch.ElapsedMilliseconds
                    });
                }

                // 3. Execute SELECT 1
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                var result = await cmd.ExecuteScalarAsync();
                _logger.LogInformation("SELECT 1 returned: {Result}", result);

                // 4. Count Users table
                var userCount = await _context.Users.CountAsync();
                _logger.LogInformation("Users table count: {Count}", userCount);

                // 5. Count Companies table
                var companyCount = await _context.Companies.CountAsync();
                _logger.LogInformation("Companies table count: {Count}", companyCount);

                stopwatch.Stop();

                return Ok(new
                {
                    ok = true,
                    message = "Database connection successful",
                    elapsedMs = stopwatch.ElapsedMilliseconds,
                    diagnostics = new
                    {
                        canConnect = canConnect,
                        selectOne = result?.ToString(),
                        userCount = userCount,
                        companyCount = companyCount,
                        host = new Npgsql.NpgsqlConnectionStringBuilder(connectionString).Host,
                        database = new Npgsql.NpgsqlConnectionStringBuilder(connectionString).Database
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database ping failed");
                stopwatch.Stop();
                
                return Ok(new
                {
                    ok = false,
                    message = $"Database ping failed: {ex.Message}",
                    elapsedMs = stopwatch.ElapsedMilliseconds,
                    error = ex.ToString()
                });
            }
        }

        [HttpGet("logo-url")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLogoUrl()
        {
            try
            {
                // Intenta obtener el logo de la primera empresa
                var company = await _context.Companies
                    .Select(c => new { c.Id, c.Name, c.Logo })
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    _logger.LogWarning("No companies found in database");
                    return Ok(new
                    {
                        ok = false,
                        message = "No companies found",
                        logoUrl = (string)null
                    });
                }

                _logger.LogInformation("Company found - Id: {Id}, Name: {Name}, Logo: {Logo}",
                    company.Id, company.Name, !string.IsNullOrEmpty(company.Logo));

                return Ok(new
                {
                    ok = true,
                    companyId = company.Id,
                    companyName = company.Name,
                    logoUrl = company.Logo,
                    hasLogo = !string.IsNullOrEmpty(company.Logo)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get logo URL");
                return Ok(new
                {
                    ok = false,
                    message = $"Failed to get logo: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        [HttpGet("tables")]
        [AllowAnonymous]
        public async Task<IActionResult> ListTables()
        {
            try
            {
                var tables = new List<object>();
                
                using var cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = @"
                    SELECT table_name, 
                           (SELECT COUNT(*) FROM information_schema.columns 
                            WHERE table_schema = 'public' 
                            AND table_name = t.table_name) as column_count
                    FROM information_schema.tables t
                    WHERE table_schema = 'public'
                    AND table_type = 'BASE TABLE'
                    ORDER BY table_name";
                
                await _context.Database.GetDbConnection().OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0);
                    var columnCount = reader.GetInt64(1);
                    
                    // Count rows for each table
                    using var countCmd = _context.Database.GetDbConnection().CreateCommand();
                    countCmd.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                    var rowCount = await countCmd.ExecuteScalarAsync();
                    
                    tables.Add(new
                    {
                        tableName = tableName,
                        columnCount = columnCount,
                        rowCount = rowCount
                    });
                    
                    _logger.LogInformation("Table {Table}: {Columns} columns, {Rows} rows",
                        tableName, columnCount, rowCount);
                }

                return Ok(new
                {
                    ok = true,
                    tableCount = tables.Count,
                    tables = tables
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list tables");
                return Ok(new
                {
                    ok = false,
                    message = $"Failed to list tables: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        [HttpGet("gatos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGatos()
        {
            try
            {
                var gatos = await _context.Gatos.ToListAsync();
                
                // Si no hay gatos, crear uno de prueba
                if (!gatos.Any())
                {
                    _logger.LogInformation("No gatos found, creating test gato");
                    var testGato = new Models.Gato
                    {
                        Nombre = "Mittens",
                        Edad = 3,
                        Domestico = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Gatos.Add(testGato);
                    await _context.SaveChangesAsync();
                    gatos = await _context.Gatos.ToListAsync();
                }
                
                return Ok(new
                {
                    ok = true,
                    count = gatos.Count,
                    gatos = gatos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get gatos");
                return Ok(new
                {
                    ok = false,
                    message = $"Failed to get gatos: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        [HttpGet("migration-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMigrationStatus()
        {
            try
            {
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                
                _logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count());
                _logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count());

                return Ok(new
                {
                    ok = true,
                    applied = appliedMigrations.ToList(),
                    pending = pendingMigrations.ToList(),
                    appliedCount = appliedMigrations.Count(),
                    pendingCount = pendingMigrations.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get migration status");
                return Ok(new
                {
                    ok = false,
                    message = $"Failed to get migration status: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }
    }
}