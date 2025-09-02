#!/bin/bash

# ============================================================================
# create-template-section.sh - Script para crear SECCIONES DEL TEMPLATE
# ============================================================================
# 
# IMPORTANTE: Este script es SOLO para crear secciones del template que:
#   - Se agregan a páginas específicas (no aparecen en todas)
#   - Se guardan en la tabla PageSections
#   - Usan updateSectionSettings() del store
#   - Ejemplos: Gallery, Testimonials, FAQ, Videos, etc.
#
# NO USAR ESTE SCRIPT PARA:
#   - Componentes estructurales (Header, Footer, AnnouncementBar, CartDrawer)
#   - Esos usan StructuralComponentsContext y APIs diferentes
#
# Última sección creada exitosamente: Slideshow
# ============================================================================
#
# 🔴 ERRORES CRÍTICOS QUE SIEMPRE SE COMETEN (Y CÓMO EVITARLOS):
# ============================================================================
# 1. BOTÓN "Add block" DEBE SER AZUL Y LIMPIO (como Slideshow):
#    ✅ CORRECTO: text-blue-600, hover:bg-blue-50, icono en círculo azul
#    ❌ INCORRECTO: text-gray-600, hover:bg-gray-100
#
# 2. FLECHA DE REGRESO EN ITEMS VA AL SIDEBAR PRINCIPAL:
#    ✅ CORRECTO: selectSection(null) - Va al sidebar
#    ❌ INCORRECTO: selectSection(parentId) - Va al padre
#    ❌ INCORRECTO: Dos flechas de regreso
#
# 3. DRAG & DROP HANDLE SOLO VISIBLE ON HOVER:
#    ✅ CORRECTO: opacity-0 group-hover:opacity-100
#    ❌ INCORRECTO: Siempre visible
#
# 4. INDENT DE ITEMS CON FLECHA CHEVRON:
#    ✅ CORRECTO: <svg> con chevron + icono del tipo + título
#    ❌ INCORRECTO: Solo texto sin indentación visual
# ============================================================================

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Validar argumentos
if [ -z "$1" ]; then
    echo -e "${RED}❌ Error: Debes proporcionar un nombre de módulo${NC}"
    echo ""
    echo -e "${YELLOW}📚 USO:${NC}"
    echo "  ./create-template-section.sh [NombreSeccion] [--with-children]"
    echo ""
    echo -e "${YELLOW}📝 EJEMPLOS:${NC}"
    echo "  ./create-template-section.sh Gallery"
    echo "  ./create-template-section.sh Testimonials --with-children"
    echo "  ./create-template-section.sh FAQ --with-children"
    echo ""
    echo -e "${BLUE}ℹ️  NOTA:${NC}"
    echo "  Este script es SOLO para secciones del template (no componentes estructurales)"
    echo "  Las secciones del template:"
    echo "    • Aparecen en páginas específicas"
    echo "    • Se pueden agregar múltiples veces"
    echo "    • Se guardan con el botón Save de la página"
    echo "    • Usan updateSectionSettings() del store"
    exit 1
fi

MODULE=$1
WITH_CHILDREN=$2
BASE_DIR="websitebuilder-admin/src/components/editor/modules"

# Verificar si el módulo ya existe
if [ -d "$BASE_DIR/$MODULE" ]; then
    echo -e "${RED}❌ Error: El módulo $MODULE ya existe${NC}"
    exit 1
fi

echo -e "${GREEN}🚀 Creando SECCIÓN DEL TEMPLATE: $MODULE${NC}"
echo -e "${BLUE}📂 Ubicación: $BASE_DIR/$MODULE${NC}"
echo ""

# Confirmar tipo de módulo
echo -e "${YELLOW}⚠️  CONFIRMACIÓN:${NC}"
echo "¿Esta sección es del tipo que se agrega a páginas individuales?"
echo "(Como Gallery, Testimonials, FAQ, Videos, etc.)"
echo ""
echo "Si es un componente que aparece en TODAS las páginas (Header, Footer, etc.),"
echo "debes usar un script diferente para componentes estructurales."
echo ""
read -p "¿Continuar? (s/n): " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Ss]$ ]]; then
    echo -e "${YELLOW}Operación cancelada${NC}"
    exit 1
fi

# Crear estructura de directorios
mkdir -p "$BASE_DIR/$MODULE"
cd "$BASE_DIR/$MODULE"

echo -e "${GREEN}✅ Creando archivos para sección del template...${NC}"

# ============================================================================
# 1. CREAR types.ts - TIPOS PARA LA SECCIÓN
# ============================================================================
cat > types.ts << EOF
/**
 * @file types.ts
 * @max-lines 100
 * @module $MODULE
 * @description Tipos para la sección del template $MODULE
 * @template-section true
 */

import { SectionType } from '@/types/editor.types';

export interface ${MODULE}Config {
  // Configuración básica de la sección
  enabled: boolean;
  colorScheme: string;
  
  // Layout y apariencia
  width: 'screen' | 'page' | 'large' | 'medium';
  padding: {
    top: 'none' | 'small' | 'medium' | 'large';
    bottom: 'none' | 'small' | 'medium' | 'large';
  };
  
  // TODO: Agregar campos específicos del módulo
  // Ejemplo para una galería:
  // columns: number;
  // spacing: 'tight' | 'normal' | 'loose';
  // imageAspectRatio: 'square' | 'portrait' | 'landscape';
EOF

# Si tiene hijos, agregar tipos para hijos
if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> types.ts << EOF
  
  // Items/hijos de la sección
  items: ${MODULE}ItemConfig[];
}

export interface ${MODULE}ItemConfig {
  id: string;
  visible: boolean;
  title: string;
  // TODO: Agregar campos específicos del item
  // Ejemplo para testimonial:
  // author: string;
  // rating: number;
  // content: string;
}

export function getDefault${MODULE}ItemConfig(): ${MODULE}ItemConfig {
  return {
    id: \`item_\${Date.now()}\`,
    visible: true,
    title: 'New Item',
    // TODO: Agregar valores por defecto
  };
EOF
fi

cat >> types.ts << EOF
}

export function getDefault${MODULE}Config(): ${MODULE}Config {
  return {
    enabled: true,
    colorScheme: '1',
    width: 'page',
    padding: {
      top: 'medium',
      bottom: 'medium',
    },
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> types.ts << EOF
    items: []
EOF
fi

cat >> types.ts << EOF
  };
}

// Tipo de sección para el editor
export const ${MODULE}SectionType = SectionType.${MODULE^^};
EOF

# ============================================================================
# 2. CREAR Editor principal - USA updateSectionSettings
# ============================================================================
cat > ${MODULE}Editor.tsx << EOF
/**
 * @file ${MODULE}Editor.tsx
 * @max-lines 300
 * @module $MODULE
 * @description Editor para la sección del template $MODULE
 * @template-section true
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
  // IMPORTANTE: Usar useEditorStore para secciones del template
  const { sections, updateSectionSettings } = useEditorStore();
  
  // Buscar la sección en todos los grupos
  const section = Object.values(sections).flat().find(s => s.id === sectionId);
  
  // Estado local con valores por defecto
  const [localConfig, setLocalConfig] = useState<${MODULE}Config>(() => {
    return section?.settings || getDefault${MODULE}Config();
  });

  // Secciones expandidas del editor
  const [expandedSections, setExpandedSections] = useState({
    general: true,
    layout: false,
    appearance: false,
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> ${MODULE}Editor.tsx << EOF
    items: false,
EOF
fi

cat >> ${MODULE}Editor.tsx << EOF
  });

  // Sincronizar con los props cuando cambien
  useEffect(() => {
    const currentSection = Object.values(sections).flat().find(s => s.id === sectionId);
    if (currentSection?.settings) {
      const newSettings = currentSection.settings;
      if (JSON.stringify(newSettings) !== JSON.stringify(localConfig)) {
        setLocalConfig(newSettings);
      }
    }
  }, [sectionId, sections]);

  // Handler para cambios simples
  const handleChange = (field: keyof ${MODULE}Config, value: any) => {
    const updatedConfig = { ...localConfig, [field]: value };
    setLocalConfig(updatedConfig);
    
    // CRÍTICO: Actualizar el store usando updateSectionSettings
    if (section) {
      const groupId = Object.keys(sections).find(key => 
        sections[key as keyof typeof sections].includes(section)
      );
      if (groupId) {
        updateSectionSettings(groupId, section.id, updatedConfig);
      }
    }
  };

  // Handler para cambios anidados (como padding)
  const handleNestedChange = (parent: string, field: string, value: any) => {
    const updatedConfig = {
      ...localConfig,
      [parent]: {
        ...(localConfig[parent as keyof ${MODULE}Config] as any || {}),
        [field]: value
      }
    };
    setLocalConfig(updatedConfig);
    
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
    <div className="w-[320px] h-full bg-white dark:bg-gray-900 flex flex-col">
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
                  className="w-full mt-1 px-2 py-1.5 text-sm border border-gray-300 rounded-md"
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
                <span className="text-xs text-gray-600 dark:text-gray-400">Show section</span>
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

        {/* Layout Settings */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('layout')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              Layout
            </span>
            {expandedSections.layout ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.layout && (
            <div className="px-3 pb-3 space-y-3">
              {/* Width */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Width</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border border-gray-300 rounded-md"
                  value={localConfig.width}
                  onChange={(e) => handleChange('width', e.target.value)}
                >
                  <option value="screen">Full screen</option>
                  <option value="page">Page width</option>
                  <option value="large">Large</option>
                  <option value="medium">Medium</option>
                </select>
              </div>

              {/* Padding Top */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Top padding</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border border-gray-300 rounded-md"
                  value={localConfig.padding?.top || 'medium'}
                  onChange={(e) => handleNestedChange('padding', 'top', e.target.value)}
                >
                  <option value="none">None</option>
                  <option value="small">Small</option>
                  <option value="medium">Medium</option>
                  <option value="large">Large</option>
                </select>
              </div>

              {/* Padding Bottom */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Bottom padding</label>
                <select 
                  className="w-full mt-1 px-2 py-1.5 text-sm border border-gray-300 rounded-md"
                  value={localConfig.padding?.bottom || 'medium'}
                  onChange={(e) => handleNestedChange('padding', 'bottom', e.target.value)}
                >
                  <option value="none">None</option>
                  <option value="small">Small</option>
                  <option value="medium">Medium</option>
                  <option value="large">Large</option>
                </select>
              </div>
            </div>
          )}
        </div>

        {/* TODO: Agregar más secciones de configuración específicas del módulo */}

      </div>
    </div>
  );
}
EOF

# ============================================================================
# 3. Si tiene hijos, crear Children component
# ============================================================================
if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo -e "${GREEN}✅ Creando componente para manejar items/hijos...${NC}"

cat > ${MODULE}Children.tsx << EOF
/**
 * @file ${MODULE}Children.tsx
 * @max-lines 300
 * @module $MODULE
 * @description Maneja los items de la sección $MODULE en el sidebar
 * @template-section true
 */

'use client';

import React, { useMemo } from 'react';
import { Plus, Eye, EyeOff, Trash2, GripVertical, Image } from 'lucide-react';
import { DndContext, DragEndEvent, closestCenter } from '@dnd-kit/core';
import { 
  SortableContext, 
  verticalListSortingStrategy, 
  useSortable,
  arrayMove 
} from '@dnd-kit/sortable';
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

      {/* Item Icon */}
      <Image className="w-3.5 h-3.5 text-gray-500 mr-2" />
      
      {/* Item Title */}
      <span className="flex-1 text-sm text-gray-700">
        {item.title || \`${MODULE} block \${index + 1}\`}
      </span>

      {/* Actions */}
      {showActions && (
        <div className="flex items-center gap-1">
          <button
            onClick={(e) => {
              e.stopPropagation();
              onToggleVisibility();
            }}
            className="p-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded"
          >
            {item.visible ? 
              <Eye className="w-3 h-3 text-gray-500" /> : 
              <EyeOff className="w-3 h-3 text-gray-500" />
            }
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onDelete();
            }}
            className="p-1 hover:bg-red-100 dark:hover:bg-red-900/20 rounded"
          >
            <Trash2 className="w-3 h-3 text-red-500" />
          </button>
        </div>
      )}
    </div>
  );
}

export default function ${MODULE}Children({ section, groupId }: ${MODULE}ChildrenProps) {
  const { 
    updateSectionSettings, 
    setSelectedSection,
    selectedSection 
  } = useEditorStore();

  const config = section.settings as ${MODULE}Config;
  const items = config?.items || [];

  // IDs para drag & drop
  const itemIds = useMemo(() => items.map(item => item.id), [items]);

  const handleAddItem = () => {
    const newItem = getDefault${MODULE}ItemConfig();
    const updatedConfig = {
      ...config,
      items: [...items, newItem]
    };
    updateSectionSettings(groupId, section.id, updatedConfig);
    
    // Seleccionar el nuevo item
    setSelectedSection({
      ...section,
      selectedChildId: newItem.id
    });
  };

  const handleSelectItem = (itemId: string) => {
    setSelectedSection({
      ...section,
      selectedChildId: itemId
    });
  };

  const handleToggleVisibility = (itemId: string) => {
    const updatedItems = items.map(item =>
      item.id === itemId ? { ...item, visible: !item.visible } : item
    );
    updateSectionSettings(groupId, section.id, {
      ...config,
      items: updatedItems
    });
  };

  const handleDeleteItem = (itemId: string) => {
    const updatedItems = items.filter(item => item.id !== itemId);
    updateSectionSettings(groupId, section.id, {
      ...config,
      items: updatedItems
    });
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    
    if (over && active.id !== over.id) {
      const oldIndex = items.findIndex(item => item.id === active.id);
      const newIndex = items.findIndex(item => item.id === over.id);
      
      const newItems = arrayMove(items, oldIndex, newIndex);
      updateSectionSettings(groupId, section.id, {
        ...config,
        items: newItems
      });
    }
  };

  return (
    <div className="pl-8 pb-2">
      {/* Add Item Button - ESTILO AZUL LIMPIO COMO SLIDESHOW */}
      <button
        onClick={handleAddItem}
        className="w-full flex items-center gap-2 pl-8 pr-4 py-2 text-xs text-blue-600 dark:text-blue-400 
                   hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors group"
      >
        <div className="w-4 h-4 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
          <Plus className="w-3 h-3" />
        </div>
        <span className="font-medium">Add ${MODULE} block</span>
      </button>

      {/* Items List */}
      {items.length > 0 && (
        <DndContext
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={itemIds}
            strategy={verticalListSortingStrategy}
          >
            {items.map((item, index) => (
              <DraggableItem
                key={item.id}
                item={item}
                index={index}
                onSelect={() => handleSelectItem(item.id)}
                onToggleVisibility={() => handleToggleVisibility(item.id)}
                onDelete={() => handleDeleteItem(item.id)}
                isSelected={selectedSection?.selectedChildId === item.id}
              />
            ))}
          </SortableContext>
        </DndContext>
      )}
    </div>
  );
}
EOF

# Crear editor para items individuales
cat > ${MODULE}ItemEditor.tsx << EOF
/**
 * @file ${MODULE}ItemEditor.tsx
 * @max-lines 300
 * @module $MODULE
 * @description Editor para items individuales de $MODULE
 * @template-section true
 */

'use client';

import React, { useState, useEffect } from 'react';
import { ArrowLeft, ChevronDown, ChevronUp } from 'lucide-react';
import { useEditorStore } from '@/stores/useEditorStore';
import { ${MODULE}Config, ${MODULE}ItemConfig } from './types';

interface ${MODULE}ItemEditorProps {
  sectionId: string;
  itemId: string;
}

export default function ${MODULE}ItemEditor({ sectionId, itemId }: ${MODULE}ItemEditorProps) {
  const { sections, updateSectionSettings, selectSection } = useEditorStore();
  
  // Buscar la sección y el item
  const section = Object.values(sections).flat().find(s => s.id === sectionId);
  const config = section?.settings as ${MODULE}Config;
  const item = config?.items?.find(i => i.id === itemId);
  
  const [localItem, setLocalItem] = useState<${MODULE}ItemConfig | null>(item || null);
  const [expandedSections, setExpandedSections] = useState({
    content: true,
    appearance: false,
  });

  useEffect(() => {
    const currentSection = Object.values(sections).flat().find(s => s.id === sectionId);
    const currentConfig = currentSection?.settings as ${MODULE}Config;
    const currentItem = currentConfig?.items?.find(i => i.id === itemId);
    
    if (currentItem && JSON.stringify(currentItem) !== JSON.stringify(localItem)) {
      setLocalItem(currentItem);
    }
  }, [sectionId, itemId, sections]);

  const handleChange = (field: keyof ${MODULE}ItemConfig, value: any) => {
    if (!localItem || !section || !config) return;
    
    const updatedItem = { ...localItem, [field]: value };
    setLocalItem(updatedItem);
    
    // Actualizar en el store
    const updatedItems = config.items.map(i =>
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
  };

  const toggleSection = (section: keyof typeof expandedSections) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  if (!localItem) {
    return (
      <div className="w-[320px] p-4 text-center text-gray-500">
        <p className="text-xs">Item not found</p>
      </div>
    );
  }

  const handleBack = () => {
    // 🔴 CRÍTICO: Regresar al SIDEBAR PRINCIPAL, NO a la configuración del padre
    selectSection(null);  // ✅ CORRECTO - Va al sidebar principal
    // ❌ NUNCA hacer: selectSection(parentId) - Eso llevaría al padre
  };

  return (
    <div className="w-[320px] h-full bg-white dark:bg-gray-900 flex flex-col">
      {/* Header con flecha de regreso - SOLO UNA FLECHA */}
      <div className="border-b border-gray-200 dark:border-gray-700">
        <button
          onClick={handleBack}
          className="w-full px-3 py-2.5 flex items-center gap-2 hover:bg-gray-50 dark:hover:bg-gray-800"
        >
          <ArrowLeft className="w-3.5 h-3.5 text-gray-500" />
          <span className="text-xs font-medium text-gray-900 dark:text-white">
            ${MODULE} Item Settings
          </span>
        </button>
      </div>
      
      <div className="flex-1 overflow-y-auto">
        
        {/* Content Settings */}
        <div className="border-b border-gray-200 dark:border-gray-700">
          <button
            onClick={() => toggleSection('content')}
            className="w-full px-3 py-2.5 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800"
          >
            <span className="text-xs font-medium text-gray-900 dark:text-white">
              Content
            </span>
            {expandedSections.content ? 
              <ChevronUp className="w-3 h-3 text-gray-500" /> : 
              <ChevronDown className="w-3 h-3 text-gray-500" />
            }
          </button>
          
          {expandedSections.content && (
            <div className="px-3 pb-3 space-y-3">
              {/* Title */}
              <div>
                <label className="text-xs text-gray-600 dark:text-gray-400">Title</label>
                <input
                  type="text"
                  className="w-full mt-1 px-2 py-1.5 text-sm border border-gray-300 rounded-md"
                  value={localItem.title}
                  onChange={(e) => handleChange('title', e.target.value)}
                />
              </div>

              {/* Visible Toggle */}
              <div className="flex items-center justify-between">
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

              {/* TODO: Agregar más campos específicos del item */}
            </div>
          )}
        </div>

      </div>
    </div>
  );
}
EOF
fi

# ============================================================================
# 4. CREAR PREVIEW UNIFICADO (Editor + Preview Real)
# ============================================================================
echo -e "${GREEN}✅ Creando componente Preview unificado...${NC}"

# Crear directorio preview si no existe
PREVIEW_DIR="../../preview"
mkdir -p "$PREVIEW_DIR"

cat > "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
/**
 * @file Preview${MODULE}.tsx
 * @max-lines 400
 * @module $MODULE
 * @description Componente Preview UNIFICADO para editor y preview real
 * @unified-architecture true
 * @created $(date +"%Y-%m-%d")
 */

'use client';

import React, { useState, useEffect, useMemo } from 'react';
import useThemeConfigStore from '@/stores/useThemeConfigStore';
import { ${MODULE}Config } from '@/components/editor/modules/${MODULE}/types';

interface Preview${MODULE}Props {
  config: ${MODULE}Config;
  theme?: any;  // Theme desde API en preview real
  deviceView?: 'desktop' | 'mobile';
  isEditor?: boolean;  // CRÍTICO: Diferencia entre contextos
}

export default function Preview${MODULE}({ 
  config, 
  theme,
  deviceView,
  isEditor = false
}: Preview${MODULE}Props) {
  
  // 🎯 PATRÓN DUAL: Theme desde prop (preview real) o store (editor)
  const storeThemeConfig = useThemeConfigStore(state => state.config);
  const themeConfig = theme || storeThemeConfig;
  
  // ⚠️ DETECCIÓN MÓVIL OBLIGATORIA
  const [isMobile, setIsMobile] = useState(() => {
    if (deviceView !== undefined) return deviceView === 'mobile';
    if (typeof window !== 'undefined') return window.innerWidth < 768;
    return false;
  });
  
  // Sincronizar con cambios de deviceView o viewport
  useEffect(() => {
    const checkMobile = () => {
      if (deviceView !== undefined) {
        setIsMobile(deviceView === 'mobile');
        return;
      }
      const isMobileView = window.innerWidth < 768;
      setIsMobile(isMobileView);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, [deviceView]);
  
  // Obtener color scheme seleccionado
  const colorScheme = useMemo(() => {
    if (!themeConfig?.colorSchemes?.schemes) {
      return {
        text: { default: '#000000' },
        background: { default: '#FFFFFF' },
        solidButton: { default: '#000000' },
        solidButtonText: { default: '#FFFFFF' },
        outlineButton: { default: '#000000' },
        outlineButtonText: { default: '#000000' },
      };
    }
    
    const schemeIndex = parseInt(config.colorScheme || '1') - 1;
    return themeConfig.colorSchemes.schemes[schemeIndex] || themeConfig.colorSchemes.schemes[0];
  }, [themeConfig, config.colorScheme]);
  
  // TODOS LOS HOOKS ANTES DE RETURNS CONDICIONALES
  
  // Validación DESPUÉS de todos los hooks
  if (!config?.enabled && !isEditor) {
    return null;
  }
  
  // Clases responsive basadas en isMobile
  const containerClass = isMobile ? 'px-4' : 'px-8';
  const textSize = isMobile ? 'text-sm' : 'text-base';
  const spacing = isMobile ? 'gap-2' : 'gap-4';
  
  // Padding basado en configuración
  const paddingClasses = {
    none: '',
    small: isMobile ? 'py-2' : 'py-4',
    medium: isMobile ? 'py-4' : 'py-8',
    large: isMobile ? 'py-6' : 'py-12',
  };
  
  // Width basado en configuración
  const widthClasses = {
    screen: 'w-full',
    page: 'max-w-7xl mx-auto',
    large: 'max-w-5xl mx-auto',
    medium: 'max-w-3xl mx-auto',
  };
  
  return (
    <section
      className={\`
        \${paddingClasses[config.padding?.top || 'medium']}
        \${paddingClasses[config.padding?.bottom || 'medium']}
      \`}
      style={{
        backgroundColor: colorScheme.background?.default || '#FFFFFF',
        color: colorScheme.text?.default || '#000000',
      }}
    >
      <div className={\`\${widthClasses[config.width || 'page']} \${containerClass}\`}>
        {/* TODO: Implementar el contenido del módulo */}
        <div className={\`\${spacing} \${textSize}\`}>
          <h2>Preview ${MODULE}</h2>
          <p>Implementar contenido aquí</p>
EOF

if [ "$WITH_CHILDREN" == "--with-children" ]; then
cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
          
          {/* Renderizar items si existen */}
          {config.items?.map((item, index) => (
            item.visible && (
              <div key={item.id} className="border p-4 rounded">
                <h3>{item.title || \`Item \${index + 1}\`}</h3>
                {/* TODO: Renderizar contenido del item */}
              </div>
            )
          ))}
EOF
fi

cat >> "$PREVIEW_DIR/Preview${MODULE}.tsx" << EOF
        </div>
      </div>
    </section>
  );
}
EOF

echo -e "${GREEN}✅ Preview unificado creado en: $PREVIEW_DIR/Preview${MODULE}.tsx${NC}"

# ============================================================================
# 5. CREAR index.ts para exports
# ============================================================================
cat > index.ts << EOF
/**
 * @file index.ts
 * @module $MODULE
 * @description Exports para el módulo $MODULE
 * @template-section true
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

# ============================================================================
# INSTRUCCIONES FINALES
# ============================================================================
echo ""
echo -e "${GREEN}✅ Módulo de sección del template creado exitosamente!${NC}"
echo ""
echo -e "${YELLOW}📋 SIGUIENTE PASO - Integración Manual:${NC}"
echo ""
echo "1. ${BLUE}Agregar el tipo de sección en /types/editor.types.ts:${NC}"
echo "   export enum SectionType {"
echo "     // ... otros tipos"
echo "     ${MODULE^^} = '${MODULE,,}',"
echo "   }"
echo ""
echo "2. ${BLUE}Integrar Editor en EditorLayout.tsx:${NC}"
echo "   - Importar: import { ${MODULE}Editor } from './modules/${MODULE}';"
echo "   - Agregar case en el switch de ConfigPanel"
echo ""
echo "3. ${BLUE}Integrar Preview UNIFICADO en EditorPreview.tsx:${NC}"
echo "   import Preview${MODULE} from '@/components/preview/Preview${MODULE}';"
echo "   "
echo "   case SectionType.${MODULE^^}:"
echo "     return ("
echo "       <Preview${MODULE}"
echo "         config={section.settings}"
echo "         theme={themeConfig}"
echo "         deviceView={deviceView}"
echo "         isEditor={true}  // CRÍTICO"
echo "       />"
echo "     );"
echo ""
echo "4. ${BLUE}Integrar Preview UNIFICADO en PreviewPage.tsx:${NC}"
echo "   import Preview${MODULE} from './Preview${MODULE}';"
echo "   "
echo "   // En la sección de renderizado:"
echo "   {pageConfig?.${MODULE,,} && ("
echo "     <Preview${MODULE}"
echo "       config={pageConfig.${MODULE,,}}"
echo "       theme={globalTheme}"
echo "       deviceView={editorDeviceView}"
echo "       isEditor={false}  // CRÍTICO"
echo "     />"
echo "   )}"
echo ""
if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo "5. ${BLUE}Integrar children en EditorSidebarWithDnD.tsx:${NC}"
echo "   - Importar ${MODULE}Children"
echo "   - Renderizar después del SectionItem correspondiente:"
echo "   "
echo "   {section.type === SectionType.${MODULE^^} && ("
echo "     <${MODULE}Children section={section} groupId={group.id} />"
echo "   )}"
echo ""
echo "6. ${BLUE}Integrar ItemEditor en ConfigPanel.tsx:${NC}"
echo "   import ${MODULE}ItemEditor from './modules/${MODULE}/${MODULE}ItemEditor';"
echo "   "
echo "   // En el switch agregar:"
echo "   if (selectedSection?.includes(':item:')) {"
echo "     const [sectionId, , itemId] = selectedSection.split(':');"
echo "     return <${MODULE}ItemEditor sectionId={sectionId} itemId={itemId} />;"
echo "   }"
echo ""
fi
echo -e "${YELLOW}⚠️  RECORDATORIOS IMPORTANTES:${NC}"
echo "  • Esta es una SECCIÓN DEL TEMPLATE (no componente estructural)"
echo "  • Usa updateSectionSettings() del store (correcto para secciones)"
echo "  • Se guarda con el botón Save de la página"
echo "  • Puede aparecer múltiples veces en una página"
echo "  • ${RED}ARQUITECTURA UNIFICADA:${NC} UN Preview para AMBOS contextos"
echo "  • ${RED}isEditor={true/false}${NC} diferencia entre editor y preview real"
echo "  • ${RED}Patrón dual de theme${NC}: theme || storeThemeConfig"
if [ "$WITH_CHILDREN" == "--with-children" ]; then
echo ""
echo -e "${RED}🔴 CRÍTICO PARA MÓDULOS CON HIJOS:${NC}"
echo "  • Botón 'Add block' DEBE ser AZUL (text-blue-600)"
echo "  • Flecha de regreso va al SIDEBAR (selectSection(null))"
echo "  • SOLO UNA flecha de regreso, NO dos"
echo "  • Drag handle solo visible on hover"
echo "  • Items con chevron > icono > título"
fi
echo ""
echo -e "${GREEN}📂 Archivos creados en: $BASE_DIR/$MODULE${NC}"
ls -la