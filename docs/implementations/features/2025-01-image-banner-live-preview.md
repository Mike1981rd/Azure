# Image Banner Live Preview Implementation

## Overview
Implementación del módulo Image Banner siguiendo la arquitectura unificada de componentes compartidos para eliminar duplicación entre editor y live preview.

**Created**: 2025-01-15
**Status**: ✅ Complete
**Category**: features

## Cambios Realizados

### 1. Consolidación de Componentes

#### Antes (Duplicado)
- `/components/editor/modules/ImageBanner/PreviewImageBanner.tsx` - Para editor
- `/components/preview/PreviewImageBanner.tsx` - Para live preview
- Código duplicado con diferencias menores

#### Después (Unificado)
- **Un solo componente**: `/components/preview/PreviewImageBanner.tsx`
- Usado tanto en EditorPreview como en PreviewPage
- Prop `isEditor` diferencia contextos cuando es necesario

### 2. Fix del Error NaN en Opacity

#### Problema
```javascript
// ❌ Causaba NaN cuando overlayOpacity era undefined
style={{ opacity: (config.desktopOverlayOpacity || 0) / 100 }}
```

#### Solución
```javascript
// ✅ Maneja correctamente undefined
style={{ opacity: config.desktopOverlayOpacity !== undefined ? config.desktopOverlayOpacity / 100 : 0 }}
```

### 3. Integración en Live Preview

#### PreviewContent.tsx
```typescript
{section.type === SectionType.IMAGE_BANNER && (
  <PreviewImageBanner 
    config={section.settings || section.config}
    isEditor={false}
  />
)}
```

### 4. Detección de Dispositivo Móvil

El componente ahora maneja correctamente la detección móvil:
- Usa `deviceView` prop cuando está disponible (editor)
- Detecta automáticamente basado en window.innerWidth (live preview)
- Sincronización correcta entre contextos

## Arquitectura Final

```
PreviewImageBanner.tsx (Componente Unificado)
├── Usado en EditorPreview.tsx
│   └── isEditor={true}
│   └── deviceView={deviceView}
└── Usado en PreviewContent.tsx
    └── isEditor={false}
    └── Detección automática móvil
```

## Características Implementadas

### Media Support
- ✅ Imágenes (jpg, png, webp, etc.)
- ✅ Videos (mp4, webm, ogg, mov)
- ✅ Diferentes imágenes para desktop/móvil
- ✅ Control de sonido en videos

### Responsive Design
- ✅ Aspect ratio configurable (desktop/móvil)
- ✅ Diferentes posiciones de contenido por dispositivo
- ✅ Tamaños de texto adaptativos
- ✅ Layouts optimizados para móvil

### Estilos y Temas
- ✅ Integración con color schemes globales
- ✅ Tipografía heredada del tema global
- ✅ Estilos de botones configurables (solid/outline/text)
- ✅ Fondos de contenido personalizables

### Configuraciones
- ✅ Overlay con opacidad ajustable
- ✅ Padding superior/inferior
- ✅ Ancho del contenedor (small/medium/large/page/screen)
- ✅ Alineación del texto (left/center)
- ✅ Espaciado entre elementos

## Testing Checklist

### Editor Context
- [x] Componente renderiza correctamente
- [x] Cambios se reflejan en tiempo real
- [x] Device toggle funciona (desktop/mobile)
- [x] No hay errores de NaN en opacity
- [x] Media upload funciona correctamente

### Live Preview Context
- [x] Componente renderiza idénticamente al editor
- [x] Detección móvil automática funciona
- [x] Videos se reproducen correctamente
- [x] Enlaces de botones funcionan
- [x] Color schemes se aplican correctamente

## API Endpoints Utilizados

### Para Editor
- GET `/api/global-theme-config/company/{companyId}` - Configuración del tema
- POST `/api/structural-components/company/{companyId}/image-banner` - Guardar config

### Para Live Preview
- GET `/api/websitepages/company/{companyId}/slug/{slug}` - Obtener página con secciones
- GET `/api/global-theme-config/company/{companyId}/published` - Tema publicado

## Estructura de Datos

### ImageBannerConfig
```typescript
interface ImageBannerConfig {
  // Media
  desktopImage?: string;
  mobileImage?: string;
  videoSound?: boolean;
  
  // Layout
  width: 'small' | 'medium' | 'large' | 'page' | 'screen';
  desktopRatio: string;
  mobileRatio: string;
  
  // Content
  heading?: string;
  subheading?: string;
  body?: string;
  headingSize: 1 | 2 | 3;
  bodySize: 1 | 2 | 3;
  
  // Positioning
  desktopPosition: string;
  mobilePosition: string;
  desktopAlignment: 'left' | 'center';
  mobileAlignment: 'left' | 'center';
  
  // Styling
  colorScheme: string;
  desktopOverlayOpacity?: number;
  mobileOverlayOpacity?: number;
  desktopBackground: string;
  mobileBackground: string;
  
  // Buttons
  firstButtonLabel?: string;
  firstButtonLink?: string;
  firstButtonStyle: 'solid' | 'outline' | 'text';
  secondButtonLabel?: string;
  secondButtonLink?: string;
  secondButtonStyle: 'solid' | 'outline' | 'text';
  
  // Spacing
  topPadding: number;
  bottomPadding: number;
  desktopSpacing: number;
  addSidePaddings: boolean;
}
```

## Próximos Pasos

1. **Optimizaciones**:
   - Lazy loading de imágenes
   - Preload de videos críticos
   - Optimización de tamaños de imagen

2. **Nuevas Features**:
   - Animaciones de entrada
   - Múltiples imágenes (carousel)
   - Parallax effect opcional

3. **Mejoras UX**:
   - Preview de hover en botones
   - Indicador de carga para videos
   - Fallback para errores de carga

## Notas Importantes

- El componente sigue el principio DRY (Don't Repeat Yourself)
- Todos los hooks están antes de returns condicionales (regla de React)
- La detección móvil es consistente entre contextos
- Los estilos inline tienen prioridad para valores dinámicos

## Troubleshooting

### Si el ImageBanner no aparece en live preview
1. Verificar que la página tenga secciones guardadas
2. Verificar que el tipo de sección sea `SectionType.IMAGE_BANNER`
3. Verificar que la configuración esté en `section.settings` o `section.config`

### Si hay diferencias visuales entre editor y preview
1. Verificar que ambos usen el mismo componente PreviewImageBanner
2. Verificar que el theme config esté cargado en ambos contextos
3. Verificar deviceView vs detección automática móvil

## Search Keywords
image banner, live preview, unified components, media upload, responsive design, overlay opacity, video support