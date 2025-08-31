# Permissions Not Showing in Sidebar After Role Assignment

## Problem Summary
- **Affects**: Frontend sidebar menu visibility
- **Frequency**: Always when permissions mismatch
- **Severity**: High - Users cannot access assigned features
- **First Occurred**: 2025-08-08

## Symptoms Checklist
- [ ] User assigned role with specific permissions
- [ ] After login, sidebar appears completely empty
- [ ] JWT token contains permissions but UI doesn't show them
- [ ] Console shows no JavaScript errors
- [ ] Network tab shows successful API calls

### Exact Error Messages
No error messages - Silent failure with empty sidebar.

## Root Causes

### 1. Permission Name Mismatch
**Verification Steps:**
```javascript
// Check what permissions are in the token
const token = localStorage.getItem('token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Token permissions:', payload.permissions);
// Output: ["clients.read", "products.read"]

// Check what Sidebar expects
console.log('Sidebar expects:', menuItems[0].permission);
// Output: "clients.view" ❌ MISMATCH!
```

The database stores permissions as `.read`, `.write`, `.create` but Sidebar was expecting `.view`.

### 2. Token Not Refreshed After Permission Change
JWT tokens cache permissions. Changes require new login to take effect.

## Solutions

### Quick Fix (< 5 min)
1. **Force user to re-login:**
```javascript
// Clear all auth data
localStorage.removeItem('token');
localStorage.removeItem('user');
localStorage.removeItem('expiresAt');
window.location.href = '/login';
```

### Step-by-Step Solution

#### 1. Fix Permission Names in Sidebar.tsx
```typescript
// ❌ WRONG - Before
const menuItems: MenuItem[] = [
  {
    id: 'clientes',
    nameKey: 'navigation.clientes',
    href: '/clientes',
    icon: ClientsIcon,
    permission: 'clients.view'  // ❌ Doesn't exist in DB
  },
  // ...
];

// ✅ CORRECT - After
const menuItems: MenuItem[] = [
  {
    id: 'clientes',
    nameKey: 'navigation.clientes',
    href: '/clientes',
    icon: ClientsIcon,
    permission: 'clients.read'  // ✅ Matches DB
  },
  // ...
];
```

#### 2. Update All Menu Items
Change all permissions from `.view` to `.read`:
- `dashboard.view` → `dashboard.read`
- `company.view` → `company.read`
- `users.view` → `users.read`
- `clients.view` → `clients.read`
- `products.view` → `products.read`
- etc.

#### 3. Verify Database Permissions
```sql
-- Check what permissions exist
SELECT DISTINCT resource, action FROM permissions ORDER BY resource, action;

-- Should show:
-- clients | read
-- clients | write
-- clients | create
-- (NO .view actions)
```

### Alternative Solutions
1. **Change database to use .view instead of .read:**
   - Update PermissionsSeeder.cs
   - Run new migration
   - Re-seed permissions

2. **Support both .view and .read:**
```typescript
const hasPermission = (permission: string) => {
  // Support both formats
  const readPermission = permission.replace('.view', '.read');
  return checkPermission(permission) || checkPermission(readPermission);
};
```

## Prevention

### Best Practices
1. **Keep permission naming consistent** across backend and frontend
2. **Document permission format** in CLAUDE.md
3. **Create constants file** for permission names:
```typescript
// lib/permissions.constants.ts
export const PERMISSIONS = {
  CLIENTS: {
    READ: 'clients.read',
    WRITE: 'clients.write',
    CREATE: 'clients.create'
  },
  // ...
};
```

### Configuration Template
```typescript
// Correct menu item configuration
{
  id: 'resource-name',
  nameKey: 'navigation.resourceName',
  href: '/resource-path',
  icon: ResourceIcon,
  permission: 'resource.read'  // Always use .read for view permissions
}
```

## Related Issues
- [Role changes not saving](./auth-07-role-update-fails.md)
- [JWT token not refreshing](./auth-08-jwt-refresh-issues.md)
- [Implementation: Roles & Permissions System](/docs/implementations/auth/2025-08-roles-permissions-system.md)

## Search Keywords
permissions, sidebar, empty, not showing, role assignment, JWT, token, menu items, navigation, RBAC, access control, clients.view, clients.read