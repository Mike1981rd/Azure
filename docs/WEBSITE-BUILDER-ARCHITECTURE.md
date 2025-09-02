# 🏗️ WEBSITE BUILDER - ARQUITECTURA Y DOCUMENTACIÓN TÉCNICA

## 📅 Última actualización: 13 de enero 2025
## ⚠️ DOCUMENTO CRÍTICO - LEER ANTES DE MODIFICAR CUALQUIER CÓDIGO

---

## 🎯 RESUMEN EJECUTIVO

El Website Builder es un sistema complejo con múltiples capas de estado que deben mantenerse sincronizadas:

1. **Estado Global (Zustand)** - Fuente de verdad para configuraciones
2. **Estado Local (React)** - Para optimización y prevención de re-renders
3. **Backend (PostgreSQL)** - Persistencia con JSONB
4. **Preview** - Renderizado en tiempo real
5. **Sistema Undo/Redo** - Historial de 50 estados

### ⚡ FLUJO DE DATOS CRÍTICO

```
Usuario modifica UI → Estado Local → Zustand Store → API Backend → Preview Update
                           ↓
                    Detección isDirty → Botón Save visible
                           ↓
                    History Stack → Undo/Redo disponible
```

---

## 🔴 ADVERTENCIAS CRÍTICAS - NO ROMPER ESTAS REGLAS

### 1. NUNCA modificar directamente el store sin actualizar isDirty
```typescript
// ❌ INCORRECTO - Romperá el botón Save
set({ config: newConfig });

// ✅ CORRECTO
set({ 
  config: newConfig,
  isDirty: true  // CRÍTICO para el botón Save
});
```

### 2. NUNCA usar comparación de objetos para detectar cambios
```typescript
// ❌ INCORRECTO - JavaScript no compara objetos correctamente
if (oldConfig !== newConfig) { setDirty(true) }

// ✅ CORRECTO - Usar JSON.stringify o deep comparison
if (JSON.stringify(oldConfig) !== JSON.stringify(newConfig)) {
  setDirty(true);
}
```

### 3. SIEMPRE sincronizar estado local con props en useEffect
```typescript
// ✅ OBLIGATORIO en todos los editores
useEffect(() => {
  setLocalConfig(value || defaultConfig);
}, [value, JSON.stringify(value)]); // JSON.stringify para deep comparison
```

### 4. NUNCA mezclar estados de diferentes módulos
```typescript
// ❌ INCORRECTO
const [allConfigs, setAllConfigs] = useState({...});

// ✅ CORRECTO - Un estado por módulo
const [headerConfig, setHeaderConfig] = useState<HeaderConfig>(...);
const [footerConfig, setFooterConfig] = useState<FooterConfig>(...);
```

---

## 📦 ARQUITECTURA DE STORES

### 1. useThemeConfigStore (Configuraciones Globales)
**Ubicación**: `/src/stores/useThemeConfigStore.ts`

**Responsabilidades**:
- Gestión de configuraciones del tema (appearance, typography, colors, etc.)
- Sincronización con backend via API
- Detección de cambios no guardados

**Estado**:
```typescript
{
  config: {
    appearance: AppearanceConfig,
    typography: TypographyConfig,
    colorSchemes: ColorSchemesConfig,
    productCards: ProductCardsConfig,
    productBadges: ProductBadgesConfig,
    cart: CartConfig,
    favicon: FaviconConfig,
    navigation: NavigationConfig,
    socialMedia: SocialMediaConfig,
    swatches: SwatchesConfig
  },
  loading: boolean,
  error: string | null,
  hasUnsavedChanges: boolean
}
```

### 2. useEditorStore (Componentes Estructurales)
**Ubicación**: `/src/stores/useEditorStore.ts`

**Responsabilidades**:
- Gestión de secciones (header, footer, aside, template)
- Sistema drag & drop
- Sistema undo/redo
- Estado isDirty para cambios estructurales

**Estado**:
```typescript
{
  sections: {
    headerGroup: Section[],
    asideGroup: Section[],
    template: Section[],
    footerGroup: Section[]
  },
  isDirty: boolean,          // Para cambios estructurales
  isGlobalSettingsOpen: boolean,
  history: any[],            // Stack de undo
  historyIndex: number       // Posición actual en history
}
```

---

## 🔄 SISTEMA DE SINCRONIZACIÓN

### Flujo de Actualización de Configuración Global

1. **Usuario modifica un slider/input**:
```typescript
// En GlobalSettingsPanel.tsx
const handleAppearanceChange = (newAppearance: AppearanceConfig) => {
  setLocalAppearance(newAppearance);  // Estado local primero
  updateAppearance(newAppearance);     // Luego al store
};
```

2. **Store actualiza y marca como dirty**:
```typescript
// En useThemeConfigStore.ts
updateAppearance: (appearance) => set((state) => ({
  config: { ...state.config, appearance },
  hasUnsavedChanges: true  // Activa el botón Save
}))
```

3. **Botón Save aparece**:
```typescript
// GlobalSettingsPanel verifica
const hasChanges = hasUnsavedChanges || hasStructuralChanges;
{hasChanges && <SaveButton />}
```

4. **Preview se actualiza automáticamente**:
```typescript
// EditorPreview escucha cambios del store
const { config } = useThemeConfigStore();
useEffect(() => {
  applyThemeToPreview(config);
}, [config]);
```

---

## 💾 SISTEMA DE GUARDADO

### Proceso de Guardado Completo

1. **Detectar cambios (isDirty)**:
```typescript
// Dos fuentes de cambios
const hasGlobalChanges = useThemeConfigStore(s => s.hasUnsavedChanges);
const hasStructuralChanges = useEditorStore(s => s.isDirty);
const hasChanges = hasGlobalChanges || hasStructuralChanges;
```

2. **Guardar ambos tipos de cambios**:
```typescript
const handleSave = async () => {
  setSaving(true);
  
  // Guardar configuraciones globales
  if (hasGlobalChanges) {
    await saveGlobalConfig();
  }
  
  // Guardar cambios estructurales
  if (hasStructuralChanges) {
    await savePage();
  }
  
  // Resetear flags
  setHasUnsavedChanges(false);
  setIsDirty(false);
  setSaving(false);
};
```

3. **Sincronización post-guardado**:
```typescript
// CRÍTICO: Refrescar datos después de guardar
await fetchConfig(companyId);
toast.success('Changes saved successfully');
```

---

## ↩️ SISTEMA UNDO/REDO

### Implementación del History Stack

**Ubicación**: `/src/stores/useEditorStore.ts`

```typescript
// Guardar estado en history
saveHistory: () => {
  const currentState = {
    sections: get().sections,
    timestamp: Date.now()
  };
  
  const newHistory = [
    ...get().history.slice(0, get().historyIndex + 1),
    currentState
  ];
  
  // Mantener máximo 50 estados
  if (newHistory.length > 50) {
    newHistory.shift();
  }
  
  set({
    history: newHistory,
    historyIndex: newHistory.length - 1
  });
}

// Undo
undo: () => {
  const { history, historyIndex } = get();
  if (historyIndex > 0) {
    const previousState = history[historyIndex - 1];
    set({
      sections: previousState.sections,
      historyIndex: historyIndex - 1,
      isDirty: true
    });
  }
}
```

### Integración con HeaderEditor

```typescript
// HeaderEditor debe respetar el value del padre
useEffect(() => {
  // CRÍTICO: Actualizar cuando viene del undo
  setLocalConfig(value || defaultHeaderConfig);
}, [value, JSON.stringify(value)]);
```

---

## 🎨 COMPONENTES DEL EDITOR

### GlobalSettingsPanel
**Rol**: Panel principal de configuraciones globales
**Estado**: Local + Store sincronizado
**Comunicación**: Via useGlobalThemeConfig hook

### HeaderEditor
**Rol**: Configuración del header
**Estado**: Local sincronizado con props
**Comunicación**: onChange callback al padre

### EditorPreview
**Rol**: Visualización en tiempo real
**Estado**: Read-only desde stores
**Comunicación**: Escucha cambios de ambos stores

---

## 🐛 TROUBLESHOOTING COMÚN

### Problema 1: El botón Save no aparece
**Causa**: isDirty no se está actualizando
**Solución**:
```typescript
// Verificar que TODOS los cambios actualicen isDirty
onChange={() => {
  updateConfig(newConfig);
  setIsDirty(true);  // No olvidar esto
}}
```

### Problema 2: Undo no actualiza la vista
**Causa**: Componente no escucha cambios del value
**Solución**:
```typescript
useEffect(() => {
  setLocalState(value);
}, [value, JSON.stringify(value)]);
```

### Problema 3: Preview no se actualiza
**Causa**: Preview no está suscrito al store correcto
**Solución**:
```typescript
const config = useThemeConfigStore(s => s.config);
const sections = useEditorStore(s => s.sections);
// Usar ambos en el render
```

### Problema 4: Se pierden cambios al cambiar de sección
**Causa**: Estado local no se guarda antes de cambiar
**Solución**:
```typescript
// Antes de cambiar de sección
if (hasLocalChanges) {
  await saveLocalChanges();
}
```

---

## 📋 CHECKLIST ANTES DE MODIFICAR

- [ ] ¿Entiendo el flujo de datos completo?
- [ ] ¿Mi cambio actualiza isDirty cuando debe?
- [ ] ¿Los componentes hijos respetan el value del padre?
- [ ] ¿El undo/redo seguirá funcionando?
- [ ] ¿El preview se actualizará correctamente?
- [ ] ¿El botón Save aparecerá cuando debe?
- [ ] ¿He probado guardar y recargar?

---

## 🚀 COMANDOS ÚTILES

```bash
# Ver logs del sistema
grep -r "isDirty" src/

# Verificar sincronización
grep -r "useEffect.*value" src/components/editor/

# Buscar actualizaciones de estado
grep -r "set.*dirty.*true" src/

# Ver todos los hooks de configuración
ls src/hooks/use*Config.ts
```

---

## ⚠️ ÚLTIMA LÍNEA DE DEFENSA

Si algo se rompe y no sabes por qué:

1. **Revisa los useEffect** - La mayoría de problemas vienen de ahí
2. **Verifica isDirty** - Si el botón Save no aparece, es por esto
3. **Check JSON.stringify** - Para comparaciones profundas
4. **Console.log en stores** - Para ver el flujo de datos
5. **Git diff** - Para ver qué cambió exactamente

**NUNCA** hagas cambios sin entender el impacto en:
- Sistema de guardado
- Sistema undo/redo
- Sincronización con preview
- Detección de cambios

---

**Autor**: Sistema de desarrollo
**Fecha**: 13 de enero 2025
**Versión**: 1.0.0
**Estado**: DOCUMENTO VIVO - Actualizar con cada cambio mayor