# BLUEPRINT - PARTE 2

## 🔄 SISTEMA UNDO/REDO COMPLETO (SOLUCIONA PROBLEMA #9)

### GESTIÓN DE HISTORIAL DEL EDITOR

```typescript
// lib/editorHistory.ts
export interface EditorState {
  id: string;
  timestamp: Date;
  pageConfig: PageConfig;
  sections: SectionConfig[];
  description: string;
}

export class EditorHistoryManager {
  private history: EditorState[] = [];
  private currentIndex: number = -1;
  private maxHistorySize: number = 50;

  saveState(state: EditorState) {
    // Remover estados futuros si estamos en el medio del historial
    this.history = this.history.slice(0, this.currentIndex + 1);
    
    // Agregar nuevo estado
    this.history.push(state);
    this.currentIndex++;
    
    // Limitar tamaño del historial
    if (this.history.length > this.maxHistorySize) {
      this.history.shift();
      this.currentIndex--;
    }
  }

  undo(): EditorState | null {
    if (this.canUndo()) {
      this.currentIndex--;
      return this.history[this.currentIndex];
    }
    return null;
  }

  redo(): EditorState | null {
    if (this.canRedo()) {
      this.currentIndex++;
      return this.history[this.currentIndex];
    }
    return null;
  }

  canUndo(): boolean {
    return this.currentIndex > 0;
  }

  canRedo(): boolean {
    return this.currentIndex < this.history.length - 1;
  }
}
```

### HOOK PARA UNDO/REDO CON ATAJOS DE TECLADO

```typescript
// hooks/useEditorHistory.ts
export function useEditorHistory() {
  const [historyManager] = useState(() => new EditorHistoryManager());
  const [currentState, setCurrentState] = useState<EditorState | null>(null);

  const saveState = useCallback((description: string, pageConfig: PageConfig, sections: SectionConfig[]) => {
    const state: EditorState = {
      id: nanoid(),
      timestamp: new Date(),
      pageConfig,
      sections,
      description
    };
    
    historyManager.saveState(state);
    setCurrentState(state);
  }, [historyManager]);

  const undo = useCallback(() => {
    const previousState = historyManager.undo();
    if (previousState) {
      setCurrentState(previousState);
      applyStateToEditor(previousState);
    }
  }, [historyManager]);

  const redo = useCallback(() => {
    const nextState = historyManager.redo();
    if (nextState) {
      setCurrentState(nextState);
      applyStateToEditor(nextState);
    }
  }, [historyManager]);

  // Atajos de teclado
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !e.shiftKey) {
        e.preventDefault();
        undo();
      } else if ((e.ctrlKey || e.metaKey) && (e.key === 'y' || (e.key === 'z' && e.shiftKey))) {
        e.preventDefault();
        redo();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [undo, redo]);

  return {
    saveState,
    undo,
    redo,
    canUndo: historyManager.canUndo(),
    canRedo: historyManager.canRedo(),
    currentState
  };
}
```

## 🚀 MÓDULOS DEL BACK OFFICE COMPLETOS

### ESTRUCTURA DEL PANEL LATERAL

#### 📊 Dashboard
- Métricas de reservas y ventas
- Gráficos de performance
- Notificaciones importantes

#### 🏢 Empresa
- Configuración de la empresa
- Información de contacto
- Configuraciones generales

#### 👥 Roles & Usuarios
- Gestión de roles y permisos
- CRUD de usuarios
- Asignación de roles

#### 👨‍💼 Clientes
- Base de datos de clientes
- Historial de reservas y compras
- Comunicación con clientes

#### 🏨 Reservaciones
- Gestión de reservas de habitaciones
- Calendario de disponibilidad
- Check-in/Check-out

#### 💳 Métodos de Pago
- Configuración de gateways
- Procesamiento de pagos
- Historial de transacciones

#### 📁 Colecciones
- Agrupación de productos
- Gestión de categorías
- Organización de inventario

#### 📦 Productos
- CRUD de productos e-commerce
- Sistema de variantes completo
- Gestión de inventario

#### 📄 Páginas
- Gestión de páginas del website
- Templates predefinidos (5 tipos)
- Páginas custom

#### 📋 Políticas
- Términos y condiciones
- Políticas de privacidad
- Políticas de cancelación

#### 🌐 Sitio Web
- Website builder interface
- Editor visual drag & drop
- Preview en tiempo real
- Sistema undo/redo

#### 🔗 Dominios
- Gestión de dominios custom
- Configuración DNS
- SSL certificates

## ⚙️ SISTEMA DE PERSONALIZACIÓN UI

### CONFIGURACIONES DISPONIBLES

```typescript
// types/ui-settings.ts
export interface UISettings {
  language: 'es' | 'en';
  theme: 'light' | 'dark';
  notifications: boolean;
  sidebar: {
    color: string;
    textColor: string;
    layout: 'vertical' | 'horizontal';
    collapsed: boolean;
  };
  primaryColor: string; // Para botones guardar/cancelar
}
```

### COMPONENTE DE PERSONALIZACIÓN

```typescript
// components/ui/ThemeCustomizer.tsx
export function ThemeCustomizer() {
  const [settings, setSettings] = useUISettings();

  return (
    <div className="p-4">
      <h3>🔧 Personalización</h3>
      
      {/* Idioma */}
      <div>
        <label>Idioma</label>
        <select value={settings.language} onChange={handleLanguageChange}>
          <option value="es">Español</option>
          <option value="en">English</option>
        </select>
      </div>

      {/* Tema */}
      <div>
        <label>Tema</label>
        <select value={settings.theme} onChange={handleThemeChange}>
          <option value="light">🌞 Light</option>
          <option value="dark">🌙 Dark</option>
        </select>
      </div>

      {/* Color del sidebar */}
      <div>
        <label>Color del Panel</label>
        <input 
          type="color" 
          value={settings.sidebar.color}
          onChange={handleSidebarColorChange}
        />
      </div>

      {/* Layout del sidebar */}
      <div>
        <label>Layout del Panel</label>
        <select value={settings.sidebar.layout} onChange={handleLayoutChange}>
          <option value="vertical">📱 Lateral</option>
          <option value="horizontal">💻 Horizontal</option>
        </select>
      </div>

      {/* Estado del sidebar */}
      <div>
        <label>Estado del Panel</label>
        <select value={settings.sidebar.collapsed ? 'collapsed' : 'expanded'}>
          <option value="expanded">🔓 Siempre Abierto</option>
          <option value="collapsed">🔒 Siempre Cerrado</option>
        </select>
      </div>
    </div>
  );
}
```

## 🔄 SISTEMA DE CACHE Y PERFORMANCE (SOLUCIONA PROBLEMA #5)

### ESTRATEGIA DE CACHING OPTIMIZADA

```csharp
// Services/CacheService.cs
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;

    public async Task<string> GetWebsiteContent(string domain, bool isPreview = false)
    {
        var cacheKey = isPreview 
            ? $"website:preview:{domain}" 
            : $"website:production:{domain}";
        
        // 1. Buscar en cache
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null) return cached;
        
        // 2. Generar contenido optimizado
        var content = isPreview 
            ? await GeneratePreviewContent(domain) 
            : await GenerateOptimizedProductionContent(domain);
        
        // 3. Cachear con diferentes tiempos
        var cacheTime = isPreview 
            ? TimeSpan.FromMinutes(5)   // Preview cache corto
            : TimeSpan.FromHours(24);   // Production cache largo
            
        await _cache.SetStringAsync(cacheKey, content, cacheTime);
        
        return content;
    }

    private async Task<string> GenerateOptimizedProductionContent(string domain)
    {
        // Contenido optimizado para producción:
        // - Minified CSS/JS
        // - Imágenes optimizadas
        // - CDN URLs
        // - Sin herramientas de edición
        return optimizedContent;
    }

    private async Task<string> GeneratePreviewContent(string domain)
    {
        // Contenido para preview:
        // - CSS/JS sin minificar 
        // - Herramientas de edición
        // - Debug info
        return previewContent;
    }
}
```

## 📱 EDITOR VISUAL CON PREVIEW MEJORADO

### FUNCIONALIDADES DEL EDITOR

```typescript
// components/builder/LiveEditor.tsx
export function LiveEditor() {
  const { undo, redo, canUndo, canRedo, saveState } = useEditorHistory();
  const [viewMode, setViewMode] = useState<'desktop' | 'mobile'>('desktop');
  const [isPreviewMode, setIsPreviewMode] = useState(false);

  return (
    <div className="flex h-screen">
      {/* Panel izquierdo - Secciones disponibles */}
      <div className="w-80 border-r">
        <SectionLibrary />
      </div>
      
      {/* Centro - Editor visual */}
      <div className="flex-1 flex flex-col">
        {/* Toolbar del editor */}
        <div className="flex items-center gap-4 p-4 border-b">
          {/* Botones Undo/Redo */}
          <div className="flex gap-2">
            <button 
              onClick={undo}
              disabled={!canUndo}
              className="p-2 rounded hover:bg-gray-100 disabled:opacity-50"
              title="Deshacer (Ctrl+Z)"
            >
              ↶ Undo
            </button>
            
            <button 
              onClick={redo}
              disabled={!canRedo}
              className="p-2 rounded hover:bg-gray-100 disabled:opacity-50"
              title="Rehacer (Ctrl+Y)"
            >
              ↷ Redo
            </button>
          </div>

          <div className="h-6 w-px bg-gray-300"></div>

          {/* Botones de vista */}
          <div className="flex gap-2">
            <button 
              onClick={() => setViewMode('desktop')}
              className={`p-2 rounded ${viewMode === 'desktop' ? 'bg-blue-100' : 'hover:bg-gray-100'}`}
            >
              💻 Desktop
            </button>
            <button 
              onClick={() => setViewMode('mobile')}
              className={`p-2 rounded ${viewMode === 'mobile' ? 'bg-blue-100' : 'hover:bg-gray-100'}`}
            >
              📱 Móvil
            </button>
          </div>

          <div className="h-6 w-px bg-gray-300"></div>

          {/* Botones de preview y guardar */}
          <button 
            onClick={() => openPreviewWindow()}
            className="p-2 rounded hover:bg-gray-100"
            title="Abrir vista real"
          >
            🔗 Vista Real
          </button>
          
          <button 
            onClick={() => saveChanges()}
            className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
          >
            💾 Guardar
          </button>
        </div>

        {/* Canvas del editor */}
        <div className="flex-1 overflow-auto">
          <DragDropCanvas viewMode={viewMode} />
        </div>
      </div>
      
      {/* Panel derecho - Configuraciones */}
      <div className="w-80 border-l">
        <ConfigurationPanel />
      </div>
    </div>
  );
}
```

<!-- Continúa en blueprint3.md -->