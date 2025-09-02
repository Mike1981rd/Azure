# 📋 PLAN DE IMPLEMENTACIÓN: Drag & Drop para Website Builder

## 🎯 Objetivo Principal
Implementar un sistema de drag & drop moderno y robusto para reordenar secciones en el panel lateral del editor, con validaciones jerárquicas y sincronización en tiempo real con el preview.

## 📊 Análisis del Sistema Actual

### Páginas que usan el Editor
1. **Página de Inicio** (Home)
2. **Página de Producto** (Product)
3. **Página de Carrito** (Cart)
4. **Página de Checkout** (Checkout)
5. **Página de Colección** (Collection)
6. **Todas las Colecciones** (All Collections)
7. **Todos los Productos** (All Products)

### Componentes Estructurales (Globales)
- **Header** - Compartido entre todas las páginas
- **AnnouncementBar** - Compartido entre todas las páginas
- **Footer** - Compartido entre todas las páginas
- **CartDrawer** - Compartido entre todas las páginas

### Grupos de Secciones
```
headerGroup     → [AnnouncementBar, Header]
asideGroup      → [CartDrawer, SearchDrawer]
template        → [Secciones específicas de cada página]
footerGroup     → [Footer, Newsletter]
```

## 🏗️ Arquitectura de la Solución

### 1. Estructura de Archivos (Modular < 300 líneas)

```
/websitebuilder-admin/src/
├── lib/dragDrop/
│   ├── index.ts                    (30 líneas - exports)
│   ├── types.ts                    (80 líneas - interfaces)
│   ├── rules.ts                    (150 líneas - reglas de validación)
│   ├── validators.ts               (120 líneas - funciones de validación)
│   └── constants.ts                (50 líneas - constantes y config)
│
├── hooks/
│   ├── useSectionDragDrop.ts       (180 líneas - lógica principal)
│   └── useDragValidation.ts        (100 líneas - validaciones)
│
├── components/editor/
│   ├── DraggableSection.tsx        (150 líneas - wrapper draggable)
│   ├── DroppableGroup.tsx          (120 líneas - contenedor de grupo)
│   └── DragIndicator.tsx           (80 líneas - indicador visual)
│
└── stores/
    └── useEditorStore.ts            (actualizar con nuevas acciones)
```

### 2. Dependencias a Instalar

```json
{
  "@dnd-kit/core": "^6.1.0",
  "@dnd-kit/sortable": "^8.0.0",
  "@dnd-kit/utilities": "^3.2.2",
  "@dnd-kit/modifiers": "^7.0.0"
}
```

## 📐 Reglas de Validación

### Nivel 1: Restricciones de Grupo
```typescript
const GROUP_RESTRICTIONS = {
  headerGroup: {
    canReceiveFrom: ['headerGroup'],  // Solo puede recibir de sí mismo
    canMoveTo: ['headerGroup'],        // Solo puede mover dentro de sí mismo
    allowedTypes: [SectionType.HEADER, SectionType.ANNOUNCEMENT_BAR]
  },
  template: {
    canReceiveFrom: ['template'],      // Solo reordenar interno
    canMoveTo: ['template'],
    allowedTypes: [/* tipos dinámicos según página */]
  },
  footerGroup: {
    canReceiveFrom: ['footerGroup'],
    canMoveTo: ['footerGroup'],
    allowedTypes: [SectionType.FOOTER, SectionType.NEWSLETTER]
  },
  asideGroup: {
    canReceiveFrom: ['asideGroup'],
    canMoveTo: ['asideGroup'],
    allowedTypes: [SectionType.CART_DRAWER, SectionType.SEARCH_DRAWER]
  }
};
```

### Nivel 2: Restricciones Jerárquicas
```typescript
const HIERARCHY_RULES = {
  // Componentes que NO pueden tener hijos
  standalone: [
    SectionType.HEADER,
    SectionType.FOOTER,
    SectionType.ANNOUNCEMENT_BAR
  ],
  
  // Componentes que pueden contener otros
  containers: [
    SectionType.PRODUCT_GRID,
    SectionType.IMAGE_WITH_TEXT
  ],
  
  // Máxima profundidad de anidamiento
  maxNestingLevel: 2
};
```

### Nivel 3: Restricciones de Orden
```typescript
const ORDER_RULES = {
  headerGroup: {
    // AnnouncementBar SIEMPRE debe estar antes que Header
    fixedOrder: [
      SectionType.ANNOUNCEMENT_BAR,
      SectionType.HEADER
    ]
  }
};
```

## 🔄 Flujo de Implementación

### Fase 1: Setup Base (2 horas)
1. ✅ Instalar @dnd-kit
2. ✅ Crear estructura de carpetas
3. ✅ Definir tipos e interfaces
4. ✅ Configurar constantes

### Fase 2: Reglas y Validadores (3 horas)
1. ✅ Implementar reglas de grupo
2. ✅ Implementar validadores jerárquicos
3. ✅ Crear sistema de feedback visual
4. ✅ Tests de validación

### Fase 3: Componentes Core (4 horas)
1. ✅ DraggableSection wrapper
2. ✅ DroppableGroup container
3. ✅ DragIndicator visual
4. ✅ Integración con SectionItem existente

### Fase 4: Hook Principal (3 horas)
1. ✅ useSectionDragDrop
2. ✅ Manejo de eventos
3. ✅ Sincronización con store
4. ✅ Optimización de renders

### Fase 5: Integración (4 horas)
1. ✅ Actualizar EditorSidebar
2. ✅ Sincronizar con todas las páginas
3. ✅ Mantener sincronización de componentes estructurales
4. ✅ Actualizar preview en tiempo real

### Fase 6: Testing y Polish (2 horas)
1. ✅ Test en todas las páginas
2. ✅ Validar reglas de negocio
3. ✅ Optimizar performance
4. ✅ Documentación

## 🎨 Experiencia de Usuario

### Visual Feedback
```typescript
const DRAG_STATES = {
  idle: {
    opacity: 1,
    cursor: 'grab'
  },
  dragging: {
    opacity: 0.5,
    cursor: 'grabbing',
    scale: 1.02
  },
  validDrop: {
    borderColor: 'green',
    backgroundColor: 'rgba(0, 255, 0, 0.1)'
  },
  invalidDrop: {
    borderColor: 'red',
    backgroundColor: 'rgba(255, 0, 0, 0.1)',
    cursor: 'not-allowed'
  }
};
```

### Animaciones
- Uso de `spring` animations de @dnd-kit
- Transiciones suaves al reordenar
- Feedback inmediato sin lag

## 🔌 Integración con Sistema Actual

### 1. Store Updates
```typescript
// useEditorStore.ts - Nuevas acciones
interface EditorStore {
  // Existentes
  reorderSections: (groupId, fromIndex, toIndex) => void;
  
  // Nuevas
  canDropSection: (sourceId, targetGroup, targetIndex) => boolean;
  validateSectionMove: (sourceId, targetId) => ValidationResult;
  batchReorderSections: (changes: ReorderChange[]) => void;
}
```

### 2. Sincronización Global
```typescript
// Los componentes estructurales se sincronizan via:
- StructuralComponentsContext (para Header, Footer, etc.)
- useEditorStore (para secciones de template)
- Auto-sync con preview via useEffect
```

### 3. Persistencia
```typescript
// Al reordenar:
1. Update local state (inmediato)
2. Update store (trigger isDirty)
3. Show Save button
4. On Save → Persist to DB
```

## 🚫 Casos Edge a Manejar

1. **Drag entre páginas**: No permitido (cada página tiene su propio template)
2. **Secciones vacías**: Permitir drop en grupos vacíos
3. **Única sección**: No permitir drag si es la única en su grupo
4. **Componentes requeridos**: Header/Footer no pueden ser eliminados
5. **Límites de cantidad**: Algunas secciones tienen max instances

## 📊 Métricas de Éxito

- ✅ Drag & drop funciona en las 7 páginas
- ✅ Validaciones previenen movimientos inválidos
- ✅ Preview se actualiza en < 100ms
- ✅ Sin pérdida de estado al reordenar
- ✅ Undo/Redo compatible
- ✅ Archivos < 300 líneas
- ✅ Performance: 60fps durante drag

## 🔧 Configuración Inicial

### package.json
```bash
npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities @dnd-kit/modifiers
```

### Configuración TypeScript
```typescript
// Ya compatible, no requiere cambios
```

## 📝 Notas de Implementación

1. **NO modificar** drag & drop existente en otros módulos (menus, collections)
2. **Respetar** la arquitectura modular (archivos pequeños)
3. **Mantener** sincronización con componentes estructurales
4. **Preservar** funcionalidad de visibility toggle
5. **Integrar** con sistema undo/redo existente

## 🎯 Resultado Esperado

Un sistema de drag & drop que:
- Sea intuitivo como Shopify
- Respete las reglas de negocio
- Funcione en todas las páginas
- Sea performante y moderno
- Sea fácil de mantener y extender

---

**Fecha de creación**: 2025-01-14
**Autor**: Claude Code
**Estado**: PLANIFICADO - Listo para implementar