# 🔐 Auth-05: DTO Naming Conflicts in Swagger

[← Back to Auth Index](./auth-00-index.md) | [← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Problem Summary](#problem-summary)
- [Symptoms](#symptoms)
- [Root Causes](#root-causes)
- [Solutions](#solutions)
- [Prevention](#prevention)
- [Related Issues](#related-issues)

---

## Problem Summary

**Swagger fails to generate schema when multiple DTOs have the same class name in different namespaces, causing Internal Server Error.**

**Affects**: Swagger documentation, API testing
**Frequency**: Common when organizing DTOs by feature
**Severity**: High (blocks Swagger UI completely)
**First seen**: When implementing role/permission system with separate Auth and User DTOs

---

## Symptoms

### Primary Symptoms
- [ ] Swagger shows "Failed to load API definition"
- [ ] Internal Server Error on `/swagger/v1/swagger.json`
- [ ] API works fine but documentation fails
- [ ] Error mentions "same schemaId already used"

### Error Messages
```
Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorException: 
Can't use schemaId "$UserDto" for type "$WebsiteBuilderAPI.DTOs.Users.UserDto". 
The same schemaId is already used for type "$WebsiteBuilderAPI.DTOs.Auth.UserDto"
```

### When It Occurs
- Multiple DTOs with same class name
- Different namespaces but same class name
- Swagger tries to generate schemas
- Usually after adding new feature modules

---

## Root Causes

### Cause 1: Duplicate Class Names
**Description**: Two or more DTOs have the same class name in different namespaces
**How to verify**: 
```bash
# Search for duplicate DTO names
grep -r "class UserDto" --include="*.cs"
# Will show multiple files with same class name
```

### Cause 2: Default Schema ID Strategy
**Description**: Swagger uses class name only (not full namespace) for schema IDs by default
**How to verify**: Check if SwaggerGen configuration has custom schema ID strategy

### Cause 3: Nested DTOs in Same File
**Description**: Multiple public classes in same file can cause naming issues
**How to verify**: Check if DTOs are defined as nested classes in same file

---

## Solutions

### 🚀 Quick Fix
**Time**: < 5 minutes
**Success Rate**: 100%

Rename one of the conflicting DTOs:
```csharp
// Before: DTOs/Auth/AuthResponseDto.cs
public class UserDto { }  // ❌ Conflicts

// After: DTOs/Auth/AuthResponseDto.cs  
public class AuthUserDto { }  // ✅ Unique name
```

### 📋 Step-by-Step Solution

#### Step 1: Find All Conflicting DTOs
```bash
# Find all DTOs with same name
grep -r "class UserDto" --include="*.cs" DTOs/
```

#### Step 2: Rename in DTO File
```csharp
// DTOs/Auth/AuthResponseDto.cs
public class AuthResponseDto
{
    public string Token { get; set; }
    public AuthUserDto User { get; set; }  // Changed from UserDto
}

public class AuthUserDto  // Renamed from UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    // ... other properties
}
```

#### Step 3: Update References
```csharp
// Services/AuthService.cs
return new AuthResponseDto
{
    Token = token,
    User = new AuthUserDto  // Updated reference
    {
        Id = user.Id,
        // ... map properties
    }
};
```

#### Step 4: Update Controller Returns
```csharp
// Controllers/AuthController.cs
[HttpGet("me")]
public async Task<ActionResult<AuthUserDto>> GetCurrentUser()  // Updated return type
{
    // ... implementation
}
```

### 🔧 Alternative Solutions

**Option 1: Configure Custom Schema IDs**
```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);  // Use full namespace
});
```

**Option 2: Use Namespace Prefix**
```csharp
// Program.cs  
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => 
    {
        var name = type.Name;
        if (type.IsNested)
            name = $"{type.DeclaringType.Name}{name}";
        return name.Replace("+", "");
    });
});
```

---

## Prevention

### Best Practices
1. **Use descriptive DTO names** that include context
   - ❌ `UserDto` (too generic)
   - ✅ `AuthUserDto`, `UserManagementDto`, `UserProfileDto`

2. **Organize DTOs by feature** with unique names
   ```
   DTOs/
   ├── Auth/
   │   └── AuthUserDto.cs
   ├── Users/
   │   └── UserManagementDto.cs
   └── Profile/
       └── UserProfileDto.cs
   ```

3. **One class per file** for DTOs
   - Avoid nested public classes
   - Makes refactoring easier

4. **Naming convention** for DTOs
   - `[Feature][Entity]Dto`
   - Examples: `AuthUserDto`, `CreateProductDto`, `OrderSummaryDto`

5. **Configure Swagger early** in project
   ```csharp
   c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
   ```

### Validation Checklist
Before adding new DTOs:
- [ ] Check if class name already exists
- [ ] Use feature-specific prefix
- [ ] One public class per file
- [ ] Update all references if renaming

---

## Related Issues

### See Also
- [API-03: Swagger Configuration](../api/api-03-swagger-setup.md) - General Swagger setup
- [General-03: Naming Conventions](../general/general-03-naming-conventions.md) - Project naming standards

### Often Occurs With
- Adding new feature modules
- Refactoring DTOs
- Merging branches with new DTOs
- Copy-paste DTO creation

### Prerequisites
- Swagger/Swashbuckle installed
- Multiple feature modules
- DTOs organized by namespace

---

## 🏷️ Search Keywords

`swagger` `schemaId` `duplicate` `DTO` `naming` `conflict` `Swashbuckle` `UserDto` `same schemaId`

---

## 📝 Notes

### Version-Specific Notes
- **Swashbuckle 6.x**: Default uses class name only
- **Swashbuckle 5.x**: Same issue, different error format
- **.NET 6+**: Can use file-scoped namespaces

### Alternative Approaches
1. **Single DTO library** - All DTOs in one project
2. **Versioned DTOs** - UserDtoV1, UserDtoV2
3. **Generic base DTOs** - Inherit and specialize

---

**Last Updated**: 2025-08-11
**Contributors**: WebsiteBuilder Team
**Verified On**: ASP.NET Core 8, Swashbuckle.AspNetCore 6.x