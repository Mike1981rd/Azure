# Variable Scope Issues in React Components

## Problem Description
ReferenceError when variables defined in one scope are accessed in another scope.

**Error Message**:
```
ReferenceError: menuTypographyStyles is not defined
```

**Category**: features  
**Severity**: Medium  
**Frequency**: Common  
**Created**: 2025-01-13  

## Symptoms
- Variable undefined errors at runtime
- Works in one part of component, fails in another
- Drawer/modal components often affected
- Error only appears when specific UI elements render

## Root Cause
Variable defined inside a function or block scope but accessed outside that scope.

### Example Scenario
```typescript
function renderSectionPreview(section) {
  const menuTypographyStyles = {...}; // Defined here
  // Works fine here
}

// Later in component...
return (
  <Drawer>
    <span style={menuTypographyStyles}> // Error: not defined
  </Drawer>
);
```

## Solution

### Quick Fix
Move variable definition to the appropriate scope:

```typescript
// Before: Variable in wrong scope
if (isDrawerLayout) {
  // menuTypographyStyles defined in renderSectionPreview
  return (
    <Drawer>
      <span style={menuTypographyStyles}> // Error
    </Drawer>
  );
}

// After: Variable in correct scope
if (isDrawerLayout) {
  const menuTypographyStyles = {...}; // Define here
  return (
    <Drawer>
      <span style={menuTypographyStyles}> // Works
    </Drawer>
  );
}
```

### Complete Solution Pattern
```typescript
export function EditorPreview() {
  // Option 1: Define at component level for global access
  const globalStyles = useMemo(() => {...}, [deps]);
  
  const renderSection = (section) => {
    // Option 2: Define in function scope
    const localStyles = {...};
    return <div style={localStyles} />;
  };
  
  if (specialCase) {
    // Option 3: Define in conditional scope
    const conditionalStyles = {...};
    return <div style={conditionalStyles} />;
  }
}
```

## Prevention

### Best Practices

1. **Identify scope requirements early**
   - Will multiple components need this variable?
   - Is it only used in one conditional branch?
   - Does it depend on local state?

2. **Use appropriate scope level**
   ```typescript
   // Component level - accessible everywhere
   const componentVar = value;
   
   // Function level - only in function
   const functionVar = value;
   
   // Block level - only in block
   if (condition) {
     const blockVar = value;
   }
   ```

3. **Consider memoization for expensive calculations**
   ```typescript
   const memoizedStyles = useMemo(() => {
     return calculateStyles(config);
   }, [config]);
   ```

## Common Patterns

### Pattern 1: Shared Styles Across Renders
```typescript
function Component() {
  // Define once, use everywhere
  const sharedStyles = {
    font: 'Arial',
    size: 14
  };
  
  return (
    <>
      <Header style={sharedStyles} />
      <Body style={sharedStyles} />
      <Footer style={sharedStyles} />
    </>
  );
}
```

### Pattern 2: Conditional Style Generation
```typescript
function Component({ variant }) {
  // Generate styles based on props
  const styles = useMemo(() => {
    switch(variant) {
      case 'primary': return { color: 'blue' };
      case 'secondary': return { color: 'gray' };
      default: return { color: 'black' };
    }
  }, [variant]);
  
  return <div style={styles} />;
}
```

### Pattern 3: Drawer/Modal with Parent Styles
```typescript
function Component() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  
  // Define styles that drawer needs
  const drawerStyles = {
    background: 'white',
    padding: 20
  };
  
  return (
    <>
      <Button onClick={() => setDrawerOpen(true)} />
      {drawerOpen && (
        <Drawer style={drawerStyles} /> // Has access
      )}
    </>
  );
}
```

## Debugging Tips

### Find Variable Declaration
```javascript
// In browser console
console.trace(); // Shows call stack
```

### Check Scope
```typescript
function debugScope() {
  console.log('Outer scope');
  
  if (true) {
    console.log('Inner scope');
    const innerVar = 'test';
  }
  
  // console.log(innerVar); // Would error
}
```

### Use Debugger
```typescript
if (isDrawerLayout) {
  debugger; // Pause here
  // Check available variables in console
}
```

## Related Issues
- Closure issues in React hooks
- Stale closures in event handlers
- Memory leaks from captured variables
- Performance issues from recreating objects

## Search Keywords
ReferenceError, not defined, scope, closure, variable scope, React scope, drawer, modal, conditional rendering, block scope, function scope

## See Also
- [Typography Header Implementation](/docs/implementations/features/2025-01-typography-header.md)
- [MDN: Block Scope](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/block)
- [React: Rules of Hooks](https://react.dev/rules-of-hooks)