using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Permissions;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermissionDto>> GetAllAsync()
        {
            var permissions = await _context.Permissions
                .OrderBy(p => p.Resource)
                .ThenBy(p => p.Action)
                .ToListAsync();

            return permissions.Select(p => MapToDto(p)).ToList();
        }

        public async Task<List<GroupedPermissionsDto>> GetGroupedAsync()
        {
            var permissions = await GetAllAsync();

            var grouped = permissions
                .GroupBy(p => p.Resource)
                .Select(g => new GroupedPermissionsDto
                {
                    Resource = g.Key,
                    DisplayName = GetResourceDisplayName(g.Key),
                    Permissions = g.ToList()
                })
                .OrderBy(g => g.DisplayName)
                .ToList();

            return grouped;
        }

        public async Task<List<PermissionDto>> GetByResourceAsync(string resource)
        {
            var permissions = await _context.Permissions
                .Where(p => p.Resource == resource)
                .OrderBy(p => p.Action)
                .ToListAsync();

            return permissions.Select(p => MapToDto(p)).ToList();
        }

        public async Task SeedPermissionsAsync()
        {
            var existingPermissions = await _context.Permissions.ToListAsync();
            var permissionsToAdd = new List<Permission>();

            // Definir todos los permisos del sistema
            var systemPermissions = new List<(string Resource, string Action, string Description)>
            {
                // Dashboard
                ("dashboard", "view", "Ver dashboard en el menú"),
                ("dashboard", "read", "Acceder al dashboard"),

                // Users
                ("users", "view", "Ver usuarios en el menú"),
                ("users", "read", "Listar usuarios"),
                ("users", "create", "Crear nuevos usuarios"),
                ("users", "update", "Editar usuarios"),
                ("users", "delete", "Eliminar usuarios"),

                // Roles
                ("roles", "view", "Ver roles en el menú"),
                ("roles", "read", "Listar roles"),
                ("roles", "create", "Crear nuevos roles"),
                ("roles", "update", "Editar roles"),
                ("roles", "delete", "Eliminar roles"),

                // Products
                ("products", "view", "Ver productos en el menú"),
                ("products", "read", "Listar productos"),
                ("products", "create", "Crear nuevos productos"),
                ("products", "update", "Editar productos"),
                ("products", "delete", "Eliminar productos"),

                // Rooms
                ("rooms", "view", "Ver habitaciones en el menú"),
                ("rooms", "read", "Listar habitaciones"),
                ("rooms", "create", "Crear nuevas habitaciones"),
                ("rooms", "update", "Editar habitaciones"),
                ("rooms", "delete", "Eliminar habitaciones"),

                // Reservations
                ("reservations", "view", "Ver reservaciones en el menú"),
                ("reservations", "read", "Listar reservaciones"),
                ("reservations", "create", "Crear nuevas reservaciones"),
                ("reservations", "update", "Editar reservaciones"),
                ("reservations", "delete", "Cancelar reservaciones"),

                // Website
                ("website", "view", "Ver website builder en el menú"),
                ("website", "read", "Acceder al website builder"),
                ("website", "update", "Editar contenido del sitio web"),

                // Reports
                ("reports", "view", "Ver reportes en el menú"),
                ("reports", "read", "Acceder a reportes"),

                // Settings
                ("settings", "view", "Ver configuración en el menú"),
                ("settings", "read", "Ver configuración"),
                ("settings", "update", "Modificar configuración"),

                // Collections
                ("collections", "view", "Ver colecciones en el menú"),
                ("collections", "read", "Listar colecciones"),
                ("collections", "create", "Crear nuevas colecciones"),
                ("collections", "update", "Editar colecciones"),
                ("collections", "delete", "Eliminar colecciones"),

                // Payment Methods
                ("payment_methods", "view", "Ver métodos de pago en el menú"),
                ("payment_methods", "read", "Listar métodos de pago"),
                ("payment_methods", "create", "Crear nuevos métodos de pago"),
                ("payment_methods", "update", "Editar métodos de pago"),
                ("payment_methods", "delete", "Eliminar métodos de pago"),

                // Customers
                ("customers", "view", "Ver clientes en el menú"),
                ("customers", "read", "Listar clientes"),
                ("customers", "create", "Crear nuevos clientes"),
                ("customers", "update", "Editar clientes"),
                ("customers", "delete", "Eliminar clientes"),

                // Company/Hotel
                ("company", "view", "Ver empresa en el menú"),
                ("company", "read", "Ver información de la empresa"),
                ("company", "update", "Editar información de la empresa"),

                // Domains
                ("domains", "view", "Ver dominios en el menú"),
                ("domains", "read", "Listar dominios"),
                ("domains", "create", "Crear nuevos dominios"),
                ("domains", "update", "Editar dominios"),
                ("domains", "delete", "Eliminar dominios"),

                // Policies
                ("policies", "view", "Ver políticas en el menú"),
                ("policies", "read", "Listar políticas"),
                ("policies", "create", "Crear nuevas políticas"),
                ("policies", "update", "Editar políticas"),
                ("policies", "delete", "Eliminar políticas")
            };

            foreach (var (resource, action, description) in systemPermissions)
            {
                if (!existingPermissions.Any(p => p.Resource == resource && p.Action == action))
                {
                    permissionsToAdd.Add(new Permission
                    {
                        Resource = resource,
                        Action = action,
                        Description = description
                    });
                }
            }

            if (permissionsToAdd.Any())
            {
                _context.Permissions.AddRange(permissionsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        private string GetResourceDisplayName(string resource)
        {
            return resource switch
            {
                "dashboard" => "Dashboard",
                "users" => "Usuarios",
                "roles" => "Roles",
                "products" => "Productos",
                "rooms" => "Habitaciones",
                "reservations" => "Reservaciones",
                "website" => "Sitio Web",
                "reports" => "Reportes",
                "settings" => "Configuración",
                "collections" => "Colecciones",
                "payment_methods" => "Métodos de Pago",
                "customers" => "Clientes",
                "company" => "Empresa",
                "domains" => "Dominios",
                "policies" => "Políticas",
                _ => resource
            };
        }

        private PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Resource = permission.Resource,
                Action = permission.Action,
                Description = permission.Description
            };
        }
    }
}