# Typography System for Header Navigation Implementation

## Overview
Implementation of typography configuration system that applies custom font settings to header navigation menu items, ensuring consistent styling across all header layouts.

**Created**: 2025-01-13  
**Category**: features  
**Status**: ✅ Complete  
**Time Spent**: 45 minutes  

## Table of Contents
- [Architecture](#architecture)
- [Implementation Details](#implementation-details)
- [Components](#components)
- [Configuration Options](#configuration-options)
- [Code Examples](#code-examples)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Related Documentation](#related-documentation)

## Architecture

### System Design
```
Typography Configuration (typography.menu)
    ↓
useThemeConfigStore (Global State)
    ↓
EditorPreview.tsx (Apply Styles)
    ↓
All Menu Items (Consistent Styling)
```

### Key Decisions
1. **Separate typography types**: 5 distinct typography configurations (headings, body, menu, productCardName, buttons)
2. **Menu-specific styling**: `typography.menu` specifically for navigation items
3. **Dynamic style generation**: Convert configuration to CSS properties at runtime
4. **Universal application**: Apply to all menu contexts (inline, dropdown, drawer)

## Implementation Details

### 1. Typography Type System

#### Typography Configuration Structure
```typescript
// src/types/theme/typography.ts
export interface FontConfig {
  fontFamily: string;
  fontWeight?: string;
  useUppercase?: boolean;
  fontSize?: number; // <=100 = percentage, >100 = pixels
  letterSpacing?: number;
}

export interface TypographyConfig {
  headings: FontConfig;
  body: FontConfig;
  menu: FontConfig; // ← Used for navigation
  productCardName: FontConfig;
  buttons: FontConfig;
}
```

### 2. Default Menu Typography
```typescript
menu: {
  fontFamily: 'Poppins',
  fontWeight: '400',
  useUppercase: false,
  fontSize: 94, // 94% of base size
  letterSpacing: 0
}
```

### 3. Style Application in EditorPreview

#### Import Configuration
```typescript
import useThemeConfigStore from '@/stores/useThemeConfigStore';

export function EditorPreview() {
  const { config: themeConfig } = useThemeConfigStore();
  // ...
}
```

#### Generate Typography Styles
```typescript
// Create styles from configuration
const menuTypographyStyles = themeConfig?.typography?.menu ? {
  fontFamily: `'${themeConfig.typography.menu.fontFamily}', sans-serif`,
  fontWeight: themeConfig.typography.menu.fontWeight || '400',
  textTransform: themeConfig.typography.menu.useUppercase ? 'uppercase' : 'none',
  fontSize: themeConfig.typography.menu.fontSize ? 
    (themeConfig.typography.menu.fontSize <= 100 ? 
      `${themeConfig.typography.menu.fontSize}%` : 
      `${themeConfig.typography.menu.fontSize}px`) : '94%',
  letterSpacing: `${themeConfig.typography.menu.letterSpacing || 0}px`
} : {};
```

#### Apply to Menu Items
```typescript
// Main menu items
<a 
  href="#" 
  style={{ 
    color: schemeToUse?.text || '#000000',
    ...menuTypographyStyles // Apply typography
  }}
>
  {item.label}
</a>

// Dropdown subitems
<a 
  style={{ 
    color: schemeToUse?.text || '#000000',
    ...menuTypographyStyles // Consistent styling
  }}
>
  {subItem.label}
</a>
```

## Components

### Files Modified
1. **EditorPreview.tsx** - Main preview component
   - Added `useThemeConfigStore` import
   - Created `menuTypographyStyles` object
   - Applied styles to all menu contexts

### Menu Contexts Updated
1. **Inline Menus** - Standard horizontal navigation
2. **Menu Below Layouts** - Two-row header designs
3. **Dropdown Menus** - Submenu items
4. **Drawer Menu** - Mobile/hamburger menu
5. **Drawer Submenus** - Nested drawer items

## Configuration Options

### Available Settings
```typescript
interface MenuTypographyConfig {
  fontFamily: string;       // Font face (e.g., 'Poppins', 'Inter')
  fontWeight: string;        // Weight (e.g., '400', '600', '700')
  useUppercase: boolean;     // Transform to uppercase
  fontSize: number;          // Size in % or px
  letterSpacing: number;     // Spacing in px (can be negative)
}
```

### Font Size Logic
- Values ≤ 100: Treated as percentage
- Values > 100: Treated as pixels
- Default: 94% (0.94em of parent)

## Code Examples

### Complete Implementation for Different Layouts

#### Standard Inline Menu
```typescript
<nav className="flex gap-6">
  {menuItems.map((item) => (
    <a 
      href="#"
      style={{ 
        color: schemeToUse?.text || '#000000',
        ...menuTypographyStyles
      }}
    >
      {item.label}
    </a>
  ))}
</nav>
```

#### Drawer Menu with Typography
```typescript
// Define styles in drawer context
const menuTypographyStyles = themeConfig?.typography?.menu ? {
  fontFamily: `'${themeConfig.typography.menu.fontFamily}', sans-serif`,
  fontWeight: themeConfig.typography.menu.fontWeight || '400',
  textTransform: themeConfig.typography.menu.useUppercase ? 'uppercase' : 'none',
  fontSize: themeConfig.typography.menu.fontSize ? 
    (themeConfig.typography.menu.fontSize <= 100 ? 
      `${themeConfig.typography.menu.fontSize}%` : 
      `${themeConfig.typography.menu.fontSize}px`) : '94%',
  letterSpacing: `${themeConfig.typography.menu.letterSpacing || 0}px`
} : {};

// Apply to drawer items
<span style={menuTypographyStyles}>{item.label}</span>
```

## Testing

### Test Scenarios
1. ✅ Typography changes apply to all menu items
2. ✅ Font family changes work correctly
3. ✅ Font weight variations display properly
4. ✅ Uppercase transformation toggles
5. ✅ Font size in percentage and pixels
6. ✅ Letter spacing (positive and negative)
7. ✅ Styles persist across layout changes
8. ✅ Drawer menu receives same styling
9. ✅ Dropdown items styled consistently

### Browser Compatibility
- ✅ Chrome/Edge
- ✅ Firefox
- ✅ Safari
- ✅ Mobile browsers

## Troubleshooting

### Issue 1: Import Error
**Error**: `useThemeConfigStore is not a function`
**Solution**: Changed from named import to default import
```typescript
// Before (incorrect)
import { useThemeConfigStore } from '@/stores/useThemeConfigStore';

// After (correct)
import useThemeConfigStore from '@/stores/useThemeConfigStore';
```

### Issue 2: Undefined Variable in Drawer
**Error**: `menuTypographyStyles is not defined`
**Solution**: Define styles in drawer scope
```typescript
// Added definition before drawer return
const menuTypographyStyles = themeConfig?.typography?.menu ? {...} : {};
```

## Performance Considerations

### Optimizations
1. **Style object memoization**: Consider using `useMemo` for menuTypographyStyles
2. **Conditional rendering**: Only calculate styles when menu exists
3. **CSS-in-JS**: Minimal runtime overhead

### Future Improvements
```typescript
// Potential optimization with useMemo
const menuTypographyStyles = React.useMemo(() => {
  if (!themeConfig?.typography?.menu) return {};
  return {
    fontFamily: `'${themeConfig.typography.menu.fontFamily}', sans-serif`,
    // ... rest of styles
  };
}, [themeConfig?.typography?.menu]);
```

## Migration Guide

### From Static Styles
1. Remove hardcoded font styles from menu items
2. Import `useThemeConfigStore`
3. Generate `menuTypographyStyles`
4. Apply to all menu contexts

## API Integration

### Backend Storage
```csharp
// Typography stored as JSONB in PostgreSQL
public class GlobalThemeConfig {
    public JsonDocument? Typography { get; set; }
}
```

### Frontend Retrieval
```typescript
// Automatic fetch via useThemeConfigStore
const { config: themeConfig } = useThemeConfigStore();
// Access: themeConfig.typography.menu
```

## Related Documentation

### See Also
- [Typography Configuration Types](/docs/implementations/features/2025-01-typography-types.md)
- [Header Configurations](/docs/implementations/features/2025-01-header-configurations.md)
- [Theme Configuration Store](/docs/implementations/features/theme-config-store.md)

### Troubleshooting
- [Store Import Issues](/docs/troubleshooting/features/features-02-store-imports.md)
- [Scope Errors](/docs/troubleshooting/features/features-03-scope-errors.md)

## Search Keywords
typography, menu, navigation, font, header, fontFamily, fontSize, fontWeight, letterSpacing, uppercase, textTransform, EditorPreview, useThemeConfigStore, menuTypographyStyles, drawer, dropdown, inline menu

## Notes
- Typography system follows separation of concerns principle
- Each text type (menu, body, headings) has independent configuration
- System designed for extensibility - easy to add new typography types
- Consistent application across all menu rendering contexts ensures UX coherence