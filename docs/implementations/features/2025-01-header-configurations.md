# Header Configurations Implementation

## Overview
Comprehensive header configuration system with multiple layouts, dynamic icons, and theme integration.

**Created**: 2025-01-14  
**Category**: features  
**Status**: ✅ Complete  
**Time Spent**: 4 hours  

## Table of Contents
- [Architecture](#architecture)
- [Implementation Details](#implementation-details)
- [Components](#components)
- [Configuration Options](#configuration-options)
- [Code Examples](#code-examples)
- [Testing](#testing)
- [Related Documentation](#related-documentation)

## Architecture

### System Design
```
HeaderEditor.tsx (Configuration Panel)
    ↓
useEditorStore (State Management)
    ↓
EditorPreview.tsx (Live Preview)
    ↓
Dynamic Rendering (Layouts + Icons)
```

### Key Decisions
1. **Separate layout handlers**: Each layout has its own rendering logic
2. **Function-based icon rendering**: Dynamic icons based on configuration
3. **Default values**: Sensible defaults for all options
4. **Conditional rendering**: Show/hide elements based on layout

## Implementation Details

### 1. Layout System

#### Supported Layouts
- **Drawer**: Hamburger menu with sliding panel
- **Logo left, menu center inline**: Logo on left, centered navigation
- **Logo left, menu left inline**: Logo and menu both on left
- **Logo center, menu left inline**: Centered logo, left navigation
- **Logo center, menu center below**: Two-row layout
- **Logo left, menu left below**: Two-row layout

### 2. Icon System

#### Icon Styles
- **Style 1**: Minimalist design
- **Style 2**: Detailed design (default)
- **Solid/Outline**: Fill variations

#### Cart Types
- **Bag**: Shopping bag icon
- **Cart**: Traditional cart icon

### 3. Visibility Controls
- Search icon toggle
- User icon toggle
- Cart always visible (e-commerce requirement)

## Components

### HeaderEditor.tsx Updates
```typescript
// Show/Hide Icons Controls
<div className="space-y-3 mb-4">
  {/* Show search icon */}
  <div className="flex items-center justify-between">
    <label className="text-xs font-medium text-gray-700">Show search icon</label>
    <button
      onClick={() => handleChange('showSearchIcon', !getValue('showSearchIcon'))}
      className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${
        getValue('showSearchIcon') !== false ? 'bg-blue-500' : 'bg-gray-300'
      }`}
    >
      <span className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform ${
        getValue('showSearchIcon') !== false ? 'translate-x-5' : 'translate-x-1'
      }`} />
    </button>
  </div>
</div>
```

### EditorPreview.tsx Icon Rendering
```typescript
// Icon rendering functions
const renderSearchIcon = () => {
  if (isStyle1) {
    return (
      <svg className="w-5 h-5" fill={isSolid ? (schemeToUse?.text || '#000000') : 'none'} 
           stroke={schemeToUse?.text || '#000000'} strokeWidth={isSolid ? "0" : "2"}>
        <circle cx="11" cy="11" r="8" />
        <path d="m21 21-4.35-4.35" strokeLinecap="round" />
      </svg>
    );
  } else {
    return (
      <svg className="w-5 h-5" fill={isSolid ? (schemeToUse?.text || '#000000') : 'none'}>
        <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
    );
  }
};

const renderCartIcon = () => {
  if (cartType === 'bag') {
    // Shopping bag icon
    return (
      <svg className="w-5 h-5">
        <path d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
      </svg>
    );
  } else {
    // Traditional cart icon
    return (
      <svg className="w-5 h-5">
        <path d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5..." />
      </svg>
    );
  }
};
```

### Layout Implementation - Menu Below
```typescript
// For layouts with menu below
if (isMenuBelow) {
  return (
    <div className={`${headerConfig?.width === 'screen' ? 'px-4' : 'container mx-auto px-4'} h-full flex flex-col justify-center`}>
      {/* Top row with logo and icons */}
      <div className="flex items-center justify-between">
        <div className={`flex items-center ${isLogoCenterMenuCenterBelow ? 'flex-1 justify-center' : ''}`}>
          {/* Logo rendering */}
        </div>
        <div className="flex items-center gap-4">
          {/* Icons */}
        </div>
      </div>
      
      {/* Bottom row with menu */}
      <div className={`${isLogoCenterMenuCenterBelow ? 'flex justify-center' : ''}`}>
        <nav className="flex gap-6">
          {/* Menu items */}
        </nav>
      </div>
    </div>
  );
}
```

### Drawer Implementation
```typescript
// Drawer with slide animation
<div className={`absolute bg-white transition-transform duration-300 ${
  drawerOpen ? 'translate-x-0' : '-translate-x-full'
}`}
style={{ 
  left: 0,
  top: headerConfig?.height || 80,
  bottom: 0,
  width: '280px',
  backgroundColor: schemeToUse?.background || '#ffffff',
  boxShadow: drawerOpen ? '2px 0 10px rgba(0,0,0,0.1)' : 'none',
  zIndex: 50,
  overflow: 'hidden'
}}>
  <div className="flex transition-transform duration-300 ease-in-out"
       style={{
         width: '200%',
         transform: activeDrawerSubmenu ? 'translateX(-50%)' : 'translateX(0)'
       }}>
    {/* Main menu panel */}
    {/* Submenu panel */}
  </div>
</div>
```

## Configuration Options

### HeaderConfig Interface
```typescript
interface HeaderConfig {
  // Layout
  layout: 'drawer' | 'logo-left-menu-center-inline' | 'logo-left-menu-left-inline' | 
          'logo-center-menu-left-inline' | 'logo-center-menu-center-below' | 
          'logo-left-menu-left-below';
  width: 'screen' | 'page' | 'large' | 'medium';
  height: number; // 60-150px
  
  // Appearance
  colorScheme: string; // "1", "2", "3", etc.
  showSeparator: boolean;
  sticky: { enabled: boolean };
  
  // Menu
  menuId: number;
  menuOpenOn: 'hover' | 'click';
  
  // Logo
  logo: {
    desktopUrl: string;
    mobileUrl?: string;
    height: number;
    mobileHeight?: number;
    alt: string;
  };
  
  // Icons
  iconStyle: 'style1-solid' | 'style1-outline' | 'style2-solid' | 'style2-outline';
  showSearchIcon?: boolean;
  showUserIcon?: boolean;
  cart: { style: 'bag' | 'cart' };
  
  // Advanced
  customCss?: string;
}
```

## Testing

### Test Scenarios
1. ✅ Layout switching preserves settings
2. ✅ Icon style changes apply immediately
3. ✅ Cart type toggles between bag/cart
4. ✅ Show/hide icons works correctly
5. ✅ Color scheme applies to all elements
6. ✅ Drawer animation is smooth
7. ✅ Menu dropdowns work on hover/click
8. ✅ Logo upload and sizing works
9. ✅ Mobile responsive behavior
10. ✅ Width setting affects container

### Browser Compatibility
- ✅ Chrome/Edge
- ✅ Firefox
- ✅ Safari
- ✅ Mobile browsers

## Performance Considerations

### Optimizations
1. **Debounced sliders**: 150ms delay for smooth updates
2. **Conditional rendering**: Only render visible elements
3. **CSS transitions**: Hardware-accelerated animations
4. **Memoized calculations**: Prevent unnecessary re-renders

### Image Handling
- Lazy loading for logos
- Error boundaries for failed images
- Fallback text when no image

## Known Issues & Solutions

### Issue 1: Drawer not opening with certain color schemes
**Solution**: Moved schemeToUse variable to proper scope
```typescript
// Before: Variable only in switch case
// After: Variable defined at function level
let schemeToUse: any = null;
if (section.type === SectionType.HEADER) {
  // Calculate scheme
}
```

### Issue 2: Icons not visible
**Solution**: Fixed container padding and justify-content
```typescript
// Added proper spacing
<div className={`${headerConfig?.width === 'screen' ? 'px-4' : 'container mx-auto px-4'} h-full flex items-center justify-between`}>
```

### Issue 3: Menu below layout overflow
**Solution**: Adjusted vertical centering
```typescript
// Added flex-col justify-center
<div className="h-full flex flex-col justify-center">
```

## Migration Guide

### From Old Header System
1. Map old settings to new HeaderConfig
2. Update logo URLs to new upload system
3. Convert menu IDs to new navigation system
4. Apply default values for new options

## API Integration

### Backend Endpoints
```csharp
// Upload controller for logos
[HttpPost("upload")]
public async Task<IActionResult> Upload(IFormFile file)
{
    // Validation and storage
    return Ok(new { url = $"{Request.Scheme}://{Request.Host}/uploads/logos/{fileName}" });
}
```

## Related Documentation

### See Also
- [Image Upload System](/docs/implementations/features/2025-01-shopify-image-system.md)
- [Color Schemes Implementation](/docs/implementations/features/color-schemes.md)
- [Navigation Menus](/docs/implementations/features/navigation-menus.md)

### Troubleshooting
- [Image Upload Issues](/docs/troubleshooting/features/features-01-image-upload-issues.md)

## Search Keywords
header, configuration, layout, drawer, icons, logo, menu, navigation, cart, bag, search, user, style, solid, outline, responsive, mobile, sticky, separator, color scheme, theme

## Notes
- Default values ensure backward compatibility
- Icon visibility defaults to true for better UX
- Cart icon always visible (business requirement)
- Drawer layout auto-sets width to screen
- All layouts support dropdown menus