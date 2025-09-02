# Save Button Remains Active After Saving

## Problem ID
`feature-01-save-button-persistent`

## Symptoms
- Save button remains visually active (dark color) after successful save
- Button is functionally disabled but appears enabled
- Occurs after changing header configuration and saving

## Error Messages
No error messages - visual issue only

## Root Cause
The save button state depends on two flags:
1. `hasStructuralChanges` from StructuralComponentsContext
2. `isDirty` from EditorStore

When saving structural components, only `hasStructuralChanges` was being reset, but `isDirty` remained true, keeping the button visually active.

## Solution

### Step 1: Reset isDirty After Save
```typescript
// In /src/app/editor/page.tsx
const handleSave = async () => {
  setIsSavingLocal(true);
  try {
    if (hasStructuralChanges) {
      const success = await publishStructural();
      if (success) {
        await refresh();
        // Clear isDirty state
        const store = useEditorStore.getState();
        store.setIsDirty(false);
      }
    }
  } catch (error) {
    console.error('Error saving changes:', error);
  } finally {
    setIsSavingLocal(false);
  }
};
```

### Step 2: Ensure hasChanges Resets in Context
```typescript
// In StructuralComponentsContext
const publish = useCallback(async () => {
  // ... save logic ...
  
  // Reset the hasChanges state after successful save
  setHasChanges(false);
  
  toast.success('Changes saved successfully');
  return true;
  
}, [company?.id, config]);
```

## Debugging Steps
1. Add console.log to monitor both states:
```typescript
useEffect(() => {
  console.log('[DEBUG] hasStructuralChanges:', hasStructuralChanges, 'isDirty:', isDirty);
}, [hasStructuralChanges, isDirty]);
```

2. Check button disabled condition:
```typescript
disabled={isSavingLocal || (!hasStructuralChanges && !isDirty)}
```

3. Verify both states reset after save

## Prevention
- Always reset all relevant state flags after save operations
- Consider centralizing save state management
- Add integration tests for button state

## Related Issues
- [Undo System Implementation](/docs/implementations/features/2025-01-undo-system.md)

## Search Keywords
save button, persistent state, isDirty, hasChanges, structural components, editor store