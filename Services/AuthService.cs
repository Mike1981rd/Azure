using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Auth;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await GetUserByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Intento de login fallido: Usuario no encontrado {loginDto.Email}");
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Intento de login fallido: Contraseña incorrecta para {loginDto.Email}");
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Intento de login fallido: Usuario inactivo {loginDto.Email}");
                    return null;
                }

                // Actualizar último login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var permissions = await GetUserPermissionsAsync(user.Id);
                var roles = await GetUserRolesAsync(user.Id);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = GenerateRefreshToken(), // Implementar si es necesario
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = new AuthUserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        CompanyId = user.CompanyId,
                        CompanyName = user.Company?.Name,
                        Roles = roles,
                        Permissions = permissions
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return null;
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await GetUserByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Intento de registro con email existente: {registerDto.Email}");
                    return null;
                }

                // Crear nuevo usuario
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    CompanyId = registerDto.CompanyId,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Asignar rol por defecto (Customer)
                var customerRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == Role.Customer);
                
                if (customerRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = customerRole.Id,
                        AssignedAt = DateTime.UtcNow
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                // Login automático después del registro
                var loginDto = new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };
                return await LoginAsync(loginDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            // Case-insensitive match for email to avoid login failures due to casing
            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("companyId", user.CompanyId?.ToString() ?? "")
            };

            // Agregar roles al token
            var roles = GetUserRolesAsync(user.Id).Result;
            
            // Add all roles for authorization (ClaimTypes.Role can have multiple values)
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role)); // Also add as simple "role" claim
            }

            // Agregar permisos al token
            var permissions = GetUserPermissionsAsync(user.Id).Result;
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permissions", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        private string GenerateRefreshToken()
        {
            // Implementar generación de refresh token si es necesario
            return Guid.NewGuid().ToString();
        }
    }
}
