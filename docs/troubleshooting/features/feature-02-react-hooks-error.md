# React Hooks Conditional Render Error

## Problem ID
`feature-02-react-hooks-error`

## Summary
"Rendered more hooks than during the previous render" error when navigating back from announcement child editor.

## Environment
- **Component**: ConfigPanel.tsx
- **Framework**: React 18
- **Error Type**: React Hooks Rules Violation
- **Affected Version**: Website Builder v2.0

## Problem Description

### Error Message
```
Error: Rendered more hooks than during the previous render.
    at updateWorkInProgressHook (react-dom.development.js:16506:13)
    at updateReducer (react-dom.development.js:16568:5)
    at updateState (react-dom.development.js:17004:10)
    at Object.useState (react-dom.development.js:17915:16)
    at useState (react.development.js:1622:21)
    at ConfigPanel (ConfigPanel.tsx:20:37)
```

### Symptoms
- Error occurs when clicking back button in AnnouncementItemEditor
- Component crashes with hooks error
- Application becomes unresponsive
- Requires page refresh to recover

### Trigger Conditions
1. Open announcement child editor
2. Click back button
3. Error immediately thrown

## Root Cause

The component had a conditional return statement BEFORE all React hooks were called:

```typescript
// ❌ INCORRECT - Conditional return before hooks
export function ConfigPanel({ section }: ConfigPanelProps) {
  const { selectSection, updateSectionSettings } = useEditorStore();
  const { headerConfig, updateHeaderConfigLocal } = useStructuralComponents();
  
  // Check for announcement item BEFORE hooks
  const isAnnouncementItem = section.id.startsWith('announcement-');
  if (isAnnouncementItem) {
    return <AnnouncementItemEditor announcementId={section.id} />;
  }
  
  // Hooks called AFTER conditional return - VIOLATION!
  const [settings, setSettings] = useState(section.settings);
  const [hasChanges, setHasChanges] = useState(false);
  
  useEffect(() => {
    // ...
  }, [section]);
}
```

This violates React's Rules of Hooks:
> Don't call Hooks inside loops, conditions, or nested functions. Instead, always use Hooks at the top level of your React function, before any early returns.

## Solution

### Fix Applied
Moved all hooks BEFORE any conditional returns:

```typescript
// ✅ CORRECT - All hooks before conditional return
export function ConfigPanel({ section }: ConfigPanelProps) {
  const { selectSection, updateSectionSettings } = useEditorStore();
  const { headerConfig, updateHeaderConfigLocal } = useStructuralComponents();
  
  // ALL hooks first
  const [settings, setSettings] = useState(section.settings);
  const [hasChanges, setHasChanges] = useState(false);
  const [headerLoading, setHeaderLoading] = useState(false);
  
  useEffect(() => {
    setSettings(section.settings);
    setHasChanges(false);
  }, [section, JSON.stringify(section.settings)]);
  
  // Conditional check AFTER all hooks
  const isAnnouncementItem = section.id.startsWith('announcement-');
  
  // Conditional return AFTER all hooks
  if (isAnnouncementItem) {
    return <AnnouncementItemEditor announcementId={section.id} />;
  }
  
  // ... rest of component
}
```

### Key Changes
1. Moved `useState` declarations to top
2. Moved `useEffect` before conditionals
3. Kept conditional logic but after hooks
4. Maintained same functionality

## Prevention

### ESLint Rule
Enable the React Hooks ESLint plugin:
```json
{
  "extends": ["plugin:react-hooks/recommended"],
  "rules": {
    "react-hooks/rules-of-hooks": "error",
    "react-hooks/exhaustive-deps": "warn"
  }
}
```

### Code Review Checklist
- [ ] All `useState` calls at component top
- [ ] All `useEffect` calls before conditionals
- [ ] All custom hooks before conditionals
- [ ] No hooks inside conditions/loops
- [ ] No hooks after early returns

### Common Patterns to Avoid
```typescript
// ❌ BAD PATTERNS

// 1. Hook after condition
if (condition) return null;
const [state, setState] = useState();

// 2. Hook inside condition
if (condition) {
  const [state, setState] = useState();
}

// 3. Hook inside loop
for (let i = 0; i < items.length; i++) {
  const [state, setState] = useState();
}

// 4. Hook inside callback
const handleClick = () => {
  const [state, setState] = useState();
};
```

## Debugging Steps

### Identify the Problem
1. Look for error message mentioning "hooks"
2. Check component that was rendering when error occurred
3. Find all hooks in the component
4. Look for early returns before hooks

### Quick Fix Process
1. Move all `useState` to top of component
2. Move all `useEffect` after `useState`
3. Move all custom hooks after built-in hooks
4. Place conditional returns after all hooks
5. Test the navigation flow

### Verification
```typescript
// Use this pattern for conditional renders
function Component() {
  // 1. All hooks first
  const [state1, setState1] = useState();
  const [state2, setState2] = useState();
  useEffect(() => {}, []);
  
  // 2. Conditional logic
  if (condition) {
    // 3. Conditional return last
    return <AlternativeComponent />;
  }
  
  // 4. Main render
  return <MainComponent />;
}
```

## Impact
- **Severity**: High - Application crash
- **User Experience**: Critical - Requires page refresh
- **Development**: Medium - Common React mistake
- **Testing**: Easy - Linting catches most cases

## Related Documentation
- [React Hooks Rules](https://react.dev/reference/rules/rules-of-hooks)
- [Module Guide - Hooks Error](../../implementations/WEBSITE-BUILDER-MODULE-GUIDE.md#error-común-return-condicional-antes-de-hooks)
- [Navigation Issue](./feature-01-announcement-navigation.md)

## Resolution Date
2025-01-14

## Search Keywords
react hooks, rendered more hooks, conditional return, useState, useEffect, rules of hooks, ConfigPanel, announcement

---
*Created: 2025-01-14*
*Last Updated: 2025-01-14*
*Status: ✅ Resolved*