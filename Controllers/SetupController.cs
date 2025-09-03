using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SetupController> _logger;

        public SetupController(ApplicationDbContext context, ILogger<SetupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Run EF Core migrations manually (idempotent). Use when DB is missing tables.
        /// </summary>
        [HttpPost("migrate")]
        [AllowAnonymous]
        public IActionResult RunMigrations()
        {
            try
            {
                _logger.LogInformation("Running EF Core migrations on demand (SetupController).\nConnection: {Conn}",
                    _context.Database.GetConnectionString());
                _context.Database.Migrate();
                return Ok(new { migrated = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running migrations manually");
                return StatusCode(500, new { migrated = false, error = ex.Message });
            }
        }

        /// <summary>
        /// As last resort, create schema using EnsureCreated (not for production migrations, but unblocks empty DBs).
        /// </summary>
        [HttpPost("ensure-created")]
        [AllowAnonymous]
        public IActionResult EnsureCreated()
        {
            try
            {
                _logger.LogWarning("Running EnsureCreated (fallback) - use Migrate() for real migrations");
                var created = _context.Database.EnsureCreated();
                return Ok(new { created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running EnsureCreated");
                return StatusCode(500, new { created = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Inicializa o actualiza los permisos del sistema
        /// </summary>
        [HttpPost("seed-permissions")]
        [AllowAnonymous] // Temporalmente permitir sin auth para configuración inicial
        public async Task<IActionResult> SeedPermissions()
        {
            try
            {
                _logger.LogInformation("Starting permissions seeding...");
                
                // Seed permissions
                await PermissionsSeeder.SeedPermissionsAsync(_context);
                _logger.LogInformation("Permissions seeded successfully");

                // Seed roles with permissions
                await PermissionsSeeder.SeedRolesAsync(_context);
                _logger.LogInformation("Roles seeded successfully");

                // Create default super admin user
                await PermissionsSeeder.SeedDefaultUserAsync(_context);
                _logger.LogInformation("Default user created successfully");

                return Ok(new 
                { 
                    message = "Permissions, roles and default user seeded successfully",
                    details = new
                    {
                        defaultUser = "admin@websitebuilder.com",
                        defaultPassword = "Admin@123",
                        note = "Please change the default password after first login"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding permissions");
                return StatusCode(500, new { message = "Error seeding permissions", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas del sistema de permisos
        /// </summary>
        [HttpGet("permissions-stats")]
        [Authorize]
        public async Task<IActionResult> GetPermissionsStats()
        {
            try
            {
                var permissionsCount = await _context.Permissions.CountAsync();
                var rolesCount = await _context.Roles.CountAsync();
                var usersCount = await _context.Users.CountAsync();
                var userRolesCount = await _context.UserRoles.CountAsync();

                var stats = new
                {
                    totalPermissions = permissionsCount,
                    totalRoles = rolesCount,
                    totalUsers = usersCount,
                    totalUserRoleAssignments = userRolesCount,
                    systemRoles = await _context.Roles.Where(r => r.IsSystemRole).Select(r => r.Name).ToListAsync(),
                    resources = await _context.Permissions.Select(p => p.Resource).Distinct().OrderBy(r => r).ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions stats");
                return StatusCode(500, new { message = "Error getting stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to check role status
        /// </summary>
        [HttpGet("debug-roles")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugRoles()
        {
            var roles = await _context.Roles
                .Select(r => new { r.Id, r.Name, r.IsSystemRole })
                .OrderBy(r => r.Name)
                .ToListAsync();

            var adminUser = _context.Users
                .Where(u => u.Email == "admin@websitebuilder.com")
                .Select(u => new 
                { 
                    u.Id, 
                    u.Email, 
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList() 
                })
                .FirstOrDefault();

            return Ok(new { roles, adminUser });
        }

        /// <summary>
        /// Debug specific role with permissions
        /// </summary>
        [HttpGet("debug-role/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugRole(int id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new 
                { 
                    r.Id, 
                    r.Name, 
                    r.Description,
                    r.IsSystemRole,
                    Permissions = r.RolePermissions.Select(rp => new 
                    {
                        rp.PermissionId,
                        rp.Permission.Resource,
                        rp.Permission.Action
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(role);
        }

        /// <summary>
        /// Reset permissions to only have read, write, create
        /// </summary>
        [HttpPost("reset-permissions")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPermissions()
        {
            try
            {
                // Clear all existing permissions and role permissions
                _context.RolePermissions.RemoveRange(_context.RolePermissions);
                _context.Permissions.RemoveRange(_context.Permissions);
                await _context.SaveChangesAsync();

                // Re-seed permissions with only 3 actions
                await PermissionsSeeder.SeedPermissionsAsync(_context);
                
                // Re-seed roles with updated permissions
                await PermissionsSeeder.SeedRolesAsync(_context);

                return Ok(new 
                { 
                    message = "Permissions reset successfully to read, write, create only",
                    permissionCount = _context.Permissions.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting permissions");
                return StatusCode(500, new { message = "Error resetting permissions", error = ex.Message });
            }
        }

        /// <summary>
        /// Fix system roles - only SuperAdmin and Administrator should be system roles
        /// </summary>
        [HttpPost("fix-system-roles")]
        [AllowAnonymous]
        public async Task<IActionResult> FixSystemRoles()
        {
            try
            {
                // Update all roles to non-system except SuperAdmin and Administrator
                var rolesToUpdate = _context.Roles
                    .Where(r => r.Name != "SuperAdmin" && r.Name != "Administrator")
                    .ToList();

                foreach (var role in rolesToUpdate)
                {
                    role.IsSystemRole = false;
                }

                // Ensure SuperAdmin and Administrator are system roles
                var systemRoles = _context.Roles
                    .Where(r => r.Name == "SuperAdmin" || r.Name == "Administrator")
                    .ToList();

                foreach (var role in systemRoles)
                {
                    role.IsSystemRole = true;
                }

                await _context.SaveChangesAsync();

                var updatedRoles = _context.Roles
                    .Select(r => new { r.Name, r.IsSystemRole })
                    .OrderBy(r => r.Name)
                    .ToList();

                return Ok(new 
                { 
                    message = "System roles fixed successfully",
                    roles = updatedRoles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing system roles");
                return StatusCode(500, new { message = "Error fixing system roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Create missing WhatsApp tables directly in the connected database (Supabase)
        /// </summary>
        [HttpPost("create-whatsapp-tables")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateWhatsAppTables()
        {
            try
            {
                const string sql = @"
CREATE TABLE IF NOT EXISTS public.""WhatsAppConfigs"" (
  ""Id"" uuid PRIMARY KEY,
  ""CompanyId"" integer NOT NULL REFERENCES public.""Companies""(""Id"") ON DELETE CASCADE,
  ""Provider"" varchar(20) NOT NULL DEFAULT 'Twilio',
  ""TwilioAccountSid"" varchar(500),
  ""TwilioAuthToken"" varchar(500),
  ""GreenApiInstanceId"" varchar(100),
  ""GreenApiToken"" varchar(500),
  ""GreenApiTokenMask"" varchar(100),
  ""TwilioAccountSidMask"" varchar(100),
  ""TwilioAuthTokenMask"" varchar(100),
  ""WhatsAppPhoneNumber"" varchar(20) NOT NULL,
  ""WebhookUrl"" varchar(500) NOT NULL,
  ""IsActive"" boolean NOT NULL DEFAULT false,
  ""UseSandbox"" boolean NOT NULL DEFAULT true,
  ""AutoReplySettings"" jsonb,
  ""BusinessHours"" jsonb,
  ""MessageTemplates"" jsonb,
  ""AdditionalSettings"" jsonb,
  ""WebhookToken"" varchar(100),
  ""RateLimitPerMinute"" integer NOT NULL DEFAULT 60,
  ""RateLimitPerHour"" integer NOT NULL DEFAULT 1000,
  ""MaxRetryAttempts"" integer NOT NULL DEFAULT 3,
  ""RetryDelayMinutes"" integer NOT NULL DEFAULT 5,
  ""CreatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""UpdatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""LastTestedAt"" timestamptz,
  ""LastTestResult"" varchar(500)
);
-- NOTE: Webhook authorization columns are managed by EF Core migrations.
-- Any manual ALTERs were removed to avoid drift with code-first migrations.
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WhatsAppConfigs_CompanyId"" ON public.""WhatsAppConfigs""(""CompanyId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConfigs_IsActive"" ON public.""WhatsAppConfigs""(""IsActive"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConfigs_WhatsAppPhoneNumber"" ON public.""WhatsAppConfigs""(""WhatsAppPhoneNumber"");

CREATE TABLE IF NOT EXISTS public.""WhatsAppConversations"" (
  ""Id"" uuid PRIMARY KEY,
  ""CustomerPhone"" varchar(20) NOT NULL,
  ""CustomerName"" varchar(100),
  ""CustomerEmail"" varchar(255),
  ""BusinessPhone"" varchar(20) NOT NULL,
  ""Source"" varchar(20) NOT NULL DEFAULT 'whatsapp',
  ""SessionId"" varchar(100),
  ""Status"" varchar(20) NOT NULL DEFAULT 'active',
  ""Priority"" varchar(10) NOT NULL DEFAULT 'normal',
  ""AssignedUserId"" integer REFERENCES public.""Users""(""Id"") ON DELETE SET NULL,
  ""CompanyId"" integer NOT NULL REFERENCES public.""Companies""(""Id"") ON DELETE CASCADE,
  ""CustomerId"" integer REFERENCES public.""Customers""(""Id"") ON DELETE SET NULL,
  ""UnreadCount"" integer NOT NULL DEFAULT 0,
  ""MessageCount"" integer NOT NULL DEFAULT 0,
  ""LastMessagePreview"" varchar(200),
  ""LastMessageAt"" timestamptz,
  ""LastMessageSender"" varchar(10),
  ""Tags"" jsonb,
  ""Notes"" varchar(1000),
  ""CustomerProfile"" jsonb,
  ""Metadata"" jsonb,
  ""StartedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""ClosedAt"" timestamptz,
  ""ArchivedAt"" timestamptz,
  ""CreatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""UpdatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_Company_Cust_Biz"" ON public.""WhatsAppConversations""(""CompanyId"", ""CustomerPhone"", ""BusinessPhone"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_AssignedUserId"" ON public.""WhatsAppConversations""(""AssignedUserId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_CustomerId"" ON public.""WhatsAppConversations""(""CustomerId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_LastMessageAt"" ON public.""WhatsAppConversations""(""LastMessageAt"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_Priority"" ON public.""WhatsAppConversations""(""Priority"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_Status"" ON public.""WhatsAppConversations""(""Status"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppConversations_UnreadCount"" ON public.""WhatsAppConversations""(""UnreadCount"") WHERE ""UnreadCount"" > 0;

CREATE TABLE IF NOT EXISTS public.""WhatsAppMessages"" (
  ""Id"" uuid PRIMARY KEY,
  ""TwilioSid"" varchar(100) NOT NULL,
  ""From"" varchar(255) NOT NULL,
  ""To"" varchar(255) NOT NULL,
  ""Body"" varchar(4096),
  ""MessageType"" varchar(20) NOT NULL DEFAULT 'text',
  ""MediaUrl"" varchar(500),
  ""MediaContentType"" varchar(50),
  ""Direction"" varchar(10) NOT NULL DEFAULT 'inbound',
  ""Status"" varchar(20) NOT NULL DEFAULT 'received',
  ""ErrorCode"" varchar(10),
  ""ErrorMessage"" varchar(500),
  ""ConversationId"" uuid NOT NULL REFERENCES public.""WhatsAppConversations""(""Id"") ON DELETE CASCADE,
  ""CompanyId"" integer NOT NULL REFERENCES public.""Companies""(""Id"") ON DELETE CASCADE,
  ""CustomerId"" integer REFERENCES public.""Customers""(""Id"") ON DELETE SET NULL,
  ""RepliedByUserId"" integer REFERENCES public.""Users""(""Id"") ON DELETE SET NULL,
  ""Source"" varchar(20),
  ""SessionId"" varchar(100),
  ""Timestamp"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""DeliveredAt"" timestamptz,
  ""ReadAt"" timestamptz,
  ""Metadata"" jsonb,
  ""CreatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""UpdatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_TwilioSid"" ON public.""WhatsAppMessages""(""TwilioSid"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_ConversationId"" ON public.""WhatsAppMessages""(""ConversationId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_CustomerId"" ON public.""WhatsAppMessages""(""CustomerId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_RepliedByUserId"" ON public.""WhatsAppMessages""(""RepliedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Direction"" ON public.""WhatsAppMessages""(""Direction"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_From"" ON public.""WhatsAppMessages""(""From"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_To"" ON public.""WhatsAppMessages""(""To"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Source"" ON public.""WhatsAppMessages""(""Source"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_SessionId"" ON public.""WhatsAppMessages""(""SessionId"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Status"" ON public.""WhatsAppMessages""(""Status"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Timestamp"" ON public.""WhatsAppMessages""(""Timestamp"");
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_ReadPending"" ON public.""WhatsAppMessages""(""ReadAt"") WHERE ""ReadAt"" IS NULL AND ""Direction"" = 'inbound';
CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Company_Conversation"" ON public.""WhatsAppMessages""(""CompanyId"", ""ConversationId"");

-- Optional: Green API config table
CREATE TABLE IF NOT EXISTS public.""GreenApiWhatsAppConfigs"" (
  ""Id"" uuid PRIMARY KEY,
  ""CompanyId"" integer NOT NULL REFERENCES public.""Companies""(""Id"") ON DELETE CASCADE,
  ""InstanceId"" varchar(50) NOT NULL,
  ""ApiToken"" varchar(500) NOT NULL,
  ""PhoneNumber"" varchar(20) NOT NULL,
  ""WebhookUrl"" varchar(500),
  ""EnableWebhook"" boolean NOT NULL DEFAULT true,
  ""AutoAcknowledgeMessages"" boolean NOT NULL DEFAULT true,
  ""PollingIntervalSeconds"" integer NOT NULL DEFAULT 10,
  ""IsActive"" boolean NOT NULL DEFAULT false,
  ""BusinessHoursStart"" interval,
  ""BusinessHoursEnd"" interval,
  ""AutoReplyMessage"" varchar(1000),
  ""RateLimitSettings"" jsonb,
  ""BlacklistedNumbers"" jsonb,
  ""AdditionalSettings"" jsonb,
  ""CreatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""UpdatedAt"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ""LastTestedAt"" timestamptz,
  ""LastTestResult"" varchar(500)
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GreenApiWhatsAppConfigs_CompanyId"" ON public.""GreenApiWhatsAppConfigs""(""CompanyId"");
CREATE INDEX IF NOT EXISTS ""IX_GreenApiWhatsAppConfigs_InstanceId"" ON public.""GreenApiWhatsAppConfigs""(""InstanceId"");
CREATE INDEX IF NOT EXISTS ""IX_GreenApiWhatsAppConfigs_IsActive"" ON public.""GreenApiWhatsAppConfigs""(""IsActive"");
CREATE INDEX IF NOT EXISTS ""IX_GreenApiWhatsAppConfigs_PhoneNumber"" ON public.""GreenApiWhatsAppConfigs""(""PhoneNumber"");
";

                var affected = await _context.Database.ExecuteSqlRawAsync(sql);
                return Ok(new { message = "WhatsApp tables ensured", affected });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WhatsApp tables");
                return StatusCode(500, new { message = "Error creating WhatsApp tables", error = ex.Message });
            }
        }
    }
}
