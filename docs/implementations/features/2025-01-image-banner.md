# Image Banner Implementation

## Overview
Complete implementation of the Image Banner module for the Website Builder, including configuration UI, preview rendering, media upload, color scheme integration, and typography system.

**Date**: January 15, 2025  
**Category**: Feature  
**Status**: ✅ Complete  
**Time Spent**: ~5 hours  
**Last Updated**: January 15, 2025 - Added Typography System  

## Table of Contents
- [Architecture Decisions](#architecture-decisions)
- [Implementation Details](#implementation-details)
- [File Structure](#file-structure)
- [Key Features](#key-features)
- [Color Schemes Integration](#color-schemes-integration)
- [Typography System Integration](#typography-system-integration)
- [Media Upload System](#media-upload-system)
- [Configuration Schema](#configuration-schema)
- [Troubleshooting](#troubleshooting)
- [Related Documentation](#related-documentation)

## Architecture Decisions

### 1. Modular Component Architecture
- **Decision**: Split Image Banner into 6 sub-components to maintain <300 lines per file
- **Rationale**: Follows project rules for file size limits and maintainability
- **Components**:
  - `ImageBannerEditor.tsx` - Main orchestrator (285 lines)
  - `ImageBannerMedia.tsx` - Media upload handling (245 lines)
  - `ImageBannerContent.tsx` - Text content configuration
  - `ImageBannerPosition.tsx` - Positioning and alignment
  - `ImageBannerButtons.tsx` - CTA button configuration
  - `ImageBannerSpacing.tsx` - Padding configuration

### 2. Global Color Schemes
- **Decision**: Use `useThemeConfigStore` for color schemes instead of hardcoded values
- **Rationale**: Maintains consistency across the entire website
- **Implementation**: Color schemes applied via inline styles using hex values from store

### 3. Unified Preview Architecture
- **Decision**: Single preview component serves both editor and live preview
- **Rationale**: Ensures consistency and reduces code duplication
- **Component**: `PreviewImageBanner.tsx` with `isEditor` prop

## Implementation Details

### File Structure
```
src/components/editor/modules/ImageBanner/
├── ImageBannerEditor.tsx          # Main editor component
├── ImageBannerMedia.tsx           # Media upload UI
├── ImageBannerContent.tsx         # Content configuration
├── ImageBannerPosition.tsx        # Position & alignment
├── ImageBannerButtons.tsx         # Button configuration
├── ImageBannerSpacing.tsx         # Spacing controls
├── PreviewImageBanner.tsx         # Preview component
├── useImageBannerTypography.ts   # Typography styles hook
├── types.ts                       # TypeScript interfaces
└── index.ts                       # Exports
```

### Key Components

#### ImageBannerEditor.tsx
```typescript
export default function ImageBannerEditor({ sectionId }: ImageBannerEditorProps) {
  const { sections, updateSectionSettings, toggleConfigPanel, selectSection } = useEditorStore();
  const { config: themeConfig } = useThemeConfigStore();
  
  // Local state management with sync
  const [localConfig, setLocalConfig] = useState<ImageBannerConfig>(() => {
    return section?.settings || getDefaultConfig();
  });

  // Sync with props
  useEffect(() => {
    const currentSection = Object.values(sections).flat().find(s => s.id === sectionId);
    if (currentSection?.settings) {
      const newSettings = currentSection.settings;
      if (JSON.stringify(newSettings) !== JSON.stringify(localConfig)) {
        setLocalConfig(newSettings);
      }
    }
  }, [sectionId, sections]);

  const handleChange = (updates: Partial<ImageBannerConfig>) => {
    const updatedConfig = { ...localConfig, ...updates };
    setLocalConfig(updatedConfig);
    
    // Update store (marks isDirty automatically)
    if (section) {
      const groupId = Object.keys(sections).find(key => 
        sections[key as keyof typeof sections].includes(section)
      );
      updateSectionSettings(groupId, section.id, updatedConfig);
    }
  };
}
```

#### PreviewImageBanner.tsx
```typescript
export function PreviewImageBanner({ config, isEditor = false }: PreviewImageBannerProps) {
  const { config: themeConfig } = useThemeConfigStore();
  
  // Get selected color scheme from global config
  const colorScheme = useMemo(() => {
    if (!themeConfig?.colorSchemes?.schemes) {
      return defaultColorScheme;
    }
    
    const schemeIndex = parseInt(config.colorScheme) - 1;
    const selectedScheme = themeConfig.colorSchemes.schemes[schemeIndex];
    
    return selectedScheme || themeConfig.colorSchemes.schemes[0];
  }, [themeConfig, config.colorScheme]);

  // Apply colors via inline styles
  return (
    <section style={{ 
      backgroundColor: config.colorBackground ? colorScheme.background : undefined 
    }}>
      <h2 style={{ color: colorScheme.text }}>{config.heading}</h2>
      <button style={{
        backgroundColor: colorScheme.solidButton,
        color: colorScheme.solidButtonText
      }}>
        {config.firstButtonLabel}
      </button>
    </section>
  );
}
```

## Key Features

### 1. Media Upload
- **Endpoint**: `/api/mediaupload/media`
- **Supported Formats**: 
  - Images: JPG, PNG, WebP, GIF, SVG, AVIF
  - Videos: MP4, WebM, OGG, MOV, AVI
- **File Size Limits**: 50MB (images), 100MB (videos)
- **Storage**: `/wwwroot/uploads/images/` and `/wwwroot/uploads/videos/`

### 2. Configuration Options
```typescript
interface ImageBannerConfig {
  // General
  colorScheme: '1' | '2' | '3' | '4' | '5';
  colorBackground: boolean;
  width: 'small' | 'medium' | 'large' | 'page' | 'screen';
  desktopRatio: number;
  mobileRatio: number;
  
  // Media
  desktopImage: string;
  mobileImage: string;
  desktopOverlayOpacity: number;
  mobileOverlayOpacity: number;
  
  // Content
  subheading: string;
  heading: string;
  body: string;
  headingSize: 1 | 2 | 3;
  bodySize: 1 | 2 | 3;
  
  // Position
  desktopPosition: string;
  desktopAlignment: 'left' | 'center';
  desktopWidth: number;
  desktopSpacing: number;
  mobilePosition: 'top' | 'center' | 'bottom';
  mobileAlignment: 'left' | 'center';
  
  // Background styles
  desktopBackground: 'solid' | 'outline' | 'blurred' | 'shadow' | 'transparent' | 'none';
  mobileBackground: 'solid' | 'outline' | 'blurred' | 'shadow' | 'transparent' | 'none';
  
  // Buttons
  firstButtonLabel: string;
  firstButtonLink: string;
  firstButtonStyle: 'solid' | 'outline' | 'text';
  secondButtonLabel: string;
  secondButtonLink: string;
  secondButtonStyle: 'solid' | 'outline' | 'text';
  
  // Spacing
  addSidePaddings: boolean;
  topPadding: number;
  bottomPadding: number;
}
```

## Color Schemes Integration

### Store Import (Critical)
```typescript
// ⚠️ IMPORTANT: Default export, not named export
import useThemeConfigStore from '@/stores/useThemeConfigStore';
```

### Common Error
```typescript
// ❌ WRONG - Will cause "not a function" error
import { useThemeConfigStore } from '@/stores/useThemeConfigStore';

// ✅ CORRECT
import useThemeConfigStore from '@/stores/useThemeConfigStore';
```

### Color Application
Colors must be applied using inline styles because they are hex values:
```typescript
// ✅ CORRECT - Using inline styles
<div style={{ color: colorScheme.text }}>

// ❌ WRONG - Tailwind classes don't work with dynamic values
<div className={`text-[${colorScheme.text}]`}>
```

## Typography System Integration

### Overview
Image Banner integrates with the global typography configuration while maintaining its own size controls for flexibility.

### Implementation Architecture
```
Global Typography Config
├── typography.headings → Applied to Image Banner heading
├── typography.body → Applied to subheading and body text
└── typography.buttons → Applied to CTA buttons
```

### Typography Hook
Created `useImageBannerTypography.ts` to centralize typography style generation:

```typescript
export function useImageBannerTypography(themeConfig: any) {
  // Heading styles (without fontSize - managed by Image Banner)
  const headingTypographyStyles = useMemo(() => {
    if (!themeConfig?.typography?.headings) return {};
    const headings = themeConfig.typography.headings;
    
    return {
      fontFamily: `'${headings.fontFamily}', sans-serif`,
      fontWeight: headings.fontWeight || '700',
      textTransform: headings.useUppercase ? 'uppercase' : 'none',
      letterSpacing: `${headings.letterSpacing || 0}px`
      // Note: fontSize handled by Image Banner's headingSize config
    };
  }, [themeConfig?.typography?.headings]);

  // Body styles (without fontSize - managed by Image Banner)
  const bodyTypographyStyles = useMemo(() => {
    // Similar structure for body typography
  });

  // Button styles (including fontSize - buttons don't have size control)
  const buttonTypographyStyles = useMemo(() => {
    if (!themeConfig?.typography?.buttons) return {};
    const buttons = themeConfig.typography.buttons;
    
    return {
      fontFamily: `'${buttons.fontFamily}', sans-serif`,
      fontWeight: buttons.fontWeight || '500',
      textTransform: buttons.useUppercase ? 'uppercase' : 'none',
      fontSize: buttons.fontSize ? 
        (buttons.fontSize <= 100 ? 
          `${buttons.fontSize}%` : 
          `${buttons.fontSize}px`) : '100%',
      letterSpacing: `${buttons.letterSpacing || 0}px`
    };
  }, [themeConfig?.typography?.buttons]);
}
```

### Typography Mapping
| Image Banner Element | Typography Source | Size Control |
|---------------------|------------------|--------------|
| Heading | `typography.headings` | Image Banner `headingSize` (1,2,3) |
| Subheading | `typography.body` | Image Banner `bodySize` (1,2,3) |
| Body Text | `typography.body` | Image Banner `bodySize` (1,2,3) |
| CTA Buttons | `typography.buttons` | Typography `fontSize` |

### Key Design Decisions

1. **Size Control Separation**:
   - Heading and body text sizes controlled by Image Banner configuration
   - Typography only applies font-family, weight, transform, and letter-spacing
   - Buttons use full typography config including fontSize

2. **Memoization**:
   - All typography styles are memoized with `useMemo` for performance
   - Recalculates only when theme config changes

3. **Pattern Consistency**:
   - Follows same implementation pattern as Header typography
   - Uses default export from `useThemeConfigStore`

## Media Upload System

### Backend Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaUploadController : ControllerBase
{
    [HttpPost("media")]
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
    {
        // Validate file
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        // Determine type and upload
        if (_allowedImageExtensions.Contains(extension))
            return await UploadImage(file);
        else if (_allowedVideoExtensions.Contains(extension))
            return await UploadVideo(file);
        
        // Return URL
        return Ok(new { 
            url = fileUrl,
            fileName = fileName,
            size = file.Length,
            type = mediaType
        });
    }
}
```

### Frontend Upload
```typescript
const handleFileUpload = async (file: File) => {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('/api/mediaupload/media', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  const data = await response.json();
  onChange({ desktopImage: data.url });
};
```

## Configuration Schema

### Database Storage
Configuration is stored as JSONB in PostgreSQL:
```sql
-- PageSection table
Config JSONB -- Stores the entire ImageBannerConfig object
```

### Data Flow
1. User edits in `ImageBannerEditor`
2. Changes call `updateSectionSettings()`
3. Store marks `isDirty: true`
4. Save button appears
5. User clicks Save → API call to persist

## Troubleshooting

### Issue 1: Color Scheme Not Updating
**Problem**: Preview doesn't reflect selected color scheme  
**Solution**: Ensure `useMemo` dependencies include both `themeConfig` and `config.colorScheme`

### Issue 2: Upload Fails with HTML Response
**Problem**: Media upload returns "Unexpected token '<', "<!DOCTYPE "... is not valid JSON"  
**Root Cause**: Frontend using relative URL instead of full backend URL  
**Solution**: Changed from `/api/mediaupload/media` to `http://localhost:5266/api/mediaupload/media`
```typescript
// Before (incorrect)
const response = await fetch('/api/mediaupload/media', {

// After (correct)
const response = await fetch('http://localhost:5266/api/mediaupload/media', {
```

### Issue 3: Backend Process Lock
**Problem**: Build fails with "The file is locked by: WebsiteBuilderAPI (PID)"  
**Solution**: Kill the process before rebuilding
```powershell
Stop-Process -Id [PID] -Force
```

### Issue 4: Content Overflow in Config Panel
**Problem**: Content cut off in 320px panel  
**Solution**: Reduce paddings (px-4→px-3), text sizes (text-sm→text-xs), remove fixed widths

### Issue 5: Typography fontSize Conflict
**Problem**: Typography global fontSize overriding Image Banner's size controls  
**Solution**: Remove fontSize from heading/body typography styles, keep only for buttons
```typescript
// Heading and body: no fontSize (Image Banner controls it)
return {
  fontFamily: `'${headings.fontFamily}', sans-serif`,
  fontWeight: headings.fontWeight || '700',
  textTransform: headings.useUppercase ? 'uppercase' : 'none',
  letterSpacing: `${headings.letterSpacing || 0}px`
  // fontSize removed - handled by headingSize config
};

// Buttons: include fontSize (no local size control)
return {
  fontFamily: `'${buttons.fontFamily}', sans-serif`,
  fontSize: buttons.fontSize ? ... : '100%', // Included for buttons
  // ... other styles
};
```

## Related Documentation

- [Website Builder Module Guide](/docs/WEBSITE-BUILDER-MODULE-GUIDE.md) - v1.9 with Color Schemes section
- [Website Builder Architecture](/docs/WEBSITE-BUILDER-ARCHITECTURE.md)
- [Theme Configuration Types](/websitebuilder-admin/src/types/theme/colorSchemes.ts)
- [Typography Header Implementation](/docs/implementations/features/2025-01-typography-header.md) - Reference pattern
- [Typography Types](/websitebuilder-admin/src/types/theme/typography.ts)

## Performance Considerations

- **Image Optimization**: Consider implementing server-side image optimization
- **Lazy Loading**: Videos use `loading="lazy"` attribute
- **Memoization**: Color scheme calculation is memoized to prevent recalculation

## Future Enhancements

1. **Image Optimization**: Add ImageSharp processing on upload
2. **CDN Integration**: Store media on CDN instead of local storage
3. **Drag & Drop**: Add drag-and-drop file upload
4. **Media Library**: Implement media picker from existing uploads
5. **Animation Options**: Add entrance animations for content

## Testing Checklist

- [x] Color scheme changes reflect in preview
- [x] Image upload works for all formats
- [x] Video upload and playback functional
- [x] Mobile/Desktop views render correctly
- [x] Save functionality persists configuration
- [x] Undo/Redo works with all changes
- [x] All configuration options functional
- [x] Typography system applies correct fonts
- [x] Heading/body sizes controlled by Image Banner
- [x] Button typography includes fontSize from global config
- [x] Upload URL points to correct backend endpoint

---

**Documentation created**: 2025-01-15  
**Author**: Claude (AI Assistant)  
**Review status**: Pending human review