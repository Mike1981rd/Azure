using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UtilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("update-room-slugs")]
        [AllowAnonymous] // Temporal para testing
        public async Task<IActionResult> UpdateRoomSlugs()
        {
            try
            {
                // Get ALL rooms to fix their slugs
                var allRooms = await _context.Rooms
                    .ToListAsync();

                int updatedCount = 0;
                foreach (var room in allRooms)
                {
                    // Always regenerate slug from name to ensure it's URL-friendly
                    var newSlug = GenerateSlug(room.Name);
                    if (room.Slug != newSlug)
                    {
                        room.Slug = newSlug;
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                var updatedRooms = await _context.Rooms
                    .Select(r => new { r.Id, r.Name, r.Slug })
                    .ToListAsync();

                return Ok(new
                {
                    message = $"Updated {updatedCount} rooms with proper slugs",
                    rooms = updatedRooms
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            
            // Convert to lowercase
            var slug = name.ToLowerInvariant();
            
            // Replace accented characters
            slug = slug.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                      .Replace("ñ", "n").Replace("ü", "u")
                      .Replace("à", "a").Replace("è", "e").Replace("ì", "i").Replace("ò", "o").Replace("ù", "u");
            
            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            
            // Remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // Replace multiple hyphens with single hyphen
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            slug = slug.Trim('-');
            
            return slug;
        }
    }
}