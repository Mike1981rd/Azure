# Error 415 Unsupported Media Type with FormData

## Problem Summary
- **Affects**: File uploads with multipart/form-data
- **Frequency**: Common when implementing file uploads
- **Severity**: High - Blocks functionality
- **First Observed**: 2025-08-06 during Azul implementation

## Symptoms
- [ ] HTTP 415 (Unsupported Media Type) error
- [ ] FormData not being accepted by ASP.NET Core
- [ ] Console shows "Failed to load resource: 415"
- [ ] Works with JSON but fails with files
- [ ] Axios interceptor may override Content-Type

## Root Causes

### 1. Missing [Consumes] Attribute
**Verification**: Check controller method
```csharp
// ❌ Missing attribute
[HttpPost("configure")]
public async Task<IActionResult> Configure([FromForm] MyDto dto)

// ✅ Correct
[HttpPost("configure")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> Configure([FromForm] MyDto dto)
```

### 2. Incorrect Content-Type Header
**Verification**: Check network tab
```javascript
// ❌ Wrong - manual boundary
headers: { 'Content-Type': 'multipart/form-data' }

// ✅ Correct - let browser set boundary
// Don't set Content-Type for FormData
```

### 3. Axios Global Headers
**Verification**: Check axios config
```javascript
// ❌ Forces JSON for all requests
axios.defaults.headers.common['Content-Type'] = 'application/json';

// ✅ Smart header setting
if (!(config.data instanceof FormData)) {
  config.headers['Content-Type'] = 'application/json';
}
```

### 4. Missing [FromForm] on Parameters
**Verification**: Check all parameters
```csharp
// ❌ Missing [FromForm] on file parameters
public async Task<IActionResult> Upload(
    [FromForm] string name,
    IFormFile file)  // Missing attribute

// ✅ All parameters marked
public async Task<IActionResult> Upload(
    [FromForm] string name,
    [FromForm] IFormFile file)
```

## Solutions

### Quick Fix (< 5 minutes)
1. Add `[Consumes("multipart/form-data")]` to controller method
2. Remove Content-Type header from axios request
3. Restart ASP.NET Core application

### Step-by-Step Solution

1. **Update Controller**
```csharp
[HttpPost("configure")]
[Consumes("multipart/form-data")]
public async Task<ActionResult<PaymentProviderResponseDto>> ConfigureWithFiles(
    [FromForm] ConfigureProviderFormDto request)
{
    // Implementation
}
```

2. **Create Proper DTO**
```csharp
public class ConfigureProviderFormDto
{
    public string Provider { get; set; }
    public string Name { get; set; }
    public IFormFile? CertificateFile { get; set; }
    public IFormFile? PrivateKeyFile { get; set; }
}
```

3. **Fix Frontend Service**
```typescript
async configureProvider(config: ConfigDto, certFile?: File, keyFile?: File) {
    const formData = new FormData();
    
    // Add fields
    Object.entries(config).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
            formData.append(key, value.toString());
        }
    });
    
    // Add files
    if (certFile) {
        formData.append('CertificateFile', certFile, certFile.name);
    }
    
    // No Content-Type header!
    const response = await apiClient.post('/api/paymentprovider/configure', formData);
    return response.data;
}
```

4. **Configure ASP.NET Core**
```csharp
// Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});
```

### Alternative Solutions

1. **Separate Endpoints**
   - POST JSON data first
   - Then POST files separately
   - Link them by ID

2. **Base64 Encoding**
   - Convert files to base64
   - Send as JSON strings
   - Decode on server

3. **Direct Upload**
   - Upload to cloud storage
   - Send URLs to backend

## Prevention

### Best Practices
1. **Always use [Consumes] attribute** for file uploads
2. **Let browser handle multipart boundary**
3. **Test with Postman first** to verify endpoint
4. **Use consistent naming** (match DTO property names)

### Configuration Template
```csharp
// Startup configuration for file uploads
services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});
```

### Testing Checklist
- [ ] Test with small file first
- [ ] Verify network request shows correct Content-Type with boundary
- [ ] Check all form fields are received
- [ ] Test with missing optional files
- [ ] Verify large file handling

## Related Issues
- **See Also**: 
  - Azul Implementation: `/docs/implementations/api/2025-08-azul-payment-gateway.md`
  - CORS Issues: `/docs/troubleshooting/api/api-04-cors-multipart.md`
  - File Size Limits: `/docs/troubleshooting/api/api-05-file-size-limits.md`

## Search Keywords
- ASP.NET Core FormData 415
- multipart/form-data unsupported media type
- axios FormData Content-Type boundary
- [Consumes] attribute file upload
- IFormFile 415 error