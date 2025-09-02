using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Auth;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt with invalid model state for email: {Email}", loginDto?.Email);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogDebug("Login attempt for user: {Email}", loginDto.Email);
                
                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    _logger.LogWarning("Failed login attempt for email: {Email} - Invalid credentials", loginDto.Email);
                    return Unauthorized(new { message = "Email o contraseña incorrectos" });
                }

                _logger.LogInformation("Successful login for user: {Email} with UserId: {UserId}", 
                    loginDto.Email, result.User?.Id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", loginDto.Email);
                return StatusCode(500, new { message = "Error interno del servidor durante el login" });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration attempt with invalid model state");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogDebug("Registration attempt for email: {Email}", registerDto.Email);
                
                var result = await _authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    _logger.LogWarning("Registration failed - Email already exists: {Email}", registerDto.Email);
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                _logger.LogInformation("New user registered successfully: {Email} with UserId: {UserId}", 
                    registerDto.Email, result.User?.Id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", registerDto.Email);
                return StatusCode(500, new { message = "Error interno del servidor durante el registro" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthUserDto>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var permissions = await _authService.GetUserPermissionsAsync(userId);
            var roles = await _authService.GetUserRolesAsync(userId);
            
            var userDto = new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Permissions = permissions,
                Roles = roles
            };

            return Ok(userDto);
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // En JWT, el logout se maneja del lado del cliente
            // Aquí podrías invalidar el token en una lista negra si es necesario
            _logger.LogInformation($"Usuario {User.Identity?.Name} cerró sesión");
            return Ok(new { message = "Sesión cerrada exitosamente" });
        }
    }
}