# 🏗️ BLUEPRINT - WEBSITE BUILDER NEXT GENERATION

## 📅 Información del Proyecto
- **Fecha de Inicio**: 11 de enero 2025
- **Versión**: 2.0 (Reescritura completa para evitar errores de v1)
- **Stack**: ASP.NET Core 8 + Next.js 14 + PostgreSQL + TypeScript
- **Documento Base**: prompt.pdf con especificaciones de configuraciones globales

## 🎯 OBJETIVOS PRINCIPALES

### Resolver los 9 Problemas Críticos de V1:
1. ✅ **JSON Gigante (24,000 líneas)** → Sistema modular con archivos TypeScript <300 líneas
2. ⏳ **Páginas Rígidas** → Sistema dinámico con templates flexibles
3. ⏳ **Secciones Únicas** → Múltiples instancias de cada tipo de sección
4. ⏳ **Drag & Drop sin Validaciones** → Sistema con reglas jerárquicas
5. ⏳ **Performance Lenta** → Cache diferenciado preview/producción
6. ✅ **Conceptos Mezclados** → Separación completa habitaciones/productos
7. ✅ **Sin Variantes** → Sistema completo de variantes implementado
8. ⏳ **Páginas Faltantes** → 5 tipos de páginas estándar
9. ⏳ **Sin Undo/Redo** → Sistema de 50 estados en memoria

## 🏛️ ARQUITECTURA DEL SISTEMA

### 1. CONFIGURACIONES GLOBALES (Theme System)

#### 1.1 Estructura de Tipos TypeScript
```
websitebuilder-admin/src/types/theme/
├── appearance.ts      # Ancho página, padding, border radius
├── typography.ts      # 5 tipos: headings, body, menu, productCardName, buttons
├── colorSchemes.ts    # Hasta 5 esquemas de color
├── productCards.ts    # Configuración de tarjetas de producto
├── productBadges.ts   # Insignias (agotado, oferta, custom)
├── cart.ts           # Display del carrito y envío gratis
├── favicon.ts        # Favicon personalizable
├── navigation.ts     # Búsqueda y botón back-to-top
├── socialMedia.ts    # 17 plataformas sociales
├── swatches.ts       # Muestras visuales de variantes
└── index.ts          # Exports centralizados y GlobalThemeConfig
```

#### 1.2 Principios de Diseño
- **Modular**: Un archivo por aspecto de configuración
- **Type-Safe**: Interfaces TypeScript estrictas
- **Extensible**: Fácil agregar nuevas configuraciones
- **Defaults**: Cada módulo tiene valores por defecto
- **Herencia**: Configuración global → Sección → Override específico

### 2. MODELO DE DATOS

#### 2.1 Backend (ASP.NET Core)
```csharp
// Models/ThemeConfiguration.cs
public class ThemeConfiguration
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string ConfigType { get; set; } // "appearance", "typography", etc.
    public JsonDocument ConfigData { get; set; } // JSONB
    public DateTime UpdatedAt { get; set; }
}

// Models/WebsitePage.cs
public class WebsitePage
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string PageType { get; set; } // home, product, cart, checkout, custom
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public List<PageSection> Sections { get; set; }
}

// Models/PageSection.cs
public class PageSection
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public string SectionType { get; set; } // ImageWithText, Gallery, etc.
    public JsonDocument Config { get; set; } // Configuración específica
    public JsonDocument ThemeOverrides { get; set; } // Overrides de tema
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
```

#### 2.2 Base de Datos (PostgreSQL)
```sql
-- Configuraciones globales del tema
CREATE TABLE theme_configurations (
    id SERIAL PRIMARY KEY,
    company_id INT REFERENCES companies(id),
    config_type VARCHAR(50) NOT NULL,
    config_data JSONB NOT NULL,
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(company_id, config_type)
);

-- Páginas del website
CREATE TABLE website_pages (
    id SERIAL PRIMARY KEY,
    company_id INT REFERENCES companies(id),
    page_type VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Secciones de cada página
CREATE TABLE page_sections (
    id SERIAL PRIMARY KEY,
    page_id INT REFERENCES website_pages(id) ON DELETE CASCADE,
    section_type VARCHAR(50) NOT NULL,
    config JSONB NOT NULL,
    theme_overrides JSONB,
    sort_order INT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Índices para performance
CREATE INDEX idx_theme_config_company ON theme_configurations(company_id);
CREATE INDEX idx_pages_company ON website_pages(company_id);
CREATE INDEX idx_pages_slug ON website_pages(slug);
CREATE INDEX idx_sections_page ON page_sections(page_id);
CREATE INDEX idx_sections_sort ON page_sections(sort_order);
```

### 3. SISTEMA DE SECCIONES

#### 3.1 Los 11 Tipos de Secciones
1. **ImageWithText** - Imagen con texto y botón
2. **ImageBanner** - Banner hero con texto superpuesto
3. **RichText** - Editor de texto enriquecido
4. **Gallery** - Galería de imágenes/productos
5. **ContactForm** - Formulario de contacto
6. **Newsletter** - Suscripción a boletín
7. **FeaturedProduct** - Producto destacado
8. **FeaturedCollection** - Colección destacada
9. **Testimonials** - Testimonios de clientes
10. **FAQ** - Preguntas frecuentes
11. **Videos** - Sección de videos

#### 3.2 Estructura de una Sección
```typescript
// types/sections/base.ts
export interface BaseSection {
  id: string;
  type: SectionType;
  config: any; // Configuración específica del tipo
  themeOverrides?: {
    appearance?: Partial<AppearanceConfig>;
    typography?: Partial<TypographyConfig>;
    colorScheme?: string; // ID del esquema a usar
    // ... otros overrides
  };
  sortOrder: number;
  isActive: boolean;
}

// types/sections/imageWithText.ts
export interface ImageWithTextConfig {
  image: {
    url: string;
    alt: string;
    position: 'left' | 'right';
  };
  content: {
    heading: string;
    subheading?: string;
    body: string;
    button?: {
      text: string;
      url: string;
      style: 'solid' | 'outline';
    };
  };
  layout: {
    imageWidth: '33%' | '50%' | '66%';
    verticalAlignment: 'top' | 'center' | 'bottom';
    spacing: 'tight' | 'normal' | 'loose';
  };
}
```

### 4. SISTEMA DE HERENCIA DE CONFIGURACIONES

```
┌─────────────────────────────────────┐
│     CONFIGURACIONES GLOBALES        │
│  (appearance, typography, colors)    │
└────────────┬────────────────────────┘
             │ Hereda
             ▼
┌─────────────────────────────────────┐
│         SECCIÓN BASE                │
│   (Usa configuración global)         │
└────────────┬────────────────────────┘
             │ Override (opcional)
             ▼
┌─────────────────────────────────────┐
│      INSTANCIA DE SECCIÓN           │
│  (Config específica + overrides)     │
└─────────────────────────────────────┘
```

#### 4.1 Flujo de Aplicación
1. **Carga configuración global** de la empresa
2. **Sección solicita** configuración (ej: colorScheme)
3. **Verifica overrides** en la instancia
4. **Aplica herencia**:
   - Si hay override → usar override
   - Si no → usar configuración global
   - Si no existe global → usar default del sistema

### 5. EDITOR VISUAL

#### 5.1 Estado del Editor (Frontend)
```typescript
// store/websiteBuilder.ts (usando Zustand)
interface WebsiteBuilderState {
  // Configuraciones globales
  globalTheme: GlobalThemeConfig;
  
  // Página actual
  currentPage: {
    id: number;
    type: PageType;
    sections: BaseSection[];
  };
  
  // Estado del editor
  editor: {
    selectedSectionId: string | null;
    isDragging: boolean;
    isPreviewMode: boolean;
    isSaving: boolean;
  };
  
  // Sistema undo/redo
  history: {
    states: EditorState[];
    currentIndex: number;
    maxStates: 50;
  };
  
  // Acciones
  actions: {
    // Configuración global
    updateGlobalTheme: (config: Partial<GlobalThemeConfig>) => void;
    
    // Secciones
    addSection: (type: SectionType, index?: number) => void;
    updateSection: (id: string, config: any) => void;
    deleteSection: (id: string) => void;
    reorderSections: (fromIndex: number, toIndex: number) => void;
    
    // Overrides
    applyThemeOverride: (sectionId: string, overrides: any) => void;
    removeThemeOverride: (sectionId: string) => void;
    
    // Undo/Redo
    undo: () => void;
    redo: () => void;
    saveState: (description: string) => void;
    
    // Persistencia
    savePage: () => Promise<void>;
    loadPage: (pageId: number) => Promise<void>;
    publish: () => Promise<void>;
  };
}
```

#### 5.2 Componentes del Editor
```
components/websiteBuilder/
├── editor/
│   ├── EditorCanvas.tsx        # Canvas principal con drag & drop
│   ├── SectionLibrary.tsx      # Panel de secciones disponibles
│   ├── ConfigPanel.tsx         # Panel de configuración
│   └── PreviewToggle.tsx       # Toggle editor/preview
├── sections/
│   ├── ImageWithText/
│   │   ├── index.tsx           # Componente de renderizado
│   │   ├── Config.tsx          # Panel de configuración
│   │   └── Preview.tsx         # Vista previa
│   └── [... otras 10 secciones]
├── theme/
│   ├── AppearanceEditor.tsx    # Editor de appearance
│   ├── TypographyEditor.tsx    # Editor de tipografía
│   ├── ColorSchemeEditor.tsx   # Editor de esquemas de color
│   └── [... otros editores]
└── preview/
    ├── LivePreview.tsx         # Preview en iframe
    └── DeviceToggle.tsx        # Desktop/Tablet/Mobile
```

### 6. FLUJO DE DATOS

#### 6.1 Carga Inicial
```
1. Usuario abre editor
2. GET /api/theme-configurations/{companyId}
3. GET /api/website-pages/{pageId}
4. Cargar estado en Zustand
5. Renderizar editor con configuraciones
```

#### 6.2 Edición de Configuración Global
```
1. Usuario cambia color en ColorSchemeEditor
2. updateGlobalTheme() en Zustand
3. Re-render de todas las secciones afectadas
4. Auto-save después de 2 segundos de inactividad
5. POST /api/theme-configurations
```

#### 6.3 Edición de Sección
```
1. Usuario selecciona sección
2. Abre ConfigPanel con opciones
3. Usuario puede:
   a. Cambiar configuración específica (texto, imágenes)
   b. Aplicar override de tema
   c. Reordenar o eliminar
4. updateSection() en Zustand
5. saveState() para undo/redo
6. Auto-save después de 2 segundos
```

#### 6.4 Preview y Publicación
```
1. Usuario click en "Preview"
2. Generar HTML/CSS con configuraciones aplicadas
3. Mostrar en iframe o nueva pestaña
4. Si usuario click "Publish":
   a. Validar todas las secciones
   b. Generar versión optimizada
   c. Guardar en cache de producción
   d. Invalidar cache anterior
```

### 7. SISTEMA DE CACHE

#### 7.1 Estrategia de Cache
```csharp
public class WebsiteBuilderCacheService
{
    // Cache de configuraciones (cambian poco)
    private readonly IMemoryCache _themeCache;
    
    // Cache de preview (corta duración)
    private readonly IMemoryCache _previewCache;
    
    // Cache de producción (larga duración)
    private readonly IDistributedCache _productionCache;
    
    public async Task<string> GetWebsiteContent(int companyId, bool isPreview)
    {
        var cacheKey = isPreview 
            ? $"preview:{companyId}:{pageId}"
            : $"production:{companyId}:{pageId}";
            
        if (isPreview)
        {
            // Cache 5 minutos para preview
            return await _previewCache.GetOrCreateAsync(cacheKey, 
                async entry => {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                    return await GenerateContent(companyId, pageId);
                });
        }
        else
        {
            // Cache 24 horas para producción
            return await _productionCache.GetOrCreateAsync(cacheKey,
                async entry => {
                    entry.SlidingExpiration = TimeSpan.FromHours(24);
                    return await GenerateOptimizedContent(companyId, pageId);
                });
        }
    }
}
```

### 8. APIs NECESARIAS

#### 8.1 Theme Configuration API
```
GET    /api/theme-configurations/{companyId}
GET    /api/theme-configurations/{companyId}/{configType}
POST   /api/theme-configurations/{companyId}/{configType}
PUT    /api/theme-configurations/{companyId}/{configType}
DELETE /api/theme-configurations/{companyId}/{configType}
POST   /api/theme-configurations/{companyId}/reset
```

#### 8.2 Website Pages API
```
GET    /api/website-pages/{companyId}
GET    /api/website-pages/{pageId}
POST   /api/website-pages
PUT    /api/website-pages/{pageId}
DELETE /api/website-pages/{pageId}
POST   /api/website-pages/{pageId}/duplicate
POST   /api/website-pages/{pageId}/publish
```

#### 8.3 Page Sections API
```
GET    /api/page-sections/{pageId}
POST   /api/page-sections/{pageId}
PUT    /api/page-sections/{sectionId}
DELETE /api/page-sections/{sectionId}
POST   /api/page-sections/{pageId}/reorder
POST   /api/page-sections/{sectionId}/duplicate
```

#### 8.4 Preview API
```
GET    /api/preview/{pageId}
GET    /api/preview/{pageId}/html
GET    /api/preview/{pageId}/css
POST   /api/preview/refresh/{pageId}
```

### 9. CONSIDERACIONES TÉCNICAS

#### 9.1 Performance
- Lazy loading de secciones no visibles
- Debounce en auto-save (2 segundos)
- Virtual scrolling para páginas largas
- Optimización de imágenes on-the-fly
- Minificación de CSS/JS en producción

#### 9.2 Seguridad
- Validación de configuraciones en backend
- Sanitización de HTML en RichText
- CSP headers para preview iframe
- Rate limiting en APIs
- Validación de permisos por empresa

#### 9.3 Escalabilidad
- CDN para assets estáticos
- Cache distribuido con Redis
- Queue para generación de previews
- Webhooks para invalidación de cache
- Horizontal scaling del editor

#### 9.4 UX/UI
- Drag & drop con feedback visual
- Undo/redo con Ctrl+Z/Ctrl+Y
- Auto-save indicator
- Responsive preview
- Keyboard shortcuts
- Tour guiado para nuevos usuarios

### 10. DEPENDENCIAS EXTERNAS

#### 10.1 Frontend
```json
{
  "dependencies": {
    "react-dnd": "^16.0.0",           // Drag & drop
    "react-dnd-html5-backend": "^16.0.0",
    "@dnd-kit/sortable": "^7.0.0",    // Alternative D&D
    "zustand": "^4.4.0",               // Estado global
    "immer": "^10.0.0",                // Immutable updates
    "react-frame-component": "^5.0.0", // Preview iframe
    "react-color": "^2.19.0",          // Color pickers
    "react-hot-keys": "^2.0.0",       // Keyboard shortcuts
    "@tiptap/react": "^2.0.0",        // Rich text editor
    "react-hook-form": "^7.0.0",      // Forms
    "zod": "^3.0.0"                   // Validación
  }
}
```

#### 10.2 Backend
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.0" />
<PackageReference Include="Polly" Version="8.0.0" />
<PackageReference Include="FluentValidation" Version="11.0.0" />
<PackageReference Include="AutoMapper" Version="12.0.0" />
```

### 11. ESTRUCTURA DE CARPETAS COMPLETA

```
WebsiteBuilderAPI/
├── Controllers/
│   ├── ThemeConfigurationsController.cs
│   ├── WebsitePagesController.cs
│   ├── PageSectionsController.cs
│   └── PreviewController.cs
├── Services/
│   ├── IThemeConfigurationService.cs
│   ├── ThemeConfigurationService.cs
│   ├── IWebsiteBuilderService.cs
│   ├── WebsiteBuilderService.cs
│   ├── IWebsiteBuilderCacheService.cs
│   └── WebsiteBuilderCacheService.cs
├── Models/
│   ├── ThemeConfiguration.cs
│   ├── WebsitePage.cs
│   └── PageSection.cs
├── DTOs/
│   ├── ThemeConfigurationDto.cs
│   ├── WebsitePageDto.cs
│   └── PageSectionDto.cs
└── Validators/
    ├── ThemeConfigurationValidator.cs
    └── PageSectionValidator.cs

websitebuilder-admin/
├── src/
│   ├── types/
│   │   ├── theme/           # 11 archivos de configuración
│   │   └── sections/        # Tipos para cada sección
│   ├── components/
│   │   └── websiteBuilder/  # Todos los componentes del editor
│   ├── store/
│   │   └── websiteBuilder.ts # Estado global con Zustand
│   ├── hooks/
│   │   ├── useThemeConfig.ts
│   │   ├── useWebsiteBuilder.ts
│   │   └── useUndoRedo.ts
│   └── lib/
│       ├── websiteBuilder/
│       │   ├── api.ts
│       │   ├── utils.ts
│       │   └── validators.ts
│       └── theme/
│           └── themeEngine.ts # Motor de aplicación de temas
```

### 12. ROADMAP DE IMPLEMENTACIÓN

#### FASE 1: Configuraciones Globales ✅ COMPLETADA (35%)
1. ✅ Tipos TypeScript para configuraciones globales (11 archivos)
2. ✅ Modelos y migraciones en backend (GlobalThemeConfig)
3. ✅ APIs básicas CRUD (30+ endpoints)
4. ✅ Store con Zustand y hooks personalizados
5. ✅ Editores UI parciales (Appearance, Typography, ColorScheme)

#### FASE 2: Componentes Estructurales (En Progreso)
**Objetivo**: Implementar 4 componentes globales que aparecen en todas las páginas
1. **Header Component**
   - Múltiples layouts (drawer, logo-left, logo-center, etc.)
   - Logo desktop/mobile configurable
   - Integración con menú de navegación
   - Sticky header opcional

2. **Announcement Bar**
   - Multi-instancia de anuncios
   - Animaciones (slide, fade, marquee, typewriter)
   - Autoplay configurable
   - Selectores de idioma/moneda

3. **Footer Component**
   - Blocks modulares (logo, subscribe, social, menu, text)
   - Bottom bar con menú y copyright
   - Grid responsive (3-4 columnas)
   - Iconos de pago

4. **Cart Drawer**
   - Display como drawer o página
   - Progress bar de envío gratis
   - Configuración de botones
   - Notas de pedido opcionales

5. **Sistema Undo/Redo**
   - 50 estados en memoria
   - Atajos de teclado (Ctrl+Z/Y)
   - Persistencia en EditorHistory

#### FASE 3: Páginas y Secciones (Pendiente)
**Objetivo**: Sistema completo de páginas con secciones drag & drop
1. **8 Tipos de Páginas**:
   - HOME (única)
   - PRODUCT (producto individual)
   - CART (carrito)
   - CHECKOUT
   - CUSTOM (personalizadas)
   - ALL_COLLECTIONS
   - COLLECTION (específica)
   - ALL_PRODUCTS (catálogo)

2. **11 Tipos de Secciones**:
   - ImageWithText, ImageBanner, RichText
   - Gallery, ContactForm, Newsletter
   - FeaturedProduct, FeaturedCollection
   - Testimonials, FAQ, Videos

3. **Sistema Drag & Drop**:
   - Validaciones jerárquicas
   - Límites por tipo de sección
   - Orden requerido (first/last)
   - Feedback visual

#### Fase 4: Preview y Publicación (Semana 4)
1. Preview en vivo
2. Toggle desktop/mobile
3. Sistema de publicación
4. Cache de producción

#### Fase 5: Secciones Avanzadas (Semana 5)
1. Implementar las 8 secciones restantes
2. Validaciones y reglas de negocio
3. Optimizaciones de performance

#### Fase 6: Polish y Testing (Semana 6)
1. Sistema undo/redo completo
2. Auto-save refinado
3. Testing E2E
4. Documentación

## 📋 CRITERIOS DE ÉXITO

1. ✅ Archivos TypeScript < 300 líneas
2. ✅ Sin JSON gigante (modular)
3. ✅ Preview en < 2 segundos
4. ✅ Auto-save funcional
5. ✅ Undo/redo de 50 estados
6. ✅ 11 tipos de secciones funcionales
7. ✅ Override parcial de temas
8. ✅ Drag & drop con validaciones
9. ✅ Cache diferenciado preview/producción
10. ✅ Responsive en editor y preview

## 🚨 PUNTOS CRÍTICOS A EVITAR

1. **NO** crear un JSON monolítico
2. **NO** mezclar configuraciones de diferentes aspectos
3. **NO** permitir drag & drop sin validaciones
4. **NO** usar el mismo cache para preview y producción
5. **NO** limitar secciones a una sola instancia
6. **NO** olvidar el sistema de undo/redo
7. **NO** hardcodear valores de tema
8. **NO** olvidar validaciones de seguridad

## 📚 REFERENCIAS

- Documento base: `Test Images/prompt.pdf`
- Blueprint original: `blueprint1.md`, `blueprint2.md`, `blueprint3.md`
- Problemas a resolver: 9 issues críticos documentados
- Inspiración: Sistema de temas de Shopify
- Stack: ASP.NET Core 8 + Next.js 14 + PostgreSQL

---

**Última actualización**: 11 de enero 2025
**Versión del documento**: 1.0
**Autor**: Sistema de documentación automática
**Estado**: Listo para implementación