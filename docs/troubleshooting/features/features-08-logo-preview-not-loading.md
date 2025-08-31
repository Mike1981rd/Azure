# Logo Preview Not Loading After Page Refresh

[← Back to Features Index](./features-00-index.md) | [← Back to Main Index](../00-troubleshooting-index.md)

## Problem Summary
- **Affects**: Logo display in company form
- **Frequency**: Always after page reload
- **Severity**: Medium - UX issue
- **First Observed**: 2025-08-06

## Symptoms
- [ ] Logo exists in database but doesn't show in form
- [ ] Preview area shows empty state after page refresh
- [ ] Logo URL is correct in network response
- [ ] Console shows logo path but image not displayed
- [ ] Works fine immediately after upload, fails on reload

## Root Causes

### 1. React State Not Updating with Props
**Issue**: Component state initialized once, not synced with props
```typescript
// PROBLEMA: Estado solo se inicializa una vez
const [preview, setPreview] = useState<string | null>(currentLogo || null);
// Si currentLogo es null al inicio y luego cambia, preview no se actualiza
```

**Timeline**:
1. Component mounts with `currentLogo = null`
2. API call completes with `currentLogo = "/uploads/logo.png"`
3. Preview state remains `null`

### 2. Async Data Loading Pattern
**Issue**: Parent component loads data after child initializes
```typescript
// Parent loads company data async
const { company } = useCompany(); // null → data

// Child already initialized with null
<LogoUploader currentLogo={company?.logo} />
```

## Solutions

### Quick Fix (< 5 min)
Hard refresh the page (Ctrl+F5) - temporary workaround

### Step-by-Step Solution

1. **Add useEffect to sync state with props**:
```typescript
// LogoUploader.tsx
useEffect(() => {
  if (currentLogo) {
    setPreview(currentLogo);
  }
}, [currentLogo]);

useEffect(() => {
  setLogoSize(currentSize);
}, [currentSize]);
```

2. **Import useEffect**:
```typescript
import { useState, useCallback, useRef, useEffect } from 'react';
```

3. **Ensure proper URL formatting**:
```typescript
const displayPreview = preview 
  ? (preview.startsWith('data:') ? preview : formatFileUrl(preview)) 
  : null;
```

4. **Debug with console logs**:
```typescript
useEffect(() => {
  console.log('currentLogo changed:', currentLogo);
  if (currentLogo) {
    setPreview(currentLogo);
  }
}, [currentLogo]);
```

### Alternative Solutions
1. Lift preview state to parent component
2. Use derived state instead of useState
3. Force re-mount with key prop

## Prevention

### Best Practices
1. **Always sync props with state using useEffect**
2. **Handle async data loading in components**
3. **Use proper initial states**
4. **Test with delayed data loading**

### Component Pattern
```typescript
// Pattern for props → state synchronization
function Component({ propValue }) {
  const [stateValue, setStateValue] = useState(propValue);
  
  // Sync when prop changes
  useEffect(() => {
    setStateValue(propValue);
  }, [propValue]);
  
  return <div>{stateValue}</div>;
}
```

### Testing Checklist
- [ ] Component works with initial null/undefined props
- [ ] Component updates when props change
- [ ] Component handles rapid prop changes
- [ ] Component works with direct navigation

## Related Issues
- [React Hydration Issues](./features-01-i18n-hydration-issues.md)
- [State Management Patterns](./features-03-calendar-state-management.md)

## Search Keywords
- logo preview not showing
- react state not updating with props
- useEffect props synchronization
- image not loading after refresh
- preview empty after reload