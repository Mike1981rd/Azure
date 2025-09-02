# Website Builder Editor - Drag & Drop Implementation

## Overview
Complete implementation of drag & drop functionality for the Website Builder Editor using @dnd-kit library.

**Date**: January 14, 2025  
**Category**: Features  
**Status**: ✅ Complete  
**Time Spent**: 4 hours  

## Table of Contents
1. [Problem Statement](#problem-statement)
2. [Solution Architecture](#solution-architecture)
3. [Implementation Details](#implementation-details)
4. [Key Components](#key-components)
5. [UX Improvements](#ux-improvements)
6. [Integration Points](#integration-points)
7. [Testing & Validation](#testing--validation)

## Problem Statement

The Website Builder Editor needed intuitive drag & drop functionality to:
- Reorder sections within groups
- Provide visual feedback during drag operations
- Maintain restrictions between different section groups
- Integrate seamlessly with existing save/undo system

### Requirements
- Sections can only be reordered within their own group
- Visual indicators for valid drop zones
- Smooth animations and transitions
- No interference with existing functionality
- Clear UX feedback

## Solution Architecture

### Technology Stack
- **Library**: @dnd-kit (modern, accessible, performant)
- **Framework**: React with TypeScript
- **State Management**: Zustand store integration
- **Styling**: Tailwind CSS with custom animations

### Architecture Decisions
1. **Modular Design**: Separated drag & drop logic into dedicated components
2. **Type Safety**: Full TypeScript types for drag operations
3. **Rule Engine**: Centralized validation system for drag operations
4. **UX First**: Implemented drop line indicators like Notion/Linear

## Implementation Details

### 1. Package Installation
```bash
npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

### 2. Type Definitions
```typescript
// lib/dragDrop/types.ts
export type GroupId = 'headerGroup' | 'asideGroup' | 'template' | 'footerGroup';

export interface DragItem {
  id: string;
  type: SectionType;
  groupId: GroupId;
  index: number;
  section: Section;
}

export interface DragState {
  isDragging: boolean;
  draggedItem: DragItem | null;
  draggedOverGroup: GroupId | null;
  draggedOverIndex: number | null;
  canDrop: boolean;
}
```

### 3. Validation Rules
```typescript
// lib/dragDrop/constants.ts
export const GROUP_RESTRICTIONS: Record<GroupId, GroupRestrictions> = {
  headerGroup: {
    canReceiveFrom: ['headerGroup'],
    canMoveTo: ['headerGroup'],
    allowedTypes: [
      SectionType.HEADER,
      SectionType.ANNOUNCEMENT_BAR
    ]
  },
  template: {
    canReceiveFrom: ['template'],
    canMoveTo: ['template'],
    allowedTypes: []
  }
  // ... other groups
};
```

### 4. Main Hook Implementation
```typescript
// hooks/useSectionDragDrop.ts
export function useSectionDragDrop() {
  const { sections, reorderSections, saveHistory } = useEditorStore();
  const [dragState, setDragState] = useState<DragState>(initialState);

  const handleDragEnd = useCallback((event: DndDragEndEvent) => {
    const { active, over } = event;
    
    if (!over || active.id === over.id) return;

    const dragItem = active.data.current as DragItem;
    const overItem = over.data?.current as DragItem;
    
    // Validate the move
    const validation = validateDragOperation(dragItem, dropZone, sections);
    
    if (validation.isValid && sourceGroup === targetGroup) {
      const oldIndex = groupSections.findIndex(s => s.id === active.id);
      const newIndex = groupSections.findIndex(s => s.id === over.id);
      
      if (oldIndex !== -1 && newIndex !== -1) {
        reorderSections(sourceGroup, oldIndex, newIndex);
        saveHistory();
      }
    }
  }, [sections, reorderSections, saveHistory]);

  return { dragState, activeSection, handlers };
}
```

## Key Components

### 1. DraggableSection Component
Wraps each section with drag functionality:
```typescript
// components/editor/dragDrop/DraggableSection.tsx
export function DraggableSection({ section, groupId, index, children }) {
  const { attributes, listeners, setNodeRef, transform, isDragging, isOver } = useSortable({
    id: section.id,
    data: dragItem
  });

  return (
    <>
      {/* Drop Indicator */}
      {isOver && <DropIndicator isActive={true} />}
      
      <div ref={setNodeRef} style={style} className="relative group">
        {/* Drag Handle */}
        <div {...attributes} {...listeners} className="drag-handle">
          <GripVertical />
        </div>
        {children}
      </div>
    </>
  );
}
```

### 2. Drop Indicator Component
Visual feedback for drop zones:
```typescript
// components/editor/dragDrop/DropIndicator.tsx
export function DropIndicator({ isActive }) {
  if (!isActive) return null;
  
  return (
    <div className="w-full h-0.5 bg-blue-500 animate-pulse">
      <div className="w-20 h-0.5 bg-gradient-to-r from-transparent via-blue-400 to-transparent animate-shimmer" />
    </div>
  );
}
```

### 3. EditorSidebarWithDnD
Enhanced sidebar with drag & drop context:
```typescript
// components/editor/EditorSidebarWithDnD.tsx
export function EditorSidebarWithDnD() {
  const { activeSection, handlers, dragState } = useSectionDragDrop();
  
  return (
    <DndContext
      sensors={sensors}
      onDragStart={handlers.handleDragStart}
      onDragEnd={handlers.handleDragEnd}
    >
      {/* Sidebar content with SortableContext */}
      <DragOverlay activeSection={activeSection} />
    </DndContext>
  );
}
```

### 4. Child Items Drag & Drop (Announcement Bar)

We added local DnD for Announcement Bar children (announcements) without interfering with the global sections DnD.

#### Rationale
- Announcements live inside `StructuralComponentsContext` (not in `useEditorStore.sections`).
- We need reordering only within the Announcement Bar, not across groups.
- A local `DndContext` keeps concerns isolated and avoids coupling to the global store.

#### Files
- `src/components/editor/AnnouncementChildren.tsx`
- `src/components/editor/dragDrop/DraggableAnnouncementItem.tsx`

#### Implementation
```typescript
// DraggableAnnouncementItem: lightweight wrapper around useSortable with render-prop
export function DraggableAnnouncementItem({ itemId, children }: {
  itemId: string;
  children: React.ReactNode | ((args: RenderArgs) => React.ReactNode);
})

// In AnnouncementChildren.tsx
<DndContext onDragEnd={handleDragEnd}>
  <SortableContext items={sortableIds} strategy={verticalListSortingStrategy}>
    {announcements.map(a => (
      <DraggableAnnouncementItem itemId={`ann:${section.id}:${a.id}`}>
        {({ setNodeRef, attributes, listeners, isDragging, style }) => (
          <div ref={setNodeRef} style={style} className={rowClasses}>
            {/* Handle only */}
            <button {...attributes} {...listeners} className={handleClasses} aria-label="Reordenar anuncio">
              <GripIcon />
            </button>
            {/* Content + actions */}
          </div>
        )}
      </DraggableAnnouncementItem>
    ))}
  </SortableContext>
</DndContext>

function handleDragEnd(event: DragEndEvent) {
  const { active, over } = event;
  if (!over || active.id === over.id) return;
  // map IDs → indices and reorder local announcements array
  const next = [...announcements];
  const [moved] = next.splice(oldIndex, 1);
  next.splice(newIndex, 0, moved);
  updateAnnouncementBarConfigLocal({ ...announcementConfig, announcements: next });
}
```

#### UX details
- Dedicated drag handle, hover states, ring + shadow while dragging.
- `select-none` to avoid selecting text during drag.
- Actions (show/hide, delete) appear on hover only.
- IDs namespaced (`ann:${sectionId}:${id}`) to avoid collisions.

#### Why this does not affect future modules
- Child DnD is local and acts over the Announcement Bar config only.
- Global sections DnD (groups) remains unchanged.
- Future components with true section-children (multi-instance sections) will keep using the global store + `DraggableSection` pattern.

## UX Improvements

### Visual Feedback System
1. **Drop Line Indicator**: Blue animated line shows exact drop position
2. **Opacity Changes**: Dragged item becomes 40% transparent
3. **Group Highlighting**: Valid drop zones get subtle blue background
4. **Blocked Groups**: Invalid groups fade to 30% opacity

### Animation Configuration
```typescript
// tailwind.config.ts additions
animation: {
  'shimmer': 'shimmer 2s linear infinite',
  'pulse-soft': 'pulse-soft 2s ease-in-out infinite',
},
keyframes: {
  shimmer: {
    '0%': { transform: 'translateX(-100%)' },
    '100%': { transform: 'translateX(100%)' }
  }
}
```

## Integration Points

### 1. Store Integration
```typescript
// Automatic isDirty flag setting
reorderSections: (groupId, fromIndex, toIndex) => {
  // ... reorder logic
  return { sections: updatedSections, isDirty: true };
}
```

### 2. Save System Integration
```typescript
// Updated save handler in editor/page.tsx
const handleSave = async () => {
  if (isDirty) {
    // Save section order changes
    const store = useEditorStore.getState();
    store.setIsDirty(false);
    toast.success('Cambios guardados exitosamente');
  }
};
```

### 3. Undo/Redo Integration
- History saved after successful drag operations
- Undo restores previous section order

## Testing & Validation

### Manual Testing Checklist
- [x] Drag within same group works
- [x] Drop line indicator appears correctly
- [x] Cannot drag between different groups
- [x] Save button activates after reorder
- [x] Undo/Redo works with drag operations
- [x] Visual feedback is clear and intuitive
- [x] Announcement items reorder locally and Save button appears
- [x] No interference with sections DnD

### Edge Cases Handled
1. Dropping on same position (no-op)
2. Rapid drag operations
3. Dragging hidden sections
4. Browser compatibility

## Performance Considerations

1. **Debounced Operations**: Drag events throttled
2. **Minimal Re-renders**: Using React.memo where appropriate
3. **CSS Animations**: Hardware-accelerated transforms
4. **Lazy State Updates**: Only update affected groups

## Future Enhancements

1. **Multi-select Drag**: Select multiple sections to move together
2. **Keyboard Support**: Arrow keys for fine-tuned reordering
3. **Touch Support**: Enhanced mobile drag & drop
4. **Drag Between Pages**: Copy sections across pages

## Related Documentation

- [Website Builder Architecture](/docs/WEBSITE-BUILDER-ARCHITECTURE.md)
- [Editor Store Documentation](/docs/stores/editor-store.md)
- [UI/UX Guidelines](/docs/UI-PATTERNS.md)

## Troubleshooting

### Common Issues

1. **Drag Handle Not Visible**
   - Check padding-left on section container
   - Verify overflow settings on parent

2. **Sections Not Reordering**
   - Check GROUP_RESTRICTIONS for fixedOrder
   - Verify both sections are in same group

3. **Save Button Not Activating**
   - Ensure reorderSections sets isDirty
   - Check handleSave includes isDirty logic

## Code References

- Main Hook: `src/hooks/useSectionDragDrop.ts`
- Components: `src/components/editor/dragDrop/`
- Child DnD: `src/components/editor/AnnouncementChildren.tsx`, `src/components/editor/dragDrop/DraggableAnnouncementItem.tsx`
- Types: `src/lib/dragDrop/types.ts`
- Rules: `src/lib/dragDrop/rules.ts`
- Store: `src/stores/useEditorStore.ts:reorderSections`

---

**Last Updated**: January 14, 2025  
**Author**: Development Team  
**Review Status**: ✅ Approved