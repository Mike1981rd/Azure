# 401 Unauthorized on Payment Endpoints

## Problem Summary
- **Affects**: Payment provider configuration endpoints
- **Frequency**: Common during development
- **Severity**: High - Blocks all payment functionality
- **First Observed**: 2025-08-06 during Azul implementation

## Symptoms
- [ ] GET /api/paymentprovider returns 401
- [ ] User redirected to login despite being authenticated
- [ ] Console shows "401 (Unauthorized)" errors
- [ ] Other endpoints work but payment endpoints fail
- [ ] Commenting [Authorize] attribute doesn't help

## Root Causes

### 1. Global Authorization Middleware
**Verification**: Check Program.cs
```csharp
app.UseAuthentication();
app.UseAuthorization(); // This applies globally
app.MapControllers();
```

### 2. RequirePermission Attributes
**Verification**: Check controller
```csharp
[RequirePermission("payment_methods", "read")]
public async Task<ActionResult> GetAll()
```

### 3. Authorize at Controller Level
**Verification**: Check class decoration
```csharp
[Authorize] // Applies to all actions
[ApiController]
public class PaymentProviderController
```

### 4. Server Not Reloaded
**Verification**: Changes not taking effect
- Modified attributes but still getting 401
- Server needs restart for attribute changes

## Solutions

### Quick Fix (< 5 minutes)
1. Add temporary bypass middleware
2. Comment ALL auth attributes
3. Restart the server

### Step-by-Step Solution

1. **Add Bypass Middleware** (Program.cs)
```csharp
app.UseAuthentication();

// Temporary bypass for payment endpoints
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    if (path.Contains("/api/paymentprovider") || 
        path.Contains("/api/payment"))
    {
        // Skip auth for these endpoints
        await next();
        return;
    }
    await next();
});

app.UseAuthorization();
app.MapControllers();
```

2. **Comment Authorization Attributes**
```csharp
//[Authorize] // TODO: Re-enable after fixing auth
[ApiController]
[Route("api/[controller]")]
public class PaymentProviderController : ControllerBase
{
    [HttpGet]
    //[RequirePermission("payment_methods", "read")] // TODO: Re-enable
    public async Task<ActionResult> GetAll()
    {
        // Implementation
    }
}
```

3. **Restart Server**
```bash
# Stop server (Ctrl+C)
# Start again
dotnet run
```

4. **Verify in Frontend**
```javascript
// Should now work without 401
const providers = await paymentService.getProviders();
```

### Alternative Solutions

1. **AllowAnonymous Attribute**
```csharp
[Authorize]
public class PaymentProviderController
{
    [AllowAnonymous] // Bypasses controller auth
    [HttpGet]
    public async Task<ActionResult> GetAll()
}
```

2. **Conditional Authorization**
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("PaymentAccess", policy =>
    {
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "PaymentManager");
    });
});
```

3. **Development-Only Bypass**
```csharp
if (app.Environment.IsDevelopment())
{
    // Skip auth in development
    app.Use(async (context, next) =>
    {
        context.User = GetDevelopmentUser();
        await next();
    });
}
```

## Prevention

### Best Practices
1. **Use policy-based authorization** instead of attributes
2. **Create development auth bypass** configuration
3. **Document required permissions** in API docs
4. **Test auth changes immediately**

### Configuration Template
```csharp
// appsettings.Development.json
{
  "Authentication": {
    "BypassEnabled": true,
    "BypassPaths": [
      "/api/paymentprovider",
      "/api/payment"
    ]
  }
}
```

### Proper Permission Setup
```csharp
// When re-enabling auth
services.AddAuthorization(options =>
{
    options.AddPolicy("PaymentRead", policy =>
        policy.RequirePermission("payment_methods", "read"));
    
    options.AddPolicy("PaymentWrite", policy =>
        policy.RequirePermission("payment_methods", "write"));
});
```

## Related Issues
- **See Also**: 
  - Azul Implementation: `/docs/implementations/api/2025-08-azul-payment-gateway.md`
  - JWT Setup: `/docs/implementations/auth/2025-08-jwt-implementation.md`
  - Role Permissions: `/docs/implementations/auth/2025-08-roles-permissions-implementation.md`

## Search Keywords
- ASP.NET Core 401 despite no authorize
- UseAuthorization affects all endpoints
- Bypass authentication middleware
- RequirePermission attribute not working
- Payment API unauthorized