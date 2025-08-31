# Npgsql JSON Serialization Error with List Types

## Problem Summary
- **Affects**: Shipping zones, any entity with List<T> properties stored as JSONB
- **Frequency**: Always occurs without proper configuration
- **Severity**: High - Blocks all CRUD operations on affected entities
- **First Occurrence**: 2025-08-07 when implementing ShippingZone with List<string> Countries

## Symptoms Checklist
- [x] Error 500 when creating/updating entities with List properties
- [x] Exception: `System.InvalidCastException: Writing values of 'System.Collections.Generic.List'1[[System.String]]' is not supported`
- [x] Message: `Type 'List'1' required dynamic JSON serialization, which requires an explicit opt-in`
- [x] Stack trace shows error in `Npgsql.Internal.AdoSerializerHelpers`
- [x] SaveChangesAsync() fails with DbUpdateException

## Exact Error Message
```
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> System.InvalidCastException: Writing values of 'System.Collections.Generic.List`1[[System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]' is not supported for parameters having NpgsqlDbType 'Jsonb'.
 ---> System.NotSupportedException: Type 'List`1' required dynamic JSON serialization, which requires an explicit opt-in; call 'EnableDynamicJson' on 'NpgsqlDataSourceBuilder'
```

## Root Causes

### 1. Npgsql 8.0 Breaking Change
**Verification**: Check Npgsql package version
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.x.x" />
```
- Npgsql 8.0 removed automatic JSON serialization for security/performance
- Requires explicit opt-in for dynamic type serialization

### 2. Missing EnableDynamicJson Configuration
**Verification**: Check Program.cs for DbContext configuration
```csharp
// WRONG - Missing EnableDynamicJson
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### 3. Direct Connection String Usage
**Verification**: Not using NpgsqlDataSourceBuilder
- Direct connection string doesn't allow JSON configuration
- Must use DataSourceBuilder for advanced options

## Solutions

### Quick Fix (< 5 min)
1. Open `Program.cs`
2. Replace DbContext configuration:
```csharp
// Add at top
using Npgsql;

// Replace existing DbContext configuration
var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));
```
3. Restart application

### Step-by-Step Solution

#### Step 1: Install/Verify Npgsql Package
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### Step 2: Update Program.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Npgsql; // Add this

var builder = WebApplication.CreateBuilder(args);

// Configure Npgsql with dynamic JSON support
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); // Enable JSON serialization
var dataSource = dataSourceBuilder.Build();

// Use the configured data source
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));
```

#### Step 3: Verify Entity Configuration
```csharp
// In ApplicationDbContext.OnModelCreating
modelBuilder.Entity<ShippingZone>(entity =>
{
    entity.Property(e => e.Countries)
        .HasColumnType("jsonb"); // Ensure JSONB column type
});
```

#### Step 4: Test the Fix
```csharp
// Test code
var zone = new ShippingZone
{
    Countries = new List<string> { "US", "CA", "MX" }
};
await _context.SaveChangesAsync(); // Should work now
```

### Alternative Solutions

#### Option 1: Use Newtonsoft.Json Instead
```csharp
// If you prefer Newtonsoft.Json over System.Text.Json
dataSourceBuilder.UseJsonNet(); // Instead of EnableDynamicJson()
```

#### Option 2: Use String Arrays Instead of Lists
```csharp
// Change model to use arrays
public string[] Countries { get; set; } // Arrays work without configuration
```

#### Option 3: Manual JSON Conversion
```csharp
// Store as string, convert manually
private string _countriesJson;
[NotMapped]
public List<string> Countries 
{
    get => JsonSerializer.Deserialize<List<string>>(_countriesJson);
    set => _countriesJson = JsonSerializer.Serialize(value);
}
```

## Prevention

### Best Practices
1. **Always configure Npgsql properly** when using JSONB columns
2. **Document JSON columns** in entity comments
3. **Test CRUD operations** immediately after adding JSONB properties
4. **Use integration tests** for database operations

### Configuration Template
```csharp
// Reusable Npgsql configuration
public static class NpgsqlConfiguration
{
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.EnableDynamicJson();
        // Add other configurations as needed
        return builder.Build();
    }
}

// Usage in Program.cs
var dataSource = NpgsqlConfiguration.CreateDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection"));
```

### Testing Checklist
- [ ] Create entity with List/Dictionary property
- [ ] Update entity with modified collection
- [ ] Query and deserialize collection property
- [ ] Verify JSON structure in database

## Related Issues
- [EF Core JSON Columns Documentation](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns)
- [Shipping Implementation](/docs/implementations/features/2025-08-shipping-zones-implementation.md)
- [DTO Validation Issues](/docs/troubleshooting/api/api-01-dto-validation.md)

## Search Keywords
- npgsql json serialization error
- EnableDynamicJson not found
- List string jsonb postgresql
- System.InvalidCastException jsonb
- Type List1 required dynamic JSON serialization
- NpgsqlDataSourceBuilder configuration
- Entity Framework Core PostgreSQL JSON