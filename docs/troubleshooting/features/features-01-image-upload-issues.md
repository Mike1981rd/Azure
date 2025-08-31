# Image Upload System Troubleshooting

## Quick Reference
**Category**: Features  
**Component**: Image Upload System  
**Related Implementation**: [Shopify Image System](/docs/implementations/features/2025-01-shopify-image-system.md)  
**Last Updated**: January 13, 2025

## Common Issues

### 1. Upload Returns 404 Error

#### Symptoms
```
POST /api/upload/image failed with 404
Frontend Error: Upload error: {}
```

#### Root Cause
Frontend attempting to call relative API path instead of backend URL

#### Solution
```typescript
// ❌ INCORRECT
const response = await fetch('/api/upload/image', {
  method: 'POST',
  // ...
});

// ✅ CORRECT
const response = await fetch('http://localhost:5266/api/upload/image', {
  method: 'POST',
  // ...
});
```

#### Prevention
- Always use full backend URL for API calls
- Consider using environment variables for API base URL

---

### 2. Uploaded Image Not Displaying

#### Symptoms
- Image uploads successfully
- Preview shows broken image icon
- Console shows image URL is correct

#### Root Cause 1: Missing Protocol in URL
```typescript
// URL returned as: /uploads/logos/image.png
// Needs to be: http://localhost:5266/uploads/logos/image.png
```

#### Solution
```typescript
const imageUrl = data.url.startsWith('http') 
  ? data.url 
  : `http://localhost:5266${data.url}`;
```

#### Root Cause 2: ResponsiveImage Component Issues
The ResponsiveImage component was too complex for simple logo display

#### Solution
Replace with standard img tag:
```typescript
// ❌ Complex ResponsiveImage
<ResponsiveImage
  src={headerConfig.logo.desktopUrl}
  height={headerConfig.logo.height || 40}
  priority={true}
  objectFit="contain"
/>

// ✅ Simple img tag
<img
  src={headerConfig.logo.desktopUrl}
  alt={headerConfig.logo.alt || 'Company Logo'}
  style={{ 
    height: headerConfig.logo.height || 40,
    objectFit: 'contain'
  }}
/>
```

---

### 3. Desktop and Mobile Uploaders Overlapping

#### Symptoms
- Mobile logo uploader appears inside desktop uploader
- UI elements are superimposed
- Cannot interact with mobile uploader properly

#### Root Cause
Missing proper div structure and spacing in component hierarchy

#### Solution
```typescript
// ❌ INCORRECT - Flat structure
<div className="space-y-4">
  <ImageUpload ... />
  {/* Desktop size slider */}
  <ImageUpload ... />
  {/* Mobile size slider */}
</div>

// ✅ CORRECT - Nested structure with proper spacing
<div className="space-y-6">
  {/* Desktop logo section */}
  <div className="space-y-3">
    <ImageUpload ... />
    {getValue('logo.desktopUrl') && (
      <div>{/* Desktop size slider */}</div>
    )}
  </div>
  
  {/* Mobile logo section */}
  <div className="space-y-3">
    <ImageUpload ... />
    {getValue('logo.mobileUrl') && (
      <div>{/* Mobile size slider */}</div>
    )}
  </div>
</div>
```

---

### 4. Slider Changes Cause Lag

#### Symptoms
- Moving logo size slider causes stuttering
- UI freezes momentarily during adjustment
- Preview updates are delayed and jumpy

#### Root Cause
Every slider change triggers:
1. State update
2. onChange callback
3. Store update
4. Component re-render
5. History save
6. API call (potentially)

#### Solution
Implement debouncing:
```typescript
const handleChange = (path: string, newValue: any, debounce: boolean = false) => {
  // Always update local state immediately
  setLocalConfig(updated);
  
  // Debounce the onChange callback
  if (debounce) {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }
    debounceTimerRef.current = setTimeout(() => {
      onChange(updated);
    }, 150);
  } else {
    onChange(updated);
  }
};

// Use with slider
<Slider
  onValueChange={(values) => handleChange('logo.height', values[0], true)}
/>
```

---

### 5. Remove Button Not Working

#### Symptoms
- Clicking X button doesn't remove image
- Page navigates or refreshes instead
- Event bubbles to parent container

#### Root Cause
Missing event.preventDefault() and stopPropagation()

#### Solution
```typescript
const handleRemove = (e: React.MouseEvent) => {
  e.preventDefault();
  e.stopPropagation();
  setPreview(null);
  setError(null);
  onChange('');
  if (fileInputRef.current) {
    fileInputRef.current.value = '';
  }
};
```

---

### 6. CORS Issues with Image Loading

#### Symptoms
```
Access to image at 'http://localhost:5266/uploads/...' from origin 'http://localhost:3000' has been blocked by CORS policy
```

#### Root Cause
Backend not configured to allow cross-origin requests for static files

#### Solution
In ASP.NET Core `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

app.UseCors("AllowFrontend");
```

---

### 7. Large Images Fail to Upload

#### Symptoms
- Images over 5MB show error
- Upload appears to start but fails
- No error message displayed

#### Root Cause
Multiple size limits in the stack:
1. Client-side validation (5MB)
2. ASP.NET Core request size limit
3. IIS/Kestrel limits

#### Solution
1. **Client-side validation**:
```typescript
if (file.size > 5 * 1024 * 1024) {
  return { valid: false, error: 'Image size must be less than 5MB' };
}
```

2. **ASP.NET Core configuration**:
```csharp
// In Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

3. **Kestrel configuration**:
```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});
```

---

## Debugging Commands

### Check if image is accessible
```powershell
$imageUrl = 'http://localhost:5266/uploads/logos/[image-id].png'
Invoke-WebRequest -Uri $imageUrl -Method Head -UseBasicParsing
```

### Monitor upload requests
```javascript
// Add to browser console
const originalFetch = window.fetch;
window.fetch = function(...args) {
  console.log('Fetch:', args[0]);
  return originalFetch.apply(this, args).then(response => {
    console.log('Response:', response.status);
    return response;
  });
};
```

### Check component props
```javascript
// In EditorPreview.tsx
console.log('Header config:', headerConfig);
console.log('Logo config:', headerConfig?.logo);
console.log('Desktop URL:', headerConfig?.logo?.desktopUrl);
```

## Prevention Checklist

- [ ] Always use full URLs for backend API calls
- [ ] Validate file type and size on both client and server
- [ ] Handle loading and error states in UI
- [ ] Implement proper event handling for interactive elements
- [ ] Use debouncing for frequent updates (sliders, inputs)
- [ ] Test with various image formats and sizes
- [ ] Ensure CORS is properly configured
- [ ] Add proper TypeScript types for all props

## Related Issues

- [Header Configuration Issues](/docs/troubleshooting/features/features-02-header-config.md)
- [Theme System Problems](/docs/troubleshooting/features/features-03-theme-system.md)
- [API Communication Errors](/docs/troubleshooting/api/api-01-cors-issues.md)

---

**Keywords**: image upload, 404 error, CORS, file validation, preview, drag and drop, slider lag, debouncing

**Debugging Level**: Frontend/Backend Integration
**Frequency**: Common during initial setup
**Severity**: Medium - Affects user experience but not critical functionality