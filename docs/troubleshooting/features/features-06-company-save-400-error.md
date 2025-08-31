# Company Save Returns 400 Bad Request

[← Back to Features Index](./features-00-index.md) | [← Back to Main Index](../00-troubleshooting-index.md)

## Problem Summary
- **Affects**: Company/Empresa form submission
- **Frequency**: Always when saving with minimal data
- **Severity**: High - Blocks basic functionality
- **First Observed**: 2025-08-06

## Symptoms
- [ ] Clicking "Save" with only company name returns 400 error
- [ ] Console shows "Failed to load resource: 400 (Bad Request)"
- [ ] Error message: "Error updating company information"
- [ ] Form requires ALL fields to be filled to save successfully
- [ ] Backend validation rejects partial updates

## Root Causes

### 1. Service Updates All Fields Unconditionally
**Issue**: CompanyService was updating all fields even when null
```csharp
// PROBLEMA: Asignaba null a todos los campos no enviados
company.PhoneNumber = request.PhoneNumber;  // Si es null, borra el valor existente
company.ContactEmail = request.ContactEmail;
company.SenderEmail = request.SenderEmail;
```

**Verification**:
```bash
# Check database after failed save
SELECT * FROM "Companies" WHERE "Id" = 1;
# Many fields show NULL when they had values before
```

### 2. Model Validation on Empty Strings
**Issue**: Some fields don't accept empty strings but frontend sends them
- Email fields expect valid format or null
- Currency expects 3-letter code or null

## Solutions

### Quick Fix (< 5 min)
Fill all fields in the form before saving (temporary workaround)

### Step-by-Step Solution

1. **Update CompanyService to handle null values properly**:
```csharp
// SOLUCIÓN: Solo actualizar si el valor no es null
if (request.PhoneNumber != null)
    company.PhoneNumber = request.PhoneNumber;
if (request.ContactEmail != null)
    company.ContactEmail = request.ContactEmail;
// ... repetir para todos los campos opcionales
```

2. **Clean empty strings in frontend before sending**:
```typescript
const cleanedData = Object.entries(data).reduce((acc, [key, value]) => {
  if (value !== '' && value !== null && value !== undefined) {
    acc[key] = value;
  }
  return acc;
}, {} as any);

await updateCompany(cleanedData);
```

3. **Add logging to debug**:
```typescript
console.log('Sending data:', cleanedData);
console.error('Error response:', err.response?.data);
```

### Alternative Solutions
1. Use PATCH semantics with JSON Patch
2. Create separate endpoints for each section
3. Implement field-level dirty tracking

## Prevention

### Best Practices
1. **Always use conditional updates in services**
2. **Don't send empty strings for optional fields**
3. **Use DTOs that match actual requirements**
4. **Test with minimal required fields**

### Configuration Template
```csharp
// Service pattern for optional field updates
public async Task UpdateAsync(UpdateDto request)
{
    var entity = await GetEntity();
    
    // Only update provided values
    if (!string.IsNullOrEmpty(request.RequiredField))
        entity.RequiredField = request.RequiredField;
        
    if (request.OptionalField != null)
        entity.OptionalField = request.OptionalField;
        
    await _context.SaveChangesAsync();
}
```

## Related Issues
- [Logo Update Data Loss](./features-07-logo-update-data-loss.md)
- [DTO Naming Conflicts](../auth/auth-05-dto-naming-conflicts.md)

## Search Keywords
- company save 400 error
- form validation bad request
- all fields required bug
- partial update not working
- empresa guardar error 400