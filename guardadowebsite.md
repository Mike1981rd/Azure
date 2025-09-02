# 📋 PROBLEMAS DE GUARDADO Y SOLUCIONES - WEBSITE BUILDER

## 📅 Fecha: 13 de Enero 2025
## 👤 Contexto: Implementación del Editor de Website Builder

---

## 🔴 PROBLEMAS ENCONTRADOS Y RESUELTOS

### 1. ERROR 404 - Endpoint No Encontrado
**Problema:**
- Al abrir la configuración del header en el editor, aparecía "Failed to load configuration" / "Recurso no encontrado"
- El frontend hacía peticiones a `/api/structural-components/company/1`
- El backend respondía con 404

**Causa Raíz:**
- El controlador usaba `[Route("api/[controller]")]` que generaba la ruta `/api/structuralcomponents` (sin guión)
- El frontend esperaba `/api/structural-components` (con guión)

**Solución:**
```csharp
// StructuralComponentsController.cs
// ANTES:
[Route("api/[controller]")]

// DESPUÉS:
[Route("api/structural-components")]
```

---

### 2. MÉTODO NO IMPLEMENTADO - CreateOrUpdateAsync
**Problema:**
- Después de corregir la ruta, el backend lanzaba error porque el método `CreateOrUpdateAsync` no existía
- El controlador intentaba crear configuraciones por defecto si no existían

**Causa Raíz:**
- El servicio `StructuralComponentsService` no tenía implementado el método `CreateOrUpdateAsync`
- El controlador lo llamaba cuando no encontraba configuraciones existentes

**Solución:**
```csharp
// StructuralComponentsService.cs
public async Task<StructuralComponentsDto> CreateOrUpdateAsync(int companyId, CreateStructuralComponentsDto dto)
{
    var existing = await _context.StructuralComponentsSettings
        .Where(s => s.CompanyId == companyId && s.IsActive)
        .FirstOrDefaultAsync();

    if (existing != null)
    {
        // Update existing
        existing.HeaderConfig = dto.HeaderConfig ?? existing.HeaderConfig;
        // ... actualizar otros campos
        await _context.SaveChangesAsync();
        return MapToDto(existing);
    }
    else
    {
        // Create new with defaults
        var settings = new StructuralComponentsSettings
        {
            CompanyId = companyId,
            HeaderConfig = dto.HeaderConfig ?? DEFAULT_HEADER,
            // ... otros valores por defecto
        };
        _context.StructuralComponentsSettings.Add(settings);
        await _context.SaveChangesAsync();
        return MapToDto(settings);
    }
}
```

---

### 3. ERROR 404 - Logo No Encontrado
**Problema:**
- La configuración por defecto incluía `"desktopUrl": "/logo.png"`
- Como el usuario no había subido ningún logo, el navegador mostraba error 404

**Causa Raíz:**
- Los valores por defecto asumían que existía un archivo de logo

**Solución:**
```csharp
// StructuralComponentsService.cs - DEFAULT_HEADER
"logo": {
    "desktopUrl": "",  // Cambiar de "/logo.png" a cadena vacía
    "mobileUrl": "",   // Cambiar de "/logo.png" a cadena vacía
    // ...
}
```

---

### 4. ERROR 405 - Method Not Allowed
**Problema:**
- Al intentar guardar cambios en la configuración del header, el servidor respondía con 405
- El frontend enviaba una petición `PUT` pero el backend esperaba `PATCH`

**Causa Raíz:**
- Discrepancia entre el método HTTP del frontend y el backend
- El controlador tenía `[HttpPatch]` pero el frontend usaba `PUT`

**Solución:**
```csharp
// StructuralComponentsController.cs
// ANTES:
[HttpPatch("company/{companyId}/component")]

// DESPUÉS:
[HttpPut("company/{companyId}/component")]
```

---

### 5. ERROR DE FOREIGN KEY - EditorHistories
**Problema:**
- Al guardar cambios, error de constraint de foreign key en la tabla EditorHistories
- Mensaje: "insert or update on table 'EditorHistories' violates foreign key constraint"

**Causa Raíz:**
- El `CreateHistoryDto` no incluía el campo `CompanyId`
- La tabla EditorHistories requiere CompanyId como foreign key

**Solución Temporal:**
```csharp
// StructuralComponentsService.cs
// Comentar temporalmente las llamadas al servicio de historial
// await _historyService.SaveHistoryAsync(new CreateHistoryDto { ... });

// TODO: Fix history service to include CompanyId properly
```

**Solución Definitiva Pendiente:**
- Agregar `CompanyId` al `CreateHistoryDto`
- Actualizar todas las llamadas para incluir el CompanyId

---

### 6. ERROR CONCEPTUAL - Persistencia de Secciones
**Problema:**
- Al refrescar la página, el header desaparecía de la barra lateral
- Al cambiar entre páginas (Home, Product, etc.), los componentes estructurales se perdían

**Causa Raíz:**
- Error conceptual: Los componentes estructurales (Header, Footer, etc.) se trataban como secciones normales
- El store solo persistía `selectedPageId` y `selectedPageType`, no las secciones
- Al cambiar de página, se cargaban nuevas secciones borrando las estructurales

**Solución:**
```typescript
// useEditorStore.ts
// 1. Persistir las secciones en localStorage
partialize: (state) => ({
  selectedPageId: state.selectedPageId,
  selectedPageType: state.selectedPageType,
  sections: state.sections // Agregar persistencia de secciones
})

// 2. Crear método para inicializar componentes estructurales
initializeStructuralComponents: () => {
  // Crear Header, Announcement Bar, Footer, Cart Drawer si no existen
}

// 3. Modificar loadPageSections para preservar componentes estructurales
loadPageSections: (sections) => {
  const state = get();
  const newSections = {
    headerGroup: state.sections.headerGroup,    // Mantener
    asideGroup: state.sections.asideGroup,      // Mantener
    template: [],                                // Reemplazar con nuevas
    footerGroup: state.sections.footerGroup     // Mantener
  };
  // Solo procesar secciones de template...
}
```

---

### 7. ERROR DE LAYOUT - Footer No al Final
**Problema:**
- El footer aparecía inmediatamente después del header, no al final de la página
- No respetaba la estructura típica de una página web

**Causa Raíz:**
- El preview combinaba todas las secciones linealmente sin respetar la estructura
- No se usaba flexbox correctamente para posicionar el footer

**Solución:**
```tsx
// EditorPreview.tsx
<div className="flex flex-col min-h-full">
  {/* Header sections */}
  <div className="flex-shrink-0">
    {headerSections.map(renderSection)}
  </div>
  
  {/* Main content - crece para llenar espacio */}
  <div className="flex-1">
    {templateSections.map(renderSection)}
  </div>
  
  {/* Footer sections - se pega al fondo */}
  <div className="flex-shrink-0 mt-auto">
    {footerSections.map(renderSection)}
  </div>
</div>
```

---

## ✅ MEJORAS IMPLEMENTADAS

### 1. Componentes Estructurales como Elementos Globales
- Header, Announcement Bar, Footer y Cart Drawer son globales en todas las páginas
- No se pueden eliminar, solo ocultar/mostrar
- Se inicializan automáticamente al cargar el editor
- Se mantienen al navegar entre páginas

### 2. Prevención de Eliminación
```tsx
// SectionItem.tsx
const isStructuralComponent = [
  SectionType.HEADER,
  SectionType.ANNOUNCEMENT_BAR,
  SectionType.FOOTER,
  SectionType.CART_DRAWER
].includes(section.type);

// Solo mostrar botón de eliminar para secciones no estructurales
{!isStructuralComponent && (
  <button onClick={handleDelete}>
    <Trash2 />
  </button>
)}
```

### 3. Restricción de Agregar Secciones en Header Group
```tsx
// EditorSidebar.tsx
// Ocultar botón de agregar sección solo en headerGroup
{group.id !== 'headerGroup' && (
  <button onClick={() => openAddModal(group.id)}>
    <Plus /> Agregar sección
  </button>
)}
```

---

## 🔧 CONFIGURACIÓN ACTUAL FUNCIONANDO

### Backend (ASP.NET Core 8)
- ✅ Ruta correcta: `/api/structural-components`
- ✅ Métodos HTTP correctos: GET, PUT, POST
- ✅ Creación automática de configuraciones por defecto
- ✅ Almacenamiento en PostgreSQL con JSONB
- ⚠️ Servicio de historial temporalmente deshabilitado

### Frontend (Next.js 14)
- ✅ Hook `useStructuralComponents` funcionando
- ✅ Editor con componentes estructurales globales
- ✅ Persistencia en localStorage
- ✅ Layout correcto con footer al fondo
- ✅ Prevención de eliminación de componentes estructurales

---

## 📝 TAREAS PENDIENTES

1. **Arreglar EditorHistoryService**
   - Agregar campo CompanyId al CreateHistoryDto
   - Re-habilitar el tracking de cambios

2. **Crear Editores Faltantes**
   - AnnouncementBarEditor.tsx
   - FooterEditor.tsx
   - CartDrawerEditor.tsx

3. **Mejorar Sistema de Logs**
   - Implementar mejor tracking de errores
   - Agregar más información de contexto

---

## 🎯 LECCIONES APRENDIDAS

1. **Consistencia en Naming**: Mantener consistencia entre frontend y backend (structural-components vs structuralcomponents)
2. **Métodos HTTP**: Verificar que frontend y backend usen los mismos métodos (PUT vs PATCH)
3. **Foreign Keys**: Siempre incluir todos los campos requeridos en DTOs
4. **Valores por Defecto**: No asumir que existen recursos (como logos)
5. **Arquitectura Clara**: Separar claramente componentes estructurales de secciones de contenido
6. **Persistencia**: Considerar qué datos deben persistir al refrescar/navegar
7. **Layout Web**: Usar flexbox correctamente para estructura header-content-footer

---

## 🚀 COMANDOS ÚTILES PARA DEBUG

```powershell
# Ver logs del backend
Get-Content "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\logs\app-*.log" | Select-Object -Last 100

# Detener proceso del backend
powershell.exe -Command "Get-Process dotnet | Stop-Process -Force"

# Verificar puerto libre
powershell.exe -Command "Get-NetTCPConnection | Where-Object {$_.LocalPort -eq 5266}"
```

---

**Última actualización:** 13 de Enero 2025
**Versión:** 1.0
**Estado:** Sistema funcionando con soluciones temporales aplicadas