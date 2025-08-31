# i18n System - Hydration and Component Issues

**Navigation**: [🏠 Home](../../00-troubleshooting-index.md) > [📋 Features](features-00-index.md) > i18n Hydration Issues

## Problem Summary

**Affects**: React components using i18n translations
**Frequency**: Durante implementación inicial 
**Severity**: 🟡 MEDIUM - Bloquea renderizado correcto

### Quick Facts
- **Error Type**: Hydration mismatch errors
- **Components**: StatsGrid, WeeklySales, ActivityTimeline
- **Root Cause**: Server/client rendering mismatch con Context

## Symptoms

### ✅ Checklist - Do you have these symptoms?
- [ ] Console error: "Hydration failed because the initial UI does not match"
- [ ] Components showing English initially, then Spanish
- [ ] Blank components on initial load
- [ ] React warnings about useContext in server components
- [ ] Translations not loading on first render

### Exact Error Messages
```
Warning: Text content did not match. Server: "Dashboard" Client: "Panel de Control"
Error: Hydration failed because the initial UI does not match what was rendered on the server.
```

## Root Causes

### 1. Server-Side Rendering Mismatch
**Verification**: Check if component uses 'use client' directive
```typescript
// ❌ WRONG - Server component trying to use Context
export function StatsGrid() {
  const { t } = useI18n(); // Error: useContext in server component
```

### 2. localStorage Access During SSR
**Verification**: Check if component accesses localStorage on initial render
```typescript
// ❌ WRONG - localStorage not available during SSR
useEffect(() => {
  const lang = localStorage.getItem('language'); // Error during hydration
}, []);
```

### 3. Dynamic Translation Loading
**Verification**: Check if translations loaded before component render
```typescript
// ❌ WRONG - Component renders before translations load
const { t } = useI18n();
return <h1>{t('title')}</h1>; // Shows fallback initially
```

## Solutions

### 🚀 Quick Fix (< 5 minutes)

#### Step 1: Add 'use client' directive
```typescript
'use client'; // Add this at the top

import React from 'react';
import { useI18n } from '@/contexts/I18nContext';

export function MyComponent() {
  const { t } = useI18n();
  // Component code...
}
```

#### Step 2: Add mounted check
```typescript
export function MyComponent() {
  const [mounted, setMounted] = useState(false);
  const { t } = useI18n();

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null; // Prevent hydration mismatch

  return <div>{t('content')}</div>;
}
```

### 🔧 Step-by-Step Solution

#### Step 1: Identify Problem Components
```bash
# Search for components using i18n without 'use client'
grep -r "useI18n" src/components --include="*.tsx" -A 1 -B 1
```

#### Step 2: Convert to Client Components
```typescript
// Before
import React from 'react';

// After  
'use client';
import React from 'react';
```

#### Step 3: Implement Proper Loading State
```typescript
export function MyComponent() {
  const [mounted, setMounted] = useState(false);
  const { t } = useI18n();

  useEffect(() => {
    setMounted(true);
  }, []);

  // Prevent hydration mismatch
  if (!mounted) {
    return <div>Loading...</div>; // Or skeleton
  }

  return (
    <div>
      <h1>{t('title')}</h1>
    </div>
  );
}
```

#### Step 4: Handle Translation Loading
```typescript
// In I18nContext.tsx
export function I18nProvider({ children }) {
  const [isLoaded, setIsLoaded] = useState(false);
  
  useEffect(() => {
    loadTranslations().then(() => setIsLoaded(true));
  }, []);

  const t = (key, defaultValue) => {
    if (!isLoaded) return defaultValue || key;
    // Translation logic...
  };
}
```

### 🔄 Alternative Solutions

#### Option A: Suspense Boundary
```typescript
<Suspense fallback={<ComponentSkeleton />}>
  <TranslatedComponent />
</Suspense>
```

#### Option B: Dynamic Import
```typescript
const TranslatedComponent = dynamic(() => import('./TranslatedComponent'), {
  ssr: false,
  loading: () => <ComponentSkeleton />
});
```

## Prevention

### Best Practices
1. **Always use 'use client'** for components with useI18n
2. **Implement mounted checks** for client-only logic
3. **Provide fallbacks** during translation loading
4. **Test SSR/hydration** in development

### Configuration Templates

#### Component Template
```typescript
'use client';

import React, { useState, useEffect } from 'react';
import { useI18n } from '@/contexts/I18nContext';

export function MyComponent() {
  const [mounted, setMounted] = useState(false);
  const { t } = useI18n();

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  return <div>{t('content')}</div>;
}
```

#### Testing Checklist
```bash
# 1. Check hydration warnings
npm run dev # Look for warnings in console

# 2. Test with JS disabled
# In browser DevTools -> Settings -> Debugger -> Disable JavaScript

# 3. Check Network tab for SSR/CSR differences
```

## Related Issues

### See Also
- [Next.js SSR Issues](../../frontend/frontend-02-nextjs-ssr-problems.md)
- [React Context Hydration](../../frontend/frontend-03-context-hydration.md)
- [i18n System Implementation](../../implementations/features/2025-08-i18n-system.md)

### Cross-References
- **Similar Problem**: [Theme Switching Hydration](../ui/ui-01-theme-hydration.md)
- **Related Fix**: [Dynamic Loading Issues](../../general/general-03-dynamic-imports.md)

## Search Keywords
- i18n hydration mismatch
- useContext server component error
- Next.js translation hydration
- React i18n SSR issues
- client component translation loading
- localStorage hydration Next.js