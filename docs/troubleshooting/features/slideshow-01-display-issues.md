# 🔧 SlideShow Display Issues Troubleshooting

## 📅 Troubleshooting Details
- **Date Encountered**: August 15-16, 2025
- **Category**: Website Builder Features
- **Severity**: Medium
- **Resolution Time**: 2-3 hours
- **Status**: ✅ Resolved

## 🐛 Problems Encountered

### Problem 1: Body Text Not Displaying

#### Symptoms
- Body text configured in editor but not showing in preview
- Only heading and subheading visible
- Console showing no errors

#### Root Cause
The preview component was checking for `slide.text` instead of `slide.body` property.

#### Solution
```typescript
// ❌ Before (PreviewSlideshow.tsx)
{slide.text && (
  <p className="text-sm">{slide.text}</p>
)}

// ✅ After
{slide.body && (
  <p className="text-sm" style={{ fontSize: `${slide.bodySize}px` }}>
    {slide.body}
  </p>
)}
```

---

### Problem 2: Font Size Sliders Not Working

#### Symptoms
- Moving font size sliders had no effect
- Text always used global typography settings
- Slider values saved but not applied

#### Root Cause
Preview component was not using the `headingSize` and `bodySize` properties from slide config.

#### Solution
```typescript
// ✅ Apply font sizes directly with inline styles
<h2 style={{ 
  fontSize: `${slide.headingSize}px`,
  lineHeight: '1.2'
}}>
  {slide.heading}
</h2>

<p style={{ 
  fontSize: `${slide.bodySize}px`,
  lineHeight: '1.5'
}}>
  {slide.body}
</p>
```

---

### Problem 3: Arrows Position "Corner" Not Working

#### Symptoms
- When "Arrows in corner" selected, arrows appeared on sides
- Bottom-right corner positioning not applied
- Hover animations not smooth

#### Root Cause
CSS positioning logic was incorrect for corner mode, treating it same as sides.

#### Solution
```typescript
// ✅ Correct corner positioning
const arrowClasses = showNavigationArrows === 'never' ? 'hidden' :
  desktopArrowsPosition === 'corner' ? 
    'absolute bottom-4 right-4 flex gap-2' :  // Together in corner
    'absolute inset-y-0 flex items-center';   // On sides

// ✅ Individual arrow positioning for corner mode
{desktopArrowsPosition === 'corner' ? (
  <div className="flex gap-2">
    <button className="arrow-button">←</button>
    <button className="arrow-button">→</button>
  </div>
) : (
  <>
    <button className="absolute left-4">←</button>
    <button className="absolute right-4">→</button>
  </>
)}
```

---

### Problem 4: Mobile Navigation Dots Collision

#### Symptoms
- Navigation dots overlapping with buttons on mobile
- Content at bottom position causing UI conflicts
- Poor visibility of dots on some backgrounds

#### Root Cause
Fixed positioning of dots didn't account for content position and button presence.

#### Solution
```typescript
// ✅ Smart positioning based on content
const dotsPosition = () => {
  const hasButtons = slide.firstButtonLabel || slide.secondButtonLabel;
  const isContentBottom = slide.mobilePosition === 'bottom';
  
  if (isContentBottom && hasButtons) {
    return 'bottom-24'; // Move dots higher
  }
  return 'bottom-8';   // Default position
};

// ✅ Enhanced dot styling
.navigation-dot {
  width: 12px;
  height: 12px;
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.8);
}
```

---

### Problem 5: Video Upload Not Working

#### Symptoms
- URL input for videos confusing users
- YouTube URLs not parsing correctly
- No preview for uploaded videos

#### Root Cause
System was expecting URL input instead of file upload, inconsistent with image handling.

#### Solution
```typescript
// ✅ Changed to file upload
const handleVideoUpload = async (file: File) => {
  // Validate format
  const validFormats = ['video/mp4', 'video/webm', 'video/ogg'];
  if (!validFormats.includes(file.type)) {
    toast.error('Invalid video format');
    return;
  }

  // Upload to media library
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await fetch(`/api/media/upload/${companyId}`, {
    method: 'POST',
    body: formData
  });
  
  const { url } = await response.json();
  onChange({ desktopVideo: url });
};

// ✅ Add video preview
{slide.desktopVideo && (
  <video 
    src={slide.desktopVideo} 
    className="w-full h-32 object-cover"
    controls
  />
)}
```

---

### Problem 6: Hover State for Arrows Not Working

#### Symptoms
- "Show on hover" setting had no effect
- Arrows always visible or always hidden
- No smooth transition

#### Root Cause
Missing hover state tracking and CSS transitions.

#### Solution
```typescript
// ✅ Track hover state
const [isHovered, setIsHovered] = useState(false);

<div 
  className="slideshow-container"
  onMouseEnter={() => setIsHovered(true)}
  onMouseLeave={() => setIsHovered(false)}
>
  {/* Arrow visibility logic */}
  {(showNavigationArrows === 'always' || 
    (showNavigationArrows === 'hover' && isHovered)) && (
    <NavigationArrows />
  )}
</div>

// ✅ CSS transitions
.navigation-arrow {
  transition: opacity 0.3s ease, transform 0.2s ease;
  opacity: 0;
}

.slideshow-container:hover .navigation-arrow {
  opacity: 1;
}
```

## 🔍 Debugging Steps

### 1. Console Logging
```typescript
// Added debug logging to track state
useEffect(() => {
  console.log('Slide config:', slide);
  console.log('Font sizes:', { 
    heading: slide.headingSize, 
    body: slide.bodySize 
  });
}, [slide]);
```

### 2. React DevTools
- Inspected props being passed to preview
- Verified state updates in editor components
- Checked for prop drilling issues

### 3. CSS Inspector
- Identified conflicting styles
- Found specificity issues
- Discovered missing hover states

## 💡 Lessons Learned

1. **Always verify property names** between editor and preview components
2. **Use inline styles** for dynamic values that override theme
3. **Test responsive design** at multiple breakpoints
4. **Consider content positioning** when placing UI elements
5. **Maintain consistency** in media handling (upload vs URL)

## 🚀 Prevention Strategies

1. **Type Safety**: Use shared TypeScript interfaces
2. **Component Testing**: Add unit tests for critical props
3. **Visual Testing**: Screenshot comparisons for UI changes
4. **Code Review**: Check editor-preview synchronization
5. **Documentation**: Clear prop documentation

## 📊 Impact

- **Users Affected**: All Website Builder users
- **Features Impacted**: SlideShow module functionality
- **Business Impact**: Delayed feature release by 2 hours
- **Customer Complaints**: None (caught in development)

## 🔗 Related Issues

- Implementation: `/docs/implementations/features/2025-08-slideshow-module.md`
- Architecture: `/docs/WEBSITE-BUILDER-ARCHITECTURE.md`
- Similar Issue: `/docs/troubleshooting/features/image-banner-01-upload-issues.md`

## ✅ Verification Steps

1. Create new slideshow with 3 slides
2. Configure different font sizes per slide
3. Test all arrow positions and hover states
4. Upload both images and videos
5. Verify mobile navigation doesn't overlap
6. Test all transition styles
7. Confirm autoplay works correctly

---

**Author**: Development Team
**Last Updated**: August 16, 2025
**Status**: Resolved
**Tags**: #slideshow #display #ui #responsive