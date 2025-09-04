using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.NavigationMenu;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class NavigationMenuService : INavigationMenuService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NavigationMenuService> _logger;

        public NavigationMenuService(ApplicationDbContext context, ILogger<NavigationMenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<NavigationMenuResponseDto>> GetAllAsync(int companyId, int page = 1, int pageSize = 20, string? search = null)
        {
            try
            {
                var query = _context.NavigationMenus
                    .Where(m => m.CompanyId == companyId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(m => 
                        m.Name.Contains(search) || 
                        m.Identifier.Contains(search) ||
                        (m.MenuType != null && m.MenuType.Contains(search)));
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var menus = await query
                    .OrderByDescending(m => m.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => MapToResponseDto(m))
                    .ToListAsync();

                return new PagedResult<NavigationMenuResponseDto>
                {
                    Items = menus,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menus for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<NavigationMenuResponseDto?> GetByIdAsync(int companyId, int id)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

                return menu != null ? MapToResponseDto(menu) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu {Id} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task<NavigationMenuResponseDto?> GetByIdentifierAsync(int companyId, string identifier)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Identifier == identifier && m.CompanyId == companyId);

                return menu != null ? MapToResponseDto(menu) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu by identifier {Identifier} for company {CompanyId}", identifier, companyId);
                throw;
            }
        }

        public async Task<NavigationMenuResponseDto> CreateAsync(int companyId, CreateNavigationMenuDto dto)
        {
            try
            {
                // Verificar si el identificador ya existe
                var existingMenu = await _context.NavigationMenus
                    .AnyAsync(m => m.Identifier == dto.Identifier && m.CompanyId == companyId);

                if (existingMenu)
                {
                    throw new InvalidOperationException($"A menu with identifier '{dto.Identifier}' already exists");
                }

                var menu = new NavigationMenu
                {
                    CompanyId = companyId,
                    Name = dto.Name,
                    Identifier = dto.Identifier,
                    MenuType = dto.MenuType,
                    Items = JsonSerializer.Serialize(dto.Items),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.NavigationMenus.Add(menu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created navigation menu {MenuId} for company {CompanyId}", menu.Id, companyId);
                return MapToResponseDto(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating navigation menu for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<NavigationMenuResponseDto?> UpdateAsync(int companyId, int id, UpdateNavigationMenuDto dto)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

                if (menu == null)
                    return null;

                // Verificar si el nuevo identificador ya existe (si se está cambiando)
                if (!string.IsNullOrWhiteSpace(dto.Identifier) && dto.Identifier != menu.Identifier)
                {
                    var existingMenu = await _context.NavigationMenus
                        .AnyAsync(m => m.Identifier == dto.Identifier && m.CompanyId == companyId && m.Id != id);

                    if (existingMenu)
                    {
                        throw new InvalidOperationException($"A menu with identifier '{dto.Identifier}' already exists");
                    }
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    menu.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Identifier))
                    menu.Identifier = dto.Identifier;

                if (dto.MenuType != null)
                    menu.MenuType = dto.MenuType;

                if (dto.Items != null)
                    menu.Items = JsonSerializer.Serialize(dto.Items);

                if (dto.IsActive.HasValue)
                    menu.IsActive = dto.IsActive.Value;

                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated navigation menu {MenuId} for company {CompanyId}", id, companyId);
                return MapToResponseDto(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating navigation menu {MenuId} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

                if (menu == null)
                    return false;

                _context.NavigationMenus.Remove(menu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted navigation menu {MenuId} for company {CompanyId}", id, companyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting navigation menu {MenuId} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task<bool> ToggleActiveAsync(int companyId, int id)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

                if (menu == null)
                    return false;

                menu.IsActive = !menu.IsActive;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled active status for navigation menu {MenuId} to {IsActive}", id, menu.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for menu {MenuId}", id);
                throw;
            }
        }

        public async Task<List<NavigationMenuResponseDto>> GetActiveMenusAsync(int companyId)
        {
            try
            {
                var menus = await _context.NavigationMenus
                    .Where(m => m.CompanyId == companyId && m.IsActive)
                    .OrderBy(m => m.MenuType)
                    .ThenBy(m => m.Name)
                    .Select(m => MapToResponseDto(m))
                    .ToListAsync();

                return menus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active menus for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> DuplicateAsync(int companyId, int id)
        {
            try
            {
                var menu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

                if (menu == null)
                    return false;

                // Generar nuevo identificador único
                var newIdentifier = $"{menu.Identifier}-copy";
                var counter = 1;
                while (await _context.NavigationMenus.AnyAsync(m => m.Identifier == newIdentifier && m.CompanyId == companyId))
                {
                    newIdentifier = $"{menu.Identifier}-copy-{counter++}";
                }

                var newMenu = new NavigationMenu
                {
                    CompanyId = companyId,
                    Name = $"{menu.Name} (Copy)",
                    Identifier = newIdentifier,
                    MenuType = menu.MenuType,
                    Items = menu.Items,
                    IsActive = false, // Duplicados empiezan inactivos
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.NavigationMenus.Add(newMenu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Duplicated navigation menu {OriginalId} to {NewId}", id, newMenu.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating navigation menu {MenuId}", id);
                throw;
            }
        }

        private static NavigationMenuResponseDto MapToResponseDto(NavigationMenu menu)
        {
            var items = new List<MenuItemDto>();
            var itemsSummary = "";

            if (!string.IsNullOrWhiteSpace(menu.Items))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    items = JsonSerializer.Deserialize<List<MenuItemDto>>(menu.Items, options) ?? new List<MenuItemDto>();
                    
                    // Crear resumen de elementos para la lista (primeros 3 elementos)
                    var labels = items.Take(3).Select(i => i.Label ?? "Untitled").ToList();
                    itemsSummary = string.Join(", ", labels);
                    if (items.Count > 3)
                        itemsSummary += $" +{items.Count - 3} more";
                }
                catch (JsonException jsonEx)
                {
                    // Log the actual JSON error for debugging
                    Console.WriteLine($"JSON Deserialization error for menu {menu.Id}: {jsonEx.Message}");
                    Console.WriteLine($"JSON content: {menu.Items}");
                    items = new List<MenuItemDto>();
                    itemsSummary = "Invalid menu data";
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Error mapping menu {menu.Id}: {ex.Message}");
                    items = new List<MenuItemDto>();
                    itemsSummary = "Error loading menu";
                }
            }

            return new NavigationMenuResponseDto
            {
                Id = menu.Id,
                CompanyId = menu.CompanyId,
                Name = menu.Name ?? "Unnamed Menu",
                Identifier = menu.Identifier ?? $"menu-{menu.Id}",
                MenuType = menu.MenuType ?? "header",
                Items = items,
                IsActive = menu.IsActive,
                CreatedAt = menu.CreatedAt,
                UpdatedAt = menu.UpdatedAt,
                ItemCount = items.Count,
                ItemsSummary = itemsSummary
            };
        }
    }
}