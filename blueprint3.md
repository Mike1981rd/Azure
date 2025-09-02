# BLUEPRINT - PARTE 3

## 🛒 SEPARACIÓN DE CARRITOS (SOLUCIONA PROBLEMA #6)

### SISTEMA DUAL DE CARRITOS

```typescript
// types/cart.ts
export interface RoomReservationCart {
  id: string;
  sessionId: string;
  type: 'reservation';
  items: RoomCartItem[];
  checkIn: Date;
  checkOut: Date;
  totalAmount: number;
}

export interface ProductShoppingCart {
  id: string;
  sessionId: string;
  type: 'ecommerce';
  items: ProductCartItem[];
  shippingAddress?: Address;
  totalAmount: number;
}

export interface RoomCartItem {
  roomId: number;
  roomName: string;
  checkIn: Date;
  checkOut: Date;
  nights: number;
  pricePerNight: number;
  totalPrice: number;
}

export interface ProductCartItem {
  productId: number;
  variantId?: number;
  productName: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}
```

### SERVICIOS SEPARADOS

```csharp
// Services/ReservationCartService.cs
public class ReservationCartService : IReservationCartService
{
    public async Task<RoomReservationCart> AddRoomToCart(string sessionId, int roomId, DateTime checkIn, DateTime checkOut)
    {
        // Lógica específica para reservas
        // NO se mezcla con productos e-commerce
    }

    public async Task ClearReservationCart(string sessionId)
    {
        // Limpiar solo carritos de reservas
        // NO afecta carrito de productos
    }
}

// Services/ProductCartService.cs  
public class ProductCartService : IProductCartService
{
    public async Task<ProductShoppingCart> AddProductToCart(string sessionId, int productId, int? variantId, int quantity)
    {
        // Lógica específica para productos e-commerce
        // NO se mezcla con reservas
    }

    public async Task ClearProductCart(string sessionId)
    {
        // Limpiar solo carrito de productos
        // NO afecta reservas
    }
}
```

## 📅 CRONOGRAMA DE DESARROLLO

### SEMANA 1 (11-17 AGOSTO 2025)

#### LUNES 11 AGOSTO - DÍA 1 (7:00 PM - 3:00 AM)

**7:00-8:00 PM: Setup completo del proyecto**
- Crear solución ASP.NET Core API
- Setup Next.js frontend
- Configurar PostgreSQL

**8:00-10:00 PM: Base de datos y modelos**
- Crear todas las entidades
- Configurar Entity Framework
- Migrations iniciales

**10:00-12:00 AM: Autenticación y estructura base**
- Sistema de login
- Layout del back office

**12:00-3:00 AM: Primeros módulos**
- Módulo Empresa
- Módulo Usuarios/Roles

#### MARTES 12 AGOSTO - DÍA 2
- Módulo Habitaciones (separado de productos)
- Módulo Productos con sistema de variantes
- Módulo Colecciones

#### MIÉRCOLES 13 AGOSTO - DÍA 3
- Website Builder - Editor base
- Sistema de páginas (5 tipos)
- Primeras 3 secciones (ImageWithText, RichText, ImageBanner)

#### JUEVES 14 AGOSTO - DÍA 4
- Resto de secciones del builder (8 restantes)
- Sistema drag & drop con validaciones
- Sistema undo/redo completo

#### VIERNES 15 AGOSTO - DÍA 5
- Sistema de cache optimizado
- Preview vs Production
- Personalización UI completa

#### SÁBADO-DOMINGO 16-17 AGOSTO
- Testing completo
- Optimizaciones de performance
- Deploy en Azure
- Configuración de dominios

## 🔧 COMANDOS ESPECÍFICOS CLAUDE CODE

### TEMPLATES PERSONALIZADOS

#### .claude/commands/create-section.md
```markdown
Crear nueva sección para website builder: $ARGUMENTS

Seguir este patrón exacto:
1. Crear componente React en /components/sections/[NombreSeccion].tsx
2. Crear interface de configuración en /types/sections.ts
3. Agregar al enum SectionType
4. Implementar validaciones de drag & drop en dragDrop.ts
5. Crear preview en el editor
6. Agregar a la librería de secciones
7. Tests unitarios automáticos

El componente debe:
- Aceptar prop config tipado
- Manejar múltiples instancias
- Ser responsive (desktop/mobile)
- Incluir data-attributes para el editor
```

#### .claude/commands/create-module.md
```markdown
Crear módulo completo del back office: $ARGUMENTS

Estructura completa:
1. Backend API:
   - Controller con CRUD completo (/Controllers/)
   - Service con lógica de negocio (/Services/)
   - Repository con Entity Framework (/Repositories/)
   - Modelo con validaciones (/Models/)
   - DTOs para transferencia de datos

2. Frontend Next.js:
   - Página principal del módulo (/app/[modulo]/)
   - Componentes de formularios (/components/forms/)
   - Tablas de datos con paginación
   - Modales para crear/editar
   - Rutas y navegación

3. Integraciones:
   - Agregar al sidebar de navegación
   - Configurar permisos de roles
   - Tests de integración
   - Documentación del módulo

Archivos máximo 300 líneas cada uno.
```

#### .claude/commands/debug-builder.md
```markdown
Debuggear problemas del website builder: $ARGUMENTS

Proceso de debugging sistemático:
1. Verificar estado del editor (sections, pageConfig)
2. Revisar validaciones de drag & drop
3. Comprobar historial de undo/redo
4. Validar configuración de secciones
5. Verificar cache de preview vs production
6. Revisar logs del navegador
7. Comprobar API calls y responses
8. Generar reporte del problema encontrado

Incluir pasos específicos para reproducir el error.
```

#### .claude/commands/optimize-performance.md
```markdown
Optimizar performance del sistema: $ARGUMENTS

Áreas a revistar:
1. Cache del sistema:
   - Cache de contenido de páginas
   - Cache de configuraciones de tema
   - Cache de menús de navegación

2. Base de datos:
   - Queries N+1
   - Índices faltantes  
   - Joins innecesarios

3. Frontend:
   - Code splitting de Next.js
   - Lazy loading de componentes
   - Optimización de imágenes

4. API:
   - Paginación de resultados
   - Compresión de responses
   - Rate limiting

Generar reporte con métricas antes/después.
```

## 🎯 DESIGN SYSTEM Y ESTÁNDARES

### COLORES Y TIPOGRAFÍA

```typescript
// lib/design-system.ts
export const colors = {
  primary: {
    50: '#f0fdf4',
    500: '#22c55e', // Verde farmacia/empresa
    600: '#16a34a',
    700: '#15803d',
    900: '#14532d'
  },
  secondary: {
    50: '#f8fafc',
    500: '#64748b',
    600: '#475569',
    900: '#0f172a'
  },
  success: '#10b981',
  warning: '#f59e0b', 
  error: '#ef4444'
};

export const typography = {
  fontFamily: 'Inter, system-ui, sans-serif',
  sizes: {
    xs: '0.75rem',    // 12px
    sm: '0.875rem',   // 14px  
    base: '1rem',     // 16px
    lg: '1.125rem',   // 18px
    xl: '1.25rem',    // 20px
    '2xl': '1.5rem',  // 24px
    '3xl': '1.875rem' // 30px
  }
};
```

### COMPONENTES BASE ESTÁNDAR

```typescript
// components/ui/Button.tsx
interface ButtonProps {
  variant: 'primary' | 'secondary' | 'danger';
  size: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
}

export function Button({ variant, size, children, ...props }: ButtonProps) {
  const baseClasses = 'rounded-lg font-medium transition-colors';
  const variantClasses = {
    primary: 'bg-green-600 text-white hover:bg-green-700',
    secondary: 'bg-gray-200 text-gray-900 hover:bg-gray-300',
    danger: 'bg-red-600 text-white hover:bg-red-700'
  };
  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-base', 
    lg: 'px-6 py-3 text-lg'
  };

  return (
    <button 
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]}`}
      {...props}
    >
      {children}
    </button>
  );
}
```

## ✅ CHECKLIST DE ÉXITO FINAL

### PROBLEMAS RESUELTOS (9/9):
- ✅ JSON gigante → Base de datos relacional modular
- ✅ Páginas rígidas → Sistema dinámico de páginas (5 tipos)
- ✅ Secciones únicas → Componentes multi-instancia (11 tipos)
- ✅ Drag & drop roto → Sistema con validaciones jerárquicas
- ✅ Performance lenta → Cache separado preview/production
- ✅ Conceptos mezclados → Separación completa carritos
- ✅ Sin variantes → Sistema completo implementado
- ✅ Páginas faltantes → Templates estándar (home, product, cart, checkout, custom)
- ✅ Sin undo/redo → Historial de 50 estados + atajos de teclado

### FUNCIONALIDADES AGREGADAS:
- ✅ 11 tipos de secciones modulares con configuraciones específicas
- ✅ Editor visual en tiempo real con drag & drop validado
- ✅ Sistema de personalización UI completo (idioma, tema, colores, layout)
- ✅ Preview móvil y vista real separados del editor
- ✅ Menús de navegación configurables para header y footer
- ✅ Carritos completamente separados (habitaciones vs productos)
- ✅ Sistema multi-idioma (ES/EN) con traducción completa
- ✅ Themes light/dark con personalización de colores
- ✅ Sistema de variantes de productos con atributos flexibles
- ✅ Auto-save cada 30 segundos con historial persistente
- ✅ Sistema single-tenant (una empresa por base de datos)
- ✅ 12 módulos completos del back office

### TECNOLOGÍAS Y HERRAMIENTAS:
- ✅ ASP.NET Core 8 con Clean Architecture
- ✅ Next.js 14 con App Router y TypeScript
- ✅ PostgreSQL con Entity Framework Core
- ✅ Tailwind CSS para estilos consistentes
- ✅ Claude Code integrado con comandos personalizados

## 🚀 RESULTADO FINAL

Este blueprint resuelve **TODOS** los 9 problemas críticos identificados y crea la base para un sistema escalable, mantenible y moderno que supera ampliamente las limitaciones del proyecto actual.

### El nuevo sistema será:
- 🏗️ **Arquitectura limpia y modular**
- ⚡ **Performance superior** (cache optimizado)
- 🎨 **UX moderna** (drag & drop, undo/redo, preview)
- 🔧 **Fácil mantenimiento** (archivos pequeños, código organizado)
- 📈 **Escalable** (single-tenant, separación de conceptos)
- 🌍 **Internacional** (multi-idioma, multi-timezone)

**¡LISTO PARA IMPLEMENTACIÓN EL 11 DE AGOSTO 2025!** 🎯

[FIN DEL BLUEPRINT]