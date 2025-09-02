# 🎨 SETUP DESIGNS - UI Style Guide

## 📋 Tabla de Contenidos
1. [Filosofía de Diseño](#filosofía-de-diseño)
2. [Colores](#colores)
3. [Tipografía](#tipografía)
4. [Componentes Base](#componentes-base)
5. [Componentes Complejos](#componentes-complejos)
6. [Layout y Espaciado](#layout-y-espaciado)
7. [Estados Interactivos](#estados-interactivos)
8. [Dark Mode](#dark-mode)
9. [Animaciones](#animaciones)

---

## 🎯 Filosofía de Diseño

El diseño sigue los principios de **Shopify Polaris** adaptados a nuestro sistema:
- **Claridad**: Interfaz limpia y sin distracciones
- **Consistencia**: Patrones reutilizables en toda la aplicación
- **Accesibilidad**: Contraste adecuado y elementos interactivos claros
- **Responsividad**: Adaptable a diferentes tamaños de pantalla

---

## 🎨 Colores

### Paleta Principal

```css
/* Primary Colors */
--primary-blue: #3b82f6;      /* Acciones principales, links */
--primary-blue-hover: #2563eb; /* Hover state */

/* Grays - Light Mode */
--gray-50: #f9fafb;   /* Backgrounds sutiles */
--gray-100: #f3f4f6;  /* Hover backgrounds */
--gray-200: #e5e7eb;  /* Borders, divisores */
--gray-300: #d1d5db;  /* Borders de inputs */
--gray-400: #9ca3af;  /* Placeholder text */
--gray-500: #6b7280;  /* Texto secundario */
--gray-600: #4b5563;  /* Texto de labels */
--gray-700: #374151;  /* Texto principal */
--gray-800: #1f2937;  /* Headings */
--gray-900: #111827;  /* Texto muy importante */

/* States */
--success: #10b981;   /* Verde - Success */
--warning: #f59e0b;   /* Amarillo - Warning */
--error: #ef4444;     /* Rojo - Error */
--info: #3b82f6;      /* Azul - Info */
```

### Uso de Colores

| Elemento | Color | Uso |
|----------|-------|-----|
| Texto principal | `gray-700` | Contenido general |
| Texto secundario | `gray-500` | Descripciones, ayuda |
| Labels | `gray-700` | Etiquetas de formularios |
| Borders | `gray-300` | Inputs, cards |
| Hover backgrounds | `gray-100` | Botones secundarios |
| Active/Selected | `gray-900` con `bg-white` o `blue-500` | Estados activos |

---

## 📝 Tipografía

### Tamaños de Fuente

```css
/* Text Sizes */
text-xs: 0.75rem;   /* 12px - Labels, hints, metadata */
text-sm: 0.875rem;  /* 14px - Body text, inputs */
text-base: 1rem;    /* 16px - Default */
text-lg: 1.125rem;  /* 18px - Títulos de sección */
text-xl: 1.25rem;   /* 20px - Headings principales */
```

### Pesos de Fuente

```css
font-normal: 400;   /* Texto regular */
font-medium: 500;   /* Labels, énfasis sutil */
font-semibold: 600; /* Títulos de sección */
font-bold: 700;     /* Headings importantes */
```

### Jerarquía Tipográfica

| Elemento | Clase | Ejemplo |
|----------|-------|---------|
| Label de campo | `text-xs font-medium text-gray-700` | "Color scheme" |
| Texto de input | `text-sm` | Contenido del input |
| Título de sección | `text-sm font-semibold text-gray-900` | "Menu" |
| Texto de ayuda | `text-xs text-gray-500` | "Layout is auto-optimized" |
| Links | `text-xs text-blue-600 hover:text-blue-700` | "Learn about color schemes" |

---

## 🧩 Componentes Base

### 1. Inputs de Texto

```jsx
<input
  type="text"
  className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-md 
             focus:outline-none focus:ring-1 focus:ring-blue-500"
  placeholder="Placeholder text"
/>
```

### 2. Select/Dropdown

```jsx
<select 
  className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-md 
             focus:outline-none focus:ring-1 focus:ring-blue-500"
>
  <option value="1">Option 1</option>
</select>
```

### 3. Toggle Switch

```jsx
<button
  onClick={() => handleToggle()}
  className={`relative inline-flex h-5 w-9 items-center rounded-full 
              transition-colors ${
    isEnabled ? 'bg-blue-500' : 'bg-gray-300'
  }`}
>
  <span className={`inline-block h-3.5 w-3.5 transform rounded-full 
                    bg-white transition-transform ${
    isEnabled ? 'translate-x-5' : 'translate-x-1'
  }`} />
</button>
```

**Especificaciones:**
- Altura: 20px (h-5)
- Ancho: 36px (w-9)
- Círculo interno: 14px (h-3.5 w-3.5)
- Color activo: `bg-blue-500`
- Color inactivo: `bg-gray-300`

### 4. Botones de Radio (Estilo Toggle Group)

```jsx
<div className="flex gap-2">
  <button
    onClick={() => setValue('option1')}
    className={`flex-1 px-3 py-1.5 text-xs rounded-md transition-colors ${
      value === 'option1' 
        ? 'bg-gray-900 text-white' 
        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
    }`}
  >
    Option 1
  </button>
</div>
```

**Especificaciones:**
- Padding: `px-3 py-1.5`
- Texto: `text-xs`
- Activo: `bg-gray-900 text-white`
- Inactivo: `bg-gray-100 text-gray-700`
- Hover: `hover:bg-gray-200`

### 5. Textarea

```jsx
<textarea
  className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md 
             focus:outline-none focus:ring-1 focus:ring-blue-500 font-mono"
  rows={6}
  placeholder="/* Add your custom CSS here */"
/>
```

---

## 🎚️ Componentes Complejos

### 1. Slider con Input Numérico

```jsx
<div className="flex items-center gap-3">
  <Slider
    value={[value]}
    onValueChange={(values) => onChange(values[0])}
    min={50}
    max={250}
    step={5}
    className="flex-1"
  />
  <div className="flex items-center gap-1">
    <input
      type="number"
      className="w-14 px-2 py-1 text-sm border border-gray-300 rounded"
      value={value}
      onChange={(e) => onChange(parseInt(e.target.value))}
    />
    <span className="text-xs text-gray-500">px</span>
  </div>
</div>
```

**Especificaciones:**
- Gap entre slider e input: `gap-3`
- Ancho del input: `w-14` (56px)
- Unidad: `text-xs text-gray-500`

### 2. Image Picker

```jsx
<div className="border-2 border-dashed border-gray-300 rounded-lg p-4 bg-gray-50">
  {imageUrl ? (
    <div className="flex items-center justify-between">
      <img src={imageUrl} alt="Preview" className="h-12 object-contain" />
      <button className="px-3 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300">
        Change
      </button>
    </div>
  ) : (
    <button className="w-full py-2 text-sm text-gray-600 hover:text-gray-800">
      Select image
    </button>
  )}
</div>
```

### 3. Sección Expandible

```jsx
<button
  onClick={() => toggleSection('sectionId')}
  className="flex items-center justify-between w-full text-left"
>
  <h3 className="text-sm font-semibold text-gray-900">Section Title</h3>
  {isExpanded ? (
    <ChevronUp className="w-4 h-4 text-gray-500" />
  ) : (
    <ChevronDown className="w-4 h-4 text-gray-500" />
  )}
</button>
```

---

## 📐 Layout y Espaciado

### Contenedores de Sección

```jsx
<div className="space-y-4">
  {/* Contenido con espaciado vertical consistente */}
</div>
```

### Separadores de Sección

```jsx
<div className="border-t border-gray-200 pt-4">
  <h3 className="text-sm font-semibold text-gray-900 mb-3">Section Title</h3>
  {/* Contenido */}
</div>
```

### Estructura de Campo de Formulario

```jsx
<div>
  <label className="block text-xs font-medium text-gray-700 mb-1.5">
    Field Label
  </label>
  {/* Input/Select/Control */}
  <button className="text-xs text-blue-600 hover:text-blue-700 mt-1">
    Help link
  </button>
</div>
```

**Espaciados estándar:**
- Entre label e input: `mb-1.5` (6px)
- Entre campos: `space-y-3` o `space-y-4`
- Padding de sección: `pt-4` después de border
- Margen de título de sección: `mb-3`

---

## 🎭 Estados Interactivos

### Hover States

| Componente | Estado Normal | Estado Hover |
|------------|--------------|--------------|
| Botón secundario | `bg-gray-100` | `bg-gray-200` |
| Link | `text-blue-600` | `text-blue-700` |
| Texto sutil | `text-gray-600` | `text-gray-800` |
| Botón de acción | `bg-gray-200` | `bg-gray-300` |

### Focus States

Todos los inputs usan:
```css
focus:outline-none focus:ring-1 focus:ring-blue-500
```

### Estados de Loading

```jsx
{loading ? (
  <Loader2 className="w-4 h-4 animate-spin" />
) : (
  <Save className="w-4 h-4" />
)}
```

---

## 🌙 Dark Mode

### Clases Dark Mode

```jsx
// Backgrounds
className="bg-white dark:bg-gray-900"

// Borders
className="border-gray-200 dark:border-gray-700"

// Text
className="text-gray-700 dark:text-gray-300"

// Hover
className="hover:bg-gray-100 dark:hover:bg-gray-800"
```

### Paleta Dark Mode

```css
/* Dark Mode Grays */
--dark-bg: #111827;        /* gray-900 */
--dark-surface: #1f2937;   /* gray-800 */
--dark-border: #374151;    /* gray-700 */
--dark-text: #e5e7eb;      /* gray-200 */
--dark-text-muted: #9ca3af; /* gray-400 */
```

---

## ✨ Animaciones

### Transiciones Estándar

```css
/* Para colores y backgrounds */
transition-colors

/* Para transformaciones (toggles) */
transition-transform

/* Duración por defecto: 150ms */
```

### Animaciones Específicas

```jsx
// Spinner de carga
className="animate-spin"

// Pulse para elementos que necesitan atención
className="animate-pulse"
```

---

## 📱 Responsive Design

### Breakpoints

```css
sm: 640px   /* Tablets pequeñas */
md: 768px   /* Tablets */
lg: 1024px  /* Desktop pequeño */
xl: 1280px  /* Desktop */
2xl: 1536px /* Desktop grande */
```

### Patrones Responsive

```jsx
// Ocultar en móvil
className="hidden sm:block"

// Grid responsive
className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3"

// Padding responsive
className="p-4 sm:p-6 lg:p-8"
```

---

## 🎯 Mejores Prácticas

### 1. Consistencia
- Usar siempre las mismas clases para elementos similares
- Mantener la jerarquía visual clara
- Respetar los espaciados definidos

### 2. Accesibilidad
- Labels claros y descriptivos
- Contraste adecuado (WCAG AA mínimo)
- Estados focus visibles
- Áreas clickeables de tamaño adecuado (mínimo 44x44px en móvil)

### 3. Performance
- Usar clases de Tailwind en lugar de estilos inline
- Evitar animaciones complejas
- Lazy loading para imágenes grandes

### 4. Mantenibilidad
- Componentes reutilizables
- Nombres de clases semánticos
- Documentar casos especiales

---

## 📊 Tabla de Referencia Rápida

| Elemento | Clases |
|----------|--------|
| **Label** | `block text-xs font-medium text-gray-700 mb-1.5` |
| **Input** | `w-full px-2 py-1.5 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500` |
| **Select** | Igual que Input |
| **Toggle activo** | `bg-blue-500` |
| **Toggle inactivo** | `bg-gray-300` |
| **Botón primario** | `bg-blue-500 text-white hover:bg-blue-600` |
| **Botón secundario** | `bg-gray-100 text-gray-700 hover:bg-gray-200` |
| **Botón activo (toggle group)** | `bg-gray-900 text-white` |
| **Link** | `text-blue-600 hover:text-blue-700` |
| **Texto secundario** | `text-xs text-gray-500` |
| **Título de sección** | `text-sm font-semibold text-gray-900` |
| **Separador** | `border-t border-gray-200` |
| **Contenedor de sección** | `space-y-4` |

---

## 🚀 Implementación Futura

### Componentes Pendientes
- [ ] Date pickers
- [ ] Time selectors
- [ ] Color pickers con preview
- [ ] Gradient pickers
- [ ] File upload con drag & drop
- [ ] Tooltips informativos
- [ ] Modales de confirmación
- [ ] Toast notifications mejoradas

### Mejoras de Accesibilidad
- [ ] Soporte completo de teclado
- [ ] Aria labels en todos los componentes
- [ ] Modo de alto contraste
- [ ] Reducción de movimiento

---

**Última actualización:** 2025-01-12
**Versión:** 1.0.0
**Autor:** Claude Code + Usuario