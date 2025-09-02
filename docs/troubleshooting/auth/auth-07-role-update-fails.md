# Role Changes Not Saving - Permission Updates Lost

## Problem Summary
- **Affects**: Role permission management
- **Frequency**: Always with incorrect DTO mapping
- **Severity**: Critical - Permissions system unusable
- **First Occurred**: 2025-08-08

## Symptoms Checklist
- [ ] Edit role and change permissions
- [ ] Click save - shows success message
- [ ] Navigate back to role list
- [ ] Edit same role again - permissions reverted
- [ ] Database shows no changes
- [ ] No error messages displayed

### Exact Error Messages
No visible errors - Silent failure with HTTP 200 response but no persistence.

## Root Causes

### 1. Frontend/Backend DTO Mismatch
**Verification Steps:**
```javascript
// Check what frontend sends
console.log('Request body:', JSON.stringify(formData));
// Output: { name: "Admin", description: "...", permissions: [1,2,3] }

// Backend expects:
// { name: "Admin", description: "...", permissionIds: [1,2,3] }
```

Frontend was sending `permissions` array but backend UpdateRoleDto expects `permissionIds`.

### 2. Backend Silently Ignoring Invalid Fields
ASP.NET Core model binding was ignoring the `permissions` field since it doesn't exist in the DTO, resulting in empty `PermissionIds` list.

## Solutions

### Quick Fix (< 5 min)
Update the frontend to send correct field name:

```typescript
// In edit/page.tsx and new/page.tsx
const response = await fetch(`http://localhost:5266/api/roles/${roleId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    name: formData.name,
    description: formData.description,
    permissionIds: formData.permissions // ✅ Changed from 'permissions' to 'permissionIds'
  })
});
```

### Step-by-Step Solution

#### 1. Fix Role Edit Page
```typescript
// ❌ WRONG - Before
body: JSON.stringify(formData)
// This sends: { name, description, permissions: [...] }

// ✅ CORRECT - After
body: JSON.stringify({
  name: formData.name,
  description: formData.description,
  permissionIds: formData.permissions // Map to correct field name
})
```

#### 2. Fix Role Create Page (Same Issue)
```typescript
// Apply same fix in roles/new/page.tsx
body: JSON.stringify({
  name: formData.name,
  description: formData.description,
  permissionIds: formData.permissions
})
```

#### 3. Verify Backend DTO
```csharp
// DTOs/Roles/UpdateRoleDto.cs
public class UpdateRoleDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public List<int> PermissionIds { get; set; } // ← This is what backend expects
}
```

### Alternative Solutions

1. **Change Backend to Accept 'permissions':**
```csharp
public class UpdateRoleDto
{
    // ...
    public List<int> Permissions { get; set; } // Change property name
}
```

2. **Add Validation to Catch Empty Permissions:**
```csharp
if (!dto.PermissionIds?.Any() ?? true)
{
    _logger.LogWarning("No permissions provided for role update");
}
```

3. **Use AutoMapper with Custom Mapping:**
```csharp
CreateMap<UpdateRoleRequest, UpdateRoleDto>()
    .ForMember(dest => dest.PermissionIds, 
               opt => opt.MapFrom(src => src.Permissions));
```

## Prevention

### Best Practices
1. **Use TypeScript interfaces** matching backend DTOs:
```typescript
interface UpdateRoleDto {
  name: string;
  description: string;
  permissionIds: number[]; // Match backend exactly
}
```

2. **Add backend validation:**
```csharp
if (dto.PermissionIds == null || !dto.PermissionIds.Any())
{
    return BadRequest("At least one permission is required");
}
```

3. **Log request bodies** during development:
```csharp
_logger.LogInformation($"Updating role {id} with: {JsonSerializer.Serialize(dto)}");
```

### Configuration Template
```typescript
// Correct API call structure
const updateRole = async (roleId: number, data: RoleFormData) => {
  const dto: UpdateRoleDto = {
    name: data.name,
    description: data.description,
    permissionIds: data.selectedPermissionIds // Explicit mapping
  };
  
  return fetch(`/api/roles/${roleId}`, {
    method: 'PUT',
    body: JSON.stringify(dto)
  });
};
```

## Related Issues
- [Permissions not showing in sidebar](./auth-06-permissions-not-showing.md)
- [DTO naming conflicts](./auth-05-dto-naming-conflicts.md)
- [Implementation: Roles & Permissions System](/docs/implementations/auth/2025-08-roles-permissions-system.md)

## Search Keywords
role update, permissions not saving, DTO mismatch, permissionIds, UpdateRoleDto, silent failure, role edit, permission assignment, RBAC