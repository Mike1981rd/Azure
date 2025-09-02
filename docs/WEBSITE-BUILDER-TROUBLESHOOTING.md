# 🔧 WEBSITE BUILDER - GUÍA DE TROUBLESHOOTING

## 📅 Última actualización: 13 de enero 2025
## 🚨 USAR ESTA GUÍA CUANDO ALGO SE ROMPA

---

## 🔴 PROBLEMAS CRÍTICOS Y SOLUCIONES

### 1. EL BOTÓN SAVE NO APARECE

#### Síntomas:
- Haces cambios pero el botón Save no se muestra
- El botón aparece a veces sí, a veces no

#### Diagnóstico:
```typescript
// En la consola del navegador
console.log(useThemeConfigStore.getState().hasUnsavedChanges);
console.log(useEditorStore.getState().isDirty);
```

#### Soluciones:

**Solución A: Para configuraciones globales**
```typescript
// En GlobalSettingsPanel.tsx
const handleChange = (newConfig) => {
  setLocalConfig(newConfig);
  updateConfig(newConfig);  // DEBE llamar al store
  // El store debe hacer: set({ hasUnsavedChanges: true })
};
```

**Solución B: Para componentes estructurales**
```typescript
// En HeaderEditor.tsx o similar
const handleChange = (newValue) => {
  onChange(newValue);  // Propaga al padre
  // El padre debe llamar a useEditorStore.setState({ isDirty: true })
};
```

**Solución C: Verificar el SaveButton**
```typescript
// En GlobalSettingsPanel.tsx
const hasChanges = hasUnsavedChanges || hasStructuralChanges;
console.log('Has changes?', hasChanges); // Debug

{hasChanges && !saving && (
  <button onClick={handleSave}>
    <Save className="w-4 h-4" />
    Save
  </button>
)}
```

---

### 2. UNDO NO ACTUALIZA LA VISTA

#### Síntomas:
- Presionas Ctrl+Z pero la vista no cambia
- El estado cambia pero los componentes no se actualizan

#### Diagnóstico:
```typescript
// Verificar que el history está funcionando
console.log(useEditorStore.getState().history);
console.log(useEditorStore.getState().historyIndex);
```

#### Soluciones:

**Solución A: Agregar useEffect en el componente**
```typescript
// En HeaderEditor.tsx
useEffect(() => {
  // CRÍTICO: Actualizar estado local cuando viene del padre
  setLocalConfig(value || defaultConfig);
}, [value, JSON.stringify(value)]); // JSON.stringify para deep comparison
```

**Solución B: Verificar que undo actualiza las props**
```typescript
// En el componente padre
const headerConfig = sections.headerGroup[0]?.settings;

// Pasar correctamente al hijo
<HeaderEditor 
  value={headerConfig}  // DEBE venir del store
  onChange={handleHeaderChange}
/>
```

---

### 3. PREVIEW NO SE ACTUALIZA

#### Síntomas:
- Cambias configuración pero el preview no refleja los cambios
- El preview se actualiza con delay o inconsistentemente

#### Diagnóstico:
```typescript
// En EditorPreview.tsx, agregar logs
useEffect(() => {
  console.log('Config changed:', config);
}, [config]);

useEffect(() => {
  console.log('Sections changed:', sections);
}, [sections]);
```

#### Soluciones:

**Solución A: Verificar suscripciones al store**
```typescript
// EditorPreview.tsx DEBE suscribirse a ambos stores
const config = useThemeConfigStore(state => state.config);
const sections = useEditorStore(state => state.sections);

// Aplicar cambios
useEffect(() => {
  applyConfigToPreview(config);
}, [config]);

useEffect(() => {
  applySectionsToPreview(sections);
}, [sections]);
```

**Solución B: Forzar re-render del iframe**
```typescript
// Si usas iframe para preview
const [iframeKey, setIframeKey] = useState(0);

useEffect(() => {
  setIframeKey(prev => prev + 1); // Fuerza recrear iframe
}, [config, sections]);

<iframe key={iframeKey} ... />
```

---

### 4. SE PIERDEN CAMBIOS AL NAVEGAR

#### Síntomas:
- Cambias de sección y los cambios no guardados se pierden
- Al volver a una sección, está en su estado anterior

#### Soluciones:

**Solución A: Auto-save antes de navegar**
```typescript
const handleSectionChange = async (newSection) => {
  if (hasLocalChanges) {
    await saveCurrentSection();
  }
  setActiveSection(newSection);
};
```

**Solución B: Advertencia antes de navegar**
```typescript
const handleNavigation = () => {
  if (isDirty) {
    if (!confirm('You have unsaved changes. Continue?')) {
      return;
    }
  }
  navigate();
};
```

---

### 5. ERROR 400 AL GUARDAR

#### Síntomas:
- El botón Save da error 400
- La consola muestra "Bad Request"

#### Diagnóstico:
```typescript
// Ver el payload que se envía
console.log('Saving payload:', JSON.stringify(config, null, 2));
```

#### Soluciones:

**Solución A: Verificar campos null**
```typescript
// Asegurar que no hay null donde no debe
const sanitizedConfig = {
  ...config,
  appearance: config.appearance || defaultAppearance,
  typography: config.typography || defaultTypography,
  // etc...
};
```

**Solución B: Verificar estructura del DTO**
```csharp
// En el backend, hacer campos nullable si es necesario
public class UpdateGlobalThemeConfigDto
{
    public AppearanceConfig? Appearance { get; set; }
    public TypographyConfig? Typography { get; set; }
    // etc...
}
```

---

### 6. CONFLICTO ENTRE ESTADO LOCAL Y GLOBAL

#### Síntomas:
- El componente muestra un valor pero el store tiene otro
- Los cambios se "revierten" solos

#### Soluciones:

**Solución A: Sincronización correcta**
```typescript
// Estado local SIEMPRE debe sincronizarse con props
const [localValue, setLocalValue] = useState(propValue);

useEffect(() => {
  setLocalValue(propValue);
}, [propValue]);

const handleChange = (newValue) => {
  setLocalValue(newValue);  // Local primero
  onChange(newValue);        // Luego propagar
};
```

**Solución B: Eliminar estado local innecesario**
```typescript
// Si no necesitas optimización, usa directamente props
<input 
  value={value}  // Directo de props
  onChange={(e) => onChange(e.target.value)}
/>
```

---

## 🛠️ COMANDOS DE DEBUGGING

### En la consola del navegador:

```javascript
// Ver estado completo del theme config
useThemeConfigStore.getState()

// Ver estado del editor
useEditorStore.getState()

// Forzar isDirty
useEditorStore.setState({ isDirty: true })

// Ver history de undo
useEditorStore.getState().history

// Limpiar todo y empezar fresh
localStorage.clear()
sessionStorage.clear()
location.reload()
```

### En el código:

```typescript
// Debug hook para ver cambios
useEffect(() => {
  console.log('[DEBUG] Config changed:', config);
  console.trace(); // Ver stack trace
}, [config]);

// Debug render
console.log(`[${new Date().toISOString()}] Rendering ComponentName`);

// Debug props
console.table(props);
```

---

## 📊 DIAGRAMA DE DEBUGGING

```
¿El botón Save no aparece?
    ↓
¿isDirty es true? → NO → Actualizar isDirty en el onChange
    ↓ SÍ
¿hasChanges se calcula bien? → NO → Verificar lógica de hasChanges
    ↓ SÍ
¿El botón está condicionalmente renderizado? → NO → Agregar condición
    ↓ SÍ
Verificar CSS (puede estar oculto)

---

¿Undo no funciona?
    ↓
¿History tiene estados? → NO → Implementar saveHistory()
    ↓ SÍ
¿historyIndex cambia? → NO → Verificar lógica de undo()
    ↓ SÍ
¿Componente escucha cambios? → NO → Agregar useEffect
    ↓ SÍ
Verificar que el value viene del store

---

¿Preview no se actualiza?
    ↓
¿Store tiene los cambios? → NO → Verificar updateConfig()
    ↓ SÍ
¿Preview está suscrito? → NO → Agregar useStore hook
    ↓ SÍ
¿useEffect se ejecuta? → NO → Verificar dependencias
    ↓ SÍ
Verificar aplicación de estilos
```

---

## 🚑 RECUPERACIÓN DE EMERGENCIA

### Si todo está roto:

1. **Backup actual**:
```bash
git add .
git commit -m "BACKUP: Before emergency fix"
```

2. **Resetear stores**:
```javascript
// En consola del navegador
useThemeConfigStore.setState(useThemeConfigStore.getInitialState())
useEditorStore.setState(useEditorStore.getInitialState())
```

3. **Limpiar localStorage**:
```javascript
localStorage.clear()
sessionStorage.clear()
```

4. **Recargar fresh**:
```javascript
location.href = location.href + '?fresh=' + Date.now()
```

5. **Si sigue roto, volver al backup**:
```bash
git reset --hard HEAD^
```

---

## 📝 LOGS PARA COPIAR Y PEGAR

```typescript
// Agregar al inicio del componente problemático
useEffect(() => {
  console.group(`[${new Date().toISOString()}] ComponentName Debug`);
  console.log('Props:', props);
  console.log('State:', { localState });
  console.log('Store:', useStore.getState());
  console.groupEnd();
});

// Para rastrear cambios
useEffect(() => {
  console.log(`[CHANGE] propertyName:`, propertyName);
}, [propertyName]);

// Para medir performance
console.time('OperationName');
// ... código ...
console.timeEnd('OperationName');
```

---

## 🔍 CHECKLIST DE DEBUGGING

Antes de pedir ayuda, verifica:

- [ ] ¿Revisaste la consola del navegador?
- [ ] ¿Agregaste console.logs en los puntos clave?
- [ ] ¿Verificaste el Network tab para errores de API?
- [ ] ¿Revisaste que los stores tienen los datos correctos?
- [ ] ¿Los useEffect tienen las dependencias correctas?
- [ ] ¿Probaste en modo incógnito?
- [ ] ¿Hiciste hard refresh (Ctrl+Shift+R)?
- [ ] ¿Funciona en otro navegador?

---

**Última actualización**: 13 de enero 2025
**Versión**: 1.0.0
**Mantener actualizado con cada problema nuevo resuelto**