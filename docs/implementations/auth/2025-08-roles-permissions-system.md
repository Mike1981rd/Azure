# Sistema de Roles y Permisos RBAC - Implementación Completa

## Overview
- **Purpose**: Implementar un sistema RBAC (Role-Based Access Control) completo con filtrado dinámico de UI
- **Scope**: Backend (ASP.NET Core) + Frontend (Next.js) con permisos granulares
- **Dependencies**: JWT, Entity Framework Core, React Hooks
- **Date Implemented**: 2025-08-08
- **Last Updated**: 2025-08-08
- **Time Spent**: ~5 horas (initial + fixes)

## Architecture Decisions

### Pattern Used
- **RBAC Pattern**: Usuario → Rol → Permisos
- **JWT Claims**: Permisos incluidos en el token para validación rápida
- **Dynamic UI Filtering**: Sidebar y componentes se filtran según permisos
- **Hook Pattern**: usePermissions para reutilización de lógica

### Technology Choices
- **Backend**: ASP.NET Core con atributos personalizados para validación
- **Frontend**: React Hooks para gestión de permisos
- **Storage**: PostgreSQL con relaciones many-to-many
- **Token**: JWT con claims de permisos y roles

### Security Considerations
- Solo SuperAdmin es intocable (prevenir bloqueo del sistema)
- Permisos validados tanto en frontend como backend
- Token JWT incluye todos los permisos del usuario
- Refresh de token al cambiar permisos

## CRITICAL: Permission Naming Convention

### ⚠️ ONLY 3 Actions Per Resource
The system uses exactly **3 actions** for each resource. NO OTHER ACTIONS EXIST:
- `read` - View/List resources
- `write` - Edit/Update/Delete resources  
- `create` - Create new resources

### ❌ Common Mistakes to Avoid
```csharp
// WRONG - These permissions DO NOT EXIST
[RequirePermission("users.update")]  // ❌ Does not exist
[RequirePermission("users.delete")]  // ❌ Does not exist
[RequirePermission("roles.read")]    // ❌ Roles use users.* permissions

// CORRECT - Use only these 3 actions
[RequirePermission("users.read")]    // ✅ For viewing
[RequirePermission("users.write")]   // ✅ For editing/deleting
[RequirePermission("users.create")]  // ✅ For creating
```

### 📋 Complete Permission Mapping
| Controller | Action | Required Permission |
|------------|--------|-------------------|
| **RolesController** |
| GET /api/roles | users.read |
| GET /api/roles/{id} | users.read |
| POST /api/roles | users.create |
| PUT /api/roles/{id} | users.write |
| DELETE /api/roles/{id} | users.write |
| **UsersController** |
| GET /api/users | users.read |
| GET /api/users/{id} | users.read |
| POST /api/users | users.create |
| PUT /api/users/{id} | users.write |
| DELETE /api/users/{id} | users.write |
| POST /api/users/{id}/roles | users.write |
| **PermissionsController** |
| GET /api/permissions | users.read |
| GET /api/permissions/grouped | users.read |

## Implementation Details

### Backend

#### Models Modified
```csharp
// Models/Role.cs
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    
    // Navegación
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}

// Models/Permission.cs
public class Permission
{
    public int Id { get; set; }
    public string Resource { get; set; } // e.g., "products"
    public string Action { get; set; }    // e.g., "read", "write", "create"
    public string Description { get; set; }
}
```

#### Services Implementation

##### RoleService.cs - Key Methods
```csharp
public async Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto)
{
    var role = await _context.Roles
        .Include(r => r.RolePermissions)
        .FirstOrDefaultAsync(r => r.Id == id);
        
    if (role == null)
        throw new KeyNotFoundException($"Role with id {id} not found.");

    // ONLY SuperAdmin role cannot be edited
    if (role.Name == "SuperAdmin")
        throw new InvalidOperationException("SuperAdmin role cannot be modified to prevent system lockout.");
    
    // Update basic properties
    role.Name = dto.Name;
    role.Description = dto.Description;
    
    // Update permissions - clear and re-add
    _context.RolePermissions.RemoveRange(role.RolePermissions);
    await _context.SaveChangesAsync();
    
    // Add new permissions
    foreach (var permissionId in dto.PermissionIds)
    {
        _context.RolePermissions.Add(new RolePermission
        {
            RoleId = id,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        });
    }
    
    await _context.SaveChangesAsync();
    return await GetByIdAsync(id);
}
```

##### AuthService.cs - JWT Generation with Permissions
```csharp
public string GenerateJwtToken(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("companyId", user.CompanyId?.ToString() ?? "")
    };

    // Add roles
    var roles = GetUserRolesAsync(user.Id).Result;
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
        claims.Add(new Claim("role", role));
    }

    // Add permissions
    var permissions = GetUserPermissionsAsync(user.Id).Result;
    foreach (var permission in permissions)
    {
        claims.Add(new Claim("permissions", permission));
    }

    // Generate token...
}
```

#### DTOs
```csharp
// DTOs/Roles/UpdateRoleDto.cs
public class UpdateRoleDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public List<int> PermissionIds { get; set; } = new List<int>();
}
```

### Frontend

#### Hook Implementation - usePermissions.ts
```typescript
export function usePermissions() {
  const [permissions, setPermissions] = useState<string[]>([]);
  const [roles, setRoles] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadPermissions = () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) return;

        // Decode JWT token
        const payload = JSON.parse(atob(token.split('.')[1]));
        
        // Extract roles and permissions
        let userRoles = Array.isArray(payload.role) ? payload.role : [payload.role];
        let userPermissions = Array.isArray(payload.permissions) 
          ? payload.permissions 
          : [payload.permissions];
        
        setRoles(userRoles.filter(Boolean));
        setPermissions(userPermissions.filter(Boolean));
      } catch (error) {
        console.error('Error loading permissions:', error);
      } finally {
        setLoading(false);
      }
    };

    loadPermissions();
  }, []);

  const hasPermission = useCallback((permission: string): boolean => {
    if (roles.includes('SuperAdmin')) return true;
    return permissions.includes(permission);
  }, [permissions, roles]);

  const canRead = useCallback((resource: string): boolean => {
    return hasPermission(`${resource}.read`);
  }, [hasPermission]);

  // More helper methods...

  return {
    permissions,
    roles,
    loading,
    hasPermission,
    canRead,
    canCreate,
    canUpdate,
    canDelete,
    isSuperAdmin
  };
}
```

#### Sidebar Dynamic Filtering
```typescript
// components/layout/Sidebar.tsx
export function Sidebar() {
  const { hasPermission: checkPermission, loading } = usePermissions();
  
  // Filter menu items based on permissions
  const filterMenuItems = (items: MenuItem[]): MenuItem[] => {
    return items.filter(item => {
      // Check permission
      if (!checkPermission(item.permission)) {
        return false;
      }
      
      // Filter children recursively
      if (item.children) {
        const filteredChildren = filterMenuItems(item.children);
        if (filteredChildren.length > 0) {
          item.children = filteredChildren;
          return true;
        }
        return false;
      }
      
      return true;
    });
  };

  const visibleMenuItems = filterMenuItems([...menuItems]);
  
  // Render only visible items...
}
```

#### Role Edit/Create Pages
- Unified UI design for both create and edit
- Permissions grouped by resource
- Real-time permission counter
- Select All/Deselect All functionality
- Form validation with error display

### Database Changes
```sql
-- Permissions are seeded with this pattern:
-- Resource: dashboard, company, users, clients, products, etc.
-- Actions: read, write, create
-- Examples: products.read, products.write, products.create
```

## Configuration

### Environment Variables
No new environment variables required.

### Program.cs Changes
```csharp
// Already configured JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // JWT configuration...
    });

// Services registration
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

## Testing

### Manual Testing Checklist
- [x] Create new role with specific permissions
- [x] Edit existing role permissions
- [x] Assign role to user
- [x] Login with user and verify sidebar filtering
- [x] Verify SuperAdmin sees everything
- [x] Verify role without permissions shows empty sidebar
- [x] Test permission persistence after logout/login

### Test Scenarios
1. **Scenario: Assistant Role**
   - Create role with only `clients.read`
   - Assign to user
   - Login: Should only see "Clientes" in sidebar

2. **Scenario: Manager Role**
   - Create role with all read permissions
   - Assign to user
   - Login: Should see all menu items but no edit buttons

## Known Issues & Limitations

### Current Limitations
1. Permissions are cached in JWT - requires re-login after permission changes
2. No real-time permission updates
3. No permission inheritance between roles

### Future Improvements
1. Implement WebSocket for real-time permission updates
2. Add role hierarchy/inheritance
3. Add audit log for permission changes
4. Implement permission templates

## Troubleshooting

### Common Problems
1. **Sidebar shows empty after role assignment**
   - **Fixed**: Changed permission naming from `.view` to `.read` in Sidebar.tsx
   - See: `/docs/troubleshooting/auth/auth-06-permissions-not-showing.md`

2. **Role changes not saving**
   - **Fixed**: Frontend was sending `permissions` but backend expected `permissionIds`
   - See: `/docs/troubleshooting/auth/auth-07-role-update-fails.md`

3. **Roles not showing for users with permissions**
   - **Fixed**: Changed all controllers from `roles.*` permissions to `users.*`
   - Controllers affected: RolesController, PermissionsController
   - Root cause: Roles are combined with users management in permission system

4. **Cannot edit roles despite having permissions**
   - **Fixed**: Changed protection from all `isSystemRole` to only `SuperAdmin` role
   - Updated edit page to check `role.name === 'SuperAdmin'` instead of `isSystemRole`
   - Updated PermissionsSeeder to set only SuperAdmin as system role

5. **Cannot edit users despite having users.write permission**
   - **Fixed**: Changed UsersController permissions from non-existent ones to actual DB permissions
   - UsersController was requiring `users.update` and `users.delete` which don't exist
   - Changed all to use the 3 actual permissions: `users.read`, `users.write`, `users.create`

6. **Missing Add User button in desktop view**
   - **Fixed**: Added header section with Export and Add User buttons for desktop
   - Mobile keeps the floating action button (FAB)

7. **Create Role UI different from Edit Role UI**
   - **Fixed**: Updated Create Role page to match Edit Role's horizontal layout
   - Desktop: 3-column header (name, description, stats) with full-width permissions below
   - Mobile: Vertical layout with sidebar

### Debug Tips
```javascript
// Check permissions in browser console
const token = localStorage.getItem('token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Roles:', payload.role);
console.log('Permissions:', payload.permissions);
```

## References
- Original Blueprint: `/blueprint1.md`
- CLAUDE.md Rules: `/CLAUDE.md`
- Related PR: "feat: Implementar sistema de Roles y Usuarios con avatares mejorados"
- Troubleshooting Docs: `/docs/troubleshooting/auth/`