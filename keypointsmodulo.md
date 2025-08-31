## Guía integral para construir un módulo del Website Builder

Esta guía resume, en orden lógico y operativo, cómo crear un módulo completo (tipos, vistas de configuración, preview unificado, children, i18n, integraciones, validaciones, problemas comunes y fixes). Úsala como checklist práctico para entregar módulos consistentes y sin sorpresas.

### 1) Reglas y contexto críticos
- Ejecutar frontend/backend desde Windows (PowerShell). No uses WSL para `npm run dev` o `dotnet run`.
- Arquitectura modular: cada módulo bajo `src/components/editor/modules/[ModuleName]/` con archivos separados (< 300 líneas c/u).
- Preview unificado en `src/components/preview/Preview[ModuleName].tsx` (mismo componente sirve para editor y preview real).
- isDirty: toda modificación en el editor debe pasar por el store con `updateSectionSettings(...)` para marcar cambios.
- Sincronización: sincroniza estado local desde props con `useEffect` (compara con `JSON.stringify` si necesitas evitar renders).
- i18n: agregar claves en `src/lib/i18n/translations/modules/sections-en.json` y `sections-es.json` sin duplicar secciones.
- No añadas procesos largos desde el CLI; compila/ejecuta solo desde PowerShell del usuario.

### 2) Estructura de archivos del módulo
- `src/components/editor/modules/[ModuleName]/types.ts`
- `src/components/editor/modules/[ModuleName]/[ModuleName]Editor.tsx` (config padre)
- `src/components/editor/modules/[ModuleName]/[ModuleName]Children.tsx` (si tiene hijos)
- `src/components/editor/modules/[ModuleName]/[ModuleName]ItemEditor.tsx` (si tiene hijos)
- `src/components/editor/modules/[ModuleName]/index.ts`
- `src/components/preview/Preview[ModuleName].tsx`

### 3) Tipos y defaults (types.ts)
- Define interfaces del config (incluye `enabled`, `colorScheme`, `colorBackground`, `width`, heading/body, paddings, `customCss`, y `items` si hay hijos).
- Provee `getDefault[ModuleName]Config()` con valores razonables, y `getDefault[ModuleName]ItemConfig()` si aplica.
- Si habrá botón: `button: { label?, link?, style: 'solid'|'outline'|'text' }`.
- Si habrá layouts o estilos de colapsador: agrega enums/strings con valores explícitos.

### 4) Traducciones (i18n)
- Agrega un bloque `sections.[moduleName]` en `sections-en.json` y `sections-es.json` con:
  - `name`, `description` y `settings` (claves amigables para cada control del editor).
- Evita crear secciones duplicadas; si existe, amplíala.

### 5) Editor (configuración del padre)
- Patrón de estado: `localConfig` sincronizado desde `section.settings`; escribe con `updateSectionSettings`.
- Controles comunes:
  - Select de `colorScheme` (1–5) leyendo de `useThemeConfigStore` si requieres mostrar nombres.
  - Toggles de `colorBackground`, `addSidePaddings` y `expandFirst*` si aplica.
  - Selects de `width`, `layout`, tamaños de tipografía, y `weight` (mapéalos luego en preview).
  - Inputs de `heading`, `body` (textarea o RTE simple), `customCss`.
  - Campos de botón: label/link/style.
- Reglas UI:
  - Evita anchos fijos; usa contenedores flex y sliders `flex-1 min-w-0`.
  - Secciones colapsables con headers simples (ChevronDown/Up).
  - Mantén < 300 líneas; si crece, divide en subcomponentes.

### 6) Children (lista y drag & drop)
- `Children.tsx`: lista de items con DnD (dnd-kit), botón “Add [Module] block” azul, acciones de visibilidad y eliminar.
- IDs: usa `id: item_${Date.now()}` o similar.
- Selección de hijo: formato CRÍTICO `selectSection(`${parentId}:child:${itemId}`)` y abre el panel.
- Reordenar con `arrayMove`; persiste con `updateSectionSettings`.

### 7) ItemEditor (config específico del hijo)
- Estado local sincronizado a partir de `section.settings.items.find(...)`.
- Controles típicos: `heading`, `source` (HTML/RTE), icon select (lucide-react), media (imagen/video), tamaños.
- Uploads (dos opciones):
  - Modo rápido (editor): `URL.createObjectURL(file)` para previsualizar en el editor.
  - Modo real (producción): POST a `http://localhost:5266/api/MediaUpload/image|video` con token en `Authorization` y maneja respuesta `data.url`.
- Mantén el archivo < 300 líneas.

### 8) Preview unificado (Preview[ModuleName].tsx)
- Patrón dual de theme: usa `theme` prop si llega; si no, toma del store (`useThemeConfigStore(state => state.config)`).
- Detección móvil canónica (con listener de resize). No uses `deviceView || 'desktop'`.
- Color scheme (estructura plana): `text`, `background`, `solidButton`, `solidButtonText`, `outlineButton`, `outlineButtonText`.
- Tipografía: compón `headingTypographyStyles` y `bodyTypographyStyles` desde `themeConfig.typography` y luego ajusta `fontWeight`/`size` según config.
- Render:
  - Aplica paddings, `width` y `colorBackground`.
  - Si hay botón: mapea colores correctamente según `style` (solid/outline/text) usando campos del scheme (incluye `outlineButtonText`).
  - Si hay hijos: filtra `visible`, pinta alineaciones, íconos (lucide-react), colapsadores y layouts necesarios.
- Reglas de visibilidad: regresar `null` SOLO si `config.enabled === false && !isEditor`.
- Inserta `customCss` con `<style>` al final de la sección si corresponde.

### 9) Integraciones
- `SectionType` y `SECTION_CONFIGS` (defaults) en `src/types/editor.types.ts`.
- Editor Preview del editor: evita modificar piezas congeladas; usa las extensiones/casos existentes para renderizar el preview del nuevo tipo.
- `PreviewContent.tsx` (preview real): agrega caso en `getSectionType` y renderiza el preview unificado con `isEditor={false}`.
- `useEditorStore.savePage`: asegúrate de que tu `type` mapea a PascalCase en `typeMapping` al guardar en backend.
- `ConfigPanel.tsx`: si hay hijos, detecta `:child:` y renderiza el `[ModuleName]ItemEditor`.
- `EditorSidebarWithDnD.tsx`: renderiza `[ModuleName]Children` debajo del padre y crea la sección virtual si `selectedSectionId` es de un hijo.

### 10) Validaciones y pruebas
- PowerShell (usuario): `npm run type-check`, `npm run build`, `npm run dev`.
- Pruebas manuales:
  - Agregar módulo desde el editor.
  - Cambiar cada control: verifica en preview (sizes, weights, layout, colores, botón, paddings, width).
  - Hijos: agregar/editar/reordenar/eliminar/visibilidad; abrir item al hacer clic.
  - Preview real (`/preview/[page]`): renderiza igual que en el editor.
  - i18n: labels correctos en EN/ES sin duplicados.

### 11) Problemas comunes y fixes
- No se ve en preview real: faltan casos en `PreviewContent.tsx` o enabled check incorrecto (usa `config.enabled === false`).
- Hook error (“Rendered more hooks…”): mueve todos los hooks ANTES de returns condicionales.
- Móvil no responde: aplica el patrón canónico de `isMobile` con listener de resize.
- Colores de botón incorrectos: usa campos planos del scheme y `outlineButtonText` para texto en outline.
- Color scheme no cambia: asegura índice `parseInt(config.colorScheme)-1` y fallback al esquema 0.
- Hijo no abre editor: usa `:child:` (no `:item:`) y crea sección virtual en sidebar DnD.
- Drag & drop no persiste: recuerda `arrayMove` y `updateSectionSettings` con los nuevos `items`.
- Subida de media falla: verifica backend en 5266, token `Authorization`, tamaños/formatos, y usa `credentials: 'include'` si aplica.
- i18n duplicado: busca la sección existente y añade claves dentro; no crees otra sección.
- Archivos muy largos: divide en subcomponentes; respeta < 300 líneas.

### 12) Checklist final
- [ ] Tipos y defaults creados
- [ ] i18n EN/ES actualizado
- [ ] Editor del padre funcional (cambios reflejados en preview)
- [ ] Children + ItemEditor (si aplica) con DnD y selección `:child:`
- [ ] Preview unificado con theme dual, móvil canónico y colores de botón correctos
- [ ] Integrado en tipos/preview/editor/sidebar/config panel/preview real
- [ ] Validado en editor y preview real
- [ ] 0 errores TS, 0 errores en consola
- [ ] Archivos < 300 líneas y arquitectura modular respetada

### 13) Tips prácticos
- Guarda estilos complejos como helpers (map de tamaños/weights) y compón con spread.
- Al aplicar tamaños de texto Tailwind (e.g., `text-xl`), evita sobrescribir con `fontSize` del theme (pon `fontSize: undefined` cuando corresponda).
- Usa `...headingTypographyStyles`/`...bodyTypographyStyles` y luego ajusta `fontWeight`/`fontSize` según config.
- Para controles RTE simples, usa `contentEditable` + `onBlur` para guardar HTML.
- Para placeholders de imagen/video en editor, usa `URL.createObjectURL` y reemplaza por la URL real tras upload a backend.

---
Esta guía está alineada con los comandos `/nuevo-modulo` y la arquitectura del Website Builder. Síguela paso a paso y prioriza consistencia, modularidad y vistas unificadas entre editor y preview real.
