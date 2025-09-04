using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.NavigationMenu;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NavigationMenuController : ControllerBase
    {
        private readonly INavigationMenuService _navigationMenuService;
        private readonly ILogger<NavigationMenuController> _logger;

        public NavigationMenuController(INavigationMenuService navigationMenuService, ILogger<NavigationMenuController> logger)
        {
            _navigationMenuService = navigationMenuService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var result = await _navigationMenuService.GetAllAsync(companyId, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menus");
                return StatusCode(500, new { error = "An error occurred while getting navigation menus" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var menu = await _navigationMenuService.GetByIdAsync(companyId, id);
                if (menu == null)
                    return NotFound(new { error = "Navigation menu not found" });

                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu {MenuId}", id);
                return StatusCode(500, new { error = "An error occurred while getting the navigation menu" });
            }
        }

        [HttpGet("{id}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdPublic(int id)
        {
            try
            {
                // For public access, we need to determine the company ID differently
                // In a real scenario, this might come from the domain or a header
                // For now, we'll use company 1 as default
                int companyId = 1;

                _logger.LogInformation("Getting public navigation menu {MenuId} for company {CompanyId}", id, companyId);

                var menu = await _navigationMenuService.GetByIdAsync(companyId, id);
                if (menu == null)
                {
                    _logger.LogWarning("Navigation menu {MenuId} not found for company {CompanyId}", id, companyId);
                    // Return empty menu structure to avoid breaking the frontend
                    return Ok(new NavigationMenuResponseDto
                    {
                        Id = id,
                        CompanyId = companyId,
                        Name = "Default Menu",
                        Identifier = "default",
                        MenuType = "header",
                        Items = new List<MenuItemDto>(),
                        IsActive = true,
                        ItemCount = 0,
                        ItemsSummary = string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public navigation menu {MenuId}. Exception: {Message}", id, ex.Message);
                // Return empty menu structure to avoid breaking the frontend
                return Ok(new NavigationMenuResponseDto
                {
                    Id = id,
                    CompanyId = 1,
                    Name = "Default Menu",
                    Identifier = "default",
                    MenuType = "header",
                    Items = new List<MenuItemDto>(),
                    IsActive = true,
                    ItemCount = 0,
                    ItemsSummary = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        [HttpGet("by-identifier/{identifier}")]
        public async Task<IActionResult> GetByIdentifier(string identifier)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var menu = await _navigationMenuService.GetByIdentifierAsync(companyId, identifier);
                if (menu == null)
                    return NotFound(new { error = "Navigation menu not found" });

                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu by identifier {Identifier}", identifier);
                return StatusCode(500, new { error = "An error occurred while getting the navigation menu" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMenus()
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var menus = await _navigationMenuService.GetActiveMenusAsync(companyId);
                return Ok(menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active navigation menus");
                return StatusCode(500, new { error = "An error occurred while getting active navigation menus" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNavigationMenuDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var menu = await _navigationMenuService.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetById), new { id = menu.Id }, menu);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating navigation menu");
                return StatusCode(500, new { error = "An error occurred while creating the navigation menu" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNavigationMenuDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var menu = await _navigationMenuService.UpdateAsync(companyId, id, dto);
                if (menu == null)
                    return NotFound(new { error = "Navigation menu not found" });

                return Ok(menu);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating navigation menu {MenuId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the navigation menu" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var result = await _navigationMenuService.DeleteAsync(companyId, id);
                if (!result)
                    return NotFound(new { error = "Navigation menu not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting navigation menu {MenuId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the navigation menu" });
            }
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var result = await _navigationMenuService.ToggleActiveAsync(companyId, id);
                if (!result)
                    return NotFound(new { error = "Navigation menu not found" });

                return Ok(new { message = "Active status toggled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for menu {MenuId}", id);
                return StatusCode(500, new { error = "An error occurred while toggling the active status" });
            }
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var result = await _navigationMenuService.DuplicateAsync(companyId, id);
                if (!result)
                    return NotFound(new { error = "Navigation menu not found" });

                return Ok(new { message = "Navigation menu duplicated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating navigation menu {MenuId}", id);
                return StatusCode(500, new { error = "An error occurred while duplicating the navigation menu" });
            }
        }
    }
}