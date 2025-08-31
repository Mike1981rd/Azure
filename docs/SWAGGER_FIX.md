# Swagger Internal Server Error Fix - Complete Solution

## Problem
Swagger was returning "Internal Server Error" for `/swagger/v1/swagger.json`

## Root Causes
1. Circular references in the Customer entity models were causing serialization issues
2. Duplicate IEncryptionService interfaces in different namespaces causing DI conflicts
3. Navigation properties in related models causing circular reference loops

## Solution Applied

### 1. Added JsonIgnore Attributes to ALL Navigation Properties
Added `[JsonIgnore]` attributes to prevent circular references:

**Customer-related models:**
- `CustomerAddress.cs` - Added JsonIgnore to `Customer` navigation property
- `CustomerPaymentMethod.cs` - Added JsonIgnore to `Customer` navigation property  
- `CustomerNotificationPreference.cs` - Added JsonIgnore to `Customer` navigation property
- `CustomerDevice.cs` - Added JsonIgnore to `Customer` navigation property
- `CustomerWishlistItem.cs` - Added JsonIgnore to `Customer`, `Product`, and `ProductVariant` navigation properties
- `CustomerCoupon.cs` - Added JsonIgnore to `Customer` navigation property

**Product-related models:**
- `Product.cs` - Added JsonIgnore to `Company` and `Variants` navigation properties
- `ProductVariant.cs` - Added JsonIgnore to `Product` navigation property

### 2. Enhanced JSON Serialization Options
Updated `Program.cs` to include additional JSON serialization options:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular references
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
```

### 3. Resolved Duplicate IEncryptionService Conflict

**Problem:** Two IEncryptionService interfaces existed:
- `WebsiteBuilderAPI.Services.IEncryptionService`
- `WebsiteBuilderAPI.Services.Encryption.IEncryptionService`

**Solution:**
1. Renamed the duplicate `Services/EncryptionService.cs` to `.old` to prevent conflicts
2. Updated `AzulPaymentService.cs` to use the correct namespace: `using WebsiteBuilderAPI.Services.Encryption;`
3. Removed duplicate service registration in `Program.cs`

## Why This Works

1. **JsonIgnore on Navigation Properties**: Prevents Entity Framework navigation properties from being serialized, breaking circular references
2. **ReferenceHandler.IgnoreCycles**: System-wide handling of any remaining circular references
3. **DefaultIgnoreCondition**: Reduces payload size by not serializing null values

## Testing
After applying these changes:
1. Rebuild the project in Visual Studio
2. Run the application
3. Navigate to `/swagger` to verify Swagger UI loads correctly
4. Test API endpoints to ensure they still return proper data

## Note for Future Models
When adding new models with navigation properties, always add `[JsonIgnore]` to prevent circular reference issues:

```csharp
using System.Text.Json.Serialization;

// Navigation property
[JsonIgnore]
public virtual ParentEntity Parent { get; set; }
```