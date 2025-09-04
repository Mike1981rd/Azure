using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Host;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HostService _hostService;
        private readonly ILogger<HostsController> _logger;
        
        public HostsController(ApplicationDbContext context, HostService hostService, ILogger<HostsController> logger)
        {
            _context = context;
            _hostService = hostService;
            _logger = logger;
        }
        
        // GET: api/hosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HostListDto>>> GetHosts()
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var hosts = await _hostService.GetAllHostsAsync(companyId);
            return Ok(hosts);
        }
        
        // GET: api/hosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HostDetailDto>> GetHost(int id)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var host = await _hostService.GetHostByIdAsync(id, companyId);
            
            if (host == null)
                return NotFound();
                
            return Ok(host);
        }
        
        // GET: api/hosts/by-room/5
        [HttpGet("by-room/{roomId}")]
        [AllowAnonymous] // Allow anonymous for public room pages
        public async Task<ActionResult<HostDetailDto>> GetHostByRoomId(int roomId)
        {
            // Get room with host information
            var room = await _context.Rooms
                .Include(r => r.Host)
                .Where(r => r.Id == roomId && r.IsActive)
                .FirstOrDefaultAsync();
                
            if (room == null || room.Host == null || !room.HostId.HasValue)
                return NotFound();
                
            var hostDto = await _hostService.GetHostByIdAsync(room.HostId.Value, room.CompanyId);
            
            if (hostDto == null)
                return NotFound();
                
            return Ok(hostDto);
        }
        
        // POST: api/hosts
        [HttpPost]
        public async Task<ActionResult<WebsiteBuilderAPI.Models.Host>> CreateHost(CreateHostDto dto)
        {
            _logger.LogInformation("CreateHost called with data: {@dto}", dto);
            
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
                _logger.LogWarning("CompanyId not found in token, using default: {companyId}", companyId);
            }
                
            // Try multiple common claim types for user identifier to be robust across issuers
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value ??
                User.FindFirst(ClaimTypes.Name)?.Value;
            _logger.LogInformation("UserId from token: {userId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID not found in token");
                return BadRequest("User ID not found");
            }
                
            dto.CompanyId = companyId;
            
            try
            {
                _logger.LogInformation("Calling CreateHostAsync with companyId: {companyId}, userId: {userId}", companyId, userId);
                var host = await _hostService.CreateHostAsync(dto, userId);
                _logger.LogInformation("Host created successfully with Id: {hostId}", host.Id);
                return CreatedAtAction(nameof(GetHost), new { id = host.Id }, host);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating host: {message}", ex.Message);
                return BadRequest(new { message = "Error creating host", error = ex.Message, details = ex.InnerException?.Message });
            }
        }
        
        // PUT: api/hosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHost(int id, UpdateHostDto dto)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var result = await _hostService.UpdateHostAsync(id, dto, companyId);
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
        
        // PATCH: api/hosts/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchHost(int id, [FromBody] Dictionary<string, object> updates)
        {
            _logger.LogInformation("PatchHost called for id: {id} with updates: {@updates}", id, updates);
            
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
                _logger.LogWarning("CompanyId not found in token, using default: {companyId}", companyId);
            }
            
            // Create UpdateHostDto with only the fields that are being updated
            var dto = new UpdateHostDto();
            
            // Handle isActive update
            if (updates.ContainsKey("isActive") && updates["isActive"] != null)
            {
                if (bool.TryParse(updates["isActive"].ToString(), out bool isActive))
                {
                    dto.IsActive = isActive;
                }
            }
            
            // Add other fields as needed in the future
            if (updates.ContainsKey("isSuperhost") && updates["isSuperhost"] != null)
            {
                if (bool.TryParse(updates["isSuperhost"].ToString(), out bool isSuperhost))
                {
                    dto.IsSuperhost = isSuperhost;
                }
            }
            
            try
            {
                var result = await _hostService.UpdateHostAsync(id, dto, companyId);
                
                if (!result)
                {
                    _logger.LogWarning("Host not found or not authorized: {id}", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Host patched successfully: {id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching host: {message}", ex.Message);
                return BadRequest(new { message = "Error updating host", error = ex.Message });
            }
        }
        
        // DELETE: api/hosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHost(int id)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var result = await _hostService.DeleteHostAsync(id, companyId);
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
        
        // GET: api/hosts/search?q=john
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<HostListDto>>> SearchHosts(string q)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var hosts = await _context.Hosts
                .Where(h => h.CompanyId == companyId && 
                           (h.FirstName.Contains(q) || 
                            h.LastName.Contains(q) || 
                            h.Email.Contains(q)))
                .Select(h => new HostListDto
                {
                    Id = h.Id,
                    FirstName = h.FirstName,
                    LastName = h.LastName,
                    FullName = h.FullName,
                    Email = h.Email,
                    ProfilePicture = h.ProfilePicture,
                    OverallRating = h.OverallRating,
                    TotalReviews = h.TotalReviews,
                    ResponseTimeMinutes = h.ResponseTimeMinutes,
                    IsSuperhost = h.IsSuperhost,
                    IsActive = h.IsActive,
                    JoinedDate = h.JoinedDate,
                    RoomCount = h.Rooms.Count(),
                    IsVerified = h.IsVerified
                })
                .ToListAsync();
                
            return Ok(hosts);
        }
        
        // POST: api/hosts/5/verify
        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyHost(int id, [FromBody] VerifyHostDto dto)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var host = await _context.Hosts
                .Where(h => h.Id == id && h.CompanyId == companyId)
                .FirstOrDefaultAsync();
                
            if (host == null)
                return NotFound();
                
            if (dto.VerifyPhone)
                host.IsPhoneVerified = true;
            if (dto.VerifyEmail)
                host.IsEmailVerified = true;
            if (dto.VerifyIdentity)
                host.IsIdentityVerified = true;
                
            host.IsVerified = host.IsPhoneVerified && host.IsEmailVerified && host.IsIdentityVerified;
            host.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // GET: api/hosts/5/reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<HostReview>>> GetHostReviews(int id)
        {
            // Match the token's lowercase claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int companyId;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                // Fallback to default company (same as CompanyService)
                companyId = 1;
            }
                
            var reviews = await _context.HostReviews
                .Include(r => r.Customer)
                .Where(r => r.HostId == id && r.CompanyId == companyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
                
            return Ok(reviews);
        }
    }
    
    public class VerifyHostDto
    {
        public bool VerifyPhone { get; set; }
        public bool VerifyEmail { get; set; }
        public bool VerifyIdentity { get; set; }
    }
}
