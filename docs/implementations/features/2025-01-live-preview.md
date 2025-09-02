# Live Preview Implementation - Refactored Architecture

## Overview
Complete refactor of the Website Builder Live Preview system to eliminate code duplication and ensure consistency between editor and live preview using shared components.

**Created**: 2025-01-14  
**Refactored**: 2025-01-14  
**Category**: features  
**Status**: ✅ Complete with unified components  
**Time Spent**: 5 hours (3 initial + 2 refactor)  

## Table of Contents
- [Architecture](#architecture)
- [Refactor Strategy](#refactor-strategy)
- [Implementation Details](#implementation-details)
- [Components](#components)
- [Benefits](#benefits)
- [Usage Pattern](#usage-pattern)
- [Migration Guide](#migration-guide)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

## Architecture

### Previous Architecture (Duplicated)
```
EditorPreview.tsx (1,700+ lines)
├── Header rendering code
├── AnnouncementBar rendering code
└── Footer rendering code

PreviewPage.tsx
├── PreviewHeader.tsx (separate implementation)
├── PreviewAnnouncementBar.tsx (separate implementation)
└── PreviewFooter.tsx (separate implementation)

Result: DUPLICATED CODE, INCONSISTENT BEHAVIOR
```

### New Architecture (Unified)
```
Shared Preview Components:
├── PreviewHeader.tsx (single source of truth)
├── PreviewAnnouncementBar.tsx (single source of truth)
└── PreviewFooter.tsx (single source of truth)

Used by:
├── EditorPreview.tsx (imports components)
│   └── <PreviewHeader isEditor={true} />
└── PreviewPage.tsx (imports same components)
    └── <PreviewHeader isEditor={false} />

Result: NO DUPLICATION, GUARANTEED CONSISTENCY
```

## Refactor Strategy

### Key Principles
1. **Single Source of Truth**: One component handles both editor and live preview
2. **Props-based Context**: `isEditor` prop differentiates behavior when needed
3. **Shared State Management**: Same Zustand stores for both contexts
4. **Responsive by Design**: Mobile detection works in both contexts

### Component Structure
```typescript
interface PreviewComponentProps {
  config: any;           // Component configuration
  theme: any;           // Theme configuration
  deviceView?: 'desktop' | 'mobile';  // Device view
  isEditor?: boolean;   // True when in editor context
}
```

## Implementation Details

### 1. PreviewAnnouncementBar.tsx
```typescript
export default function PreviewAnnouncementBar({ 
  config, 
  theme, 
  pageType, 
  deviceView,
  isEditor = false 
}: PreviewAnnouncementBarProps) {
  // Mobile detection
  const [isMobile, setIsMobile] = useState(() => {
    if (deviceView !== undefined) return deviceView === 'mobile';
    if (typeof window !== 'undefined') return window.innerWidth < 768;
    return false;
  });

  // ALL hooks MUST come before conditional returns
  useEffect(() => {
    // Mobile detection logic
  }, [deviceView]);

  // Conditional returns AFTER all hooks
  if (!config?.enabled) return null;

  // Render logic
  return (
    <div className={isMobile ? 'mobile-styles' : 'desktop-styles'}>
      {/* Component content */}
    </div>
  );
}
```

### 2. PreviewHeader.tsx
```typescript
export default function PreviewHeader({ 
  config, 
  theme, 
  deviceView, 
  isEditor = false 
}: PreviewHeaderProps) {
  // Unified rendering logic for both contexts
  // Mobile-specific adjustments
  const iconClass = isMobile ? "w-4 h-4" : "w-5 h-5";
  
  // Perfect centering on mobile
  return (
    <header>
      {/* Logo absolutely positioned for centering */}
      <div className="absolute left-1/2 transform -translate-x-1/2">
        {/* Logo */}
      </div>
    </header>
  );
}
```

### 3. EditorPreview.tsx Integration
```typescript
// Before: 1,700+ lines with duplicated code
// After: Clean imports and usage

import PreviewHeader from '@/components/preview/PreviewHeader';
import PreviewAnnouncementBar from '@/components/preview/PreviewAnnouncementBar';

// Inside render switch
case SectionType.HEADER:
  return (
    <PreviewHeader
      config={headerConfig || structuralComponents?.header}
      theme={themeConfig}
      deviceView={deviceView}
      isEditor={true}
    />
  );

case SectionType.ANNOUNCEMENT_BAR:
  return (
    <PreviewAnnouncementBar
      config={announcementConfig}
      theme={themeConfig}
      pageType={selectedPageType}
      deviceView={deviceView}
      isEditor={true}
    />
  );
```

## Components

### Shared Preview Components
| Component | Location | Lines | Purpose |
|-----------|----------|-------|---------|
| PreviewHeader.tsx | `/src/components/preview/` | ~630 | Renders header in both contexts |
| PreviewAnnouncementBar.tsx | `/src/components/preview/` | ~400 | Renders announcement bar in both contexts |
| PreviewFooter.tsx | `/src/components/preview/` | TBD | Renders footer in both contexts |

### Integration Points
| File | Role | Changes |
|------|------|---------|
| EditorPreview.tsx | Editor preview | Imports and uses shared components |
| PreviewPage.tsx | Live preview | Imports same shared components |

## Benefits

### Development Benefits
1. **Write Once, Use Everywhere**: Single implementation for both contexts
2. **Consistency Guaranteed**: Same component = same behavior
3. **Easier Maintenance**: Fix bugs in one place
4. **Faster Development**: No need to implement features twice
5. **Reduced File Size**: EditorPreview.tsx reduced significantly

### Performance Benefits
1. **Less Code to Load**: No duplicate implementations
2. **Better Caching**: Same component cached once
3. **Smaller Bundle**: Shared code is bundled once

### Quality Benefits
1. **No Divergence**: Editor and preview always match
2. **Single Testing Point**: Test component once
3. **Predictable Behavior**: Same logic everywhere

## Usage Pattern

### Adding New Modules
```typescript
// 1. Create preview component
// src/components/preview/PreviewNewModule.tsx
export default function PreviewNewModule({ config, theme, isEditor }) {
  // Implementation
  return <div>...</div>;
}

// 2. Use in EditorPreview.tsx
case SectionType.NEW_MODULE:
  return (
    <PreviewNewModule
      config={config}
      theme={themeConfig}
      isEditor={true}
    />
  );

// 3. Use in PreviewPage.tsx
<PreviewNewModule
  config={structuralComponents.newModule}
  theme={globalTheme}
  isEditor={false}
/>
```

## Migration Guide

### For Existing Modules
1. Extract rendering logic from EditorPreview.tsx
2. Create new Preview component in `/src/components/preview/`
3. Add `isEditor` prop support
4. Replace duplicated code with component import
5. Test in both contexts

### Hook Rules (Critical)
```typescript
// ❌ WRONG - Conditional return before hooks
function Component() {
  if (!enabled) return null; // ERROR!
  useEffect(() => {});
}

// ✅ CORRECT - All hooks before conditionals
function Component() {
  useEffect(() => {});
  if (!enabled) return null; // OK
}
```

## Testing

### Test Scenarios
1. ✅ Component renders identically in both contexts
2. ✅ Mobile view consistent between editor and real device
3. ✅ Props correctly passed in both contexts
4. ✅ State management works in both contexts
5. ✅ Responsive behavior matches

### Mobile Consistency Checks
- Logo centering: `absolute left-1/2 transform -translate-x-1/2`
- Icon sizing: `isMobile ? "w-4 h-4" : "w-5 h-5"`
- Spacing adjustments: `isMobile ? "gap-0.5" : "gap-4"`
- Element visibility: `{!isMobile && <DesktopElement />}`

## Troubleshooting

### Common Issues

#### Hooks Error: "Rendered more hooks than during the previous render"
**Cause**: Conditional returns before hooks
**Solution**: Move all hooks before any conditional returns

#### Mobile View Inconsistency
**Cause**: Different detection methods or missing deviceView prop
**Solution**: Use unified mobile detection with deviceView prop

### Device View Contract (Critical)

To guarantee identical behavior between the Editor and the Live Preview, all preview components and their containers MUST follow this contract:

1. Containers (e.g., PreviewContent, PreviewPage):
   - Pass `deviceView` through as-is. Do NOT coalesce to desktop.
   - Correct: `deviceView={deviceView}`
   - Incorrect: `deviceView={deviceView || 'desktop'}`

2. Components (all preview modules):
   - Implement the canonical mobile detection pattern below.
   - If `deviceView` is provided, honor it; otherwise detect via viewport and listen to resize.

3. Editor Mobile Override:
   - The Editor may set `localStorage.editorDeviceView = 'mobile'`. Containers should not override this; components will still behave correctly using the pattern below.

### Canonical Mobile Detection (Copy/Paste for all preview components)

```tsx
// Canonical mobile detection (use in all preview components)
const [isMobile, setIsMobile] = useState<boolean>(() => {
  if (deviceView !== undefined) return deviceView === 'mobile';
  if (typeof window !== 'undefined') return window.innerWidth < 768;
  return false;
});

useEffect(() => {
  if (deviceView !== undefined) {
    setIsMobile(deviceView === 'mobile');
    return;
  }
  const onResize = () => setIsMobile(window.innerWidth < 768);
  onResize();
  window.addEventListener('resize', onResize);
  return () => window.removeEventListener('resize', onResize);
}, [deviceView]);
```

### Container Usage Guidelines

- In `PreviewContent.tsx` and `PreviewPage.tsx`, always pass `deviceView` without forcing desktop:

```tsx
// Good
<PreviewSomeComponent deviceView={deviceView} />

// Bad
<PreviewSomeComponent deviceView={deviceView || 'desktop'} />
```

### PR Checklist for New/Updated Preview Modules

- [ ] Container passes `deviceView` as-is (no `|| 'desktop'`)
- [ ] Component uses the Canonical Mobile Detection pattern
- [ ] Manual test: Editor mobile vs. Live Preview mobile look identical
- [ ] No conditional returns before hooks
- [ ] Resize listener added when `deviceView` is undefined

### Anti-patterns to Avoid

- `const isMobile = deviceView === 'mobile'` without viewport fallback and resize listener
- Forcing desktop in containers with `deviceView || 'desktop'`
- Divergent logic between Editor and Live Preview components

### Optional: Shared Hook (Future Improvement)

Centralize the detection to a hook to prevent drift:

```tsx
// useDeviceView.ts
export function useDeviceView(deviceView?: 'desktop'|'mobile') {
  const [isMobile, setIsMobile] = useState(false);
  useEffect(() => {
    if (deviceView !== undefined) {
      setIsMobile(deviceView === 'mobile');
      return;
    }
    const onResize = () => setIsMobile(window.innerWidth < 768);
    onResize();
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, [deviceView]);
  return isMobile;
}
```

### Gotchas: deviceView defaults and container coalescing

- Avoid forcing desktop by default in preview components.
  - Incorrect: setting a default `deviceView = 'desktop'` in the component props causes the component to ignore `undefined` (intended to mean “use viewport”).
  - Correct: accept `deviceView?: 'desktop' | 'mobile'` with no default and use the canonical detection.

- Do not coalesce `deviceView` in containers.
  - Incorrect: `deviceView={deviceView || 'desktop'}`
  - Correct: `deviceView={deviceView}` (pass through as-is)

Code examples

Incorrect (breaks mobile in real preview):
```tsx
// Anti-pattern: default prop forces desktop
export default function PreviewFeaturedCollection({ deviceView = 'desktop' }: { deviceView?: 'desktop'|'mobile' }) {
  // ...
}
```

Correct (respects editor override and real viewport):
```tsx
export default function PreviewFeaturedCollection({ deviceView }: { deviceView?: 'desktop'|'mobile' }) {
  const [isMobile, setIsMobile] = useState<boolean>(() => {
    if (deviceView !== undefined) return deviceView === 'mobile';
    if (typeof window !== 'undefined') return window.innerWidth < 768;
    return false;
  });
  useEffect(() => {
    if (deviceView !== undefined) {
      setIsMobile(deviceView === 'mobile');
      return;
    }
    const onResize = () => setIsMobile(window.innerWidth < 768);
    onResize();
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, [deviceView]);
  // ...
}
```

### QA Checklist (Mobile Consistency)

- Verify Image Banner, Image With Text, Slideshow, and Multicolumns in both Editor and Live Preview
- Toggle Editor mobile override and confirm parity with Live Preview
- Resize browser in Live Preview to confirm responsive updates

#### Components Not Updating
**Cause**: Missing dependencies in useEffect
**Solution**: Include all reactive values in dependency array

#### Styles Not Applying
**Cause**: CSS class conflicts or specificity issues
**Solution**: Use inline styles for dynamic values, Tailwind for static

#### Scroll Not Working in Preview
**Cause**: Global `overflow-hidden` on body element in `globals.css` prevents scrolling
**Context**: Dashboard needs `overflow-hidden` for its fixed layout, but preview needs scroll
**Wrong Solutions to AVOID**:
- ❌ Modifying global CSS rules (affects dashboard)
- ❌ Adding conditional classes with `:has()` (can break other components)
- ❌ Using `overflow-y-auto` on container (can affect child layouts like announcement bar)

**Correct Solution**: Override specifically in PreviewPage component
```typescript
// In PreviewPage.tsx return statement
<div className="min-h-screen" style={{...themeStyles, overflowY: 'auto', height: '100vh'}}>
```
**Why this works**: 
- Inline styles have highest specificity
- Only affects preview page, not dashboard
- Doesn't interfere with child component layouts
- `height: '100vh'` ensures container has defined height for scroll

### Debug Helpers
```typescript
// Add to components for debugging
console.log('Component context:', {
  isEditor,
  deviceView,
  isMobile,
  windowWidth: typeof window !== 'undefined' ? window.innerWidth : 'SSR'
});
```

## Performance Considerations

### Optimizations
1. **Lazy Loading**: Components load only when needed
2. **Memoization**: Use React.memo for expensive renders
3. **Conditional Imports**: Dynamic imports for large components

### Future Improvements
- Add React.memo to preview components
- Implement virtual scrolling for long sections
- Add skeleton loaders during transitions
- Cache component configurations

## API Integration

### Data Flow
```
Editor Mode:
Zustand Store → Preview Component → Real-time Update

Live Preview Mode:
Database → API → Preview Component → Static Render
```

### Endpoints Used
Same as before, but now consumed by shared components:
- GET `/api/structural-components/company/{companyId}/published`
- GET `/api/global-theme-config/company/{companyId}/published`
- GET `/api/NavigationMenu/{menuId}/public`

## Production Deployment

### Build Optimization
```typescript
// Production builds will tree-shake unused isEditor code
if (isEditor) {
  // This code is removed in production preview
}
```

### Performance Impact
- **Development**: Full component with editor features
- **Production**: Optimized component without editor code
- **Result**: No performance impact from refactor

## Related Documentation
- [Website Builder Architecture](/docs/WEBSITE-BUILDER-ARCHITECTURE.md)
- [Component Development Guide](/docs/component-development.md)
- [Mobile Responsive Design](/docs/mobile-responsive.md)

## Search Keywords
preview, refactor, unified components, code duplication, live preview, editor preview, shared components, isEditor, mobile consistency, hooks order, single source of truth

## Notes
- Always test new components in both editor and live preview
- Mobile detection must work with and without deviceView prop
- Hook order is critical - all hooks before conditionals
- Use isEditor prop sparingly, only when behavior must differ