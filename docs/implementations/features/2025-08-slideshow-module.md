# 📸 SlideShow Module Implementation

## 📅 Implementation Details
- **Date**: August 15-16, 2025
- **Version**: 2.0
- **Category**: Website Builder Feature
- **Status**: ✅ Complete
- **Complexity**: High
- **Time Spent**: ~8 hours

## 📋 Overview

The SlideShow module is a comprehensive carousel/slider component for the Website Builder that supports both images and videos, with separate desktop and mobile configurations, advanced content positioning, and multiple transition styles.

## 🎯 Requirements

### Functional Requirements
- Support for unlimited slides
- Image and video media support
- Separate desktop/mobile configurations
- Content overlay with customizable positioning
- Navigation controls (dots, arrows, play/pause)
- Multiple transition styles
- Autoplay functionality
- Responsive design

### Technical Requirements
- Modular architecture (<300 lines per file)
- TypeScript strict mode
- Integration with global theme system
- Media upload functionality
- Preview synchronization

## 🏗️ Architecture

### Component Structure
```
components/editor/modules/Slideshow/
├── SlideshowEditor.tsx        # Main editor (parent)
├── SlideshowChildren.tsx      # Child sections manager
├── SlideEditor.tsx            # Individual slide editor
├── SlideContentEditor.tsx     # Content configuration
├── SlideImageVideoEditor.tsx  # Media upload handler
└── types.ts                   # TypeScript definitions
```

### Data Model
```typescript
interface SlideshowConfig {
  // General Settings
  colorScheme: '1' | '2' | '3' | '4' | '5';
  colorBackground: boolean;
  width: 'screen' | 'page' | 'large';
  desktopRatio: number; // 0.1-2.0
  mobileRatio: number; // 0.1-2.0
  
  // Navigation
  showNavigationCircles: boolean;
  showNavigationArrows: 'never' | 'always' | 'hover';
  desktopArrowsPosition: 'corner' | 'sides';
  transitionStyle: 'fade' | 'swipe' | 'seamless';
  
  // Autoplay
  autoplayMode: 'none' | 'one-at-a-time';
  autoplaySpeed: number; // 3-10 seconds
  showPlayPauseButton: boolean;
  
  // Slides
  slides: SlideConfig[];
}

interface SlideConfig {
  id: string;
  
  // Media
  desktopImage: string;
  mobileImage: string;
  desktopVideo: string;
  mobileVideo: string;
  desktopOverlayOpacity: number;
  mobileOverlayOpacity: number;
  
  // Content
  subheading: string;
  heading: string;
  body: string;
  headingSize: number; // 20-80px
  bodySize: number; // 12-24px
  
  // Positioning
  desktopPosition: string; // 9 positions
  desktopAlignment: 'left' | 'center' | 'right';
  desktopWidth: number; // 200-800px
  mobilePosition: 'top' | 'center' | 'bottom';
  mobileAlignment: 'left' | 'center';
  
  // Styling
  desktopBackground: string; // 6 styles
  mobileBackground: string; // 6 styles
  
  // Buttons
  firstButtonLabel: string;
  firstButtonLink: string;
  firstButtonStyle: 'solid' | 'outline' | 'text';
  secondButtonLabel: string;
  secondButtonLink: string;
  secondButtonStyle: 'solid' | 'outline' | 'text';
  
  visible: boolean;
}
```

## 💻 Implementation Details

### 1. Main Editor Component (SlideshowEditor.tsx)
```typescript
export function SlideshowEditor({ 
  value, 
  onChange,
  companyId 
}: SlideshowEditorProps) {
  const [localConfig, setLocalConfig] = useState<SlideshowConfig>(
    value || getDefaultSlideshowConfig()
  );

  // Sync with parent
  useEffect(() => {
    setLocalConfig(value || getDefaultSlideshowConfig());
  }, [value]);

  const handleChange = (updates: Partial<SlideshowConfig>) => {
    const newConfig = { ...localConfig, ...updates };
    setLocalConfig(newConfig);
    onChange?.(newConfig);
  };

  return (
    <div className="space-y-6">
      {/* General Settings */}
      <GeneralSettings config={localConfig} onChange={handleChange} />
      
      {/* Navigation Settings */}
      <NavigationSettings config={localConfig} onChange={handleChange} />
      
      {/* Autoplay Settings */}
      <AutoplaySettings config={localConfig} onChange={handleChange} />
      
      {/* Slides Management */}
      <SlideshowChildren 
        slides={localConfig.slides}
        onChange={(slides) => handleChange({ slides })}
        companyId={companyId}
      />
    </div>
  );
}
```

### 2. Slide Management (SlideshowChildren.tsx)
```typescript
export function SlideshowChildren({ 
  slides, 
  onChange,
  companyId 
}: Props) {
  const [expandedSlide, setExpandedSlide] = useState<string | null>(null);

  const handleAddSlide = () => {
    const newSlide = getDefaultSlideConfig();
    onChange([...slides, newSlide]);
    setExpandedSlide(newSlide.id);
  };

  const handleUpdateSlide = (index: number, updates: Partial<SlideConfig>) => {
    const newSlides = [...slides];
    newSlides[index] = { ...newSlides[index], ...updates };
    onChange(newSlides);
  };

  const handleDeleteSlide = (index: number) => {
    onChange(slides.filter((_, i) => i !== index));
  };

  const handleReorderSlides = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) {
      const oldIndex = slides.findIndex(s => s.id === active.id);
      const newIndex = slides.findIndex(s => s.id === over.id);
      onChange(arrayMove(slides, oldIndex, newIndex));
    }
  };

  return (
    <DndContext onDragEnd={handleReorderSlides}>
      <SortableContext items={slides.map(s => s.id)}>
        {slides.map((slide, index) => (
          <SortableSlide
            key={slide.id}
            slide={slide}
            index={index}
            expanded={expandedSlide === slide.id}
            onToggle={() => setExpandedSlide(
              expandedSlide === slide.id ? null : slide.id
            )}
            onUpdate={(updates) => handleUpdateSlide(index, updates)}
            onDelete={() => handleDeleteSlide(index)}
            companyId={companyId}
          />
        ))}
      </SortableContext>
    </DndContext>
  );
}
```

### 3. Media Upload (SlideImageVideoEditor.tsx)
```typescript
export function SlideImageVideoEditor({ 
  slide, 
  onChange,
  companyId 
}: Props) {
  const handleImageUpload = async (file: File, type: 'desktop' | 'mobile') => {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch(`/api/media/upload/${companyId}`, {
      method: 'POST',
      body: formData
    });
    
    const { url } = await response.json();
    onChange({
      [type === 'desktop' ? 'desktopImage' : 'mobileImage']: url
    });
  };

  const handleVideoUpload = async (file: File, type: 'desktop' | 'mobile') => {
    // Validate video format
    const validFormats = ['video/mp4', 'video/webm', 'video/ogg'];
    if (!validFormats.includes(file.type)) {
      toast.error('Please upload MP4, WebM, or OGG video');
      return;
    }

    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch(`/api/media/upload/${companyId}`, {
      method: 'POST',
      body: formData
    });
    
    const { url } = await response.json();
    onChange({
      [type === 'desktop' ? 'desktopVideo' : 'mobileVideo']: url
    });
  };

  return (
    <div className="space-y-6">
      {/* Desktop Media */}
      <MediaUploader
        label="Desktop Image"
        value={slide.desktopImage}
        onUpload={(file) => handleImageUpload(file, 'desktop')}
        onRemove={() => onChange({ desktopImage: '' })}
        accept="image/*"
      />
      
      <MediaUploader
        label="Desktop Video (optional)"
        value={slide.desktopVideo}
        onUpload={(file) => handleVideoUpload(file, 'desktop')}
        onRemove={() => onChange({ desktopVideo: '' })}
        accept="video/mp4,video/webm,video/ogg"
      />

      {/* Mobile Media */}
      <MediaUploader
        label="Mobile Image"
        value={slide.mobileImage}
        onUpload={(file) => handleImageUpload(file, 'mobile')}
        onRemove={() => onChange({ mobileImage: '' })}
        accept="image/*"
      />
    </div>
  );
}
```

### 4. Content Editor with Font Size Control
```typescript
export function SlideContentEditor({ 
  slide, 
  onChange 
}: Props) {
  // Font size sliders override global typography
  const handleHeadingSizeChange = (size: number) => {
    onChange({ headingSize: size });
  };

  const handleBodySizeChange = (size: number) => {
    onChange({ bodySize: size });
  };

  return (
    <div className="space-y-4">
      {/* Text Content */}
      <Input
        label="Subheading"
        value={slide.subheading}
        onChange={(e) => onChange({ subheading: e.target.value })}
      />
      
      <Input
        label="Heading"
        value={slide.heading}
        onChange={(e) => onChange({ heading: e.target.value })}
      />
      
      <Textarea
        label="Body Text"
        value={slide.body}
        onChange={(e) => onChange({ body: e.target.value })}
        rows={3}
      />

      {/* Font Size Controls */}
      <div className="space-y-3">
        <label className="text-sm font-medium">
          Heading Size: {slide.headingSize}px
        </label>
        <Slider
          value={[slide.headingSize]}
          onValueChange={([v]) => handleHeadingSizeChange(v)}
          min={20}
          max={80}
          step={1}
        />
      </div>

      <div className="space-y-3">
        <label className="text-sm font-medium">
          Body Size: {slide.bodySize}px
        </label>
        <Slider
          value={[slide.bodySize]}
          onValueChange={([v]) => handleBodySizeChange(v)}
          min={12}
          max={24}
          step={1}
        />
      </div>

      {/* Position Controls */}
      <PositionSelector
        label="Desktop Position"
        value={slide.desktopPosition}
        onChange={(pos) => onChange({ desktopPosition: pos })}
        options={DESKTOP_POSITIONS}
      />

      <PositionSelector
        label="Mobile Position"
        value={slide.mobilePosition}
        onChange={(pos) => onChange({ mobilePosition: pos })}
        options={MOBILE_POSITIONS}
      />
    </div>
  );
}
```

## 🎨 Preview Integration

The SlideShow preview is rendered in `PreviewSlideshow.tsx` with:

### Navigation Arrows
- Modern circled design with backdrop blur
- Corner positioning (bottom-right together)
- Hover animations for "show on hover" mode
- Smooth transitions

### Mobile Optimizations
- Larger navigation dots (12x12px)
- Smart positioning to avoid button collisions
- Centered content by default
- Touch-friendly controls

### Video Handling
- HTML5 video with autoplay and loop
- Overlay opacity control
- Fallback to image if video fails
- Mobile-specific video sources

## 🔧 Key Implementation Decisions

### 1. Modular Architecture
- Split into 6 files to maintain <300 lines per file
- Clear separation of concerns
- Reusable sub-components

### 2. Media Upload vs URL Input
- Changed from URL input to file upload for consistency
- Better UX with preview and validation
- Eliminates need for URL parsing logic

### 3. Font Size Independence
- Slider controls override global typography
- Per-slide customization
- Maintains theme inheritance for other properties

### 4. Responsive Design Strategy
- Separate desktop/mobile configurations
- Device-specific media sources
- Adaptive navigation controls

## 📊 Performance Optimizations

1. **Lazy Loading**: Images loaded only when visible
2. **Video Optimization**: Compressed formats, lazy loading
3. **State Management**: Local state with parent sync
4. **Render Optimization**: Memoized expensive calculations

## 🧪 Testing Checklist

- [x] Add/remove slides
- [x] Reorder slides via drag & drop
- [x] Upload images and videos
- [x] Configure content positioning
- [x] Test all transition styles
- [x] Verify mobile responsiveness
- [x] Test autoplay functionality
- [x] Validate navigation controls
- [x] Check theme color inheritance

## 📝 Lessons Learned

1. **State Synchronization**: Always sync local state with parent props via useEffect
2. **Media Handling**: File upload provides better UX than URL input
3. **Mobile First**: Design mobile experience separately, don't just scale desktop
4. **Navigation UX**: "Show on hover" requires careful animation timing

## 🔗 Related Documentation

- Troubleshooting: `/docs/troubleshooting/features/slideshow-01-display-issues.md`
- Architecture: `/docs/WEBSITE-BUILDER-ARCHITECTURE.md`
- Module Guide: `/NEW-MODULE-GUIDE.md`

## 📚 References

- [Shopify Slideshow Documentation](https://help.shopify.com/en/manual/online-store/themes/theme-structure/sections/slideshow-section)
- [Swiper.js API](https://swiperjs.com/swiper-api)
- [React DnD Kit](https://docs.dndkit.com/)

---

**Author**: Development Team
**Last Updated**: August 16, 2025
**Version**: 1.0.0