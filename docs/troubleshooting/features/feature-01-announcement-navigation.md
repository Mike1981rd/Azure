# Announcement Module - Back Button Navigation Issue

## Problem ID
`feature-01-announcement-navigation`

## Summary
Back button in child announcement editor was incorrectly navigating to parent configuration instead of returning to sidebar.

## Environment
- **Component**: AnnouncementItemEditor.tsx
- **Framework**: Next.js 14 / React 18
- **State Management**: Zustand
- **Affected Version**: Website Builder v2.0

## Problem Description

### Symptoms
When clicking the back arrow (←) in the announcement item editor, the user was taken to the AnnouncementBar parent configuration panel instead of returning to the sidebar list view.

### Expected Behavior
Back button should close the configuration panel and return to the sidebar, showing the list of announcements.

### Actual Behavior
Back button opened the parent AnnouncementBar configuration panel, creating a confusing navigation flow.

## Root Cause

The `handleBack` function was incorrectly implemented to search for and navigate to the parent section:

```typescript
// ❌ INCORRECT IMPLEMENTATION
const handleBack = () => {
  const state = useEditorStore.getState();
  const { sections } = state;
  
  const announcementBarSection = sections.headerGroup?.find(
    s => s.type === SectionType.ANNOUNCEMENT_BAR
  );
  
  if (announcementBarSection) {
    selectSection(announcementBarSection.id);  // Opens parent config
    toggleConfigPanel(true);                   // Keeps panel open
  }
};
```

This violated the UX pattern where child editors should return to the sidebar list, not to another configuration panel.

## Solution

### Fix Applied
Changed the `handleBack` function to simply close the panel and return to sidebar:

```typescript
// ✅ CORRECT IMPLEMENTATION
const handleBack = () => {
  toggleConfigPanel(false);  // Close configuration panel
  selectSection(null);        // Deselect all sections
};
```

### Why This Works
- `toggleConfigPanel(false)` closes the configuration panel
- `selectSection(null)` ensures no section is selected
- User returns to the sidebar view with the announcement list visible

## Prevention

### UX Design Pattern
**Rule**: Back buttons in child editors ALWAYS return to the sidebar list view, never to parent configuration.

### Code Review Checklist
- [ ] Back button uses `toggleConfigPanel(false)`
- [ ] Back button uses `selectSection(null)`
- [ ] No navigation to parent configuration
- [ ] Follows Shopify-style navigation patterns

### Testing Scenarios
1. Open announcement child editor
2. Click back button
3. Verify sidebar is visible
4. Verify no configuration panel is open
5. Verify announcement list is accessible

## Related Issues
- [React Hooks Error](./feature-02-react-hooks-error.md) - Related navigation issue
- [Module Guide - Navigation](../../implementations/WEBSITE-BUILDER-MODULE-GUIDE.md#navegación-incorrecta-del-botón-back)

## Code Examples

### Complete Working Implementation
```typescript
export default function AnnouncementItemEditor({ announcementId }: AnnouncementItemEditorProps) {
  const { config: structuralComponents, updateAnnouncementBarConfigLocal } = useStructuralComponents();
  const { selectSection, toggleConfigPanel } = useEditorStore();
  
  // ... component logic ...

  const handleBack = () => {
    // Close config panel and return to sidebar
    toggleConfigPanel(false);
    selectSection(null);
  };

  return (
    <div className="w-[320px] h-full bg-white dark:bg-gray-900">
      <div className="px-4 py-3 border-b">
        <button
          onClick={handleBack}
          className="p-1 hover:bg-gray-100 dark:hover:bg-gray-800 rounded"
        >
          <ArrowLeft className="w-4 h-4" />
        </button>
        {/* ... rest of header ... */}
      </div>
      {/* ... rest of component ... */}
    </div>
  );
}
```

## Debugging Steps

### If Navigation Still Incorrect:
1. **Check store imports**: Ensure using `useEditorStore` correctly
2. **Verify function calls**: Both `toggleConfigPanel` and `selectSection` must be called
3. **Check parent component**: EditorSidebarWithDnD should handle virtual sections
4. **Test in console**:
   ```javascript
   // Debug in browser console
   const { toggleConfigPanel, selectSection } = useEditorStore.getState();
   toggleConfigPanel(false);
   selectSection(null);
   ```

## Impact
- **User Experience**: High - Navigation confusion
- **Development**: Medium - Required pattern documentation
- **Testing**: Low - Easy to test once understood

## Resolution Date
2025-01-14

## Search Keywords
back button, navigation, parent-child, announcement, sidebar, config panel, toggleConfigPanel, selectSection, UX pattern

---
*Created: 2025-01-14*
*Last Updated: 2025-01-14*
*Status: ✅ Resolved*