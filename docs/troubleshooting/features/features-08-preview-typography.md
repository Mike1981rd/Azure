# Preview Typography Not Applying

## Problem
Typography configuration from the theme editor was not being applied to menu items in the live preview, despite being correctly saved in the database.

**Category**: features  
**Severity**: Medium  
**Date Encountered**: 2025-01-14  
**Status**: ✅ Resolved  

## Symptoms
- Menu items in preview showed default browser font
- Font size, weight, and text transform not applied
- Letter spacing configuration ignored
- Editor preview showed correct typography but live preview did not

## Error Messages
No error messages - silent failure of style application

## Root Cause
The PreviewHeader component was not extracting and applying typography styles from the theme configuration passed as props.

## Solution

### Step 1: Extract Typography from Theme
```typescript
// Add typography extraction
const menuTypographyStyles = theme?.typography?.menu ? {
  fontFamily: `'${theme.typography.menu.fontFamily}', sans-serif`,
  fontWeight: theme.typography.menu.fontWeight || '400',
  textTransform: theme.typography.menu.useUppercase ? 'uppercase' as const : 'none' as const,
  fontSize: theme.typography.menu.fontSize ? 
    (theme.typography.menu.fontSize <= 100 ? 
      `${theme.typography.menu.fontSize}%` : 
      `${theme.typography.menu.fontSize}px`) : '94%',
  letterSpacing: `${theme.typography.menu.letterSpacing || 0}px`
} : {};
```

### Step 2: Apply to All Menu Elements
```typescript
// In menu item render
<span style={menuTypographyStyles}>{item.label}</span>

// In dropdown items
<Link
  href={child.url || '#'}
  className="block px-4 py-2 hover:bg-gray-50"
  style={{ ...menuTypographyStyles, color: colorScheme?.text?.default || '#000000' }}
>
  {child.label}
</Link>
```

### Step 3: Apply to Drawer Menu
```typescript
// Drawer menu items
<span style={menuTypographyStyles}>{item.label}</span>

// Drawer submenu items
<a
  href={subItem.url || '#'}
  style={{ 
    color: colorScheme?.text?.default || '#000000',
    ...menuTypographyStyles
  }}
>
  {subItem.label}
</a>
```

## Prevention
1. Always check that theme props contain expected nested properties
2. Create a central style extraction function for consistency
3. Apply styles to all text elements, not just top-level ones
4. Test preview with various typography configurations

## Testing
After applying the fix:
1. Change font family in editor → Save → Refresh preview
2. Toggle uppercase setting → Verify in preview
3. Adjust font size → Check all menu items update
4. Modify letter spacing → Confirm spacing changes

## Related Issues
- [Live Preview Implementation](/docs/implementations/features/2025-01-live-preview.md)
- Similar issue might affect other theme properties (buttons, headings)

## Configuration
Ensure theme is passed correctly to PreviewHeader:
```typescript
<PreviewHeader 
  config={structuralComponents.header} 
  theme={globalTheme}  // Must include typography property
/>
```

## Search Keywords
typography, preview, font, menu, style, theme, text transform, font family, font size, letter spacing

## Notes
- Typography is stored in JSONB column as nested object
- Font families must be wrapped in quotes for CSS
- Font size can be percentage (≤100) or pixels (>100)
- Text transform uses TypeScript const assertion for type safety