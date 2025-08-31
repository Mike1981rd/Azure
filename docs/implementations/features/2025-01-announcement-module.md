# Announcement Module Implementation

## Overview
Implementation of a Shopify-style announcement bar module for the Website Builder with parent-child architecture, allowing multiple rotating announcements with icons and links.

**Date**: 2025-01-14  
**Category**: features  
**Status**: ✅ Complete (v1.6)  
**Time Spent**: ~8 hours  
**Developer**: Claude + User

## Table of Contents
1. [Requirements](#requirements)
2. [Architecture Decisions](#architecture-decisions)
3. [Implementation Details](#implementation-details)
4. [Code Structure](#code-structure)
5. [UI/UX Patterns](#uiux-patterns)
6. [Problems Encountered](#problems-encountered)
7. [Testing Checklist](#testing-checklist)
8. [Future Enhancements](#future-enhancements)

## Requirements

### Functional Requirements
- ✅ Multiple announcements with rotation
- ✅ Icon selection from predefined library (150+ icons in 7 categories)
- ✅ Custom icon support via URL
- ✅ Rich text editor for announcement text
- ✅ Link support for each announcement
- ✅ Individual visibility toggle
- ✅ Global announcement bar configuration
- ✅ Save functionality integrated with global save system
- ✅ Social media integration (19 platforms)
- ✅ Width control (Screen/Page/Large/Medium) - Now as select dropdown
- ✅ Autoplay modes (None/One-at-a-time) - Button toggles
- ✅ Animation styles (6 options: No animation, Fade, Slide horizontal, Slide vertical, Typewriter, Infinite marquee)
- ✅ Navigation arrows for switching between announcements

### Technical Requirements
- ✅ React/Next.js 14 component architecture
- ✅ Zustand store integration
- ✅ Structural components context synchronization
- ✅ Parent-child module pattern
- ✅ Shopify Polaris design system adaptation

## Architecture Decisions

### 1. Parent-Child Module Pattern
**Decision**: Separate parent configuration from child management

**Rationale**:
- Follows Shopify's established UX patterns
- Keeps configuration views clean and focused
- Allows for scalable child element management

**Implementation**:
```typescript
// Parent: Global settings only
AnnouncementBarEditor.tsx - Color scheme, autoplay, selectors

// Children management in sidebar
AnnouncementChildren.tsx - Add/delete/toggle announcements

// Individual child configuration
AnnouncementItemEditor.tsx - Text, icon, link settings
```

### 2. Icon Library Structure
**Decision**: Organize 150+ icons into 7 categories

**Categories**:
- General (28 icons)
- Shop (19 icons)
- Shipping (11 icons)
- Payment security (10 icons)
- Communication (7 icons)
- Ecology (4 icons)
- Social (21 icons)

### 3. Dropdown Direction
**Decision**: Icon selector opens upward

**Rationale**:
- Better visibility of options
- Consistent with Shopify's pattern
- Prevents content being hidden below fold

## Implementation Details

### Component Hierarchy
```
EditorSidebarWithDnD.tsx
├── AnnouncementBar (section)
│   └── AnnouncementChildren.tsx
│       ├── Add Announcement button
│       └── Announcement items list
│           └── onClick → AnnouncementItemEditor.tsx
```

### Configuration Fields (Shopify Standard)

#### Width Options
- **Screen**: Full browser width
- **Page**: Container width with margins
- **Large**: Wide container (1400px)
- **Medium**: Standard container (1200px)

#### Autoplay Modes
- **None**: Manual navigation only
- **One-at-a-time**: Auto-rotate announcements

#### Social Media Platforms Supported
19 platforms with URL configuration:
- Main: Instagram, Facebook, Twitter, YouTube
- Media: Pinterest, TikTok, Vimeo, Flickr
- Professional: LinkedIn, Medium
- Community: Reddit, Discord, Tumblr
- Messaging: WhatsApp, Viber, Telegram, Email
- Streaming: Twitch, Snapchat

### State Management
```typescript
// Structural Components Context
const updateAnnouncementBarConfigLocal = (config) => {
  setHasChanges(true);
  setConfig(prev => ({
    ...prev,
    announcementBar: config
  }));
};

// Individual announcement updates
const handleUpdate = (field, value) => {
  const updatedAnnouncement = {
    ...localAnnouncement,
    [field]: value
  };
  
  const updatedAnnouncements = announcements.map(a => 
    a.id === announcementId ? updatedAnnouncement : a
  );
  
  updateAnnouncementBarConfigLocal({
    ...announcementConfig,
    announcements: updatedAnnouncements
  });
};
```

## Code Structure

### File Organization
```
/components/editor/
├── AnnouncementBarEditor.tsx      # Parent configuration
├── AnnouncementChildren.tsx       # Sidebar child management
├── AnnouncementItemEditor.tsx     # Individual announcement editor
├── ConfigPanel.tsx                 # Updated for virtual sections
└── EditorSidebarWithDnD.tsx       # Renders children components
```

### Key Components

#### AnnouncementBarEditor.tsx (Parent Configuration)
- Color scheme selector (4 schemes)
- Width selector (Select dropdown: Screen/Page/Large/Medium)
- Autoplay configuration (Button toggles: None/One-at-a-time)
- Speed controls (3-10 seconds with slider)
- Animation style selector (6 options):
  - No animation
  - Fade
  - Slide horizontal
  - Slide vertical
  - Typewriter
  - Infinite marquee
- Language selector with desktop visibility (borderless text dropdown)
- Currency selector with desktop visibility (borderless text dropdown)
- Social media section "Configuración del tema":
  - Toggle to enable/disable social icons
  - Style selection (solid/outline)
  - Desktop visibility toggle
  - URLs configuration (19 platforms) in separate section:
    * Instagram, Facebook, Twitter, YouTube, Pinterest
    * TikTok, Tumblr, Snapchat, LinkedIn, Vimeo
    * Flickr, Reddit, Email, Discord, Medium
    * Twitch, WhatsApp, Viber, Telegram
- Edge rounding control (0-10)
- Show only on home page option
- Show navigation arrows toggle

#### AnnouncementChildren.tsx
```typescript
export function AnnouncementChildren({ section, groupId }) {
  const handleAddAnnouncement = () => {
    const newAnnouncement = {
      id: `announcement-${Date.now()}`,
      text: 'New Announcement',
      link: ''
    };
    // Update config...
  };
  
  return (
    <div className="pl-8">
      <button onClick={handleAddAnnouncement}>
        <Plus className="w-3 h-3" />
        <span>Agregar Announcement</span>
      </button>
      {/* List announcements */}
    </div>
  );
}
```

#### AnnouncementItemEditor.tsx
- Rich text toolbar
- Icon selector with categories
- Link input
- Custom icon support
- Back navigation to sidebar

## UI/UX Patterns

### Preview Layout (Shopify Style)
```
[Social Icons]  ← [Text] →  [Language ▼] [Currency ▼]
    Left         Center         Right (gap-4)
```

### Field Types (Shopify Patterns)
- **Width**: Select dropdown with 4 options
- **Autoplay Mode**: Button toggle group
- **Animation Style**: Select dropdown with 6 animations
- **Selectors**: Borderless text dropdowns with custom arrows
- **Social URLs**: Text inputs in separate configuration section

### Navigation Flow
1. User clicks announcement bar in sidebar
2. Configuration panel opens (global settings)
3. User sees "Agregar Announcement" in sidebar
4. Clicking adds new announcement child
5. Clicking child opens individual editor
6. Back button returns to sidebar (NOT parent config)

### Visual Hierarchy
```
Sidebar                    Config Panel
─────────                  ────────────
▼ HEADER GROUP            
  ▼ Announcement bar       [Global Settings]
    (+) Agregar            - Color scheme
    • Announcement 1  →    - Autoplay
    • Announcement 2       - Speed
```

### Icon Selector Pattern
- Dropdown opens upward with `bottom-full` positioning
- Categories with sticky headers
- "None" option to remove icon
- Visual feedback with rotating chevron
- Compact design with `text-xs` sizing

## Problems Encountered

### 1. Save Button Success Message
**Problem**: Save worked but didn't show success toast
**Solution**: Added changesSaved flag in handleSave
**Documentation**: [Module Guide Step 6](../WEBSITE-BUILDER-MODULE-GUIDE.md#paso-6-sistema-de-guardado-y-botón-save-crítico)

### 2. Child Management in Wrong Location
**Problem**: Initially placed child management inside parent config
**Solution**: Created separate AnnouncementChildren component for sidebar
**Documentation**: [Module Guide Step 7](../WEBSITE-BUILDER-MODULE-GUIDE.md#paso-7-módulos-con-elementos-hijos-importante)

### 3. Back Button Navigation
**Problem**: Back button navigated to parent config instead of sidebar
**Solution**: Changed to `toggleConfigPanel(false)` and `selectSection(null)`
**Documentation**: [Troubleshooting](../../troubleshooting/features/feature-01-announcement-navigation.md)

### 4. React Hooks Error
**Problem**: "Rendered more hooks than during the previous render"
**Solution**: Moved conditional return after all hooks in ConfigPanel.tsx
**Documentation**: [Troubleshooting](../../troubleshooting/features/feature-02-react-hooks-error.md)

### 5. Preview Not Updating
**Problem**: Changes in AnnouncementBarEditor weren't showing in preview
**Root Cause**: Two separate preview implementations (PreviewAnnouncementBar.tsx vs EditorPreview.tsx)
**Solution**: Updated EditorPreview.tsx which controls the editor preview
**Key Learning**: Always verify which file controls the actual preview in editor

### 6. Selectors Style Issues
**Problem**: Language/Currency selectors had unwanted borders
**Solution**: Changed from `border border-current` to `border-0` with custom arrow icons
**Implementation**: Used `appearance-none` and positioned SVG arrows absolutely

### 7. Social Media Icons Display
**Problem**: Icons showing as circles instead of actual platform logos
**Solution**: Added complete SVG icon library for 8 main platforms
**Platforms**: Instagram, Facebook, Twitter, YouTube, Pinterest, TikTok, LinkedIn, WhatsApp

### 8. Navigation State Management
**Problem**: useState hook inside renderSectionPreview causing hooks error
**Solution**: Moved state to top level of EditorPreview component
**Key Rule**: Hooks must be called at component top level, not inside functions

### 9. Duplicate Keys in Icon Selector
**Problem**: React warning "Encountered two children with the same key" when opening icon selector
**Root Cause**: Multiple icons had the same `value` in different categories (e.g., "Star", "Heart", "Truck", "Lock", "Circle")
**Solution 1**: Made all icon values unique:
- `Heart` → `HeartOutline` and `HeartSolid`
- `Star` → `StarOutline` and `StarSolid`
- `Shirt` → `TShirt` and `Dress`
- `Truck` → `FastDelivery` and `DeliveryTruck`
- And similar changes for all duplicates
**Solution 2**: Enhanced key generation using indices: `key={\`icon-${categoryIndex}-${iconIndex}-${value}\`}`
**File**: AnnouncementItemEditor.tsx lines 431-439

### 10. Icon Selection Not Persisting
**Problem**: When selecting an icon from dropdown, it immediately reverted to "None"
**Root Cause 1**: useEffect with `JSON.stringify(announcements)` dependency was resetting local state on every update
**Root Cause 2**: Multiple sequential `handleUpdate` calls causing state synchronization issues
**Solution**: 
1. Removed `JSON.stringify(announcements)` from useEffect dependencies (line 191)
2. Consolidated icon update into single atomic operation instead of multiple handleUpdate calls
3. Update both `icon` and `customIcon` fields in one state update
**Code Fix**:
```typescript
// Before: Multiple updates causing race conditions
handleUpdate('icon', iconName);
handleUpdate('customIcon', ''); 

// After: Single atomic update
const updatedAnnouncement = {
  ...localAnnouncement,
  icon: iconName,
  customIcon: ''
};
setLocalAnnouncement(updatedAnnouncement);
// Then update global state once
```
**File**: AnnouncementItemEditor.tsx lines 246-279

### 11. Infinite Marquee Animation Issues
**Problem**: Marquee animation was too wide and took too long to complete its cycle
**Root Cause**: Animation settings were not optimized for conveyor belt effect
**Solution**:
1. Reduced animation duration from 30s to 8s for faster cycling
2. Reduced array copies from 3 to 2 for shorter belt
3. Adjusted translateX from -50% to -33% for proper looping
4. Removed initial paddingLeft to start content immediately visible
5. Reduced spacing between announcements from mx-6 to mx-2
**Implementation**:
```css
@keyframes marquee {
  from { transform: translateX(0); }
  to { transform: translateX(-33%); }
}
```
**File**: EditorPreview.tsx lines 63-69, 535-547

### 12. Animation Synchronization with Preview
**Problem**: Animations weren't synchronizing properly when switching between styles
**Root Cause**: useEffect for autoplay wasn't checking for marquee animation style
**Solution**: Updated useEffect to:
1. Skip interval timer for marquee (CSS handles it)
2. Reset index when switching animation styles
3. Better dependency tracking for animation changes
**File**: EditorPreview.tsx lines 129-163

### 13. Social Media Icons Spacing
**Problem**: Social media icons were too close together
**Solution**: Changed gap from `gap-2` to `gap-3` in both components
**Files**: 
- EditorPreview.tsx line 525
- PreviewAnnouncementBar.tsx line 429

### 14. Show Only on Home Page Implementation
**Problem**: "Show only on home page" option wasn't working
**Root Cause**: Components weren't checking page type
**Solution**:
1. PreviewPage.tsx now passes `pageType` to PreviewAnnouncementBar
2. PreviewAnnouncementBar checks if should show based on page type
3. EditorPreview uses `selectedPageType` from store to hide on non-home pages
**Implementation**:
```typescript
// Check if should show on this page
if (config?.showOnlyHomePage && pageType !== PageType.HOME) {
  return null;
}
```
**Files**:
- PreviewPage.tsx lines 119-124
- PreviewAnnouncementBar.tsx lines 98-102
- EditorPreview.tsx lines 508-511

### 15. Live Preview Animations Implementation
**Problem**: Animations weren't working in the live preview (PreviewAnnouncementBar)
**Solution**: Complete implementation of animation system in preview component:
1. Added all CSS keyframes and animation classes
2. Implemented `renderAnnouncementIcon` function with 100+ Lucide icons
3. Added marquee structure for infinite scroll
4. Synchronized animation state with configuration
**Features Added**:
- All 6 animation styles (no-animation, fade, slide-horizontal, slide-vertical, typewriter, infinite-marquee)
- Icon rendering from Lucide React library
- Proper animation key for re-rendering
- CSS-in-JS animation styles injection
**File**: PreviewAnnouncementBar.tsx lines 13-311

### 16. Mobile View Synchronization Issue
**Problem**: When switching to mobile view in the editor, the preview real (opened in new tab) wasn't synchronized
**Root Cause**: Preview real was detecting actual browser viewport while editor had simulated mobile view
**Solution**:
1. Store device view in localStorage when changed in editor
2. Pass deviceView as prop to PreviewAnnouncementBar from PreviewPage
3. PreviewAnnouncementBar uses deviceView prop when available (from editor) or falls back to viewport detection
**Implementation**:
```typescript
// Editor stores device view
onClick={() => {
  setDeviceView('mobile');
  localStorage.setItem('editorDeviceView', 'mobile');
}}

// PreviewPage reads and passes it
const [editorDeviceView, setEditorDeviceView] = useState<'desktop' | 'mobile' | undefined>();
// Listen for storage changes
window.addEventListener('storage', handleStorageChange);

// PreviewAnnouncementBar uses it
useEffect(() => {
  if (deviceView !== undefined) {
    setIsMobile(deviceView === 'mobile');
    return;
  }
  // Fall back to viewport detection
}, [deviceView]);
```
**Files**:
- EditorPage (page.tsx) lines 306-343
- PreviewPage.tsx lines 27-57, 146
- PreviewAnnouncementBar.tsx lines 86-109

## Testing Checklist

### Functionality Tests
- [x] Add new announcement
- [x] Edit announcement text
- [x] Select icon from library
- [x] Add custom icon via URL
- [x] Toggle announcement visibility
- [x] Delete announcement
- [x] Global save button activation
- [x] Save persistence to database

### UI/UX Tests
- [x] Dropdown opens upward
- [x] Categories display correctly
- [x] Back button returns to sidebar
- [x] Icon displays in selector
- [x] Rich text formatting works
- [x] Responsive design on mobile

### Edge Cases
- [x] Empty announcements array
- [x] Long announcement text
- [x] Invalid custom icon URL
- [x] Multiple rapid clicks
- [x] Undo/Redo functionality

## Future Enhancements

### Recently Implemented (v1.2)
1. ✅ **Animation Styles**: 6 different transition animations
2. ✅ **Improved Selectors**: Borderless text-style dropdowns
3. ✅ **Social Media Icons**: Actual platform SVG icons
4. ✅ **Navigation Arrows**: Functional announcement switching
5. ✅ **Proper Spacing**: Shopify-consistent selector spacing

### Planned Features
1. **Drag & Drop Reordering**: Allow announcement reordering
2. **Preview Mode**: Live preview of rotation
3. **Analytics**: Track announcement clicks
4. **A/B Testing**: Test different messages
5. **Scheduling**: Time-based announcements

### Technical Improvements
1. **Icon Search**: Add search functionality to icon picker
2. **Batch Operations**: Select multiple announcements
3. **Import/Export**: Save announcement templates
4. **Keyboard Navigation**: Full keyboard support
5. **Accessibility**: ARIA labels and screen reader support

## Related Documentation
- [Website Builder Module Guide](../WEBSITE-BUILDER-MODULE-GUIDE.md)
- [Troubleshooting: Navigation Issues](../../troubleshooting/features/feature-01-announcement-navigation.md)
- [Troubleshooting: React Hooks](../../troubleshooting/features/feature-02-react-hooks-error.md)
- [Website Builder Architecture](../WEBSITE-BUILDER-ARCHITECTURE.md)

## Search Keywords
announcement, banner, notification, bar, shopify, icon, picker, parent-child, module, website builder, structural components, rich text, editor

---
*Last Updated: 2025-01-14*
*Version: 1.6*

### Changelog
- v1.6 (2025-01-14):
  - Fixed mobile view synchronization between editor and preview real
  - Implemented localStorage-based device view communication
  - Added storage event listener for real-time sync across tabs
- v1.5 (2025-01-14):
  - Increased social media icons spacing from gap-2 to gap-3
  - Implemented "Show only on home page" functionality
  - Added live preview animations for all 6 styles
  - Complete icon rendering system in PreviewAnnouncementBar
  - Fixed EditorPreview to respect page type restrictions
- v1.4 (2025-01-14):
  - Fixed infinite marquee animation belt width and speed
  - Added animation synchronization with preview
  - Optimized marquee settings for better conveyor belt effect
  - Updated useEffect to handle marquee style properly
- v1.3 (2025-01-14):
  - Fixed duplicate keys warning in icon selector by making all icon values unique
  - Enhanced key generation using categoryIndex and iconIndex for absolute uniqueness
  - Fixed icon selection not persisting issue caused by useEffect dependency
  - Consolidated icon updates into single atomic operation to prevent race conditions
  - Added extensive debugging logs for troubleshooting
- v1.2 (2025-01-14): 
  - Changed Width to select dropdown (Screen/Page/Large/Medium)
  - Updated Autoplay to button toggles (None/One-at-a-time)
  - Added Animation Style selector with 6 options
  - Fixed social media icons to show actual platform SVGs
  - Made selectors borderless (text-style dropdowns)
  - Fixed navigation arrows functionality
  - Improved spacing between selectors
  - Fixed React hooks error in EditorPreview
  - Reorganized social media section as "Configuración del tema"
- v1.1: Added social media integration, width selector update, autoplay modes refinement
- v1.0: Initial implementation with parent-child architecture