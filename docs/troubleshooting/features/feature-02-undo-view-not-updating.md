# Undo Doesn't Update Configuration View

## Problem ID
`feature-02-undo-view-not-updating`

## Symptoms
- After clicking undo, the editor preview reverts correctly
- Configuration panel (HeaderEditor) doesn't update to show reverted values
- Select dropdowns and inputs show old values despite state being correct

## Error Messages
No error messages - visual synchronization issue

## Root Cause
Components with local state (`localConfig` in HeaderEditor, `settings` in ConfigPanel) were not detecting external state changes from undo operations. The useEffect dependencies weren't triggering updates.

## Solution

### Step 1: Force Update HeaderEditor on Value Change
```typescript
// In HeaderEditor.tsx
useEffect(() => {
  // Force update local config when value changes (important for undo)
  setLocalConfig(value || defaultHeaderConfig);
}, [value, JSON.stringify(value)]); // Add JSON.stringify for deep comparison
```

### Step 2: Update ConfigPanel Dependencies
```typescript
// In ConfigPanel.tsx
useEffect(() => {
  setSettings(section.settings);
  setHasChanges(false);
}, [section, JSON.stringify(section.settings)]); // Force update when settings change
```

### Step 3: Use Correct Value Source
```typescript
// In ConfigPanel.tsx - Use settings instead of headerConfig || settings
case SectionType.HEADER:
  return (
    <HeaderEditor
      value={settings as HeaderConfig} // Use settings directly
      onChange={(newConfig) => {
        // ... onChange logic
      }}
    />
  );
```

## Debugging Steps
1. Add logs to track state updates:
```typescript
console.log('[DEBUG] HeaderEditor value changed:', value);
```

2. Verify local state updates:
```typescript
console.log('[DEBUG] localConfig updated to:', localConfig);
```

3. Check if useEffect is triggering on undo

## Prevention
- Avoid unnecessary local state in controlled components
- Use proper dependency arrays in useEffect
- Consider using keys to force re-mount when needed
- Test undo/redo with all form inputs

## Related Issues
- [Undo System Implementation](/docs/implementations/features/2025-01-undo-system.md)
- [Save Button Persistent Issue](/docs/troubleshooting/features/feature-01-save-button-persistent.md)

## Search Keywords
undo, view update, configuration panel, HeaderEditor, local state, useEffect, synchronization