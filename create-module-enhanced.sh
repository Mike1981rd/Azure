#!/bin/bash

# create-module-enhanced.sh - Script mejorado para crear módulos con soporte para hijos
# Basado en las lecciones aprendidas del módulo Slideshow

if [ -z "$1" ]; then
    echo "❌ Error: Debes proporcionar un nombre de módulo"
    echo "Uso: ./create-module-enhanced.sh [NombreModulo] [--with-children]"
    echo "Ejemplo: ./create-module-enhanced.sh Gallery --with-children"
    exit 1
fi

MODULE=$1
WITH_CHILDREN=$2
BASE_DIR="websitebuilder-admin/src/components/editor/modules"

# Verificar si el módulo ya existe
if [ -d "$BASE_DIR/$MODULE" ]; then
    echo "❌ Error: El módulo $MODULE ya existe"
    exit 1
fi

echo "🚀 Creando módulo: $MODULE"

# Crear estructura de directorios
mkdir -p "$BASE_DIR/$MODULE"
cd "$BASE_DIR/$MODULE"

# 1. Crear types.ts
cat > types.ts << EOF
/**
 * @file types.ts
 * @max-lines 100
 * @module $MODULE
 */

export interface ${MODULE}Config {
  // TODO: Define your module configuration
  enabled: boolean;
  colorScheme: string;
  // Add more configuration fields
}

EOF

# Si tiene hijos, agregar tipos para hijos
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> types.ts << EOF
export interface ${MODULE}ItemConfig {
  id: string;
  visible: boolean;
  title: string;
  // TODO: Add child item fields
}

export function getDefault${MODULE}ItemConfig(): ${MODULE}ItemConfig {
  return {
    id: \`item_\${Date.now()}\`,
    visible: true,
    title: 'New Item',
  };
}
EOF
fi

cat >> types.ts << EOF
export function getDefault${MODULE}Config(): ${MODULE}Config {
  return {
    enabled: true,
    colorScheme: '1',
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> types.ts << EOF
    items: []
EOF
fi

cat >> types.ts << EOF
  };
}
EOF

# 2. Crear Editor principal
cat > ${MODULE}Editor.tsx << EOF
/**
 * @file ${MODULE}Editor.tsx
 * @max-lines 300
 * @module $MODULE
 */

'use client';

import React, { useState, useEffect } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useEditorStore } from '@/stores/useEditorStore';
import { ${MODULE}Config, getDefault${MODULE}Config } from './types';

interface ${MODULE}EditorProps {
  sectionId: string;
}

export default function ${MODULE}Editor({ sectionId }: ${MODULE}EditorProps) {
  const { sections, updateSectionSettings } = useEditorStore();
  
  // Find section
  const section = Object.values(sections).flat().find(s => s.id === sectionId);
  
  // Local state
  const [localConfig, setLocalConfig] = useState<${MODULE}Config>(() => {
    return section?.settings || getDefault${MODULE}Config();
  });

  // Expanded sections
  const [expandedSections, setExpandedSections] = useState({
    general: true,
    appearance: false,
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

  const handleChange = (field: keyof ${MODULE}Config, value: any) => {
    const updatedConfig = { ...localConfig, [field]: value };
    setLocalConfig(updatedConfig);
    
    // Update store
    if (section) {
      const groupId = Object.keys(sections).find(key => 
        sections[key as keyof typeof sections].includes(section)
      );
      if (groupId) {
        updateSectionSettings(groupId, section.id, updatedConfig);
      }
    }
  };

  const toggleSection = (section: keyof typeof expandedSections) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  return (
    <div className="h-full bg-white dark:bg-gray-900 flex flex-col">
      <div className="flex-1 overflow-y-auto">
        
        {/* General Settings */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('general')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              General Settings
            </span>
            {expandedSections.general ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.general && (
            <div className="px-3 pb-3 space-y-3">
              {/* Color Scheme */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Color scheme</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded"
                  value={localConfig.colorScheme}
                  onChange={(e) => handleChange('colorScheme', e.target.value)}
                >
                  <option value="1">Color scheme 1</option>
                  <option value="2">Color scheme 2</option>
                  <option value="3">Color scheme 3</option>
                  <option value="4">Color scheme 4</option>
                  <option value="5">Color scheme 5</option>
                </select>
              </div>

              {/* Enabled Toggle */}
              <div className="flex items-center justify-between">
                <span className="text-xs text-gray-600 dark:text-gray-400">Enabled</span>
                <button
                  onClick={() => handleChange('enabled', !localConfig.enabled)}
                  className={\`w-10 h-5 rounded-full transition-colors \${
                    localConfig.enabled ? 'bg-blue-600' : 'bg-gray-300'
                  }\`}
                >
                  <div className={\`w-4 h-4 rounded-full bg-white transition-transform \${
                    localConfig.enabled ? 'translate-x-5' : 'translate-x-0.5'
                  }\`} />
                </button>
              </div>
            </div>
          )}
        </div>

        {/* TODO: Add more configuration sections */}

      </div>
    </div>
  );
}
EOF

# 3. Si tiene hijos, crear Children component
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat > ${MODULE}Children.tsx << EOF
/**
 * @file ${MODULE}Children.tsx
 * @max-lines 300
 * @module $MODULE
 * Manages ${MODULE} items in the sidebar
 */

'use client';

import React, { useMemo } from 'react';
import { Plus, Eye, EyeOff, Trash2, GripVertical } from 'lucide-react';
import { DndContext, DragEndEvent } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { useEditorStore } from '@/stores/useEditorStore';
import { Section } from '@/types/editor.types';
import { ${MODULE}Config, ${MODULE}ItemConfig, getDefault${MODULE}ItemConfig } from './types';

interface ${MODULE}ChildrenProps {
  section: Section;
  groupId: string;
}

interface DraggableItemProps {
  item: ${MODULE}ItemConfig;
  index: number;
  onSelect: () => void;
  onToggleVisibility: () => void;
  onDelete: () => void;
  isSelected: boolean;
}

function DraggableItem({ 
  item, 
  index, 
  onSelect, 
  onToggleVisibility, 
  onDelete,
  isSelected 
}: DraggableItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: item.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  const [showActions, setShowActions] = React.useState(false);

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={\`
        group relative flex items-center px-4 py-2 cursor-pointer transition-all
        hover:bg-gray-100
        \${isDragging ? 'shadow-lg bg-white' : ''}
        \${!item.visible ? 'opacity-50' : ''}
      \`}
      onMouseEnter={() => setShowActions(true)}
      onMouseLeave={() => setShowActions(false)}
      onClick={onSelect}
    >
      {/* Drag Handle - Only visible on hover */}
      <button
        className="absolute left-0 opacity-0 group-hover:opacity-100 cursor-grab active:cursor-grabbing p-1 hover:bg-gray-200 rounded transition-opacity"
        {...attributes}
        {...listeners}
      >
        <GripVertical className="w-3 h-3 text-gray-400" />
      </button>

      {/* Indent for nested item */}
      <div className="ml-4 mr-2">
        <svg className="w-2 h-2 text-gray-400" fill="currentColor" viewBox="0 0 6 10">
          <path d="M1 1l4 4-4 4" />
        </svg>
      </div>
      
      {/* Item Title */}
      <span className="flex-1 text-sm text-gray-700">
        {item.title || 'Untitled'}
      </span>
      
      {/* Actions - Only visible on hover */}
      {showActions && (
        <div className="flex items-center gap-1">
          <button
            onClick={(e) => {
              e.stopPropagation();
              onToggleVisibility();
            }}
            className="p-1 hover:bg-gray-200 rounded transition-colors"
            title={item.visible ? 'Hide item' : 'Show item'}
          >
            {item.visible ? (
              <Eye className="w-3.5 h-3.5 text-gray-500" />
            ) : (
              <EyeOff className="w-3.5 h-3.5 text-gray-400" />
            )}
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onDelete();
            }}
            className="p-1 hover:bg-red-100 rounded transition-colors"
            title="Delete item"
          >
            <Trash2 className="w-3.5 h-3.5 text-gray-500 hover:text-red-600" />
          </button>
        </div>
      )}
    </div>
  );
}

export default function ${MODULE}Children({ section, groupId }: ${MODULE}ChildrenProps) {
  const { selectSection, toggleConfigPanel, updateSectionSettings } = useEditorStore();
  const [selectedItemId, setSelectedItemId] = React.useState<string | null>(null);
  
  // Get config
  const config = section.settings as ${MODULE}Config;
  const items = (config as any)?.items || [];

  // Sortable IDs
  const sortableIds = useMemo(
    () => items.map((item: ${MODULE}ItemConfig) => item.id),
    [items]
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;

    const oldIndex = items.findIndex((i: ${MODULE}ItemConfig) => i.id === active.id);
    const newIndex = items.findIndex((i: ${MODULE}ItemConfig) => i.id === over.id);
    
    if (oldIndex !== -1 && newIndex !== -1) {
      const newItems = [...items];
      const [movedItem] = newItems.splice(oldIndex, 1);
      newItems.splice(newIndex, 0, movedItem);

      updateSectionSettings(groupId, section.id, {
        ...config,
        items: newItems
      });
    }
  };

  const handleAddItem = () => {
    const newItem = getDefault${MODULE}ItemConfig();
    const updatedConfig = {
      ...config,
      items: [...items, newItem]
    };
    
    updateSectionSettings(groupId, section.id, updatedConfig);
    
    // Auto-select new item
    setSelectedItemId(newItem.id);
    selectSection(\`\${section.id}:item:\${newItem.id}\`);
    toggleConfigPanel(true);
  };

  const handleSelectItem = (itemId: string) => {
    setSelectedItemId(itemId);
    // Use special ID format for child selection
    selectSection(\`\${section.id}:item:\${itemId}\`);
    toggleConfigPanel(true);
  };

  const handleToggleItemVisibility = (itemId: string) => {
    const updatedItems = items.map((item: ${MODULE}ItemConfig) => 
      item.id === itemId 
        ? { ...item, visible: !item.visible }
        : item
    );
    
    updateSectionSettings(groupId, section.id, {
      ...config,
      items: updatedItems
    });
  };

  const handleDeleteItem = (itemId: string) => {
    const updatedItems = items.filter((item: ${MODULE}ItemConfig) => item.id !== itemId);
    
    updateSectionSettings(groupId, section.id, {
      ...config,
      items: updatedItems
    });
    
    // Clear selection if deleted item was selected
    if (selectedItemId === itemId) {
      setSelectedItemId(null);
      selectSection(section.id);
    }
  };

  return (
    <div>
      {/* Items List with DnD */}
      {items.length > 0 && (
        <DndContext onDragEnd={handleDragEnd}>
          <SortableContext items={sortableIds} strategy={verticalListSortingStrategy}>
            <div className="pl-4">
              {items.map((item: ${MODULE}ItemConfig, index: number) => (
                <DraggableItem
                  key={item.id}
                  item={item}
                  index={index}
                  onSelect={() => handleSelectItem(item.id)}
                  onToggleVisibility={() => handleToggleItemVisibility(item.id)}
                  onDelete={() => handleDeleteItem(item.id)}
                  isSelected={selectedItemId === item.id}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}
      
      {/* Add Item Button */}
      <button
        onClick={handleAddItem}
        className="w-full flex items-center gap-2 pl-8 pr-4 py-2 text-xs text-blue-600 dark:text-blue-400 
                   hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors group"
      >
        <div className="w-4 h-4 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
          <Plus className="w-3 h-3" />
        </div>
        <span>Add Item</span>
      </button>

      {/* Empty State */}
      {items.length === 0 && (
        <div className="px-4 py-3 text-xs text-gray-500 text-center">
          No items. Add one to get started.
        </div>
      )}
    </div>
  );
}
EOF

# 4. Crear ItemEditor para hijos
cat > ${MODULE}ItemEditor.tsx << EOF
/**
 * @file ${MODULE}ItemEditor.tsx
 * @max-lines 300
 * @module $MODULE
 * Editor for individual item configuration
 */

'use client';

import React, { useState, useEffect } from 'react';
import { ArrowLeft } from 'lucide-react';
import { useEditorStore } from '@/stores/useEditorStore';
import { ${MODULE}Config, ${MODULE}ItemConfig } from './types';

interface ${MODULE}ItemEditorProps {
  sectionId: string;
  itemId: string;
}

export default function ${MODULE}ItemEditor({ sectionId, itemId }: ${MODULE}ItemEditorProps) {
  const { sections, updateSectionSettings, selectSection } = useEditorStore();
  
  // Find section and item
  const section = Object.values(sections).flat().find(s => s.id === sectionId);
  const config = section?.settings as ${MODULE}Config;
  const items = (config as any)?.items || [];
  const currentItem = items.find((i: ${MODULE}ItemConfig) => i.id === itemId);
  
  // Local state
  const [localItem, setLocalItem] = useState<${MODULE}ItemConfig | null>(currentItem || null);
  
  // Sync with props
  useEffect(() => {
    const updatedSection = Object.values(sections).flat().find(s => s.id === sectionId);
    const updatedConfig = updatedSection?.settings as ${MODULE}Config;
    const updatedItem = (updatedConfig as any)?.items?.find((i: ${MODULE}ItemConfig) => i.id === itemId);
    
    if (updatedItem && JSON.stringify(updatedItem) !== JSON.stringify(localItem)) {
      setLocalItem(updatedItem);
    }
  }, [sectionId, itemId, sections]);
  
  if (!localItem) {
    return (
      <div className="w-[320px] h-full bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-700 p-4">
        <p className="text-sm text-gray-500">Item not found</p>
      </div>
    );
  }
  
  const handleBack = () => {
    selectSection(null);  // Go back to main sidebar, not parent config
  };
  
  const handleChange = (field: keyof ${MODULE}ItemConfig, value: any) => {
    const updatedItem = { ...localItem, [field]: value };
    setLocalItem(updatedItem);
    
    // Update in store
    if (section && config) {
      const updatedItems = items.map((i: ${MODULE}ItemConfig) => 
        i.id === itemId ? updatedItem : i
      );
      
      const groupId = Object.keys(sections).find(key => 
        sections[key as keyof typeof sections].includes(section)
      );
      
      if (groupId) {
        updateSectionSettings(groupId, section.id, {
          ...config,
          items: updatedItems
        });
      }
    }
  };
  
  return (
    <div className="w-[320px] h-full bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-700 flex flex-col">
      {/* Header */}
      <div className="p-3 border-b border-gray-200 dark:border-gray-700">
        <div className="flex items-center gap-2">
          <button
            onClick={handleBack}
            className="p-1 hover:bg-gray-100 dark:hover:bg-gray-800 rounded"
          >
            <ArrowLeft className="w-4 h-4" />
          </button>
          <h3 className="text-sm font-medium">Edit Item</h3>
        </div>
      </div>
      
      {/* Content */}
      <div className="flex-1 overflow-y-auto p-3 space-y-4">
        {/* Title */}
        <div>
          <label className="text-xs text-gray-600 dark:text-gray-400">Title</label>
          <input
            type="text"
            value={localItem.title}
            onChange={(e) => handleChange('title', e.target.value)}
            className="w-full mt-1 px-2 py-1.5 text-sm border rounded"
            placeholder="Enter title"
          />
        </div>
        
        {/* TODO: Add more configuration fields */}
        
        {/* Visibility Toggle */}
        <div className="flex items-center justify-between pt-2 border-t">
          <span className="text-xs text-gray-600 dark:text-gray-400">Visible</span>
          <button
            onClick={() => handleChange('visible', !localItem.visible)}
            className={\`w-10 h-5 rounded-full transition-colors \${
              localItem.visible ? 'bg-blue-600' : 'bg-gray-300'
            }\`}
          >
            <div className={\`w-4 h-4 rounded-full bg-white transition-transform \${
              localItem.visible ? 'translate-x-5' : 'translate-x-0.5'
            }\`} />
          </button>
        </div>
      </div>
    </div>
  );
}
EOF
fi

# 5. Crear index.ts
cat > index.ts << EOF
/**
 * @file index.ts
 * @module $MODULE
 * Module exports
 */

export { default as ${MODULE}Editor } from './${MODULE}Editor';
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> index.ts << EOF
export { default as ${MODULE}Children } from './${MODULE}Children';
export { default as ${MODULE}ItemEditor } from './${MODULE}ItemEditor';
EOF
fi

cat >> index.ts << EOF
export * from './types';
EOF

# 6. Crear archivo de integración con instrucciones
cat > INTEGRATION.md << EOF
# Integración del módulo $MODULE

## 1. Agregar tipo de sección en editor.types.ts

\`\`\`typescript
export enum SectionType {
  // ... otros tipos
  ${MODULE^^} = '${MODULE,,}',
}
\`\`\`

## 2. Agregar configuración en editor.types.ts

\`\`\`typescript
export const SECTION_CONFIGS: Record<SectionType, SectionConfig> = {
  // ... otras configuraciones
  [SectionType.${MODULE^^}]: {
    name: '$MODULE',
    category: 'template', // o 'structural' si es estructural
    defaultSettings: getDefault${MODULE}Config(),
    maxInstances: 1, // o el número que necesites
    allowedPages: [PageType.HOME, PageType.PRODUCT, /* etc */]
  },
};
\`\`\`

## 3. Agregar en ConfigPanel.tsx

\`\`\`typescript
import ${MODULE}Editor from './modules/$MODULE/${MODULE}Editor';

// En renderConfigFields()
case SectionType.${MODULE^^}:
  return <${MODULE}Editor sectionId={section.id} />;
\`\`\`

EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> INTEGRATION.md << EOF
## 4. Para módulos con hijos - En EditorSidebarWithDnD.tsx

\`\`\`typescript
import ${MODULE}Children from './modules/$MODULE/${MODULE}Children';

// Después de otros children renders
{section.type === SectionType.${MODULE^^} && section.visible && (
  <${MODULE}Children section={section} groupId={group.id} />
)}
\`\`\`

## 5. Para módulos con hijos - Agregar sección virtual en EditorSidebarWithDnD.tsx

\`\`\`typescript
// Check if it's a ${MODULE} item (virtual section)
if (!selectedSection && selectedSectionId?.includes(':item:')) {
  const [sectionId] = selectedSectionId.split(':item:');
  const parentSection = Object.values(sections).flat().find(s => s.id === sectionId);
  
  if (parentSection?.type === SectionType.${MODULE^^}) {
    selectedSection = {
      id: selectedSectionId,
      type: '${MODULE^^}_ITEM' as any,
      name: '${MODULE} Item',
      visible: true,
      settings: parentSection?.settings || {},
      sortOrder: 0
    } as any;
  }
}
\`\`\`

## 6. Para módulos con hijos - En ConfigPanel.tsx

\`\`\`typescript
import ${MODULE}ItemEditor from './modules/$MODULE/${MODULE}ItemEditor';

// Detectar si es un item del módulo
const is${MODULE}Item = selectedSectionId?.includes(':item:') && 
  section.type === SectionType.${MODULE^^};

// En los returns tempranos
if (is${MODULE}Item) {
  const sectionId = selectedSectionId.split(':item:')[0];
  const itemId = selectedSectionId.split(':item:')[1];
  if (sectionId && itemId) {
    return <${MODULE}ItemEditor sectionId={sectionId} itemId={itemId} />;
  }
}
\`\`\`
EOF
fi

cat >> INTEGRATION.md << EOF

## 7. Agregar al backend (si necesita persistencia)

En Models/PageSection.cs:
\`\`\`csharp
public const string ${MODULE^^} = "${MODULE}";

// En ModularTypes o el HashSet correspondiente
ModularTypes.Add(${MODULE^^});
AllTypes.Add(${MODULE^^});
\`\`\`

EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> INTEGRATION.md << EOF
// Si tiene hijos, agregarlo también a:
SectionsWithChildren.Add(${MODULE^^});
EOF
fi

cat >> INTEGRATION.md << EOF

En Services/WebsiteBuilderService.cs:
\`\`\`csharp
"${MODULE,,}" => SectionTypes.${MODULE^^},
\`\`\`

## 8. Testing checklist

- [ ] El módulo aparece en el modal de agregar secciones
- [ ] Se puede agregar el módulo al editor
- [ ] El editor de configuración abre correctamente
- [ ] Los cambios se guardan y persisten
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> INTEGRATION.md << EOF
- [ ] Se pueden agregar items hijos
- [ ] El drag & drop funciona para reordenar items
- [ ] Click en item hijo abre su editor
- [ ] El botón volver en item editor regresa al sidebar principal
- [ ] Los items se ven con la indentación correcta
EOF
fi

echo ""
echo "✅ Módulo $MODULE creado exitosamente en $BASE_DIR/$MODULE/"
echo ""
echo "📋 Estructura creada:"
echo "   - types.ts"
echo "   - ${MODULE}Editor.tsx"

if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo "   - ${MODULE}Children.tsx"
echo "   - ${MODULE}ItemEditor.tsx"
fi

echo "   - index.ts"
echo "   - INTEGRATION.md (instrucciones de integración)"
echo ""
echo "⚠️  IMPORTANTE: Lee INTEGRATION.md para los pasos de integración"
echo ""
echo "📝 Próximos pasos:"
echo "   1. Revisar y personalizar los archivos generados"
echo "   2. Seguir las instrucciones en INTEGRATION.md"
echo "   3. Probar el módulo en el editor"
echo ""

if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo "🎯 Módulo creado con soporte para elementos hijos"
echo "   - Drag & drop habilitado"
echo "   - Editor de items incluido"
echo "   - Navegación correcta configurada"
fi