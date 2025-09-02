#!/bin/bash

# create-module-complete.sh - Script completo para crear módulos con preview y responsive
# Incluye: Editor, Preview (desktop/mobile), Children support, y toda la integración

if [ -z "$1" ]; then
    echo "❌ Error: Debes proporcionar un nombre de módulo"
    echo "Uso: ./create-module-complete.sh [NombreModulo] [--with-children]"
    echo "Ejemplo: ./create-module-complete.sh Gallery --with-children"
    exit 1
fi

MODULE=$1
WITH_CHILDREN=$2
EDITOR_DIR="websitebuilder-admin/src/components/editor/modules"
PREVIEW_DIR="websitebuilder-admin/src/components/preview"

# Verificar si el módulo ya existe
if [ -d "$EDITOR_DIR/$MODULE" ]; then
    echo "❌ Error: El módulo $MODULE ya existe"
    exit 1
fi

echo "🚀 Creando módulo completo: $MODULE"
echo "   ✅ Editor components"
echo "   ✅ Preview component con responsive"
echo "   ✅ Integración completa"

# Crear estructura de directorios
mkdir -p "$EDITOR_DIR/$MODULE"
mkdir -p "$PREVIEW_DIR"

# ============================================
# 1. CREAR TYPES.TS
# ============================================
cat > "$EDITOR_DIR/$MODULE/types.ts" << EOF
/**
 * @file types.ts
 * @max-lines 100
 * @module $MODULE
 * Type definitions for $MODULE module
 */

export interface ${MODULE}Config {
  // General settings
  enabled: boolean;
  colorScheme: '1' | '2' | '3' | '4' | '5';
  
  // Layout settings
  width: 'screen' | 'page' | 'large';
  
  // Desktop specific
  desktopLayout: 'grid' | 'list' | 'carousel';
  desktopColumns: number; // 1-4
  
  // Mobile specific
  mobileLayout: 'stack' | 'carousel';
  mobileColumns: number; // 1-2
  
  // Spacing
  topPadding: number; // 0-120px
  bottomPadding: number; // 0-120px
  itemSpacing: number; // 0-40px
  
  // Content
  showTitle: boolean;
  title: string;
  subtitle: string;
EOF

# Si tiene hijos, agregar array de items
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF
  
  // Items
  items: ${MODULE}ItemConfig[];
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF
}

EOF

# Si tiene hijos, agregar tipos para items
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF
export interface ${MODULE}ItemConfig {
  id: string;
  visible: boolean;
  
  // Content
  title: string;
  description: string;
  
  // Media
  desktopImage: string;
  mobileImage: string;
  imageAlt: string;
  
  // Link
  linkUrl: string;
  linkText: string;
  
  // Desktop positioning
  desktopAlignment: 'left' | 'center' | 'right';
  
  // Mobile positioning  
  mobileAlignment: 'left' | 'center';
}

export function getDefault${MODULE}ItemConfig(): ${MODULE}ItemConfig {
  return {
    id: \`item-\${Date.now()}\`,
    visible: true,
    title: 'Item Title',
    description: 'Item description goes here',
    desktopImage: '',
    mobileImage: '',
    imageAlt: '',
    linkUrl: '',
    linkText: 'Learn More',
    desktopAlignment: 'left',
    mobileAlignment: 'center'
  };
}
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF

export function getDefault${MODULE}Config(): ${MODULE}Config {
  return {
    enabled: true,
    colorScheme: '1',
    width: 'page',
    desktopLayout: 'grid',
    desktopColumns: 3,
    mobileLayout: 'stack',
    mobileColumns: 1,
    topPadding: 48,
    bottomPadding: 48,
    itemSpacing: 20,
    showTitle: true,
    title: '$MODULE Section',
    subtitle: 'Add a description here',
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF
    items: []
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/types.ts" << EOF
  };
}
EOF

# ============================================
# 2. CREAR EDITOR PRINCIPAL
# ============================================
cat > "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF
/**
 * @file ${MODULE}Editor.tsx
 * @max-lines 300
 * @module $MODULE
 * Main editor component for $MODULE configuration
 */

'use client';

import React, { useState, useEffect } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useEditorStore } from '@/stores/useEditorStore';
import { Slider } from '@/components/ui/slider';
import { ${MODULE}Config, getDefault${MODULE}Config } from './types';
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF
import ${MODULE}Children from './${MODULE}Children';
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF

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
    layout: false,
    spacing: false,
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF
    items: false,
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF
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

  const handleChange = (updates: Partial<${MODULE}Config>) => {
    const updatedConfig = { ...localConfig, ...updates };
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
              {/* Title */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Title</label>
                <input
                  type="text"
                  value={localConfig.title}
                  onChange={(e) => handleChange({ title: e.target.value })}
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                />
              </div>

              {/* Subtitle */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Subtitle</label>
                <textarea
                  value={localConfig.subtitle}
                  onChange={(e) => handleChange({ subtitle: e.target.value })}
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                  rows={2}
                />
              </div>

              {/* Color Scheme */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Color scheme</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                  value={localConfig.colorScheme}
                  onChange={(e) => handleChange({ colorScheme: e.target.value as any })}
                >
                  <option value="1">Color scheme 1</option>
                  <option value="2">Color scheme 2</option>
                  <option value="3">Color scheme 3</option>
                  <option value="4">Color scheme 4</option>
                  <option value="5">Color scheme 5</option>
                </select>
              </div>

              {/* Show Title Toggle */}
              <div className="flex items-center justify-between">
                <span className="text-xs text-gray-600 dark:text-gray-400">Show Title</span>
                <button
                  onClick={() => handleChange({ showTitle: !localConfig.showTitle })}
                  className={\`w-10 h-5 rounded-full transition-colors \${
                    localConfig.showTitle ? 'bg-blue-600' : 'bg-gray-300'
                  }\`}
                >
                  <div className={\`w-4 h-4 rounded-full bg-white transition-transform \${
                    localConfig.showTitle ? 'translate-x-5' : 'translate-x-0.5'
                  }\`} />
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Layout Settings */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('layout')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              Layout & Responsive
            </span>
            {expandedSections.layout ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.layout && (
            <div className="px-3 pb-3 space-y-4">
              {/* Width */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400 mb-2 block">Width</label>
                <div className="flex gap-2">
                  {['screen', 'page', 'large'].map((width) => (
                    <button
                      key={width}
                      onClick={() => handleChange({ width: width as any })}
                      className={\`flex-1 py-1.5 px-2 text-xs rounded border transition-colors \${
                        localConfig.width === width
                          ? 'bg-blue-600 text-white border-blue-600'
                          : 'bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-600'
                      }\`}
                    >
                      {width.charAt(0).toUpperCase() + width.slice(1)}
                    </button>
                  ))}
                </div>
              </div>

              {/* Desktop Layout */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400 font-medium">Desktop Layout</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                  value={localConfig.desktopLayout}
                  onChange={(e) => handleChange({ desktopLayout: e.target.value as any })}
                >
                  <option value="grid">Grid</option>
                  <option value="list">List</option>
                  <option value="carousel">Carousel</option>
                </select>
              </div>

              {/* Desktop Columns */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">
                  Desktop Columns: {localConfig.desktopColumns}
                </label>
                <Slider
                  value={[localConfig.desktopColumns]}
                  onValueChange={([v]) => handleChange({ desktopColumns: v })}
                  min={1}
                  max={4}
                  step={1}
                  className="mt-2"
                />
              </div>

              {/* Mobile Layout */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400 font-medium">Mobile Layout</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                  value={localConfig.mobileLayout}
                  onChange={(e) => handleChange({ mobileLayout: e.target.value as any })}
                >
                  <option value="stack">Stack</option>
                  <option value="carousel">Carousel</option>
                </select>
              </div>

              {/* Mobile Columns */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">
                  Mobile Columns: {localConfig.mobileColumns}
                </label>
                <Slider
                  value={[localConfig.mobileColumns]}
                  onValueChange={([v]) => handleChange({ mobileColumns: v })}
                  min={1}
                  max={2}
                  step={1}
                  className="mt-2"
                />
              </div>
            </div>
          )}
        </div>

        {/* Spacing Settings */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('spacing')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              Spacing
            </span>
            {expandedSections.spacing ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.spacing && (
            <div className="px-3 pb-3 space-y-3">
              {/* Top Padding */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">
                  Top Padding: {localConfig.topPadding}px
                </label>
                <Slider
                  value={[localConfig.topPadding]}
                  onValueChange={([v]) => handleChange({ topPadding: v })}
                  min={0}
                  max={120}
                  step={4}
                  className="mt-2"
                />
              </div>

              {/* Bottom Padding */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">
                  Bottom Padding: {localConfig.bottomPadding}px
                </label>
                <Slider
                  value={[localConfig.bottomPadding]}
                  onValueChange={([v]) => handleChange({ bottomPadding: v })}
                  min={0}
                  max={120}
                  step={4}
                  className="mt-2"
                />
              </div>

              {/* Item Spacing */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">
                  Item Spacing: {localConfig.itemSpacing}px
                </label>
                <Slider
                  value={[localConfig.itemSpacing]}
                  onValueChange={([v]) => handleChange({ itemSpacing: v })}
                  min={0}
                  max={40}
                  step={2}
                  className="mt-2"
                />
              </div>
            </div>
          )}
        </div>
EOF

# Si tiene hijos, agregar sección de items
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF

        {/* Items Management */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('items')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              Items ({localConfig.items?.length || 0})
            </span>
            {expandedSections.items ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.items && (
            <${MODULE}Children
              items={localConfig.items || []}
              onChange={(items) => handleChange({ items })}
              companyId={1} // TODO: Get from context
            />
          )}
        </div>
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/${MODULE}Editor.tsx" << EOF

      </div>
    </div>
  );
}
EOF

# ============================================
# 3. CREAR PREVIEW COMPONENT (CRÍTICO!)
# ============================================
cat > "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
/**
 * @file Preview${MODULE}.tsx
 * @description Preview component for $MODULE with responsive design
 * @max-lines 300
 */

'use client';

import React from 'react';
import { ${MODULE}Config } from '@/components/editor/modules/$MODULE/types';

interface Preview${MODULE}Props {
  settings: ${MODULE}Config;
  isEditor?: boolean;
  theme?: any;
  deviceView?: 'desktop' | 'mobile';
}

export default function Preview${MODULE}({ 
  settings, 
  isEditor = false,
  theme,
  deviceView = 'desktop'
}: Preview${MODULE}Props) {
  
  // Get color scheme
  const selectedSchemeIndex = parseInt(settings.colorScheme || '1') - 1;
  const colorScheme = theme?.colorSchemes?.schemes?.[selectedSchemeIndex] || 
                      theme?.colorSchemes?.schemes?.[0];
  
  // Get typography styles
  const headingStyles = theme?.typography?.headings ? {
    fontFamily: \`'\${theme.typography.headings.fontFamily}', sans-serif\`,
    fontWeight: theme.typography.headings.fontWeight || '700',
    fontSize: theme.typography.headings.fontSize ? 
      \`\${theme.typography.headings.fontSize}px\` : '32px',
    textTransform: theme.typography.headings.useUppercase ? 'uppercase' as const : 'none' as const,
    letterSpacing: \`\${theme.typography.headings.letterSpacing || 0}px\`
  } : {};
  
  const bodyStyles = theme?.typography?.body ? {
    fontFamily: \`'\${theme.typography.body.fontFamily}', sans-serif\`,
    fontWeight: theme.typography.body.fontWeight || '400',
    fontSize: theme.typography.body.fontSize ? 
      \`\${theme.typography.body.fontSize}px\` : '16px',
    letterSpacing: \`\${theme.typography.body.letterSpacing || 0}px\`
  } : {};
  
  // Check if mobile view
  const isMobile = deviceView === 'mobile';
  
  // Get responsive settings
  const layout = isMobile ? settings.mobileLayout : settings.desktopLayout;
  const columns = isMobile ? settings.mobileColumns : settings.desktopColumns;
  
  // Width classes
  const getWidthClass = () => {
    if (settings.width === 'screen') return 'w-full';
    if (settings.width === 'large') return 'max-w-7xl mx-auto px-4';
    return 'max-w-6xl mx-auto px-4';
  };
  
  // Grid classes for responsive layout
  const getGridClass = () => {
    if (layout === 'list') return 'flex flex-col space-y-4';
    if (layout === 'carousel' || layout === 'stack') {
      return isMobile ? 'flex flex-col space-y-4' : 'flex overflow-x-auto space-x-4';
    }
    
    // Grid layout
    const colClasses = {
      1: 'grid-cols-1',
      2: isMobile ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-2',
      3: isMobile ? 'grid-cols-1' : 'grid-cols-3',
      4: isMobile ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-4'
    };
    return \`grid \${colClasses[columns as keyof typeof colClasses] || 'grid-cols-1'} gap-\${settings.itemSpacing / 4 || 4}\`;
  };
  
  // If disabled, don't render
  if (!settings.enabled) return null;
  
  return (
    <div 
      className={\`relative \${settings.colorBackground ? 'bg-gray-50 dark:bg-gray-900' : ''}\`}
      style={{
        paddingTop: \`\${settings.topPadding}px\`,
        paddingBottom: \`\${settings.bottomPadding}px\`,
        backgroundColor: settings.colorBackground ? colorScheme?.backgroundColor : undefined,
      }}
    >
      <div className={getWidthClass()}>
        {/* Title Section */}
        {settings.showTitle && (
          <div className="text-center mb-8">
            <h2 
              className="text-3xl font-bold mb-2"
              style={{
                ...headingStyles,
                color: colorScheme?.primaryTextColor || '#000',
                fontSize: isMobile ? '24px' : headingStyles.fontSize
              }}
            >
              {settings.title}
            </h2>
            {settings.subtitle && (
              <p 
                className="text-lg"
                style={{
                  ...bodyStyles,
                  color: colorScheme?.secondaryTextColor || '#666',
                  fontSize: isMobile ? '14px' : bodyStyles.fontSize
                }}
              >
                {settings.subtitle}
              </p>
            )}
          </div>
        )}
        
        {/* Items Grid/List */}
        <div className={getGridClass()}>
EOF

# Si tiene hijos, renderizar items
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
          {settings.items?.filter(item => item.visible).map((item, index) => (
            <div 
              key={item.id} 
              className="bg-white dark:bg-gray-800 rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow"
            >
              {/* Item Image */}
              {(isMobile ? item.mobileImage : item.desktopImage) && (
                <div className="aspect-video bg-gray-200">
                  <img
                    src={isMobile ? item.mobileImage : item.desktopImage}
                    alt={item.imageAlt || item.title}
                    className="w-full h-full object-cover"
                  />
                </div>
              )}
              
              {/* Item Content */}
              <div className={\`p-4 text-\${isMobile ? item.mobileAlignment : item.desktopAlignment}\`}>
                <h3 
                  className="font-semibold mb-2"
                  style={{
                    ...headingStyles,
                    fontSize: isMobile ? '18px' : '20px',
                    color: colorScheme?.primaryTextColor
                  }}
                >
                  {item.title}
                </h3>
                
                {item.description && (
                  <p 
                    className="text-sm mb-3"
                    style={{
                      ...bodyStyles,
                      fontSize: isMobile ? '14px' : '16px',
                      color: colorScheme?.secondaryTextColor
                    }}
                  >
                    {item.description}
                  </p>
                )}
                
                {item.linkUrl && item.linkText && (
                  <a 
                    href={item.linkUrl}
                    className="inline-block px-4 py-2 rounded transition-colors"
                    style={{
                      backgroundColor: colorScheme?.buttonBackgroundColor || '#000',
                      color: colorScheme?.buttonTextColor || '#fff'
                    }}
                  >
                    {item.linkText}
                  </a>
                )}
              </div>
            </div>
          ))}
EOF
else
cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
          {/* Placeholder items for demo */}
          {[1, 2, 3].slice(0, columns).map((i) => (
            <div 
              key={i}
              className="bg-white dark:bg-gray-800 rounded-lg p-6 shadow-sm"
            >
              <div className="h-32 bg-gray-200 dark:bg-gray-700 rounded mb-4"></div>
              <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded mb-2"></div>
              <div className="h-4 bg-gray-100 dark:bg-gray-600 rounded w-3/4"></div>
            </div>
          ))}
EOF
fi

cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
        </div>
        
        {/* Empty State */}
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
        {(!settings.items || settings.items.length === 0) && (
          <div className="text-center py-12 text-gray-500">
            <p>No items added yet. Add items to see them here.</p>
          </div>
        )}
EOF
fi

cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
      </div>
    </div>
  );
}
EOF

# ============================================
# 4. SI TIENE HIJOS, CREAR CHILDREN COMPONENT
# ============================================
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat > "$EDITOR_DIR/$MODULE/${MODULE}Children.tsx" << EOF
/**
 * @file ${MODULE}Children.tsx
 * @max-lines 300
 * @module $MODULE
 * Manages ${MODULE} items with drag & drop
 */

'use client';

import React, { useState } from 'react';
import { Plus, Eye, EyeOff, Trash2, GripVertical, ChevronDown, ChevronUp, Upload } from 'lucide-react';
import { DndContext, DragEndEvent } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { ${MODULE}ItemConfig, getDefault${MODULE}ItemConfig } from './types';

interface ${MODULE}ChildrenProps {
  items: ${MODULE}ItemConfig[];
  onChange: (items: ${MODULE}ItemConfig[]) => void;
  companyId: number;
}

interface SortableItemProps {
  item: ${MODULE}ItemConfig;
  index: number;
  expanded: boolean;
  onToggle: () => void;
  onUpdate: (updates: Partial<${MODULE}ItemConfig>) => void;
  onDelete: () => void;
  companyId: number;
}

function SortableItem({ 
  item, 
  index, 
  expanded,
  onToggle,
  onUpdate,
  onDelete,
  companyId
}: SortableItemProps) {
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

  const handleImageUpload = async (file: File, type: 'desktop' | 'mobile') => {
    // TODO: Implement actual upload logic
    const url = URL.createObjectURL(file);
    onUpdate({
      [type === 'desktop' ? 'desktopImage' : 'mobileImage']: url
    });
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={\`bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg mb-2 \${
        isDragging ? 'shadow-lg' : ''
      }\`}
    >
      <div className="flex items-center px-3 py-2">
        {/* Drag Handle */}
        <button
          className="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded cursor-grab active:cursor-grabbing"
          {...attributes}
          {...listeners}
        >
          <GripVertical className="w-4 h-4 text-gray-400" />
        </button>

        {/* Item Title */}
        <span className="flex-1 ml-2 text-sm font-medium">
          {item.title || \`Item \${index + 1}\`}
        </span>

        {/* Actions */}
        <div className="flex items-center gap-1">
          <button
            onClick={() => onUpdate({ visible: !item.visible })}
            className="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded"
            title={item.visible ? 'Hide' : 'Show'}
          >
            {item.visible ? (
              <Eye className="w-4 h-4 text-gray-500" />
            ) : (
              <EyeOff className="w-4 h-4 text-gray-400" />
            )}
          </button>
          
          <button
            onClick={onToggle}
            className="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded"
          >
            {expanded ? (
              <ChevronUp className="w-4 h-4 text-gray-500" />
            ) : (
              <ChevronDown className="w-4 h-4 text-gray-500" />
            )}
          </button>
          
          <button
            onClick={onDelete}
            className="p-1 hover:bg-red-100 dark:hover:bg-red-900/20 rounded"
            title="Delete"
          >
            <Trash2 className="w-4 h-4 text-gray-500 hover:text-red-600" />
          </button>
        </div>
      </div>

      {/* Expanded Content */}
      {expanded && (
        <div className="px-3 pb-3 border-t border-gray-200 dark:border-gray-700 mt-2 pt-3 space-y-3">
          {/* Title */}
          <div>
            <label className="text-xs text-gray-600 dark:text-gray-400">Title</label>
            <input
              type="text"
              value={item.title}
              onChange={(e) => onUpdate({ title: e.target.value })}
              className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
            />
          </div>

          {/* Description */}
          <div>
            <label className="text-xs text-gray-600 dark:text-gray-400">Description</label>
            <textarea
              value={item.description}
              onChange={(e) => onUpdate({ description: e.target.value })}
              className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
              rows={2}
            />
          </div>

          {/* Desktop Image */}
          <div>
            <label className="text-xs text-gray-600 dark:text-gray-400">Desktop Image</label>
            <div className="mt-1">
              {item.desktopImage ? (
                <div className="relative">
                  <img 
                    src={item.desktopImage} 
                    alt="Desktop" 
                    className="w-full h-24 object-cover rounded"
                  />
                  <button
                    onClick={() => onUpdate({ desktopImage: '' })}
                    className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded"
                  >
                    <Trash2 className="w-3 h-3" />
                  </button>
                </div>
              ) : (
                <label className="flex items-center justify-center h-24 border-2 border-dashed rounded cursor-pointer hover:border-blue-500">
                  <input
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={(e) => {
                      const file = e.target.files?.[0];
                      if (file) handleImageUpload(file, 'desktop');
                    }}
                  />
                  <div className="text-center">
                    <Upload className="w-6 h-6 mx-auto text-gray-400" />
                    <span className="text-xs text-gray-500">Upload Image</span>
                  </div>
                </label>
              )}
            </div>
          </div>

          {/* Mobile Image */}
          <div>
            <label className="text-xs text-gray-600 dark:text-gray-400">Mobile Image</label>
            <div className="mt-1">
              {item.mobileImage ? (
                <div className="relative">
                  <img 
                    src={item.mobileImage} 
                    alt="Mobile" 
                    className="w-full h-24 object-cover rounded"
                  />
                  <button
                    onClick={() => onUpdate({ mobileImage: '' })}
                    className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded"
                  >
                    <Trash2 className="w-3 h-3" />
                  </button>
                </div>
              ) : (
                <label className="flex items-center justify-center h-24 border-2 border-dashed rounded cursor-pointer hover:border-blue-500">
                  <input
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={(e) => {
                      const file = e.target.files?.[0];
                      if (file) handleImageUpload(file, 'mobile');
                    }}
                  />
                  <div className="text-center">
                    <Upload className="w-6 h-6 mx-auto text-gray-400" />
                    <span className="text-xs text-gray-500">Upload Image</span>
                  </div>
                </label>
              )}
            </div>
          </div>

          {/* Link */}
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="text-xs text-gray-600 dark:text-gray-400">Link URL</label>
              <input
                type="text"
                value={item.linkUrl}
                onChange={(e) => onUpdate({ linkUrl: e.target.value })}
                className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                placeholder="/products"
              />
            </div>
            <div>
              <label className="text-xs text-gray-600 dark:text-gray-400">Link Text</label>
              <input
                type="text"
                value={item.linkText}
                onChange={(e) => onUpdate({ linkText: e.target.value })}
                className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                placeholder="Learn More"
              />
            </div>
          </div>

          {/* Alignment */}
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="text-xs text-gray-600 dark:text-gray-400">Desktop Align</label>
              <select 
                className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                value={item.desktopAlignment}
                onChange={(e) => onUpdate({ desktopAlignment: e.target.value as any })}
              >
                <option value="left">Left</option>
                <option value="center">Center</option>
                <option value="right">Right</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-600 dark:text-gray-400">Mobile Align</label>
              <select 
                className="w-full mt-1 px-2 py-1.5 text-sm border rounded dark:bg-gray-800 dark:border-gray-600"
                value={item.mobileAlignment}
                onChange={(e) => onUpdate({ mobileAlignment: e.target.value as any })}
              >
                <option value="left">Left</option>
                <option value="center">Center</option>
              </select>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default function ${MODULE}Children({ items, onChange, companyId }: ${MODULE}ChildrenProps) {
  const [expandedItems, setExpandedItems] = useState<Set<string>>(new Set());

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;

    const oldIndex = items.findIndex(i => i.id === active.id);
    const newIndex = items.findIndex(i => i.id === over.id);
    
    if (oldIndex !== -1 && newIndex !== -1) {
      onChange(arrayMove(items, oldIndex, newIndex));
    }
  };

  const handleAddItem = () => {
    const newItem = getDefault${MODULE}ItemConfig();
    onChange([...items, newItem]);
    setExpandedItems(new Set([newItem.id]));
  };

  const handleUpdateItem = (id: string, updates: Partial<${MODULE}ItemConfig>) => {
    onChange(items.map(item => 
      item.id === id ? { ...item, ...updates } : item
    ));
  };

  const handleDeleteItem = (id: string) => {
    onChange(items.filter(item => item.id !== id));
    setExpandedItems(prev => {
      const next = new Set(prev);
      next.delete(id);
      return next;
    });
  };

  const toggleExpanded = (id: string) => {
    setExpandedItems(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  return (
    <div className="p-3">
      <DndContext onDragEnd={handleDragEnd}>
        <SortableContext items={items.map(i => i.id)} strategy={verticalListSortingStrategy}>
          {items.map((item, index) => (
            <SortableItem
              key={item.id}
              item={item}
              index={index}
              expanded={expandedItems.has(item.id)}
              onToggle={() => toggleExpanded(item.id)}
              onUpdate={(updates) => handleUpdateItem(item.id, updates)}
              onDelete={() => handleDeleteItem(item.id)}
              companyId={companyId}
            />
          ))}
        </SortableContext>
      </DndContext>
      
      <button
        onClick={handleAddItem}
        className="w-full mt-3 py-2 px-3 text-sm text-blue-600 dark:text-blue-400 
                   border border-dashed border-blue-300 dark:border-blue-700 
                   rounded-lg hover:bg-blue-50 dark:hover:bg-blue-900/20 
                   transition-colors flex items-center justify-center gap-2"
      >
        <Plus className="w-4 h-4" />
        Add Item
      </button>
    </div>
  );
}
EOF
fi

# ============================================
# 5. CREAR INDEX.TS
# ============================================
cat > "$EDITOR_DIR/$MODULE/index.ts" << EOF
/**
 * @file index.ts
 * @module $MODULE
 * Module exports
 */

export { default as ${MODULE}Editor } from './${MODULE}Editor';
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/index.ts" << EOF
export { default as ${MODULE}Children } from './${MODULE}Children';
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/index.ts" << EOF
export * from './types';
EOF

# ============================================
# 6. CREAR INTEGRATION.md CON PREVIEW
# ============================================
cat > "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF
# 📋 Integración Completa del Módulo $MODULE

## ✅ Archivos Creados
- ✅ \`/components/editor/modules/$MODULE/types.ts\`
- ✅ \`/components/editor/modules/$MODULE/${MODULE}Editor.tsx\`
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF
- ✅ \`/components/editor/modules/$MODULE/${MODULE}Children.tsx\`
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF
- ✅ \`/components/editor/modules/$MODULE/index.ts\`
- ✅ \`/components/preview/Preview${MODULE}.tsx\` (**CRÍTICO para visualización**)

## 🔧 PASOS DE INTEGRACIÓN

### 1. Agregar tipo de sección en \`editor.types.ts\`

\`\`\`typescript
export enum SectionType {
  // ... otros tipos
  ${MODULE^^} = '${MODULE,,}',
}
\`\`\`

### 2. Agregar configuración en \`editor.types.ts\`

\`\`\`typescript
import { getDefault${MODULE}Config } from '@/components/editor/modules/$MODULE/types';

export const SECTION_CONFIGS: Record<SectionType, SectionConfig> = {
  // ... otras configuraciones
  [SectionType.${MODULE^^}]: {
    name: '$MODULE',
    category: 'template', // o 'structural' si es estructural
    defaultSettings: getDefault${MODULE}Config(),
    maxInstances: 5, // o el número que necesites
    allowedPages: [PageType.HOME, PageType.PRODUCT, PageType.COLLECTION]
  },
};
\`\`\`

### 3. Agregar Editor en \`ConfigPanel.tsx\`

\`\`\`typescript
import ${MODULE}Editor from './modules/$MODULE/${MODULE}Editor';

// En renderConfigFields()
case SectionType.${MODULE^^}:
  return <${MODULE}Editor sectionId={section.id} />;
\`\`\`

### 4. 🎨 AGREGAR PREVIEW EN \`EditorPreview.tsx\` (CRÍTICO!)

\`\`\`typescript
// Al inicio del archivo, agregar import
import Preview${MODULE} from '@/components/preview/Preview${MODULE}';

// En la función renderSection(), agregar case:
case SectionType.${MODULE^^}:
  return (
    <Preview${MODULE}
      settings={section.settings}
      isEditor={true}
      theme={themeConfig}
      deviceView={deviceView}
    />
  );
\`\`\`

**⚠️ SIN ESTE PASO, EL MÓDULO NO SE VISUALIZARÁ**
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF

### 5. Para módulos con hijos - En \`EditorSidebarWithDnD.tsx\`

\`\`\`typescript
import ${MODULE}Children from './modules/$MODULE/${MODULE}Children';

// Después de renderizar el título de la sección, agregar:
{section.type === SectionType.${MODULE^^} && section.visible && (
  <${MODULE}Children 
    items={section.settings?.items || []}
    onChange={(items) => {
      updateSectionSettings(group.id, section.id, {
        ...section.settings,
        items
      });
    }}
    companyId={1} // TODO: Get from context
  />
)}
\`\`\`
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF

### 6. Agregar al Backend (Models/PageSection.cs)

\`\`\`csharp
// En la clase SectionTypes
public const string ${MODULE^^} = "${MODULE}";

// En ModularTypes HashSet
ModularTypes.Add(${MODULE^^});

// En AllTypes HashSet  
AllTypes.Add(${MODULE^^});
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF

// Si tiene hijos, también agregar a:
SectionsWithChildren.Add(${MODULE^^});
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF
\`\`\`

### 7. Agregar al Backend (Services/WebsiteBuilderService.cs)

\`\`\`csharp
// En el método MapSectionTypeFromString
"${MODULE,,}" => SectionTypes.${MODULE^^},

// En el método MapSectionTypeToString  
SectionTypes.${MODULE^^} => "${MODULE,,}",
\`\`\`

## ✅ Testing Checklist

### Editor
- [ ] El módulo aparece en el modal de agregar secciones
- [ ] Se puede agregar el módulo al editor
- [ ] El editor de configuración abre correctamente
- [ ] Los cambios se guardan y persisten

### Preview (CRÍTICO)
- [ ] **El preview se renderiza correctamente**
- [ ] **Los estilos del theme se aplican**
- [ ] **La vista móvil funciona (responsive)**
- [ ] **Los color schemes se aplican correctamente**
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF

### Items/Children
- [ ] Se pueden agregar items hijos
- [ ] El drag & drop funciona para reordenar
- [ ] Las imágenes se pueden subir
- [ ] Desktop y mobile tienen configuraciones separadas
- [ ] Los items se muestran en el preview
EOF
fi

cat >> "$EDITOR_DIR/$MODULE/INTEGRATION.md" << EOF

## 🎨 Características Responsive Implementadas

### Desktop View
- Layout: Grid / List / Carousel
- Columns: 1-4 configurables
- Tamaños de fuente completos
- Imágenes desktop específicas

### Mobile View  
- Layout: Stack / Carousel
- Columns: 1-2 configurables
- Tamaños de fuente reducidos
- Imágenes mobile específicas
- Alineación centrada por defecto

## 🐛 Troubleshooting

### "Preview coming soon..." aparece en lugar del módulo
**Causa**: Falta agregar el Preview en EditorPreview.tsx
**Solución**: Ver paso 4 de integración

### El módulo no responde a cambios de theme
**Causa**: Preview no está usando las props de theme
**Solución**: Verificar que Preview${MODULE} recibe y usa la prop \`theme\`

### Vista móvil no funciona
**Causa**: Preview no está usando la prop deviceView
**Solución**: Verificar que Preview${MODULE} usa \`deviceView\` para responsive

## 📝 Notas Importantes

1. **SIEMPRE crear el Preview**: Sin él, el módulo es invisible
2. **Probar responsive**: Verificar desktop y mobile por separado
3. **Theme integration**: El preview debe respetar colorSchemes y typography
4. **Performance**: Para listas grandes, considerar virtualización

---
**Módulo creado**: $(date)
**Script version**: 2.0 (con Preview y Responsive)
EOF

echo ""
echo "✅ Módulo $MODULE creado COMPLETAMENTE en:"
echo "   📁 Editor: $EDITOR_DIR/$MODULE/"
echo "   📁 Preview: $PREVIEW_DIR/Preview${MODULE}.tsx"
echo ""
echo "📋 Estructura creada:"
echo "   ✅ types.ts - Tipos con configuración desktop/mobile"
echo "   ✅ ${MODULE}Editor.tsx - Editor principal con responsive settings"

if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo "   ✅ ${MODULE}Children.tsx - Gestión de items con drag & drop"
fi

echo "   ✅ Preview${MODULE}.tsx - PREVIEW CON RESPONSIVE (crítico!)"
echo "   ✅ index.ts - Exports del módulo"
echo "   ✅ INTEGRATION.md - Guía completa de integración"
echo ""
echo "⚠️  IMPORTANTE:"
echo "   1. Lee INTEGRATION.md para los pasos de integración"
echo "   2. NO OLVIDES agregar el Preview en EditorPreview.tsx"
echo "   3. Prueba las vistas desktop Y mobile"
echo ""
echo "🎯 Características incluidas:"
echo "   ✅ Editor con configuración completa"
echo "   ✅ Preview con renderizado real"
echo "   ✅ Soporte responsive (desktop/mobile)"
echo "   ✅ Integración con theme (colors, typography)"

if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo "   ✅ Items con drag & drop"
echo "   ✅ Upload de imágenes desktop/mobile"
echo "   ✅ Configuración individual por item"
fi

echo ""
echo "📊 Próximos pasos:"
echo "   1. Seguir TODOS los pasos en INTEGRATION.md"
echo "   2. Personalizar el Preview según tus necesidades"
echo "   3. Probar en ambas vistas (desktop/mobile)"
echo "   4. Verificar integración con theme"