# 🚀 GUÍA RÁPIDA: CREAR NUEVO MÓDULO

## ⚠️ IMPORTANTE: EditorPreview.tsx está CONGELADO
**NO agregar más código a EditorPreview.tsx (839 líneas actuales)**

## ✅ PASOS PARA CREAR UN NUEVO MÓDULO

### 1. Validar antes de empezar
```bash
./validate-module.sh websitebuilder-admin/src/components/editor/EditorPreview.tsx
# Resultado esperado: ❌ ERROR - usar modules/
```

### 2. Crear estructura del módulo
```bash
# Ejemplo: Crear módulo Gallery
MODULE_NAME="Gallery"
cd websitebuilder-admin/src/components/editor

# Crear carpeta del módulo
mkdir -p modules/$MODULE_NAME

# Crear archivos base
touch modules/$MODULE_NAME/${MODULE_NAME}Editor.tsx
touch modules/$MODULE_NAME/${MODULE_NAME}Preview.tsx
touch modules/$MODULE_NAME/${MODULE_NAME}Config.tsx
touch modules/$MODULE_NAME/${MODULE_NAME}Types.ts
touch modules/$MODULE_NAME/index.ts
```

### 3. Estructura de archivos

#### `GalleryTypes.ts` (máx 100 líneas)
```typescript
export interface GallerySettings {
  images: GalleryImage[];
  layout: 'grid' | 'carousel' | 'masonry';
  columns: number;
  spacing: number;
}

export interface GalleryImage {
  id: string;
  url: string;
  alt: string;
  caption?: string;
}
```

#### `GalleryEditor.tsx` (máx 300 líneas)
```typescript
/**
 * @file GalleryEditor.tsx
 * @max-lines 300
 * @module Gallery
 * @created 2025-01-15
 */

import React from 'react';
import { GallerySettings } from './GalleryTypes';

interface GalleryEditorProps {
  settings: GallerySettings;
  onChange: (settings: GallerySettings) => void;
}

export const GalleryEditor: React.FC<GalleryEditorProps> = ({
  settings,
  onChange
}) => {
  // Lógica del editor aquí (máx 280 líneas)
  return <div>Editor UI</div>;
};
```

#### `GalleryPreview.tsx` (máx 300 líneas)
```typescript
/**
 * @file GalleryPreview.tsx
 * @max-lines 300
 * @module Gallery
 */

import React from 'react';
import { GallerySettings } from './GalleryTypes';

interface GalleryPreviewProps {
  settings: GallerySettings;
}

export const GalleryPreview: React.FC<GalleryPreviewProps> = ({
  settings
}) => {
  // Renderizado del preview (máx 280 líneas)
  return <div>Preview</div>;
};
```

#### `index.ts` (exportaciones)
```typescript
export * from './GalleryEditor';
export * from './GalleryPreview';
export * from './GalleryConfig';
export * from './GalleryTypes';
```

### 4. Integración con el sistema

#### En `EditorPreview.tsx` - NO HACER ESTO ❌
```typescript
// ❌ INCORRECTO - No agregar más imports a EditorPreview.tsx
import { GalleryPreview } from './modules/Gallery';
```

#### En su lugar, usar el sistema modular ✅
```typescript
// ✅ CORRECTO - Crear un componente Registry
// components/editor/modules/ModuleRegistry.tsx
export const moduleComponents = {
  gallery: () => import('./Gallery'),
  testimonials: () => import('./Testimonials'),
  // etc...
};
```

### 5. Validación continua

Antes de cada modificación:
```bash
./validate-module.sh [archivo]
```

### 📊 Límites por archivo

| Tipo de Archivo | Líneas Máx | Alerta en |
|----------------|------------|-----------|
| Editor.tsx     | 300        | 250       |
| Preview.tsx    | 300        | 250       |
| Config.tsx     | 300        | 250       |
| Types.ts       | 100        | 80        |
| index.ts       | 20         | N/A       |

### 🎯 Checklist antes de codear

- [ ] ¿Ejecuté `validate-module.sh`?
- [ ] ¿Creé la carpeta en `/modules/`?
- [ ] ¿Separé editor, preview y config?
- [ ] ¿Los tipos están en archivo separado?
- [ ] ¿Cada archivo tiene el header de validación?
- [ ] ¿Ningún archivo excederá 300 líneas?

### 🚫 Lo que NO hacer

1. **NO** modificar EditorPreview.tsx
2. **NO** crear archivos de más de 300 líneas
3. **NO** mezclar lógica de editor y preview
4. **NO** poner todo en un solo archivo
5. **NO** ignorar las alertas del validador

## 🔴 ERRORES COMUNES Y SOLUCIONES (Lecciones del Slideshow)

### ❌ Error 1: Módulo no persiste al cambiar de página
**Problema:** El módulo desaparece cuando cambias de página y vuelves.
**Causa:** No se está guardando en localStorage o backend.
**Solución:**
```typescript
// En useEditorStore.ts - savePage()
const pageKey = `page_sections_${state.selectedPageId}`;
const sectionsToSave = state.sections.template;
localStorage.setItem(pageKey, JSON.stringify(sectionsToSave));
```

### ❌ Error 2: Módulos con hijos - Drag & Drop no habilitado
**Problema:** Los elementos hijos no tienen drag & drop.
**Solución:** Crear componente `[Module]Children.tsx`:
```typescript
// modules/[Module]/[Module]Children.tsx
import { DndContext, DragEndEvent } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';

// Implementar DraggableItem con useSortable
// Ver: modules/Slideshow/SlideshowChildren.tsx como referencia
```
**Integración en EditorSidebarWithDnD.tsx:**
```typescript
{section.type === SectionType.YOUR_MODULE && section.visible && (
  <YourModuleChildren section={section} groupId={group.id} />
)}
```

### ❌ Error 3: Click en hijo no abre configuración
**Problema:** Al hacer click en un elemento hijo, no se abre su editor.
**Solución completa:**

1. **En [Module]Children.tsx - Usar ID especial:**
```typescript
const handleSelectChild = (childId: string) => {
  // Formato especial: parentId:child:childId
  selectSection(`${section.id}:child:${childId}`);
  toggleConfigPanel(true);
};
```

2. **En EditorSidebarWithDnD.tsx - Crear sección virtual:**
```typescript
// Agregar después de las otras verificaciones de secciones virtuales
if (!selectedSection && selectedSectionId?.includes(':child:')) {
  const [sectionId] = selectedSectionId.split(':child:');
  const parentSection = Object.values(sections).flat().find(s => s.id === sectionId);
  
  selectedSection = {
    id: selectedSectionId,
    type: 'MODULE_CHILD' as any,
    name: 'Child Item',
    visible: true,
    settings: parentSection?.settings || {},
    sortOrder: 0
  } as any;
}
```

3. **En ConfigPanel.tsx - Detectar y renderizar editor hijo:**
```typescript
// Agregar al inicio del componente
const isChildItem = selectedSectionId?.includes(':child:');
const getParentSectionId = () => {
  if (!isChildItem || !selectedSectionId) return null;
  return selectedSectionId.split(':child:')[0];
};
const getChildId = () => {
  if (!isChildItem || !selectedSectionId) return null;
  return selectedSectionId.split(':child:')[1];
};

// Después de los hooks, antes de otros returns
if (isChildItem) {
  const parentId = getParentSectionId();
  const childId = getChildId();
  if (parentId && childId) {
    return <ChildEditor sectionId={parentId} childId={childId} />;
  }
}
```

### ❌ Error 4: Navegación incorrecta en editores hijos
**Problema:** Al presionar "volver" en un editor hijo, abre la configuración del padre en lugar del sidebar principal.
**Solución:** En el editor hijo, usar `selectSection(null)`:
```typescript
// En [Child]Editor.tsx
const handleBack = () => {
  selectSection(null);  // Volver al sidebar principal, NO al padre
};
```

### ❌ Error 5: UI de hijos con estilos incorrectos
**Problema:** Los elementos hijos se ven como tarjetas en lugar de items anidados.
**Solución:** Usar el mismo estilo que otros elementos del sidebar:
```typescript
// Estilo correcto para items hijos
<div className={`
  group relative flex items-center px-4 py-2 cursor-pointer transition-all
  hover:bg-gray-100
  ${isDragging ? 'shadow-lg bg-white' : ''}
  ${!item.visible ? 'opacity-50' : ''}
`}>
  {/* Chevron de indentación */}
  <div className="ml-4 mr-2">
    <svg className="w-2 h-2 text-gray-400" fill="currentColor" viewBox="0 0 6 10">
      <path d="M1 1l4 4-4 4" />
    </svg>
  </div>
  {/* Contenido */}
</div>
```

## ✅ CHECKLIST COMPLETO PARA MÓDULOS CON HIJOS

- [ ] Crear `[Module]Children.tsx` con DnD local
- [ ] Agregar integración en `EditorSidebarWithDnD.tsx`
- [ ] Crear `[Child]Editor.tsx` para configuración individual
- [ ] Agregar sección virtual en `EditorSidebarWithDnD.tsx`
- [ ] Agregar detección de hijo en `ConfigPanel.tsx`
- [ ] Usar `selectSection(null)` en handleBack del editor hijo
- [ ] Aplicar estilos consistentes (sin fondos azules, indentación correcta)
- [ ] Probar persistencia al cambiar de página
- [ ] Verificar que drag & drop funciona
- [ ] Confirmar que click abre configuración
- [ ] Validar que "volver" regresa al sidebar principal

### 💡 Comando rápido para nuevo módulo

```bash
#!/bin/bash
# create-module.sh
MODULE=$1
cd websitebuilder-admin/src/components/editor
mkdir -p modules/$MODULE
cd modules/$MODULE

cat > ${MODULE}Types.ts << 'EOF'
/**
 * @file ${MODULE}Types.ts
 * @max-lines 100
 */
export interface ${MODULE}Settings {
  // TODO: Define settings
}
EOF

cat > ${MODULE}Editor.tsx << 'EOF'
/**
 * @file ${MODULE}Editor.tsx
 * @max-lines 300
 */
import React from 'react';
import { ${MODULE}Settings } from './${MODULE}Types';

export const ${MODULE}Editor: React.FC = () => {
  return <div>${MODULE} Editor</div>;
};
EOF

cat > ${MODULE}Preview.tsx << 'EOF'
/**
 * @file ${MODULE}Preview.tsx
 * @max-lines 300
 */
import React from 'react';
import { ${MODULE}Settings } from './${MODULE}Types';

export const ${MODULE}Preview: React.FC = () => {
  return <div>${MODULE} Preview</div>;
};
EOF

cat > index.ts << 'EOF'
export * from './${MODULE}Editor';
export * from './${MODULE}Preview';
export * from './${MODULE}Types';
EOF

echo "✅ Módulo $MODULE creado en modules/$MODULE/"
```

---
**Recuerda:** El objetivo es mantener el código modular, limpio y mantenible. 
**Nunca** sacrifiques estas reglas por "rapidez".