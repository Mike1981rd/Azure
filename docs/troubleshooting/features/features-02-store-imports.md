# Store Import Issues - Default vs Named Exports

## Problem Description
TypeError when importing Zustand stores using incorrect import syntax.

**Error Message**:
```
TypeError: (0 , _stores_useThemeConfigStore__WEBPACK_IMPORTED_MODULE_7__.useThemeConfigStore) is not a function
```

**Category**: features  
**Severity**: High  
**Frequency**: Common  
**Created**: 2025-01-13  

## Symptoms
- Store hooks throw "is not a function" error
- Component fails to render
- Build succeeds but runtime fails
- Error appears in browser console

## Root Cause
Mismatch between export type (default) and import syntax (named).

### Export in Store
```typescript
// src/stores/useThemeConfigStore.ts
export default useThemeConfigStore; // Default export
```

### Incorrect Import
```typescript
// Using named import syntax for default export
import { useThemeConfigStore } from '@/stores/useThemeConfigStore';
```

## Solution

### Quick Fix
Change from named import to default import:
```typescript
// Correct import for default export
import useThemeConfigStore from '@/stores/useThemeConfigStore';
```

### Alternative Solutions

#### Option 1: Change to Named Export
```typescript
// In store file
export const useThemeConfigStore = create<ThemeConfigState>(...)

// In component
import { useThemeConfigStore } from '@/stores/useThemeConfigStore';
```

#### Option 2: Use Both Export Types
```typescript
// In store file
const useThemeConfigStore = create<ThemeConfigState>(...)
export default useThemeConfigStore;
export { useThemeConfigStore }; // Also export as named
```

## Prevention

### Coding Standards
1. **Be consistent**: Choose either default or named exports project-wide
2. **Document choice**: Add to project README or CONTRIBUTING.md
3. **Use TypeScript**: Will catch some import errors at compile time
4. **ESLint rule**: Configure import/no-default-export if using named only

### Team Guidelines
```typescript
// PROJECT STANDARD: Zustand stores use default exports
// ✅ Correct
import useStore from '@/stores/useStore';

// ❌ Incorrect
import { useStore } from '@/stores/useStore';
```

## Common Patterns

### Check Export Type
```bash
# Find export statement in store
grep "export" src/stores/useThemeConfigStore.ts
```

### Quick Reference
| Export Type | Import Syntax |
|------------|---------------|
| `export default X` | `import X from 'module'` |
| `export const X` | `import { X } from 'module'` |
| `export { X }` | `import { X } from 'module'` |
| Both | Either syntax works |

## Related Issues
- Module resolution errors
- Webpack import failures
- Next.js dynamic import issues

## Search Keywords
TypeError, not a function, import, export, default export, named export, Zustand, store, useThemeConfigStore, WEBPACK_IMPORTED_MODULE

## See Also
- [Typography Header Implementation](/docs/implementations/features/2025-01-typography-header.md)
- [MDN: Export Statement](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/export)
- [TypeScript Module Documentation](https://www.typescriptlang.org/docs/handbook/modules.html)