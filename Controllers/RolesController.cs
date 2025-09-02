using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.Roles;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los roles
        /// </summary>
        [HttpGet]
        [RequirePermission("users.read")]  // Changed from roles.read to users.read
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return StatusCode(500, new { message = "Error al obtener los roles" });
            }
        }

        /// <summary>
        /// Obtiene un rol por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("users.read")]  // Changed from roles.read to users.read
        public async Task<ActionResult<RoleDto>> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = $"Rol con id {id} no encontrado" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener rol {id}");
                return StatusCode(500, new { message = "Error al obtener el rol" });
            }
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        [HttpPost]
        [RequirePermission("users.create")]  // Changed from roles.create to users.create
        public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var role = await _roleService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return StatusCode(500, new { message = "Error al crear el rol" });
            }
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("users.write")]  // Changed from roles.update to users.write
        public async Task<ActionResult<RoleDto>> Update(int id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating role {id} with data: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state for role {id}");
                    return BadRequest(ModelState);
                }

                var role = await _roleService.UpdateAsync(id, dto);
                _logger.LogInformation($"Role {id} updated successfully");
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar rol {id}");
                return StatusCode(500, new { message = "Error al actualizar el rol" });
            }
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("users.write")]  // Changed from roles.delete to users.write (using write for delete)
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _roleService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar rol {id}");
                return StatusCode(500, new { message = "Error al eliminar el rol" });
            }
        }

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        [HttpPost("{id}/permissions")]
        [RequirePermission("users.write")]  // Changed from roles.update to users.write
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            try
            {
                await _roleService.AssignPermissionsAsync(id, permissionIds);
                return Ok(new { message = "Permisos asignados correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al asignar permisos al rol {id}");
                return StatusCode(500, new { message = "Error al asignar permisos" });
            }
        }
    }
}