# i18n System - Language Persistence and Selector Synchronization

**Navigation**: [🏠 Home](../../00-troubleshooting-index.md) > [📋 Features](features-00-index.md) > i18n Persistence & Sync

## Problem Summary

**Affects**: Language selector synchronization between Navbar and ThemeCustomizer
**Frequency**: Durante desarrollo de múltiples selectores
**Severity**: 🟡 MEDIUM - UX inconsistency

### Quick Facts
- **Error Type**: State synchronization issues
- **Components**: Navbar, ThemeCustomizer, I18nContext
- **Root Cause**: Multiple language change handlers not synchronized

## Symptoms

### ✅ Checklist - Do you have these symptoms?
- [ ] Changing language in Navbar doesn't update ThemeCustomizer
- [ ] Changing language in ThemeCustomizer doesn't update Navbar
- [ ] Language resets to default on page reload
- [ ] One selector shows different language than the other
- [ ] Language persists but UI doesn't reflect it

### Exact Error Messages
```
// No console errors, but visual inconsistency:
Navbar shows: "🇺🇸 English"
ThemeCustomizer shows: "🇩🇴 Español" (selected)
```

## Root Causes

### 1. Multiple Independent Language Handlers
**Verification**: Check if both components have separate language change logic
```typescript
// ❌ WRONG - Independent handlers
// In Navbar
const handleLanguageChange = (lang) => {
  localStorage.setItem('language', lang);
};

// In ThemeCustomizer  
const updateSettings = (updates) => {
  if (updates.language) {
    localStorage.setItem('language', updates.language);
  }
};
```

### 2. Context Not Used for Language State
**Verification**: Check if components bypass Context for language changes
```typescript
// ❌ WRONG - Direct localStorage manipulation
onClick={() => localStorage.setItem('language', 'en')}

// ✅ CORRECT - Through Context
onClick={() => setLanguage('en')}
```

### 3. Initialization Race Condition
**Verification**: Check loading order of language preference
```typescript
// ❌ WRONG - Components load before Context initializes
const [language, setLanguage] = useState('es'); // Default before localStorage check
```

## Solutions

### 🚀 Quick Fix (< 5 minutes)

#### Step 1: Centralize Language Management
```typescript
// Only use Context setLanguage function
const { setLanguage } = useI18n();

// Remove all direct localStorage calls
// ❌ localStorage.setItem('language', lang);
// ✅ setLanguage(lang);
```

#### Step 2: Remove Redundant Handlers
```typescript
// In DashboardLayout.tsx - Remove this
const handleLanguageChange = (lang: 'es' | 'en') => {
  localStorage.setItem('language', lang);
  console.log('Language changed to:', lang);
};

// In Navbar - Update to use Context only
const { setLanguage } = useI18n();
onClick={() => setLanguage('en')}
```

### 🔧 Step-by-Step Solution

#### Step 1: Update I18nContext for Proper Initialization
```typescript
// src/contexts/I18nContext.tsx
export function I18nProvider({ children }) {
  const [language, setLanguageState] = useState<Language>('es');
  const [isLoaded, setIsLoaded] = useState(false);

  // Load saved language on init
  useEffect(() => {
    if (typeof window !== 'undefined') {
      const savedLanguage = localStorage.getItem('language') as Language;
      if (savedLanguage && (savedLanguage === 'es' || savedLanguage === 'en')) {
        setLanguageState(savedLanguage);
      }
    }
    setIsLoaded(true);
  }, []);

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    if (typeof window !== 'undefined') {
      localStorage.setItem('language', lang);
    }
  };
}
```

#### Step 2: Update Navbar Component
```typescript
// src/components/layout/Navbar.tsx
export function Navbar({ /* remove onLanguageChange prop */ }) {
  const { t, language, setLanguage } = useI18n();

  return (
    // Language dropdown
    <button onClick={() => setLanguage('es')}>
      <span>{t('theme.spanish', 'Español')}</span>
    </button>
    <button onClick={() => setLanguage('en')}>
      <span>{t('theme.english', 'English')}</span>
    </button>
  );
}
```

#### Step 3: Update ThemeCustomizer Sync
```typescript
// src/components/ui/ThemeCustomizer.tsx
export function ThemeCustomizer() {
  const { t, language, setLanguage } = useI18n();
  const [settings, setSettings] = useState(defaultSettings);

  // Sync Context language with settings
  useEffect(() => {
    setSettings(prev => ({ ...prev, language }));
  }, [language]);

  const updateSettings = (updates) => {
    if (updates.language) {
      setLanguage(updates.language); // Update Context
    }
    setSettings(prev => ({ ...prev, ...updates }));
  };
}
```

#### Step 4: Clean Up DashboardLayout
```typescript
// src/components/layout/DashboardLayout.tsx
export function DashboardLayout() {
  // Remove handleLanguageChange function entirely
  
  return (
    <Navbar
      onSidebarToggle={handleSidebarToggle}
      onThemeChange={handleThemeChange}
      // Remove onLanguageChange prop
      onCustomizeOpen={handleCustomizerOpen}
      sidebarCollapsed={sidebarCollapsed}
    />
  );
}
```

### 🔄 Alternative Solutions

#### Option A: Event-Driven Sync
```typescript
// Use custom events for cross-component communication
useEffect(() => {
  const handleLanguageChange = (e) => {
    setLanguageState(e.detail.language);
  };
  
  window.addEventListener('languageChanged', handleLanguageChange);
  return () => window.removeEventListener('languageChanged', handleLanguageChange);
}, []);
```

#### Option B: Zustand Store
```typescript
// For more complex state management
import { create } from 'zustand';

const useLanguageStore = create((set) => ({
  language: 'es',
  setLanguage: (lang) => set({ language: lang }),
}));
```

## Prevention

### Best Practices
1. **Single Source of Truth**: Use Context for all language state
2. **No Direct localStorage**: Always go through Context methods
3. **Proper Initialization**: Handle SSR/hydration correctly
4. **Remove Redundant Props**: Don't pass language handlers through props

### Configuration Templates

#### Clean Context Pattern
```typescript
// Context handles ALL language logic
const { language, setLanguage, t } = useI18n();

// Components only consume, never manipulate directly
<button onClick={() => setLanguage('en')}>
  {t('theme.english')}
</button>
```

#### Testing Synchronization
```typescript
// Test both selectors
const testSync = () => {
  // Change in Navbar
  navbarSelector.click('english');
  expect(themeCustomizer.getSelected()).toBe('english');
  
  // Change in ThemeCustomizer  
  themeCustomizer.select('spanish');
  expect(navbar.getSelected()).toBe('spanish');
};
```

## Related Issues

### See Also
- [i18n Hydration Issues](features-01-i18n-hydration-issues.md)
- [React Context Best Practices](../../frontend/frontend-04-context-patterns.md)
- [localStorage SSR Issues](../../frontend/frontend-05-localstorage-ssr.md)

### Cross-References
- **Similar Problem**: [Theme Sync Issues](../ui/ui-02-theme-synchronization.md)
- **Related Implementation**: [Context State Management](../../implementations/features/2025-08-context-patterns.md)

## Search Keywords
- i18n language selector sync
- React Context synchronization
- multiple language selectors
- localStorage language persistence
- Next.js i18n state management
- Context provider language sync