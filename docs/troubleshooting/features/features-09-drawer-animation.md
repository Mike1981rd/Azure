# Drawer Animation and Layout Issues

## Problem
The drawer menu in the live preview was opening from the wrong side (right instead of left) and the submenu animation was sliding in the wrong direction, creating a confusing user experience.

**Category**: features  
**Severity**: High  
**Date Encountered**: 2025-01-14  
**Status**: ✅ Resolved  

## Symptoms
- Drawer opened from right side instead of left
- Submenu slid right instead of left when opened
- Layout showed incorrect icon positions in drawer mode
- Animation felt backwards and unnatural
- Hamburger menu was on wrong side

## Error Messages
No error messages - visual/UX issue only

## Root Cause
Multiple issues in PreviewHeader.tsx:
1. Drawer was using `translate-x-full` with `right: 0` positioning
2. Submenu transform was positive instead of negative
3. Layout structure didn't match EditorPreview.tsx

## Solution

### Step 1: Fix Drawer Opening Direction
```typescript
// BEFORE - Opening from right
<div className={`fixed top-0 right-0 h-full w-80 transform transition-transform ${
  drawerOpen ? 'translate-x-0' : 'translate-x-full'
}`}>

// AFTER - Opening from left
<div 
  className={`absolute bg-white transition-transform duration-300 ${
    drawerOpen ? 'translate-x-0' : '-translate-x-full'
  }`}
  style={{ 
    left: 0,  // Changed from right: 0
    top: headerConfig?.height || 80,
    bottom: 0,
    width: '280px',
    // ... other styles
  }}
>
```

### Step 2: Fix Submenu Slide Direction
```typescript
// Submenu slides LEFT (negative transform)
<div 
  className="flex transition-transform duration-300 ease-in-out"
  style={{
    width: '200%',
    transform: activeDrawerSubmenu ? 'translateX(-50%)' : 'translateX(0)'
    // Was translateX(50%), now -50% for left slide
  }}
>
```

### Step 3: Fix Header Layout for Drawer Mode
```typescript
// Correct layout structure
<header>
  <div className="flex items-center justify-between">
    {/* Left: Hamburger */}
    <div className="flex items-center">
      <button onClick={() => setDrawerOpen(!drawerOpen)}>
        <svg><!-- Hamburger icon --></svg>
      </button>
    </div>

    {/* Center: Logo */}
    <div className="flex items-center justify-center flex-1">
      <img src={logo} />
    </div>

    {/* Right: Icons */}
    <div className="flex items-center gap-4">
      {/* Search, User, Cart icons */}
    </div>
  </div>
</header>
```

### Step 4: Add Hover Effects
```typescript
// Add hover state to drawer menu items
onMouseEnter={(e) => {
  e.currentTarget.style.backgroundColor = colorScheme?.text?.default ? 
    `${colorScheme.text.default}10` : 'rgba(0,0,0,0.05)';
}}
onMouseLeave={(e) => {
  e.currentTarget.style.backgroundColor = 'transparent';
}}
```

## Prevention
1. Always reference the working EditorPreview implementation
2. Test drawer animations in both open/close states
3. Verify submenu navigation flows naturally
4. Check layout matches design specs for all modes

## Testing
After fix implementation:
1. Click hamburger → Drawer slides from left edge
2. Click menu item with arrow → Content slides left to show submenu
3. Click back → Content slides right to return
4. Verify hamburger is on left, logo centered, icons on right
5. Test hover states on all menu items

## Related Issues
- [Live Preview Implementation](/docs/implementations/features/2025-01-live-preview.md)
- [Header Configuration System](/docs/implementations/features/2025-01-header-configurations.md)

## Visual Comparison

### Before Fix
```
[Logo] [🔍 👤 🛒] [☰]  ← Wrong order
         Drawer →
    ← Submenu (wrong direction)
```

### After Fix
```
[☰] [Logo] [🔍 👤 🛒]  ← Correct order
← Drawer
    Submenu →
```

## Configuration
Drawer layout detection:
```typescript
const isDrawerLayout = headerConfig?.layout === 'drawer';
```

## Search Keywords
drawer, animation, slide, transform, translateX, hamburger, menu, navigation, left, right, direction, layout

## Notes
- Drawer should always open from left in LTR languages
- Submenu animation should feel like diving deeper (sliding left)
- Back animation should feel like returning (sliding right)
- Transform percentages are relative to element width
- Use `absolute` positioning for drawers to avoid layout shifts