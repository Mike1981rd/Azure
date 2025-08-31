using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Data
{
    public static class PermissionsSeeder
    {
        public static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            // Define todos los recursos basados en el panel lateral
            var resources = new[]
            {
                new { Name = "dashboard", Description = "Dashboard" },
                new { Name = "company", Description = "Empresa/Company Configuration" },
                new { Name = "users", Description = "Users and Roles Management" }, // Combinado: users y roles
                new { Name = "clients", Description = "Clients/Customers" },
                new { Name = "products", Description = "Products Catalog" },
                new { Name = "collections", Description = "Product Collections" },
                new { Name = "notifications", Description = "Notifications System" },
                new { Name = "whatsapp", Description = "WhatsApp Integration" },
                new { Name = "orders", Description = "Orders Management" },
                new { Name = "subscribers", Description = "Newsletter Subscribers" },
                new { Name = "navigation", Description = "Navigation Menus" },
                new { Name = "rooms", Description = "Hotel Rooms" },
                new { Name = "pages", Description = "Website Pages" },
                new { Name = "policies", Description = "Legal Policies" },
                new { Name = "domains", Description = "Domain Management" },
                new { Name = "reservations", Description = "Room Reservations" },
                new { Name = "website", Description = "Website Builder" },
                new { Name = "payments", Description = "Payment Methods" },
                new { Name = "shipping", Description = "Shipping Configuration" },
                new { Name = "locations", Description = "Store Locations" },
                new { Name = "checkout", Description = "Checkout Settings" }
            };

            // Define las acciones estándar para cada recurso (solo 3)
            var actions = new[]
            {
                new { Name = "read", Description = "Read/View" },
                new { Name = "write", Description = "Write/Edit" },
                new { Name = "create", Description = "Create new" }
            };

            var permissions = new List<Permission>();

            // Generar permisos para cada combinación de recurso y acción
            foreach (var resource in resources)
            {
                foreach (var action in actions)
                {
                    var permissionName = $"{resource.Name}.{action.Name}";
                    
                    // Verificar si el permiso ya existe
                    var existingPermission = await context.Permissions
                        .FirstOrDefaultAsync(p => p.Resource == resource.Name && p.Action == action.Name);

                    if (existingPermission == null)
                    {
                        permissions.Add(new Permission
                        {
                            Resource = resource.Name,
                            Action = action.Name,
                            Description = $"{action.Description} {resource.Description}"
                        });
                    }
                }
            }

            // Agregar permisos especiales que no siguen el patrón CRUD
            var specialPermissions = new[]
            {
                new Permission { Resource = "system", Action = "admin", Description = "Full system administration" },
                new Permission { Resource = "reports", Action = "view", Description = "View system reports" },
                new Permission { Resource = "reports", Action = "export", Description = "Export reports" },
                new Permission { Resource = "settings", Action = "manage", Description = "Manage system settings" },
                new Permission { Resource = "backup", Action = "create", Description = "Create system backups" },
                new Permission { Resource = "backup", Action = "restore", Description = "Restore system backups" },
                new Permission { Resource = "logs", Action = "view", Description = "View system logs" },
                new Permission { Resource = "cache", Action = "clear", Description = "Clear system cache" }
            };

            foreach (var permission in specialPermissions)
            {
                var existingPermission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Resource == permission.Resource && p.Action == permission.Action);

                if (existingPermission == null)
                {
                    permissions.Add(permission);
                }
            }

            // Guardar todos los permisos nuevos
            if (permissions.Any())
            {
                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            // Definir roles predeterminados del sistema
            var systemRoles = new[]
            {
                new
                {
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full system access",
                    IsSystemRole = true,
                    PermissionPatterns = new[] { "*.*" } // Todos los permisos
                },
                new
                {
                    Name = "Administrator",
                    Description = "Administrator with company management access",
                    IsSystemRole = false,  // Changed to false - only SuperAdmin should be protected
                    PermissionPatterns = new[]
                    {
                        "dashboard.*", "company.*", "users.*", 
                        "products.*", "collections.*", "pages.*", "website.*",
                        "settings.*", "reports.*"
                    }
                },
                new
                {
                    Name = "Manager",
                    Description = "Manager with operational access",
                    IsSystemRole = false,
                    PermissionPatterns = new[]
                    {
                        "dashboard.read",
                        "products.*", "collections.*", "orders.*", "clients.*",
                        "reservations.*", "rooms.*", "notifications.*",
                        "reports.read"
                    }
                },
                new
                {
                    Name = "Editor",
                    Description = "Content editor with limited access",
                    IsSystemRole = false,
                    PermissionPatterns = new[]
                    {
                        "dashboard.read",
                        "products.read", "products.write", "products.create",
                        "collections.read", "collections.write", "collections.create",
                        "pages.read", "pages.write", "pages.create",
                        "policies.read", "policies.write"
                    }
                },
                new
                {
                    Name = "Support",
                    Description = "Customer support staff",
                    IsSystemRole = false,
                    PermissionPatterns = new[]
                    {
                        "dashboard.view", "dashboard.read",
                        "clients.view", "clients.read", "clients.update",
                        "orders.view", "orders.read", "orders.update",
                        "reservations.view", "reservations.read", "reservations.update",
                        "notifications.view", "notifications.read", "notifications.create",
                        "whatsapp.view", "whatsapp.read", "whatsapp.create"
                    }
                },
                new
                {
                    Name = "Viewer",
                    Description = "Read-only access to basic information",
                    IsSystemRole = false,
                    PermissionPatterns = new[]
                    {
                        "dashboard.view", "dashboard.read",
                        "products.view", "products.read",
                        "collections.view", "collections.read",
                        "orders.view", "orders.read",
                        "clients.view", "clients.read"
                    }
                }
            };

            foreach (var roleData in systemRoles)
            {
                // Verificar si el rol ya existe
                var existingRole = await context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Name == roleData.Name);

                if (existingRole == null)
                {
                    // Crear nuevo rol
                    var role = new Role
                    {
                        Name = roleData.Name,
                        Description = roleData.Description,
                        IsSystemRole = roleData.IsSystemRole,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Roles.Add(role);
                    await context.SaveChangesAsync();

                    // Asignar permisos al rol
                    await AssignPermissionsToRoleAsync(context, role, roleData.PermissionPatterns);
                }
                else
                {
                    // Actualizar permisos del rol existente si es un rol del sistema
                    if (existingRole.IsSystemRole)
                    {
                        // Limpiar permisos existentes
                        context.RolePermissions.RemoveRange(existingRole.RolePermissions);
                        await context.SaveChangesAsync();

                        // Reasignar permisos
                        await AssignPermissionsToRoleAsync(context, existingRole, roleData.PermissionPatterns);
                    }
                }
            }
        }

        private static async Task AssignPermissionsToRoleAsync(ApplicationDbContext context, Role role, string[] permissionPatterns)
        {
            var allPermissions = await context.Permissions.ToListAsync();
            var rolePermissions = new List<RolePermission>();

            foreach (var pattern in permissionPatterns)
            {
                if (pattern == "*.*")
                {
                    // Asignar todos los permisos
                    foreach (var permission in allPermissions)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }
                else if (pattern.EndsWith(".*"))
                {
                    // Asignar todos los permisos de un recurso
                    var resource = pattern.Replace(".*", "");
                    var resourcePermissions = allPermissions.Where(p => p.Resource == resource);
                    
                    foreach (var permission in resourcePermissions)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    // Asignar permiso específico
                    var parts = pattern.Split('.');
                    if (parts.Length == 2)
                    {
                        var permission = allPermissions.FirstOrDefault(p => 
                            p.Resource == parts[0] && p.Action == parts[1]);
                        
                        if (permission != null)
                        {
                            rolePermissions.Add(new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = permission.Id,
                                GrantedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            if (rolePermissions.Any())
            {
                context.RolePermissions.AddRange(rolePermissions);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedDefaultUserAsync(ApplicationDbContext context)
        {
            // Crear usuario super admin por defecto si no existe
            var superAdminEmail = "admin@websitebuilder.com";
            var existingUser = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == superAdminEmail);

            if (existingUser == null)
            {
                // Obtener el rol SuperAdmin
                var superAdminRole = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

                if (superAdminRole != null)
                {
                    // Obtener la empresa por defecto
                    var company = await context.Companies.FirstOrDefaultAsync();

                    var adminUser = new User
                    {
                        Email = superAdminEmail,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        FirstName = "Super",
                        LastName = "Admin",
                        CompanyId = company?.Id,
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();

                    // Asignar rol SuperAdmin
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = superAdminRole.Id,
                        AssignedAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}