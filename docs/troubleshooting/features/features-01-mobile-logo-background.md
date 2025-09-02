# Problema: Logo con Fondo Extraño en Vista Móvil

## 🔴 Síntoma
Al cambiar a vista móvil en el editor, el logo mostraba un fondo o comportamiento visual extraño.

## 🕐 Cuándo Ocurre
- Al hacer click en el icono de móvil
- Solo en vista móvil, no en desktop
- Afectaba tanto al logo desktop como móvil

## 🔍 Diagnóstico

### Inspección del Código
```typescript
// Código problemático encontrado
className="sm:hidden"  // Línea 597 EditorPreview.tsx
className="hidden sm:block"  // Línea 502 EditorPreview.tsx
```

### Análisis
Las clases Tailwind con media queries estaban conflictuando:
- `sm:hidden` = "ocultar en pantallas pequeñas (640px+)"
- `hidden sm:block` = "ocultar por defecto, mostrar en pantallas pequeñas+"

Estas clases están diseñadas para responsive design real basado en el ancho de ventana del navegador, NO para preview simulado controlado por JavaScript.

## ⚡ Solución Rápida

### Paso 1: Identificar Elementos con Media Queries
```bash
grep -n "sm:hidden\|sm:block\|md:\|lg:\|xl:" EditorPreview.tsx
```

### Paso 2: Reemplazar con Control JavaScript
```typescript
// ❌ INCORRECTO - Media queries CSS
<div className="sm:hidden">

// ✅ CORRECTO - Control JavaScript
<div style={{ display: deviceView === 'mobile' ? 'block' : 'none' }}>
```

## 🛠️ Solución Completa

### Cambios en EditorPreview.tsx

#### Logo Desktop (Líneas 488-513)
```typescript
// Antes
<div className={`text-xl font-bold self-center ${deviceView === 'mobile' ? 'hidden sm:block' : ''}`}>

// Después  
<div 
  className="text-xl font-bold self-center"
  style={{ 
    color: schemeToUse?.text || '#000000',
    display: deviceView === 'mobile' ? 'none' : 'block'
  }}
>
```

#### Logo Móvil (Líneas 591-619)
```typescript
// Antes - con clases conflictivas
<img className="sm:hidden" ... />

// Después - sin clases, solo estilos inline
<img style={{ 
  height: headerConfig.logo.mobileHeight || 30,
  objectFit: 'contain'
}} />
```

## 🎯 Causa Raíz
**Mezcla de paradigmas de control**:
1. Media queries CSS (responsive real)
2. Control JavaScript (preview simulado)

Ambos intentaban controlar la visibilidad causando conflictos.

## ✅ Verificación
1. Cambiar entre desktop/móvil debe ser instantáneo
2. No debe haber parpadeos o fondos extraños
3. El logo debe mantener su transparencia
4. Los estilos deben aplicarse correctamente

## 🚫 Qué NO Hacer
- NO usar clases Tailwind responsive (`sm:`, `md:`, `lg:`, etc.) en el preview
- NO mezclar media queries CSS con control JavaScript
- NO usar `@media` queries en componentes de preview

## 💡 Mejores Prácticas

### Para Preview Simulado
```typescript
// Siempre usar JavaScript/React para control
const styles = {
  display: deviceView === 'mobile' ? 'none' : 'block',
  width: deviceView === 'mobile' ? '100px' : '200px'
};
```

### Para Responsive Real
```typescript
// Usar Tailwind classes para responsive real
<div className="hidden sm:block md:flex lg:grid">
```

## 📚 Aprendizajes
1. **Separar concerns**: Preview simulado ≠ Responsive real
2. **Un solo sistema de control**: O CSS o JavaScript, no ambos
3. **Estilos inline** para control dinámico basado en estado React

## 🔗 Relacionado
- Implementación: `/docs/implementations/features/2025-01-responsive-preview-toggle.md`
- Arquitectura: `/docs/WEBSITE-BUILDER-ARCHITECTURE.md`

## 🏷️ Tags
mobile, responsive, logo, background, css, tailwind, media-queries, preview

---

**Fecha**: 14 enero 2025
**Severidad**: Media
**Tiempo de resolución**: 15 minutos