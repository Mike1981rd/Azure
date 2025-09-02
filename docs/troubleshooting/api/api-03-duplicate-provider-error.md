# "A azul provider already exists" Error

## Problem Summary
- **Affects**: Payment provider configuration
- **Frequency**: Common after auto-initialization
- **Severity**: Medium - Blocks new configuration
- **First Observed**: 2025-08-06 during Azul testing

## Symptoms
- [ ] Error: "A azul provider already exists for this company"
- [ ] Cannot save provider configuration
- [ ] Provider shows as "inactive" in list
- [ ] Form shows empty fields but provider exists
- [ ] 400 Bad Request on save

## Root Causes

### 1. Auto-Initialization Creates Providers
**Verification**: Check GetAllAsync service
```csharp
if (!hasProviders)
{
    await InitializeDefaultProvidersAsync(company.Id);
}
```

### 2. Create vs Update Logic Missing
**Verification**: Controller always tries to create
```csharp
// ❌ Always creates new
var provider = await _service.CreateAsync(request);

// ✅ Should check if exists first
var existing = await GetExistingProvider(provider);
if (existing != null)
    return await _service.UpdateAsync(existing.Id, request);
```

### 3. Unique Constraint on Provider+Company
**Verification**: Database constraint
```sql
-- Unique index prevents duplicates
CREATE UNIQUE INDEX IX_PaymentProviders_CompanyId_Provider 
ON PaymentProviders(CompanyId, Provider);
```

## Solutions

### Quick Fix (< 5 minutes)
Delete existing provider and retry:
```sql
DELETE FROM PaymentProviders 
WHERE Provider = 'azul' AND CompanyId = 1;
```

### Step-by-Step Solution

1. **Update Controller to Handle Both Cases**
```csharp
[HttpPost("configure")]
[Consumes("multipart/form-data")]
public async Task<ActionResult> ConfigureWithFiles([FromForm] ConfigureProviderFormDto request)
{
    // Check if provider exists
    var existingProviders = await _paymentProviderService.GetAllAsync();
    var existingProvider = existingProviders.FirstOrDefault(
        p => p.Provider.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
    
    PaymentProviderResponseDto providerDto;
    
    if (existingProvider != null)
    {
        // Update existing
        var updateRequest = new UpdatePaymentProviderRequestDto
        {
            Name = request.Name,
            IsActive = existingProvider.IsActive,
            IsTestMode = request.IsTestMode,
            TransactionFee = request.TransactionFee,
            // Map other fields...
        };
        
        providerDto = await _paymentProviderService.UpdateAsync(
            existingProvider.Id, updateRequest);
    }
    else
    {
        // Create new
        var createRequest = new CreatePaymentProviderRequestDto
        {
            Provider = request.Provider,
            Name = request.Name,
            // Map fields...
        };
        
        providerDto = await _paymentProviderService.CreateAsync(createRequest);
    }
    
    // Handle file uploads...
    
    return Ok(providerDto);
}
```

2. **Smart Field Updates**
```csharp
// Only update non-empty credential fields
Auth1 = !string.IsNullOrWhiteSpace(request.Auth1) 
    ? request.Auth1 
    : null,
Auth2 = !string.IsNullOrWhiteSpace(request.Auth2) 
    ? request.Auth2 
    : null,
```

3. **Service Layer Protection**
```csharp
public async Task<PaymentProviderResponseDto> CreateAsync(CreatePaymentProviderRequestDto request)
{
    // Check for duplicates
    var existing = await _context.PaymentProviders
        .AnyAsync(p => p.CompanyId == companyId && 
                      p.Provider == request.Provider);
    
    if (existing)
    {
        throw new ArgumentException(
            $"A {request.Provider} provider already exists for this company");
    }
    
    // Continue with creation...
}
```

### Alternative Solutions

1. **Upsert Pattern**
```csharp
var provider = await _context.PaymentProviders
    .FirstOrDefaultAsync(p => p.Provider == request.Provider);

if (provider == null)
{
    provider = new PaymentProvider { Provider = request.Provider };
    _context.PaymentProviders.Add(provider);
}

// Update fields regardless
provider.Name = request.Name;
// ... other fields

await _context.SaveChangesAsync();
```

2. **Separate Endpoints**
```csharp
[HttpPost("providers/{provider}/configure")]
public async Task<ActionResult> Configure(
    string provider, 
    [FromForm] ConfigureDto dto)
{
    // Always updates specific provider
}
```

## Prevention

### Best Practices
1. **Design for idempotency** - Multiple calls should be safe
2. **Use upsert patterns** for configuration endpoints
3. **Clear error messages** indicating what exists
4. **Frontend should load existing data** when editing

### Auto-Init Considerations
```csharp
// InitializeDefaultProvidersAsync should be idempotent
var existingProviders = await _context.PaymentProviders
    .Where(p => p.CompanyId == companyId)
    .Select(p => p.Provider)
    .ToListAsync();

var toAdd = defaultProviders
    .Where(p => !existingProviders.Contains(p.Provider));
```

### Frontend Detection
```typescript
// Load existing provider data
const providers = await paymentService.getProviders();
const azul = providers.find(p => p.provider === 'azul');

if (azul) {
    // Show update UI
    setFormData({
        storeId: azul.storeId || '',
        // Don't show sensitive fields
    });
}
```

## Related Issues
- **See Also**: 
  - Azul Implementation: `/docs/implementations/api/2025-08-azul-payment-gateway.md`
  - CRUD Pattern: `/docs/implementations/features/2025-08-crud-pattern.md`
  - Database Constraints: `/docs/troubleshooting/database/db-01-unique-constraints.md`

## Search Keywords
- provider already exists error
- duplicate key constraint payment
- upsert pattern ASP.NET Core
- configure vs create endpoint
- payment provider initialization