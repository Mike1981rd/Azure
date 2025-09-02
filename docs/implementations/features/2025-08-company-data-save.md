# Company Data Save Implementation

## Overview
- **Purpose**: Implementar un sistema robusto de guardado de datos de empresa con manejo separado para logo y campos generales
- **Scope**: Formulario de empresa, upload de logo, auto-save, validaciones
- **Dependencies**: ASP.NET Core 8, Entity Framework Core, React Hook Form, Axios
- **Date Implemented**: 2025-08-06

## Architecture Decisions
- **Pattern Used**: Separación de endpoints para operaciones específicas
- **Technology Choices**: 
  - React Hook Form para manejo de formularios
  - Endpoints REST separados para logo vs datos generales
  - Auto-save para logo y tamaño
- **Security Considerations**: 
  - Validación de tipos de archivo para logos
  - Sanitización de inputs
  - Permisos basados en roles

## Implementation Details

### Backend

#### Models Modified
- `Company.cs` - Ya existente con todos los campos necesarios
- `UpdateCompanyRequestDto.cs` - Agregado campo Logo
- `UpdateLogoDto.cs` - Nuevo DTO específico para actualización de logo

#### API Endpoints

1. **PUT /api/company/current** - Actualización general de datos
```csharp
[HttpPut("current")]
[RequirePermission("company", "update")]
public async Task<ActionResult<CompanyResponseDto>> UpdateCurrentCompany([FromBody] UpdateCompanyRequestDto request)
```

2. **PUT /api/company/current/logo** - Actualización específica de logo
```csharp
[HttpPut("current/logo")]
[RequirePermission("company", "update")]
public async Task<ActionResult> UpdateLogo([FromBody] UpdateLogoDto request)
{
    try
    {
        await _companyService.UpdateLogoAsync(request.Logo);
        return Ok(new { message = "Logo updated successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating logo");
        return StatusCode(500, new { message = "Internal server error" });
    }
}
```

3. **PUT /api/company/current/logo-size** - Actualización de tamaño de logo
```csharp
[HttpPut("current/logo-size")]
[RequirePermission("company", "update")]
public async Task<ActionResult> UpdateLogoSize([FromBody] UpdateLogoSizeDto request)
```

#### Services

**CompanyService.cs** - Lógica de actualización condicional:
```csharp
public async Task<CompanyResponseDto?> UpdateCurrentCompanyAsync(UpdateCompanyRequestDto request)
{
    var company = await _context.Companies.FirstOrDefaultAsync();
    if (company == null) return null;

    // Actualizar SOLO si el campo tiene valor
    if (!string.IsNullOrEmpty(request.Name))
        company.Name = request.Name;
    
    if (request.Logo != null)
        company.Logo = request.Logo;
    
    // Resto de campos con validación condicional...
    if (request.PhoneNumber != null && request.PhoneNumber != "")
        company.PhoneNumber = request.PhoneNumber;
    else if (request.PhoneNumber == "")
        company.PhoneNumber = null;
    
    // ... más campos
    
    await _context.SaveChangesAsync();
    return await GetCurrentCompanyAsync();
}
```

**Método específico para logo**:
```csharp
public async Task UpdateLogoAsync(string logoUrl)
{
    var currentCompany = await _context.Companies.FirstOrDefaultAsync();
    if (currentCompany != null)
    {
        currentCompany.Logo = logoUrl;
        currentCompany.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

### Frontend

#### Components Created/Modified

1. **LogoUploader.tsx** - Componente moderno con drag & drop
```typescript
export function LogoUploader({ currentLogo, currentSize, onLogoChange, onSizeChange }: LogoUploaderProps) {
  const [preview, setPreview] = useState<string | null>(currentLogo || null);
  
  // Sincronización con props
  useEffect(() => {
    if (currentLogo) {
      setPreview(currentLogo);
    }
  }, [currentLogo]);
  
  // Manejo de upload con preview inmediato
  const handleFileUpload = useCallback(async (file: File) => {
    // Validaciones
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      setError('Tipo de archivo inválido');
      return;
    }
    
    // Preview inmediato
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
    
    // Upload
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post('/upload/image', formData);
    
    if (response.data.url) {
      onLogoChange(response.data.url);
    }
  }, [onLogoChange]);
}
```

2. **StoreDetailsForm.tsx** - Formulario principal con auto-save
```typescript
const handleLogoChange = async (newLogoUrl: string) => {
  setLogo(newLogoUrl);
  try {
    // Endpoint específico para logo
    await api.put('/company/current/logo', { logo: newLogoUrl });
    await refetch();
    
    // Notificación de éxito
    setShowLogoSuccess(true);
    setTimeout(() => setShowLogoSuccess(false), 3000);
  } catch (error) {
    console.error('Error updating logo:', error);
  }
};
```

#### State Management
- Hook `useCompany` para gestión centralizada
- Estados locales para logo y tamaño con sincronización
- React Hook Form para validación de formularios

## Configuration

### Environment Variables
```env
NEXT_PUBLIC_API_URL=http://localhost:5266/api
```

### Static Files Configuration (Program.cs)
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});
```

## Testing

### Manual Testing Checklist
- [x] Guardar solo con nombre (campo obligatorio)
- [x] Guardar con todos los campos
- [x] Subir logo y verificar que no borra otros datos
- [x] Cambiar tamaño del logo
- [x] Recargar página y verificar preview del logo
- [x] Validar tipos de archivo permitidos
- [x] Verificar notificación de éxito

### Unit Tests Location
- Backend: `WebsiteBuilderAPI.Tests/Services/CompanyServiceTests.cs` (por implementar)
- Frontend: `__tests__/components/empresa/LogoUploader.test.tsx` (por implementar)

## Known Issues & Limitations

### Current Limitations
1. El logo se guarda automáticamente sin opción de cancelar
2. No hay validación de dimensiones de imagen
3. El tamaño máximo está fijo en 5MB

### Future Improvements
1. Agregar crop/resize de imágenes antes de subir
2. Implementar compresión automática de imágenes
3. Agregar preview de diferentes tamaños (login, sidebar)
4. Implementar versionado de logos

### Performance Considerations
- Las imágenes se sirven directamente desde el servidor
- No hay CDN implementado aún
- El auto-save puede generar múltiples requests

## Troubleshooting
- **Error 400 al guardar**: Ver [Company Save 400 Error](/docs/troubleshooting/features/features-06-company-save-400-error.md)
- **Logo borra datos**: Ver [Logo Update Data Loss](/docs/troubleshooting/features/features-07-logo-update-data-loss.md)
- **Preview no aparece**: Ver [Logo Preview Not Loading](/docs/troubleshooting/features/features-08-logo-preview-not-loading.md)

## References
- [Documentación de React Hook Form](https://react-hook-form.com/)
- [Entity Framework Core Update Strategies](https://docs.microsoft.com/en-us/ef/core/saving/basic)
- [ASP.NET Core Static Files](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files)