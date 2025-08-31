using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.Permissions;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles
        /// </summary>
        [HttpGet]
        [RequirePermission("users.read")]  // Changed from roles.read to users.read
        public async Task<ActionResult<List<PermissionDto>>> GetAll()
        {
            try
            {
                var permissions = await _permissionService.GetAllAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos");
                return StatusCode(500, new { message = "Error al obtener los permisos" });
            }
        }

        /// <summary>
        /// Obtiene los permisos agrupados por recurso
        /// </summary>
        [HttpGet("grouped")]
        [RequirePermission("users.read")]  // Changed from roles.read to users.read
        public async Task<ActionResult<List<GroupedPermissionsDto>>> GetGrouped()
        {
            try
            {
                var groupedPermissions = await _permissionService.GetGroupedAsync();
                return Ok(groupedPermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos agrupados");
                return StatusCode(500, new { message = "Error al obtener los permisos agrupados" });
            }
        }

        /// <summary>
        /// Obtiene los permisos de un recurso específico
        /// </summary>
        [HttpGet("by-resource/{resource}")]
        [RequirePermission("users.read")]  // Changed from roles.read to users.read
        public async Task<ActionResult<List<PermissionDto>>> GetByResource(string resource)
        {
            try
            {
                var permissions = await _permissionService.GetByResourceAsync(resource);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener permisos del recurso {resource}");
                return StatusCode(500, new { message = "Error al obtener los permisos del recurso" });
            }
        }

        /// <summary>
        /// Inicializa los permisos del sistema (solo SuperAdmin)
        /// </summary>
        [HttpPost("seed")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SeedPermissions()
        {
            try
            {
                await _permissionService.SeedPermissionsAsync();
                return Ok(new { message = "Permisos inicializados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar permisos");
                return StatusCode(500, new { message = "Error al inicializar los permisos" });
            }
        }
    }
}