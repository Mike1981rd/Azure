using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/email/settings")]
    [Authorize]
    public class EmailSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<EmailSettingsController> _logger;

        public EmailSettingsController(ApplicationDbContext context, IEncryptionService encryption, ILogger<EmailSettingsController> logger)
        {
            _context = context;
            _encryption = encryption;
            _logger = logger;
        }

        private int GetCompanyId()
        {
            var claim = User?.FindFirst("companyId")?.Value;
            if (int.TryParse(claim, out var id)) return id;
            return 1;
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get()
        {
            var companyId = GetCompanyId();
            var s = await _context.Set<EmailProviderSettings>().FirstOrDefaultAsync(x => x.CompanyId == companyId);
            if (s == null)
            {
                s = new EmailProviderSettings { CompanyId = companyId, Provider = "Postmark" };
                _context.Set<EmailProviderSettings>().Add(s);
                await _context.SaveChangesAsync();
            }

            return Ok(new {
                s.Provider,
                s.FromEmail,
                s.FromName,
                hasApiKey = !string.IsNullOrEmpty(s.ApiKey),
                apiKeyMask = s.ApiKeyMask
            });
        }

        public class UpdateEmailSettingsDto
        {
            public string Provider { get; set; } = "Postmark";
            public string? ApiKey { get; set; }
            public string? FromEmail { get; set; }
            public string? FromName { get; set; }
        }

        [HttpPut]
        public async Task<ActionResult<object>> Update([FromBody] UpdateEmailSettingsDto dto)
        {
            var companyId = GetCompanyId();
            var s = await _context.Set<EmailProviderSettings>().FirstOrDefaultAsync(x => x.CompanyId == companyId);
            if (s == null)
            {
                s = new EmailProviderSettings { CompanyId = companyId };
                _context.Set<EmailProviderSettings>().Add(s);
            }

            s.Provider = string.IsNullOrWhiteSpace(dto.Provider) ? "Postmark" : dto.Provider;
            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
            {
                s.ApiKey = _encryption.Encrypt(dto.ApiKey);
                s.ApiKeyMask = Mask(dto.ApiKey);
            }
            if (dto.FromEmail != null) s.FromEmail = string.IsNullOrWhiteSpace(dto.FromEmail) ? null : dto.FromEmail;
            if (dto.FromName != null) s.FromName = string.IsNullOrWhiteSpace(dto.FromName) ? null : dto.FromName;
            s.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        private static string Mask(string token)
        {
            if (string.IsNullOrEmpty(token)) return string.Empty;
            var visible = Math.Min(4, token.Length);
            return new string('*', Math.Max(0, token.Length - visible)) + token.Substring(token.Length - visible);
        }
    }
}

