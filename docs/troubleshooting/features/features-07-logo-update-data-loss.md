# Logo Update Causes Data Loss

[← Back to Features Index](./features-00-index.md) | [← Back to Main Index](../00-troubleshooting-index.md)

## Problem Summary
- **Affects**: Company data when updating logo
- **Frequency**: Always when using general update endpoint
- **Severity**: Critical - Data loss
- **First Observed**: 2025-08-06

## Symptoms
- [ ] Uploading a logo erases all other company data
- [ ] Database shows NULL values for previously filled fields
- [ ] Only logo field is updated, everything else is cleared
- [ ] Company name, emails, addresses all disappear
- [ ] User sees empty form after logo upload

## Root Causes

### 1. Single Endpoint Handles All Updates
**Issue**: Using general update endpoint for logo-only updates
```typescript
// PROBLEMA: Enviaba solo el logo al endpoint general
await updateCompany({ logo: newLogoUrl });
// Esto enviaba: { logo: "url", name: null, email: null, ... }
```

**Database Impact**:
```sql
-- Antes del update
1  "Alfa Software"  "contacto@alfa.com"  "USD"  "Santo Domingo"
-- Después del update  
1  "Alfa Software"  NULL                 NULL   NULL
```

### 2. Backend Interprets Missing Fields as NULL
**Issue**: Service updates ALL fields in the DTO
```csharp
// El servicio actualizaba todo lo que venía en el request
company.PhoneNumber = request.PhoneNumber;  // null
company.ContactEmail = request.ContactEmail; // null
company.Logo = request.Logo;                 // "nuevo-logo.png"
```

## Solutions

### Quick Fix (< 5 min)
Don't use the general update endpoint for single field updates

### Step-by-Step Solution

1. **Create dedicated logo endpoint**:
```csharp
// CompanyController.cs
[HttpPut("current/logo")]
public async Task<ActionResult> UpdateLogo([FromBody] UpdateLogoDto request)
{
    await _companyService.UpdateLogoAsync(request.Logo);
    return Ok(new { message = "Logo updated successfully" });
}
```

2. **Add logo-specific DTO**:
```csharp
// UpdateLogoDto.cs
public class UpdateLogoDto
{
    [Required]
    [StringLength(500)]
    public string Logo { get; set; } = string.Empty;
}
```

3. **Implement dedicated service method**:
```csharp
// CompanyService.cs
public async Task UpdateLogoAsync(string logoUrl)
{
    var company = await _context.Companies.FirstOrDefaultAsync();
    if (company != null)
    {
        company.Logo = logoUrl;  // SOLO actualiza el logo
        company.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

4. **Update frontend to use dedicated endpoint**:
```typescript
// StoreDetailsForm.tsx
const handleLogoChange = async (newLogoUrl: string) => {
  try {
    // Usar endpoint específico
    await api.put('/company/current/logo', { logo: newLogoUrl });
    await refetch();
  } catch (error) {
    console.error('Error updating logo:', error);
  }
};
```

### Alternative Solutions
1. Implement PATCH support for partial updates
2. Use field masks to specify which fields to update
3. Separate endpoints for each form section

## Prevention

### Best Practices
1. **Use dedicated endpoints for specific operations**
2. **Never send partial objects to general update endpoints**
3. **Implement field-level update methods**
4. **Test single-field updates thoroughly**

### Architecture Pattern
```csharp
// Separate endpoints for different concerns
[HttpPut("current")]          // General update
[HttpPut("current/logo")]     // Logo only
[HttpPut("current/logo-size")] // Logo size only
[HttpPut("current/profile")]   // Profile section only
[HttpPut("current/billing")]   // Billing section only
```

## Related Issues
- [Company Save 400 Error](./features-06-company-save-400-error.md)
- [Database Update Patterns](../database/database-01-update-patterns.md)

## Search Keywords
- logo upload deletes data
- company data loss on update
- null values after logo change
- partial update overwrites data
- logo save clears form