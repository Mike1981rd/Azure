# Theme Configuration Troubleshooting Guide

## Overview
**Component**: Global Theme Configuration  
**Category**: Features  
**Created**: January 11, 2025  
**Severity**: Medium  
**Related Implementation**: `/docs/implementations/features/2025-01-global-theme-config.md`

## Problem 1: Build Errors - Process Locked

### Error Message
```
error MSB3027: Could not copy "apphost.exe" to "WebsiteBuilderAPI.exe". 
The file is locked by: "WebsiteBuilderAPI (20152)"
```

### Root Cause
The ASP.NET Core application is still running and locking the executable file.

### Solution
```powershell
# Stop the specific process
powershell.exe -Command "Stop-Process -Id 20152 -Force"

# Or stop all dotnet processes
powershell.exe -Command "Get-Process dotnet | Stop-Process -Force"

# Verify port is free
powershell.exe -Command "Get-NetTCPConnection | Where-Object {$_.LocalPort -eq 5266}"
```

### Prevention
- Always stop the backend before rebuilding
- Use Visual Studio's stop button instead of closing terminal
- Create a cleanup script for development

---

## Problem 2: TypeScript Duplicate Properties

### Error Message
```typescript
error TS2783: 'id' is specified more than once, so this usage will be overwritten.
error TS1117: An object literal cannot have multiple properties with the same name.
```

### Root Cause
Object spread operator used incorrectly, causing property duplication:
```typescript
// WRONG
const config = {
  id: 'scheme1',
  name: 'Esquema 1',
  ...defaultColorScheme,
  id: 'scheme1',  // Duplicate!
  name: 'Esquema 1'  // Duplicate!
}
```

### Solution
```typescript
// CORRECT
const config = {
  ...defaultColorScheme,
  id: 'scheme1',
  name: 'Esquema 1'
}
```

### Prevention
- Always spread defaults first, then override
- Use TypeScript strict mode
- Enable duplicate property checking in tsconfig

---

## Problem 3: JSONB Serialization Failures

### Error Message
```
System.Text.Json.JsonException: A possible object cycle was detected.
```

### Root Cause
Circular references in the C# models or missing JsonIgnore attributes.

### Solution
```csharp
public class GlobalThemeConfig
{
    // Add JsonIgnore for navigation properties
    [JsonIgnore]
    public virtual Company Company { get; set; }
    
    // JSONB columns don't need JsonIgnore
    [Column(TypeName = "jsonb")]
    public AppearanceConfig Appearance { get; set; }
}
```

### Prevention
- Always use `[JsonIgnore]` on navigation properties
- Test serialization before database operations
- Use DTOs for complex scenarios

---

## Problem 4: Cache Not Invalidating

### Symptoms
- Updates not reflecting immediately
- Old data returned after updates
- Inconsistent data between users

### Root Cause
Cache key mismatch or missing invalidation calls.

### Solution
```csharp
private void InvalidateCache(int companyId)
{
    // Ensure consistent key format
    var cacheKey = $"{CACHE_KEY_PREFIX}{companyId}";
    _cache.Remove(cacheKey);
    
    // Log for debugging
    _logger.LogDebug("Cache invalidated for company {CompanyId}", companyId);
}

// Call after EVERY update
await _context.SaveChangesAsync();
InvalidateCache(companyId);  // Don't forget this!
```

### Prevention
- Centralize cache key generation
- Add logging to cache operations
- Use cache tags for bulk invalidation

---

## Problem 5: Migration Execution Errors

### Error Message
```
Build failed. Use dotnet build to see the errors.
```

### Root Cause
PowerShell command parsing issues with parameters.

### Solution
Create PowerShell scripts instead of inline commands:

```powershell
# create-migration.ps1
Set-Location "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI"
dotnet ef migrations add AddGlobalThemeConfig

# update-database.ps1
Set-Location "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI"
dotnet ef database update
```

Execute:
```bash
powershell.exe -ExecutionPolicy Bypass -File "create-migration.ps1"
```

### Prevention
- Always use script files for complex commands
- Test commands in PowerShell directly first
- Keep migration scripts in source control

---

## Problem 6: Frontend Type Imports Not Found

### Error Message
```typescript
Cannot find module '@/types/theme' or its corresponding type declarations.
```

### Root Cause
TypeScript path mapping not configured or incorrect import paths.

### Solution
Ensure tsconfig.json has proper paths:
```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"],
      "@/types/*": ["./src/types/*"]
    }
  }
}
```

### Prevention
- Use consistent import aliases
- Run `tsc --noEmit` regularly
- Configure IDE to use project TypeScript

---

## Problem 7: Optimistic Updates Rolling Back

### Symptoms
- UI updates then reverts
- Error state not showing
- User confusion about save status

### Root Cause
Network errors not handled properly in optimistic update flow.

### Solution
```typescript
updateAppearance: async (appearance) => {
  const previousConfig = get().config;
  
  // Optimistic update
  set((state) => {
    if (state.config) {
      state.config.appearance = appearance;
      state.hasUnsavedChanges = true;
    }
  });

  try {
    const updated = await themeConfigApi.updateAppearance(companyId, appearance);
    set((state) => {
      if (state.config) {
        state.config.appearance = updated;
        state.hasUnsavedChanges = false;
      }
    });
  } catch (error) {
    // Rollback with user notification
    set((state) => {
      if (state.config && previousConfig) {
        state.config.appearance = previousConfig.appearance;
      }
      state.error = 'Failed to save changes. Please try again.';
    });
    
    // Show toast notification
    toast.error('Failed to save appearance settings');
  }
}
```

### Prevention
- Always store previous state for rollback
- Implement proper error notifications
- Add retry logic for network failures

---

## Problem 8: Memory Cache Memory Leaks

### Symptoms
- Increasing memory usage over time
- Application slowdown
- OutOfMemoryException in logs

### Root Cause
Cache entries not expiring or too many entries cached.

### Solution
```csharp
// Configure memory cache with size limit
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 100; // Maximum 100 entries
    options.CompactionPercentage = 0.25; // Remove 25% when limit reached
});

// Set size when caching
_cache.Set(cacheKey, config, new MemoryCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(30),
    Size = 1, // Each entry counts as 1 toward the limit
    Priority = CacheItemPriority.Normal
});
```

### Prevention
- Set appropriate cache size limits
- Use sliding expiration
- Monitor memory usage in production

---

## Diagnostic Commands

### Check Database Connection
```sql
-- Verify table exists
SELECT * FROM "GlobalThemeConfigs" LIMIT 1;

-- Check for orphaned configs
SELECT * FROM "GlobalThemeConfigs" 
WHERE "CompanyId" NOT IN (SELECT "Id" FROM "Companies");
```

### Check TypeScript Compilation
```bash
cd websitebuilder-admin
npx tsc --noEmit --skipLibCheck
```

### Monitor API Performance
```powershell
# Check response times
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:5266/api/GlobalThemeConfig/company/1
```

### Clear All Caches
```csharp
// Add admin endpoint
[HttpPost("admin/clear-cache")]
[Authorize(Roles = "Admin")]
public IActionResult ClearCache()
{
    _memoryCache.Clear();
    return Ok(new { message = "Cache cleared" });
}
```

---

## Quick Reference

| Issue | Quick Fix |
|-------|-----------|
| Build locked | `Stop-Process -Id [PID] -Force` |
| Type errors | `npx tsc --noEmit` |
| Cache issues | Restart application |
| Migration fails | Use PowerShell script |
| Import errors | Check tsconfig paths |
| Rollback issues | Check error handling |

---

## Related Documents
- Implementation: `/docs/implementations/features/2025-01-global-theme-config.md`
- Architecture: `/blueprintwebsite.md`
- Progress: `/websitebuilderprogress.md`

**Last Updated**: January 11, 2025