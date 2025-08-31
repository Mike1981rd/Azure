using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.Users;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        [HttpGet]
        [RequirePermission("users.read")]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            try
            {
                // Ya no necesitamos filtrar por hotel porque solo hay una empresa
                var users = await _userService.GetAllAsync(null);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = "Error al obtener los usuarios" });
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("users.read")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"Usuario con id {id} no encontrado" });
                }

                // Verificar que el usuario pertenece al hotel actual
                // Ya no necesitamos el hotel id porque solo hay una empresa
                var isSuperAdmin = User.IsInRole("SuperAdmin");
                
                // Ya no validamos hotel porque solo hay una empresa
                // Todos los usuarios autenticados pueden ver detalles de usuarios

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario {id}");
                return StatusCode(500, new { message = "Error al obtener el usuario" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        [RequirePermission("users.create")]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ya no necesitamos el hotel id porque solo hay una empresa
                var user = await _userService.CreateAsync(dto, null);
                
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { message = "Error al crear el usuario" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("users.write")]  // Changed from users.update to users.write
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar permisos sobre el usuario
                var existingUser = await _userService.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = $"Usuario con id {id} no encontrado" });
                }

                // Ya no necesitamos el hotel id porque solo hay una empresa
                var isSuperAdmin = User.IsInRole("SuperAdmin");
                
                // Ya no validamos hotel porque solo hay una empresa
                // Todos los usuarios con permisos pueden actualizar

                var user = await _userService.UpdateAsync(id, dto);
                return Ok(user);
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
                _logger.LogError(ex, $"Error al actualizar usuario {id}");
                return StatusCode(500, new { message = "Error al actualizar el usuario" });
            }
        }

        /// <summary>
        /// Desactiva un usuario (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("users.write")]  // Changed from users.delete to users.write (using write for delete)
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verificar permisos sobre el usuario
                var existingUser = await _userService.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = $"Usuario con id {id} no encontrado" });
                }

                // Ya no necesitamos el hotel id porque solo hay una empresa
                var isSuperAdmin = User.IsInRole("SuperAdmin");
                
                // Ya no validamos hotel porque solo hay una empresa
                // Todos los usuarios con permisos pueden eliminar

                // No permitir auto-eliminación
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (id == currentUserId)
                {
                    return BadRequest(new { message = "No puede eliminar su propio usuario" });
                }

                await _userService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario {id}");
                return StatusCode(500, new { message = "Error al eliminar el usuario" });
            }
        }

        /// <summary>
        /// Obtiene los permisos efectivos de un usuario
        /// </summary>
        [HttpGet("{id}/permissions")]
        [RequirePermission("users.read")]
        public async Task<ActionResult<List<string>>> GetEffectivePermissions(int id)
        {
            try
            {
                // Verificar permisos sobre el usuario
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"Usuario con id {id} no encontrado" });
                }

                // Ya no necesitamos el hotel id porque solo hay una empresa
                var isSuperAdmin = User.IsInRole("SuperAdmin");
                
                // Ya no validamos hotel porque solo hay una empresa
                // Todos los usuarios con permisos pueden ver permisos

                var permissions = await _userService.GetEffectivePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener permisos del usuario {id}");
                return StatusCode(500, new { message = "Error al obtener los permisos" });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario actual
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _userService.ChangePasswordAsync(currentUserId, dto);
                
                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { message = "Error al cambiar la contraseña" });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario actual
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetByIdAsync(currentUserId);
                
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new { message = "Error al obtener el perfil" });
            }
        }

        /// <summary>
        /// Asignar roles a un usuario
        /// </summary>
        [HttpPost("{id}/roles")]
        [RequirePermission("users.write")]  // Changed from users.update to users.write
        public async Task<IActionResult> AssignRoles(int id, [FromBody] List<int> roleIds)
        {
            try
            {
                await _userService.AssignRolesToUserAsync(id, roleIds);
                return Ok(new { message = "Roles asignados correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al asignar roles al usuario {id}");
                return StatusCode(500, new { message = "Error al asignar roles" });
            }
        }
    }
}