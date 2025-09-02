# 🗄️ General-01: Database Migration and DbContext Issues

[← Back to General Index](./general-00-index.md) | [← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Problem Summary](#problem-summary)
- [Symptoms](#symptoms)
- [Root Causes](#root-causes)
- [Solutions](#solutions)
- [Prevention](#prevention)
- [Related Issues](#related-issues)

---

## Problem Summary

**Entity Framework Core migration errors, particularly with multiple DbContext classes, missing model properties, and seed data issues.**

**Affects**: Database migrations, Model updates, Initial setup
**Frequency**: Common
**Severity**: High (blocks database updates)
**First seen**: During migration creation or database update

---

## Symptoms

### Primary Symptoms
- [ ] "More than one DbContext was found" error
- [ ] Build errors after adding new model properties
- [ ] Migration fails with property not found
- [ ] Seed data throwing exceptions

### Error Messages
```
More than one DbContext was found. Specify which one to use.
'Hotel' does not contain a definition for 'Logo'
'RolePermission' does not contain a definition for 'CreatedAt'
```

### When It Occurs
- Running `Add-Migration` without context parameter
- After adding properties to models
- When seed data references non-existent properties
- During `Update-Database`

---

## Root Causes

### Cause 1: Multiple DbContext Without Specification
**Description**: Project has ApplicationDbContext and TenantAwareDbContext
**How to verify**: 
```powershell
# This will fail
Add-Migration TestMigration
# Error: More than one DbContext was found
```

### Cause 2: Model Properties Not Matching Usage
**Description**: Code using properties that don't exist in model
**How to verify**: Check compile errors in Error List

### Cause 3: Property Name Mismatches
**Description**: Using wrong property names (e.g., CreatedAt vs GrantedAt)
**How to verify**: Compare model definition with usage in seed data

---

## Solutions

### 🚀 Quick Fix
**Time**: < 2 minutes

Always specify context in migration commands:
```powershell
Add-Migration MigrationName -Context ApplicationDbContext
Update-Database -Context ApplicationDbContext
```

### 📋 Step-by-Step Solution

#### Step 1: Fix Missing Model Properties
```csharp
// Models/Hotel.cs
public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? CustomDomain { get; set; }
    public string? Subdomain { get; set; }      // Add if missing
    public string? Logo { get; set; }           // Add if missing
    public string? PrimaryColor { get; set; }   // Add if missing
    public string? SecondaryColor { get; set; } // Add if missing
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Step 2: Fix Property Name Mismatches
```csharp
// Data/SeedData.cs
// Wrong:
context.RolePermissions.Add(new RolePermission
{
    RoleId = role.Id,
    PermissionId = permission.Id,
    CreatedAt = DateTime.UtcNow  // ❌ Wrong property
});

// Correct:
context.RolePermissions.Add(new RolePermission
{
    RoleId = role.Id,
    PermissionId = permission.Id,
    GrantedAt = DateTime.UtcNow  // ✅ Correct property
});
```

#### Step 3: Create Migration with Context
```powershell
# Always use -Context parameter
Add-Migration AddHotelProperties -Context ApplicationDbContext
```

#### Step 4: Update Database
```powershell
# Also specify context here
Update-Database -Context ApplicationDbContext
```

### 🔧 Alternative Solutions

**For removing a bad migration**:
```powershell
Remove-Migration -Context ApplicationDbContext
```

**For reverting to previous migration**:
```powershell
Update-Database PreviousMigrationName -Context ApplicationDbContext
```

---

## Prevention

### Best Practices
1. **Always check model properties** before using in code
2. **Create a migration alias** in your PowerShell profile
3. **Document context parameter** in README
4. **Use IntelliSense** to verify property names
5. **Build project** before creating migrations

### PowerShell Alias Setup
```powershell
# Add to PowerShell profile
function Add-Mig {
    param($name)
    Add-Migration $name -Context ApplicationDbContext
}

function Update-Db {
    Update-Database -Context ApplicationDbContext
}
```

### Documentation Template
```markdown
## Database Commands
Always use `-Context ApplicationDbContext`:
- `Add-Migration MigrationName -Context ApplicationDbContext`
- `Update-Database -Context ApplicationDbContext`
```

---

## Related Issues

### See Also
- [General-02: Development Setup](./general-02-development-setup.md) - Environment configuration
- [Auth-01: JWT Setup Issues](../auth/auth-01-jwt-setup-issues.md) - If auth tables missing

### Often Occurs With
- Adding new model properties
- Setting up multi-tenancy
- Initial project setup
- Team member onboarding

---

## 🏷️ Search Keywords

`DbContext` `migration` `Add-Migration` `Update-Database` `multiple context` `property not found` `CreatedAt` `GrantedAt` `-Context`

---

**Last Updated**: August 2025
**Contributors**: WebsiteBuilder Team
**Verified On**: Entity Framework Core 8, PostgreSQL