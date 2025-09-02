# 🚀 BLUEPRINT COMPLETO - SISTEMA WEBSITE BUILDER NEXT GENERATION

## 📋 RESUMEN EJECUTIVO
- **Proyecto**: Sistema de empresa única + e-commerce + website builder
- **Tecnologías**: ASP.NET Core 8 API + Next.js 14 + PostgreSQL
- **Objetivo**: Resolver los 9 problemas críticos identificados y crear arquitectura limpia y escalable
- **Inicio**: 11 de agosto 2025, 7:00 PM RD

## 🎯 PROBLEMAS A RESOLVER

### ❌ PROBLEMAS ACTUALES IDENTIFICADOS:
1. **JSON GIGANTE** (24,000 líneas de código)
2. **ARQUITECTURA DE PÁGINAS RÍGIDA** (solo página de inicio, luego módulos incorrectos)
3. **SECCIONES DE UNA SOLA INSTANCIA** (no se pueden agregar múltiples del mismo tipo)
4. **DRAG & DROP SIN VALIDACIONES** (elementos hijos en hijos, padres en hijos incorrectamente)
5. **PERFORMANCE LENTA EN DOMINIOS CUSTOM** (lee del preview en lugar de página optimizada)
6. **HABITACIONES MEZCLADAS CON PRODUCTOS** (carritos mezclados, problemas de abandono)
7. **SIN SISTEMA DE VARIANTES** (productos sin opciones de personalización)
8. **FALTA SISTEMA DE PÁGINAS STANDARD** (inicio, producto, carrito, pago, página en blanco)
9. **SIN SISTEMA UNDO/REDO** (usuarios pierden cambios al experimentar)

### ✅ SOLUCIONES ARQUITECTÓNICAS:
1. Base de datos relacional modular (entidades separadas)
2. Sistema de páginas dinámicas (templates reutilizables)
3. Componentes reutilizables multi-instancia (múltiples secciones del mismo tipo)
4. Drag & drop con validaciones jerárquicas (reglas de anidamiento)
5. Cache optimizado para producción (contenido separado de preview)
6. Separación conceptual completa (carritos independientes)
7. Sistema completo de variantes (productos con opciones)
8. Templates de páginas estándar (5 tipos predefinidos)
9. Sistema undo/redo completo (historial de 50 estados + atajos)

## 🏗️ ARQUITECTURA TÉCNICA

### BACKEND - ASP.NET Core 8 API
```
WebsiteBuilderAPI/
├── Controllers/
│   ├── CompanyController.cs
│   ├── RoomsController.cs  
│   ├── ProductsController.cs
│   ├── WebsiteController.cs
│   ├── PagesController.cs
│   ├── SectionsController.cs
│   ├── ThemeController.cs
│   └── DomainController.cs
├── Services/
│   ├── ICompanyService.cs
│   ├── IWebsiteBuilderService.cs
│   ├── ICacheService.cs
│   ├── IDomainService.cs
│   └── IHistoryService.cs
├── Repositories/
│   ├── ICompanyRepository.cs
│   ├── IWebsiteRepository.cs
│   ├── IProductRepository.cs
│   └── IRoomRepository.cs
└── Models/
    ├── Company.cs
    ├── Room.cs
    ├── Product.cs
    ├── ProductVariant.cs
    ├── WebsitePage.cs
    ├── PageSection.cs
    ├── SectionConfig.cs
    ├── ThemeSettings.cs
    └── NavigationMenu.cs
```

### FRONTEND - Next.js 14
```
websitebuilder-admin/
├── src/
│   ├── app/
│   │   ├── dashboard/
│   │   ├── empresa/
│   │   ├── roles-usuarios/
│   │   ├── clientes/
│   │   ├── reservaciones/
│   │   ├── metodos-pago/
│   │   ├── colecciones/
│   │   ├── productos/
│   │   ├── paginas/
│   │   ├── politicas/
│   │   ├── website-builder/
│   │   └── dominios/
│   ├── components/
│   │   ├── ui/           # Componentes base (Button, Card, Input)
│   │   ├── forms/        # Formularios reutilizables
│   │   ├── builder/      # Website builder components
│   │   ├── sections/     # 11 tipos de secciones
│   │   └── layout/       # Sidebar, Header, etc.
│   ├── lib/
│   │   ├── api.ts
│   │   ├── utils.ts
│   │   ├── constants.ts
│   │   ├── dragDrop.ts
│   │   └── editorHistory.ts
│   └── hooks/
│       ├── useEditorHistory.ts
│       ├── useTheme.ts
│       └── useAuth.ts
```

## 🗄️ DISEÑO DE BASE DE DATOS

### ENTIDADES PRINCIPALES

```sql
-- EMPRESA Y CONFIGURACIÓN (SINGLE-TENANT)
CREATE TABLE Companies (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Domain VARCHAR(255) UNIQUE,
    CustomDomain VARCHAR(255),
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- HABITACIONES (SEPARADAS DE PRODUCTOS)
CREATE TABLE Rooms (
    Id SERIAL PRIMARY KEY,
    CompanyId INT REFERENCES Companies(Id),
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    BasePrice DECIMAL(10,2),
    MaxOccupancy INT,
    Images JSONB,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- PRODUCTOS E-COMMERCE (SEPARADOS DE HABITACIONES)
CREATE TABLE Products (
    Id SERIAL PRIMARY KEY,
    CompanyId INT REFERENCES Companies(Id),
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    BasePrice DECIMAL(10,2),
    HasVariants BOOLEAN DEFAULT false,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- VARIANTES DE PRODUCTOS (NUEVO - SOLUCIONA PROBLEMA #7)
CREATE TABLE ProductVariants (
    Id SERIAL PRIMARY KEY,
    ProductId INT REFERENCES Products(Id),
    Name VARCHAR(255) NOT NULL,
    Price DECIMAL(10,2),
    Stock INT,
    Attributes JSONB, -- {color: "red", size: "L", material: "cotton"}
    IsActive BOOLEAN DEFAULT true
);

-- PÁGINAS DEL WEBSITE (MODULAR - SOLUCIONA PROBLEMA #2)
CREATE TABLE WebsitePages (
    Id SERIAL PRIMARY KEY,
    CompanyId INT REFERENCES Companies(Id),
    PageType VARCHAR(50) NOT NULL, -- home, product, cart, checkout, custom
    Name VARCHAR(255) NOT NULL,
    Slug VARCHAR(255),
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- SECCIONES DE PÁGINAS (REEMPLAZA JSON GIGANTE - SOLUCIONA PROBLEMA #1)
CREATE TABLE PageSections (
    Id SERIAL PRIMARY KEY,
    PageId INT REFERENCES WebsitePages(Id),
    SectionType VARCHAR(50) NOT NULL, -- image_with_text, gallery, etc.
    Config JSONB NOT NULL, -- Configuración específica de la sección
    SortOrder INT,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- CONFIGURACIONES DE TEMA (SEPARADAS DEL JSON)
CREATE TABLE ThemeSettings (
    Id SERIAL PRIMARY KEY,
    CompanyId INT REFERENCES Companies(Id),
    ColorScheme JSONB, -- {primary: "#123", secondary: "#456"}
    Typography JSONB,  -- {fontFamily: "Inter", sizes: {...}}
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- MENÚS DE NAVEGACIÓN
CREATE TABLE NavigationMenus (
    Id SERIAL PRIMARY KEY,
    CompanyId INT REFERENCES Companies(Id),
    MenuType VARCHAR(50), -- header, footer
    Items JSONB, -- [{name: "Home", url: "/", order: 1}]
    IsActive BOOLEAN DEFAULT true
);

-- HISTORIAL DEL EDITOR (NUEVO - SOLUCIONA PROBLEMA #9)
CREATE TABLE EditorHistory (
    Id SERIAL PRIMARY KEY,
    PageId INT REFERENCES WebsitePages(Id),
    UserId INT,
    StateData JSONB NOT NULL, -- Estado completo de la página
    Description VARCHAR(255), -- "Added image section", "Changed colors"
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- CARRITOS SEPARADOS (SOLUCIONA PROBLEMA #6)
CREATE TABLE RoomReservationCarts (
    Id SERIAL PRIMARY KEY,
    SessionId VARCHAR(255),
    CompanyId INT REFERENCES Companies(Id),
    Items JSONB, -- [{roomId: 1, checkIn: "...", checkOut: "..."}]
    CreatedAt TIMESTAMP DEFAULT NOW()
);

CREATE TABLE ProductShoppingCarts (
    Id SERIAL PRIMARY KEY,
    SessionId VARCHAR(255),
    CompanyId INT REFERENCES Companies(Id),
    Items JSONB, -- [{productId: 1, variantId: 2, quantity: 1}]
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

## 🧩 COMPONENTES DEL WEBSITE BUILDER

### 11 TIPOS DE SECCIONES DISPONIBLES

```typescript
// types/sections.ts
export interface SectionConfig {
  id: string;
  type: SectionType;
  config: any;
  sortOrder: number;
}

export enum SectionType {
  ImageWithText = 'image_with_text',
  ImageBanner = 'image_banner', 
  RichText = 'rich_text',
  Gallery = 'gallery',
  ContactForm = 'contact_form',
  Newsletter = 'newsletter',
  FeaturedProduct = 'featured_product',
  FeaturedCollection = 'featured_collection',
  Testimonials = 'testimonials',
  FAQ = 'faq',
  Videos = 'videos'
}
```

### COMPONENTES REACT MODULARES (SOLUCIONA PROBLEMA #3)

```typescript
// components/sections/ImageWithText.tsx
interface ImageWithTextConfig {
  image: string;
  title: string;
  description: string;
  buttonText?: string;
  buttonUrl?: string;
  layout: 'left' | 'right';
}

export function ImageWithText({ 
  config, 
  isMultiple = false, 
  index = 0 
}: { 
  config: ImageWithTextConfig;
  isMultiple?: boolean;
  index?: number;
}) {
  return (
    <section className="py-16" data-section-type="image_with_text" data-index={index}>
      <div className="container mx-auto">
        {/* Implementación que permite múltiples instancias */}
      </div>
    </section>
  );
}
```

### TIPOS DE PÁGINAS ESTÁNDAR (SOLUCIONA PROBLEMA #8)

```typescript
// types/pages.ts
export enum PageType {
  Home = 'home',           // Página de inicio
  Product = 'product',     // Página de producto individual
  Cart = 'cart',          // Página de carrito de compras
  Checkout = 'checkout',   // Página de pago
  Custom = 'custom'        // Página en blanco para personalizar
}

export interface PageTemplate {
  type: PageType;
  name: string;
  defaultSections: SectionType[];
  allowedSections: SectionType[];
}
```

## 🎨 SISTEMA DE DRAG & DROP MEJORADO (SOLUCIONA PROBLEMA #4)

### VALIDACIONES JERÁRQUICAS

```typescript
// lib/dragDrop.ts
export interface DragDropRules {
  canDropOn: (source: SectionType, target: SectionType) => boolean;
  canNest: (parent: SectionType, child: SectionType) => boolean;
  maxNestingLevel: number;
}

export const dragDropRules: DragDropRules = {
  canDropOn: (source, target) => {
    // Validaciones específicas
    const containers = [SectionType.ImageWithText];
    const standalone = [SectionType.ImageBanner, SectionType.Gallery];
    
    // Un elemento standalone no puede ir dentro de otro standalone
    if (standalone.includes(source) && standalone.includes(target)) {
      return false;
    }
    
    return true;
  },
  
  canNest: (parent, child) => {
    // Definir qué puede contener qué
    const containerSections = [SectionType.ImageWithText];
    const contentSections = [SectionType.RichText, SectionType.ContactForm];
    
    return containerSections.includes(parent) && contentSections.includes(child);
  },
  
  maxNestingLevel: 3
};

export function validateDrop(source: SectionType, target: SectionType, targetLevel: number): boolean {
  if (targetLevel >= dragDropRules.maxNestingLevel) {
    return false;
  }
  
  return dragDropRules.canDropOn(source, target);
}
```

<!-- Continúa en blueprint2.md -->