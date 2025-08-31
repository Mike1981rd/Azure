# CLAUDE.md - REGLAS Y CONTEXTO DEL PROYECTO WEBSITE BUILDER

## 🎯 CONTEXTO DEL PROYECTO
**Proyecto**: Sistema multi-tenant hotel + e-commerce + website builder
**Stack**: ASP.NET Core 8 API + Next.js 14 + PostgreSQL + Entity Framework Core
**Estado**: Reescritura completa iniciando 11 agosto 2025 para resolver 9 problemas críticos
**Arquitectura**: Separación backend/frontend (antes era monolítico ASP.NET Core)

## ⚠️ PROBLEMAS CRÍTICOS A EVITAR (DEL PROYECTO ANTERIOR)
1. **NO** crear JSON gigantes - usar base de datos relacional
2. **NO** mezclar habitaciones con productos - son conceptos separados
3. **NO** limitar secciones a una sola instancia - permitir múltiples
4. **NO** permitir drag & drop sin validaciones - implementar reglas jerárquicas
5. **NO** usar el mismo cache para preview y producción - separar ambos
6. **NO** mezclar carritos de reservas con carritos de productos
7. **SIEMPRE** implementar sistema de variantes para productos
8. **SIEMPRE** incluir 5 tipos de páginas: home, product, cart, checkout, custom
9. **SIEMPRE** implementar undo/redo con historial de 50 estados

## 📁 ESTRUCTURA DE ARCHIVOS

### Backend (WebsiteBuilderAPI/)
```
Controllers/     # Max 300 líneas por archivo
Services/        # Interfaces e implementaciones separadas
Repositories/    # Patrón Repository con EF Core
Models/          # Entidades de dominio
DTOs/           # Data Transfer Objects
```

### Frontend (websitebuilder-admin/)
```
src/app/         # App Router de Next.js 14
src/components/  # Componentes reutilizables
src/lib/         # Utilidades y lógica
src/hooks/       # Custom hooks
```

## 🗄️ ENTIDADES PRINCIPALES DE BASE DE DATOS
- **Hotels**: Multi-tenant, cada hotel tiene su dominio
- **Rooms**: Habitaciones (SEPARADAS de productos)
- **Products**: Productos e-commerce con HasVariants
- **ProductVariants**: Variantes con atributos JSONB
- **WebsitePages**: 5 tipos (home, product, cart, checkout, custom)
- **PageSections**: 11 tipos de secciones modulares
- **RoomReservationCarts**: Carrito SOLO para reservas
- **ProductShoppingCarts**: Carrito SOLO para productos
- **EditorHistory**: Estados para undo/redo

## 🧩 11 TIPOS DE SECCIONES OBLIGATORIAS
1. ImageWithText
2. ImageBanner
3. RichText
4. Gallery
5. ContactForm
6. Newsletter
7. FeaturedProduct
8. FeaturedCollection
9. Testimonials
10. FAQ
11. Videos

## 📋 REGLAS DE DESARROLLO

### ⚡ ARQUITECTURA DE COMPONENTES CON PESTAÑAS
**⚠️ IMPORTANTE**: Si el módulo a construir tiene pestañas/tabs para dividir contenido:
- **LEER PRIMERO**: `/docs/architecture/tabbed-components-architecture.md`
- **USAR SIEMPRE**: Patrón de Estado Elevado (Lifted State)
- **NUNCA**: Usar estado local en las pestañas (se pierde al cambiar)
- **REFERENCIA**: Ver implementación en `/src/components/clientes/`

### 1. SEPARACIÓN DE CONCEPTOS
```csharp
// ❌ INCORRECTO
public class Cart {
    public List<Room> Rooms { get; set; }
    public List<Product> Products { get; set; }
}

// ✅ CORRECTO
public class RoomReservationCart { /* solo reservas */ }
public class ProductShoppingCart { /* solo productos */ }
```

### 2. MULTI-INSTANCIA DE SECCIONES
```typescript
// ✅ SIEMPRE permitir múltiples instancias
<ImageWithText config={config} isMultiple={true} index={0} />
<ImageWithText config={config2} isMultiple={true} index={1} />
```

### 3. VALIDACIONES DRAG & DROP
```typescript
// ✅ SIEMPRE validar antes de permitir drop
if (!validateDrop(source, target, targetLevel)) {
    return false; // No permitir
}
```

### 4. CACHE DIFERENCIADO
```csharp
// ✅ SIEMPRE separar preview de producción
var cacheKey = isPreview 
    ? $"website:preview:{domain}" 
    : $"website:production:{domain}";
```

### 5. SISTEMA DE VARIANTES
```csharp
// ✅ SIEMPRE verificar HasVariants
if (product.HasVariants) {
    // Requerir selección de variante
    var variant = await GetVariant(variantId);
}
```

### 6. UNDO/REDO OBLIGATORIO
```typescript
// ✅ SIEMPRE guardar estado antes de cambios
saveState("Added section", pageConfig, sections);
```

### 7. PÁGINAS ESTÁNDAR
```typescript
// ✅ SIEMPRE incluir estos 5 tipos
enum PageType {
    Home = 'home',
    Product = 'product',
    Cart = 'cart',
    Checkout = 'checkout',
    Custom = 'custom'
}
```

## 🔧 PATRONES DE CÓDIGO

### Controllers (ASP.NET Core)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    
    // Constructor injection
    // Métodos async/await
    // Retornar ActionResult<T>
    // Manejo de errores con try-catch
}
```

### Componentes (Next.js)
```typescript
// ✅ SIEMPRE usar TypeScript estricto
interface Props {
    config: SectionConfig;
    isMultiple?: boolean;
    index?: number;
}

export function Section({ config, isMultiple = false, index = 0 }: Props) {
    // Hooks al inicio
    // Lógica clara
    // JSX limpio
}
```

### Componentes con Pestañas (Patrón Estado Elevado)
```typescript
// ✅ CORRECTO - Componente Padre con estado centralizado
export default function ParentWithTabs() {
    const [formData, setFormData] = useState<AllFormData>({
        tab1: { /* datos tab 1 */ },
        tab2: { /* datos tab 2 */ }
    });
    
    const updateTab1Data = (data: Partial<Tab1Data>) => {
        setFormData(prev => ({
            ...prev,
            tab1: { ...prev.tab1, ...data }
        }));
    };
    
    return (
        <Tab1Component 
            formData={formData.tab1}
            onFormChange={updateTab1Data}
        />
    );
}

// ✅ CORRECTO - Componente de pestaña SIN estado local para datos
export function Tab1Component({ formData, onFormChange }: Props) {
    // ❌ NO usar useState para datos del formulario
    // ✅ SÍ usar props formData y onFormChange
    
    return (
        <input
            value={formData.field}
            onChange={(e) => onFormChange({ field: e.target.value })}
        />
    );
}

// 📖 VER DOCUMENTACIÓN COMPLETA: /docs/architecture/tabbed-components-architecture.md
```

### Services
```csharp
public interface IProductService
{
    Task<Product> GetByIdAsync(int id);
    Task<PagedResult<Product>> GetPagedAsync(int page, int size);
    Task<Product> CreateAsync(CreateProductDto dto);
    Task<Product> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}
```

## 🚀 MÓDULOS REQUERIDOS DEL BACK OFFICE
1. Dashboard - Métricas y KPIs
2. Empresa - Configuración del hotel
3. Roles & Usuarios - Gestión de accesos
4. Clientes - Base de datos CRM
5. Reservaciones - Gestión de habitaciones
6. Métodos de Pago - Gateways de pago
7. Colecciones - Agrupación de productos
8. Productos - Catálogo e-commerce
9. Páginas - Gestión de contenido
10. Políticas - Términos legales
11. Sitio Web - Website builder
12. Dominios - Gestión DNS/SSL

## ⚡ OPTIMIZACIONES OBLIGATORIAS

### Performance
- Cache de 5 min para preview, 24h para producción
- Lazy loading de componentes pesados
- Paginación en todas las listas (20 items default)
- Índices en campos de búsqueda frecuente

### UX/UI
- Undo/Redo con Ctrl+Z / Ctrl+Y
- Auto-save cada 30 segundos
- Preview desktop/móvil en tiempo real
- Drag & drop con feedback visual

### Seguridad
- Autenticación JWT
- Middleware multi-tenant
- Validación de dominios
- Rate limiting en APIs

## 🎨 DESIGN SYSTEM
```typescript
// Colores principales
primary: '#22c55e'    // Verde hotel/farmacia
secondary: '#64748b'  // Gris neutro
error: '#ef4444'      // Rojo error

// Tipografía
fontFamily: 'Inter, system-ui, sans-serif'
// Tamaños: xs(12px), sm(14px), base(16px), lg(18px), xl(20px)
```

## 🎨 UI IMPLEMENTATION CHECKLIST - MANDATORY FOR ALL NEW PAGES
**IMPORTANTE**: Toda nueva UI/página DEBE implementar estos 6 puntos obligatorios:

### ✅ 1. TRADUCCIONES (i18n)
```typescript
// Usar siempre el hook useI18n
const { t } = useI18n();

// En el JSX:
<h1>{t('section.title', 'Default Text')}</h1>

// Agregar traducciones en:
// - /src/lib/i18n/translations/en.json
// - /src/lib/i18n/translations/es.json
```

### ✅ 2. COLOR PRIMARIO DINÁMICO
```typescript
// Obtener color primario del localStorage
const [primaryColor, setPrimaryColor] = useState('#22c55e');

useEffect(() => {
  const settings = localStorage.getItem('ui-settings');
  if (settings) {
    const parsed = JSON.parse(settings);
    setPrimaryColor(parsed.primaryColor || '#22c55e');
  }
}, []);

// Aplicar en botones principales:
<button style={{ backgroundColor: primaryColor }}>
```

### ✅ 3. DARK MODE
```typescript
// Usar clases Tailwind para dark mode
<div className="bg-white dark:bg-gray-800">
<p className="text-gray-900 dark:text-white">
<border className="border-gray-200 dark:border-gray-700">

// Colores comunes:
// - Fondos: bg-white dark:bg-gray-800
// - Textos: text-gray-900 dark:text-white
// - Bordes: border-gray-200 dark:border-gray-700
// - Hover: hover:bg-gray-50 dark:hover:bg-gray-700
```

### ✅ 4. RESPONSIVIDAD MÓVIL - PATRONES OBLIGATORIOS

#### 🎯 REGLA PRINCIPAL
**SIEMPRE diseñar primero para móvil (320px-414px), luego escalar a desktop**

#### 📐 PATRONES DE LAYOUT MÓVIL PROBADOS

##### 1️⃣ PESTAÑAS EN PÁGINAS CON MÚLTIPLES SECCIONES
```typescript
// ✅ MÓVIL: Vertical stack con pestaña activa destacada
<div className="sm:hidden">
  {/* Pestaña activa como header */}
  <div className="px-4 py-3 bg-white dark:bg-gray-800">
    <div className="px-4 py-3 rounded-xl text-white" style={{ backgroundColor: primaryColor }}>
      <div className="flex items-center gap-3">
        <span>{activeTab.icon}</span>
        <span>{activeTab.label}</span>
      </div>
    </div>
  </div>
  
  {/* Otras pestañas como lista vertical */}
  <div className="px-4 py-2 space-y-2">
    {tabs.map(tab => (
      <button className="w-full px-4 py-3.5 bg-white rounded-xl flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span>{tab.icon}</span>
          <span>{tab.label}</span>
        </div>
        <ChevronRight />
      </button>
    ))}
  </div>
</div>

// ✅ DESKTOP: Tabs horizontales tradicionales
<div className="hidden sm:block border-b">
  <nav className="flex space-x-8">
    {tabs.map(tab => <button className="py-2 px-1 border-b-2">)}
  </nav>
</div>
```

##### 2️⃣ INPUTS Y FORMULARIOS
```typescript
// ✅ SIEMPRE con padding del contenedor o w-11/12
<div className="px-4 space-y-4">
  <div className="w-11/12 md:w-full">
    <label className="block text-xs mb-1.5">Label</label>
    <input className="w-full px-3.5 py-2.5 rounded-xl" />
  </div>
</div>

// ❌ NUNCA input al 100% sin margen
<input className="w-full" /> // MAL - toca los bordes
```

##### 3️⃣ BOTONES DE ACCIÓN
```typescript
// ✅ Botón principal único - Full width
<div className="px-4 pb-6">
  <button className="w-full px-4 py-3.5 rounded-xl" style={{ backgroundColor: primaryColor }}>
    Edit Details
  </button>
</div>

// ✅ Botones duales (Cancel/Save) - Flex horizontal
<div className="fixed bottom-0 left-0 right-0 p-4 bg-white border-t md:hidden">
  <div className="flex gap-3">
    <button className="flex-1 py-3 rounded-xl bg-gray-100">Cancel</button>
    <button className="flex-1 py-3 rounded-xl text-white" style={{ backgroundColor: primaryColor }}>
      Save
    </button>
  </div>
</div>

// ✅ Múltiples opciones - Stack vertical
<div className="flex flex-col gap-2">
  <button className="w-full py-3.5">Option 1</button>
  <button className="w-full py-3.5">Option 2</button>
</div>
```

##### 4️⃣ CARDS DE MÉTRICAS/ESTADÍSTICAS
```typescript
// ✅ Grid 2x2 en móvil, 4 columnas en desktop
<div className="grid grid-cols-2 gap-3 md:grid-cols-4">
  {/* Cards importantes span 2 columnas en móvil */}
  <div className="col-span-2 md:col-span-1 bg-white rounded-xl p-4">
    <div className="p-2.5 rounded-lg bg-blue-50">
      <Icon />
    </div>
    <p className="text-2xl font-bold mt-2">{value}</p>
    <p className="text-xs text-gray-500">{label}</p>
  </div>
  
  {/* Cards normales 1 columna */}
  <div className="bg-white rounded-xl p-4">
    {/* contenido */}
  </div>
</div>
```

##### 5️⃣ AVATAR Y PERFIL CENTRADO
```typescript
// ✅ Centrado SOLO en móvil
<div className="md:hidden flex flex-col items-center py-6">
  <div className="relative">
    <div className="w-24 h-24 rounded-full overflow-hidden">
      <img src={avatar} />
    </div>
    <span className="absolute -bottom-1 -right-1 px-2.5 py-1 text-xs rounded-full bg-green-100">
      Active
    </span>
  </div>
  <h3 className="mt-3 text-lg font-semibold">{name}</h3>
  <p className="text-sm text-gray-500">Customer ID #{id}</p>
</div>
```

##### 6️⃣ HEADER COMPACTO MÓVIL
```typescript
// ✅ Header con info esencial y acción
<div className="md:hidden bg-white px-4 py-4 border-b">
  <div className="flex items-center justify-between mb-3">
    <div>
      <p className="text-xs text-gray-500">Customer ID #{id}</p>
      <p className="text-xs text-gray-500">{date}</p>
    </div>
    <button className="px-3 py-1.5 text-xs rounded-lg bg-red-50 text-red-600">
      Delete
    </button>
  </div>
</div>
```

#### 🚫 ANTI-PATRONES A EVITAR

1. **❌ NO usar scroll horizontal** para navegación principal
2. **❌ NO poner inputs al 100%** sin padding del contenedor  
3. **❌ NO usar pestañas horizontales** que se corten en móvil
4. **❌ NO poner botones pequeños** lado a lado (mínimo 44px altura)
5. **❌ NO centrar en desktop** contenido que solo debe estar centrado en móvil
6. **❌ NO usar overflow-x-auto** para cards principales

#### 📏 MEDIDAS ESTÁNDAR MÓVIL

```typescript
// Padding contenedor
className="px-4"  // 16px laterales

// Altura mínima elementos táctiles  
className="py-3.5" // 14px = ~48px altura total con texto

// Gaps consistentes
className="gap-3"  // 12px entre elementos

// Border radius
className="rounded-xl" // 12px esquinas redondeadas

// Ancho inputs móvil
className="w-11/12"  // O contenedor con px-4

// Fixed bottom bar
className="fixed bottom-0 left-0 right-0 p-4 bg-white border-t"
```

#### 🔄 TESTING OBLIGATORIO

Antes de considerar completa una vista, verificar en:
- **320px** - iPhone SE (más pequeño)
- **375px** - iPhone 12/13/14
- **414px** - iPhone Plus/Max
- **Rotación** - Landscape orientation

#### 📱 BREAKPOINTS TAILWIND

```typescript
// sm: 640px  - Tablets pequeñas
// md: 768px  - Tablets
// lg: 1024px - Desktop pequeño
// xl: 1280px - Desktop normal

// Mobile-first approach
className="block sm:hidden"     // Visible solo en móvil
className="hidden sm:block"      // Oculto en móvil
className="w-full sm:w-auto"     // Full width móvil, auto desktop
className="flex-col sm:flex-row" // Stack móvil, horizontal desktop
```