# 📝 TYPOGRAPHY MAPPING GUIDE - Website Builder

## 🎯 Overview
Esta guía muestra exactamente cómo los campos de texto en módulos se mapean con la configuración global de Typography.

---

## 🏗️ Sistema de Typography Global

### Configuración Base (typography.ts)
```typescript
interface TypographyConfig {
  headings: {
    fontFamily: string;      // ej: "Montserrat"
    fontSize: number;        // ej: 32px
    fontWeight: string;      // ej: "700"
    useUppercase: boolean;   // ej: false
    letterSpacing: number;   // ej: 0px
  };
  body: {
    fontFamily: string;      // ej: "Open Sans"
    fontSize: number;        // ej: 16px
    fontWeight: string;      // ej: "400"
    useUppercase: boolean;   // ej: false
    letterSpacing: number;   // ej: 0px
  };
  menu: {
    fontFamily: string;      // ej: "Roboto"
    fontSize: number;        // ej: 14px
    fontWeight: string;      // ej: "500"
    useUppercase: boolean;   // ej: true
    letterSpacing: number;   // ej: 1px
  };
  buttons: {
    fontFamily: string;      // ej: "Montserrat"
    fontSize: number;        // ej: 14px
    fontWeight: string;      // ej: "600"
    useUppercase: boolean;   // ej: true
    letterSpacing: number;   // ej: 2px
  };
  productCardName: {
    fontFamily: string;      // ej: "Montserrat"
    fontSize: number;        // ej: 18px
    fontWeight: string;      // ej: "600"
    useUppercase: boolean;   // ej: false
    letterSpacing: number;   // ej: 0px
  };
}
```

---

## 📊 MAPEO DE CAMPOS A TYPOGRAPHY

### 🔤 REGLAS DE MAPEO ESTÁNDAR

| Campo en Módulo | Typography Global | Uso | Ejemplo |
|-----------------|-------------------|-----|---------|
| `title` | `typography.headings` | Títulos principales de sección | "Our Products" |
| `heading` | `typography.headings` | Encabezados de componentes | "Featured Items" |
| `sectionTitle` | `typography.headings` | Títulos de sección | "Gallery" |
| `itemTitle` | `typography.productCardName` | Títulos de items/productos | "Premium Shirt" |
| `subtitle` | `typography.body` | Subtítulos y descripciones cortas | "Discover our collection" |
| `description` | `typography.body` | Textos descriptivos largos | "Lorem ipsum..." |
| `body` | `typography.body` | Contenido principal de texto | Párrafos |
| `text` | `typography.body` | Texto genérico | Cualquier texto |
| `label` | `typography.body` | Etiquetas de formularios | "Email Address" |
| `buttonText` | `typography.buttons` | Texto de botones | "SHOP NOW" |
| `linkText` | `typography.buttons` | Texto de enlaces estilizados | "View More" |
| `menuItem` | `typography.menu` | Items de navegación | "HOME" |
| `navLink` | `typography.menu` | Enlaces de navegación | "PRODUCTS" |
| `productName` | `typography.productCardName` | Nombres de productos | "Classic Tee" |
| `cardTitle` | `typography.productCardName` | Títulos en tarjetas | "Summer Sale" |

---

## 💻 IMPLEMENTACIÓN EN PREVIEW

### Ejemplo Completo de Mapeo en Preview Component

```typescript
// PreviewGallery.tsx
export default function PreviewGallery({ settings, theme, deviceView }) {
  
  // 1. EXTRAER ESTILOS DE TYPOGRAPHY
  const headingStyles = theme?.typography?.headings ? {
    fontFamily: `'${theme.typography.headings.fontFamily}', sans-serif`,
    fontWeight: theme.typography.headings.fontWeight || '700',
    fontSize: `${theme.typography.headings.fontSize}px`,
    textTransform: theme.typography.headings.useUppercase ? 'uppercase' : 'none',
    letterSpacing: `${theme.typography.headings.letterSpacing || 0}px`
  } : {};
  
  const bodyStyles = theme?.typography?.body ? {
    fontFamily: `'${theme.typography.body.fontFamily}', sans-serif`,
    fontWeight: theme.typography.body.fontWeight || '400',
    fontSize: `${theme.typography.body.fontSize}px`,
    textTransform: theme.typography.body.useUppercase ? 'uppercase' : 'none',
    letterSpacing: `${theme.typography.body.letterSpacing || 0}px`
  } : {};
  
  const buttonStyles = theme?.typography?.buttons ? {
    fontFamily: `'${theme.typography.buttons.fontFamily}', sans-serif`,
    fontWeight: theme.typography.buttons.fontWeight || '600',
    fontSize: `${theme.typography.buttons.fontSize}px`,
    textTransform: theme.typography.buttons.useUppercase ? 'uppercase' : 'none',
    letterSpacing: `${theme.typography.buttons.letterSpacing || 0}px`
  } : {};
  
  const productNameStyles = theme?.typography?.productCardName ? {
    fontFamily: `'${theme.typography.productCardName.fontFamily}', sans-serif`,
    fontWeight: theme.typography.productCardName.fontWeight || '600',
    fontSize: `${theme.typography.productCardName.fontSize}px`,
    textTransform: theme.typography.productCardName.useUppercase ? 'uppercase' : 'none',
    letterSpacing: `${theme.typography.productCardName.letterSpacing || 0}px`
  } : {};
  
  // 2. APLICAR ESTILOS A CAMPOS CORRESPONDIENTES
  return (
    <div>
      {/* TÍTULO PRINCIPAL → typography.headings */}
      <h2 style={headingStyles}>
        {settings.title}
      </h2>
      
      {/* SUBTÍTULO → typography.body */}
      <p style={bodyStyles}>
        {settings.subtitle}
      </p>
      
      {/* ITEMS */}
      {settings.items?.map(item => (
        <div key={item.id}>
          {/* TÍTULO DE ITEM → typography.productCardName */}
          <h3 style={productNameStyles}>
            {item.itemTitle}
          </h3>
          
          {/* DESCRIPCIÓN → typography.body */}
          <p style={bodyStyles}>
            {item.description}
          </p>
          
          {/* BOTÓN → typography.buttons */}
          <button style={buttonStyles}>
            {item.buttonText}
          </button>
        </div>
      ))}
    </div>
  );
}
```

---

## 🎨 CASOS ESPECIALES Y OVERRIDES

### 1. SlideShow - Override de Font Size
```typescript
// SlideShow permite override del fontSize global
const headingSize = slide.headingSize || theme.typography.headings.fontSize;

<h2 style={{
  ...headingStyles,
  fontSize: `${headingSize}px` // Override local
}}>
  {slide.heading}
</h2>
```

### 2. Responsive - Diferentes tamaños mobile/desktop
```typescript
const isMobile = deviceView === 'mobile';

<h2 style={{
  ...headingStyles,
  fontSize: isMobile 
    ? `${theme.typography.headings.fontSize * 0.75}px` // 75% en mobile
    : `${theme.typography.headings.fontSize}px`
}}>
  {settings.title}
</h2>
```

### 3. Componentes con múltiples headings
```typescript
// Heading principal
<h1 style={headingStyles}>{settings.mainTitle}</h1>

// Heading secundario (más pequeño)
<h2 style={{
  ...headingStyles,
  fontSize: `${theme.typography.headings.fontSize * 0.8}px`
}}>
  {settings.sectionTitle}
</h2>

// Heading terciario (aún más pequeño)
<h3 style={{
  ...headingStyles,
  fontSize: `${theme.typography.headings.fontSize * 0.6}px`
}}>
  {settings.subheading}
</h3>
```

---

## 📋 CHECKLIST DE VALIDACIÓN

Cuando crees un nuevo módulo, verifica:

### En el Editor (GalleryEditor.tsx):
- [ ] ¿Qué campos de texto tiene mi módulo?
- [ ] ¿Son títulos, subtítulos, body, o botones?
- [ ] ¿Necesito override de font size?

### En el Preview (PreviewGallery.tsx):
- [ ] ¿Extraje los estilos de typography correctos?
- [ ] ¿Apliqué headingStyles a campos de título?
- [ ] ¿Apliqué bodyStyles a campos de descripción?
- [ ] ¿Apliqué buttonStyles a campos de botón?
- [ ] ¿Apliqué productNameStyles a nombres de items?
- [ ] ¿Consideré responsive (mobile vs desktop)?

---

## 🔍 EJEMPLO DE ANÁLISIS DE MÓDULO

### Módulo: ImageBanner
```typescript
interface ImageBannerConfig {
  // Campos de texto:
  title: string;           // → typography.headings
  subtitle: string;        // → typography.body  
  buttonText: string;      // → typography.buttons
  secondButtonText: string;// → typography.buttons
}
```

### Mapeo Visual:
```
┌─────────────────────────────────┐
│         IMAGE BANNER            │
├─────────────────────────────────┤
│                                 │
│   [title]                       │ ← typography.headings
│   "Summer Collection 2025"      │   (Montserrat, 32px, bold)
│                                 │
│   [subtitle]                    │ ← typography.body
│   "Discover our latest styles"  │   (Open Sans, 16px, regular)
│                                 │
│   [buttonText]  [secondButton]  │ ← typography.buttons
│   "SHOP NOW"    "LEARN MORE"    │   (Montserrat, 14px, bold, uppercase)
│                                 │
└─────────────────────────────────┘
```

---

## 🚀 PLANTILLA PARA NUEVOS MÓDULOS

Cuando recibas una vista de configuración, usa esta plantilla:

```markdown
## Módulo: [NombreModulo]

### Campos de Texto Identificados:
1. `fieldName1` → typography.[headings|body|buttons|menu|productCardName]
2. `fieldName2` → typography.[headings|body|buttons|menu|productCardName]
3. `fieldName3` → typography.[headings|body|buttons|menu|productCardName]

### Implementación en Preview:
```typescript
// Extraer estilos necesarios
const headingStyles = theme?.typography?.headings ? {...} : {};
const bodyStyles = theme?.typography?.body ? {...} : {};
// etc...

// Aplicar a cada campo
<h2 style={headingStyles}>{settings.fieldName1}</h2>
<p style={bodyStyles}>{settings.fieldName2}</p>
```
```

---

## ⚠️ REGLAS IMPORTANTES

1. **NUNCA hardcodear fuentes**: Siempre usar typography del theme
2. **Respetar uppercase**: Si typography dice uppercase, aplicarlo
3. **Mantener jerarquía**: Headings > ProductCardName > Body > Buttons
4. **Mobile responsive**: Reducir fontSize en mobile (típicamente 75-80%)
5. **Overrides locales**: Solo cuando el módulo lo requiera específicamente

---

## 📊 TABLA RÁPIDA DE REFERENCIA

| Si el campo se llama... | Usa este typography... |
|------------------------|------------------------|
| title, heading, sectionTitle | headings |
| subtitle, description, text, body | body |
| buttonText, linkText, cta | buttons |
| menuItem, navLink | menu |
| productName, itemTitle, cardTitle | productCardName |

---

**Última actualización**: Agosto 2025
**Versión**: 1.0.0
**Aplicable a**: Todos los módulos del Website Builder