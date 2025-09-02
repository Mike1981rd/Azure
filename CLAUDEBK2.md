### ✅ 5. BOTONES CON ESTADOS
```typescript
// Botón primario con loading y disabled:
<button
  disabled={loading || !hasChanges}
  className="disabled:opacity-50 disabled:cursor-not-allowed"
  style={{ backgroundColor: hasChanges ? primaryColor : '#9ca3af' }}
>
  {loading ? (
    <span className="flex items-center gap-2">
      <svg className="animate-spin h-4 w-4">...</svg>
      {t('common.saving', 'Saving...')}
    </span>
  ) : (
    t('common.save', 'Save')
  )}
</button>
```

### ✅ 6. BREADCRUMBS (NAVEGACIÓN)
```typescript
// Breadcrumbs responsivos - ocultos en móvil
<nav className="hidden sm:flex mb-4 text-sm" aria-label="Breadcrumb">
  <ol className="flex items-center space-x-2">
    <li>
      <a href="/dashboard" className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300">
        {t('navigation.dashboard')}
      </a>
    </li>
    <li className="text-gray-400 dark:text-gray-500">/</li>
    <li className="text-gray-700 font-medium dark:text-gray-300">
      {t('navigation.currentPage', 'Current Page')}
    </li>
  </ol>
</nav>

// Título móvil alternativo
<div className="sm:hidden mb-4">
  <h1 className="text-xl font-semibold text-gray-900 dark:text-white">
    {t('navigation.currentPage', 'Current Page')}
  </h1>
</div>
```

### ✅ 7. FOCUS STATES CON COLOR PRIMARIO EN INPUTS
**⚠️ INSTRUCCIÓN OBLIGATORIA**: TODOS los inputs, selects, textareas y checkboxes DEBEN usar el color primario del usuario en sus estados de focus. NUNCA usar el negro/azul por defecto del navegador.

```typescript
// 1. Crear funciones helper en el componente
const getInputClassName = (hasError: boolean) => {
  return `w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-opacity-30 dark:bg-gray-700 dark:text-white transition-all ${
    hasError 
      ? 'border-red-300 dark:border-red-600' 
      : 'border-gray-300 dark:border-gray-600'
  }`;
};

const getInputStyle = () => ({
  '--tw-ring-color': primaryColor,
} as React.CSSProperties);

const handleInputFocus = (e: React.FocusEvent<HTMLInputElement | HTMLSelectElement>) => {
  e.target.style.borderColor = primaryColor;
  e.target.style.boxShadow = `0 0 0 3px ${primaryColor}33`; // 33 = 20% opacity
};

const handleInputBlur = (e: React.FocusEvent<HTMLInputElement | HTMLSelectElement>, hasError: boolean) => {
  e.target.style.borderColor = hasError ? '#fca5a5' : '#d1d5db';
  e.target.style.boxShadow = '';
};

// 2. Aplicar a TODOS los elementos del formulario
<input
  type="text"
  className={getInputClassName(!!errors.field)}
  style={getInputStyle()}
  onFocus={handleInputFocus}
  onBlur={(e) => handleInputBlur(e, !!errors.field)}
/>

// 3. Para checkboxes usar accentColor
<input
  type="checkbox"
  style={{ 
    accentColor: primaryColor,
    '--tw-ring-color': primaryColor 
  }}
  onFocus={(e) => e.target.style.boxShadow = `0 0 0 3px ${primaryColor}33`}
  onBlur={(e) => e.target.style.boxShadow = ''}
/>
```

**REGLAS CRÍTICAS**:
- SIEMPRE remover `focus:ring-blue-500` o cualquier color hardcodeado
- SIEMPRE usar `focus:outline-none` para control total
- SIEMPRE aplicar transición suave con `transition-all`
- MANTENER borde rojo en errores, pero focus sigue siendo color primario
- Box shadow al 20-30% de opacidad (usar `${primaryColor}33`)

### ✅ 8. SELECTOR DE PAÍS CON BANDERAS

#### Opción A: Select HTML nativo con bandera visible (Solución rápida - Locations)
```typescript
// 1. Instalar librería flag-icons
npm install flag-icons

// 2. Importar CSS en layout.tsx
import "flag-icons/css/flag-icons.min.css";

// 3. Usar componente CountryFlag (soporta ambas interfaces)
import { CountryFlag, countries } from '@/components/ui/CountryFlag';

// 4. Implementar select con bandera visible
<div className="relative">
  <select
    value={formData.country}
    onChange={(e) => setFormData({ ...formData, country: e.target.value })}
    className="w-full pl-10 pr-4 py-2 border rounded-lg appearance-none"
  >
    <option value="">Select a country</option>
    {Object.entries(countries).map(([code, country]) => (
      <option key={code} value={code}>
        {country.name}
      </option>
    ))}
  </select>
  
  {/* Bandera del país seleccionado */}
  <div className="absolute left-3 top-1/2 transform -translate-y-1/2 pointer-events-none">
    <CountryFlag countryCode={formData.country || 'US'} className="w-5 h-4" />
  </div>
</div>
```

#### Opción B: Radix UI Select con banderas completas (Recomendado - Store Details)
```typescript
// 1. Instalar dependencias
npm install @radix-ui/react-select flag-icons

// 2. Implementar con Radix UI para banderas en dropdown
import * as Select from '@radix-ui/react-select';
import { CountryFlag, countries } from '@/components/ui/CountryFlag';

<Select.Root value={country} onValueChange={setCountry}>
  <Select.Trigger className="w-full flex items-center justify-between">
    <Select.Value>
      {country ? (
        <div className="flex items-center gap-2">
          <CountryFlag code={countries[country].flag} />
          <span>{countries[country].name}</span>
        </div>
      ) : (
        <span>Select country...</span>
      )}
    </Select.Value>
  </Select.Trigger>
  
  <Select.Portal>
    <Select.Content>
      <Select.Viewport>
        {Object.entries(countries).map(([code, country]) => (
          <Select.Item key={code} value={code} className="flex items-center gap-2">
            <CountryFlag code={country.flag} />
            <Select.ItemText>{country.name}</Select.ItemText>
          </Select.Item>
        ))}
      </Select.Viewport>
    </Select.Content>
  </Select.Portal>
</Select.Root>
```

#### ⚠️ Notas importantes:
- **CountryFlag acepta DOS props**: 
  - `countryCode`: Para códigos ISO como 'US', 'MX' (usado en Locations)
  - `code`: Para códigos de bandera como 'us', 'mx' (usado en Store Details)
- **countries es un OBJETO**, no array - siempre usar `Object.entries(countries)`
- **Limitación HTML**: Las banderas NO se muestran dentro de `<option>` por restricciones del navegador
- **Store Details**: Usa Radix UI Select para mostrar banderas en el dropdown
- **Locations**: Usa select nativo con bandera visible mediante position absolute
```

### 📋 EJEMPLO COMPLETO DE COMPONENTE
```typescript
export function NewComponent() {
  const { t } = useI18n();
  const [primaryColor, setPrimaryColor] = useState('#22c55e');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const settings = localStorage.getItem('ui-settings');
    if (settings) {
      const parsed = JSON.parse(settings);
      setPrimaryColor(parsed.primaryColor || '#22c55e');
    }
  }, []);

  return (
    <div className="w-full min-h-screen">
      {/* Breadcrumbs - Desktop */}
      <nav className="hidden sm:flex mb-4 text-sm" aria-label="Breadcrumb">
        <ol className="flex items-center space-x-2">
          <li>
            <a href="/dashboard" className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300">
              {t('navigation.dashboard')}
            </a>
          </li>
          <li className="text-gray-400 dark:text-gray-500">/</li>
          <li className="text-gray-700 font-medium dark:text-gray-300">
            {t('component.title', 'Title')}
          </li>
        </ol>
      </nav>
      
      {/* Mobile Title */}
      <div className="sm:hidden mb-4">
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white">
          {t('component.title', 'Title')}
        </h1>
      </div>
      
      {/* Main Content */}
      <div className="p-3 sm:p-6 bg-white dark:bg-gray-800 rounded-lg">
        <button 
          className="px-4 py-2 text-white rounded-lg"
          style={{ backgroundColor: primaryColor }}
        >
          {t('common.save', 'Save')}
        </button>
      </div>
    </div>
  );
}
```

### ⚠️ VALIDACIÓN ANTES DE ENTREGAR
- [ ] ¿Todas las strings están traducidas con useI18n?
- [ ] ¿Los botones principales usan el color primario?
- [ ] ¿Los inputs/selects/checkboxes usan el color primario en focus?
- [ ] ¿Funciona correctamente en dark mode?
- [ ] ¿Se ve bien en móvil (320px) y desktop?
- [ ] ¿Los botones tienen estados loading/disabled?
- [ ] ¿Tiene breadcrumbs en desktop y título en móvil?

## ⚠️ REGLAS CRÍTICAS DE BASE DE DATOS - NUNCA VIOLAR

### 🔴 PROHIBIDO - ACCIONES DESTRUCTIVAS
**NUNCA ejecutar estos comandos sin permiso EXPLÍCITO del usuario:**
- ❌ `dotnet ef database drop`
- ❌ `DROP DATABASE`
- ❌ `DELETE FROM` (sin WHERE en tablas con datos de producción)
- ❌ `TRUNCATE TABLE`
- ❌ Cualquier comando que pueda borrar datos existentes

### ✅ PROCESO CORRECTO para cambios de esquema con datos existentes:
1. **SIEMPRE preguntar primero**: "Este cambio requiere modificar la base de datos. ¿Deseas hacer un backup primero?"
2. **Ofrecer alternativas** que preserven datos:
   - Migraciones incrementales sin DROP
   - Scripts de actualización que preserven datos
   - Columnas nullable temporales
3. **Si es absolutamente necesario** borrar datos:
   - Explicar EXACTAMENTE qué se perderá
   - Esperar confirmación explícita: "¿Confirmas que deseas borrar [datos específicos]? S/N"
   - Solo proceder con "S" o "Sí" explícito

### 📋 CHECKLIST antes de ejecutar migraciones:
- [ ] ¿El comando preserva los datos existentes?
- [ ] ¿He advertido al usuario si hay riesgo de pérdida de datos?
- [ ] ¿Tengo permiso explícito si voy a borrar algo?
- [ ] ¿He ofrecido hacer un backup primero?

## 🔴 TROUBLESHOOTING CRUD - LEER SI SE IMPLEMENTA MÉTODO CRUD
**⚠️ NOTA IMPORTANTE**: Esta sección debe consultarse SIEMPRE cuando el usuario solicite implementar operaciones CRUD (Create, Read, Update, Delete) en cualquier módulo nuevo.

### 🎯 PROBLEMAS CRÍTICOS Y SOLUCIONES CRUD

#### 1. Error 400: "Company ID not found in token"
**Problema**: El token JWT usa `"companyId"` (minúscula) pero el controller busca `"CompanyId"` (mayúscula).

**Solución Correcta**:
```csharp
// SIEMPRE usar minúscula y fallback
var companyIdClaim = User.FindFirst("companyId")?.Value;
int companyId;
if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
{
    companyId = 1; // Usar company por defecto
}
```

#### 2. PostgreSQL JSONB - Error de Serialización
**Problema**: No se pueden guardar `List<string>` en columnas JSONB.

**Solución OBLIGATORIA en Program.cs**:
```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); // CRÍTICO - Sin esto falla JSONB
var dataSource = dataSourceBuilder.Build();
```

#### 3. URLs de API - Configuración Correcta
**SIEMPRE usar el puerto correcto**:
```typescript
// Frontend - usar puerto del backend
const API_URL = 'http://localhost:5266'; // NO usar 3000, NO usar 7224 en dev
const response = await fetch(`${API_URL}/api/locations`);
```

#### 4. Actualización Parcial de Datos
**Problema**: Actualizar un campo borra otros campos.

**Solución Backend**:
```csharp
// Manejar strings vacíos explícitamente
if (request.Phone != null && request.Phone != "")
    entity.Phone = request.Phone;
else if (request.Phone == "")
    entity.Phone = null; // Explícitamente null para vacío
```

#### 5. UI No Se Actualiza Después de Guardar
**SIEMPRE refrescar datos después de operación CRUD**:
```typescript
const handleSubmit = async () => {
  const response = await fetch(url, { method: 'POST', ... });
  if (response.ok) {
    await fetchData(); // CRÍTICO: Recargar datos
    resetForm();
  }
};
```

### 📋 CHECKLIST OBLIGATORIO PARA MÓDULOS CRUD

#### Backend:
- [ ] Controller busca `"companyId"` en minúscula con fallback a 1
- [ ] Service registrado en Program.cs
- [ ] DbSet agregado a ApplicationDbContext
- [ ] DTOs separados para Create/Update/Response
- [ ] Validación ModelState en todos los endpoints
- [ ] Try-catch con mensajes de error descriptivos

#### Frontend:
- [ ] URL de API apunta a puerto 5266 (no 3000)
- [ ] Headers incluyen Authorization y Content-Type
- [ ] Refetch de datos después de Create/Update/Delete
- [ ] Manejo de errores 400/401/404/500
- [ ] Estados loading/saving con feedback visual
- [ ] Validación de formularios antes de enviar

#### Migraciones:
- [ ] NUNCA crear archivos de migración manualmente
- [ ] Usuario ejecuta Add-Migration en Visual Studio
- [ ] Especificar siempre -Context ApplicationDbContext

### 🚨 ERRORES COMUNES A EVITAR

1. **NO usar** `User.FindFirst("CompanyId")` - siempre minúscula
2. **NO olvidar** `EnableDynamicJson()` para PostgreSQL JSONB
3. **NO mezclar** puertos - frontend en 3000, backend en 5266
4. **NO asumir** que el token tiene CompanyId - usar fallback
5. **NO olvidar** refrescar datos después de guardar
6. **NO crear** migraciones manualmente - Visual Studio las genera

### 🔧 SNIPPETS REUTILIZABLES

**Controller Method Template**:
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateDto dto)
{
    try
    {
        var companyIdClaim = User.FindFirst("companyId")?.Value;
        int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);
        
        var result = await _service.CreateAsync(companyId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

**Frontend Fetch Template**:
```typescript
const fetchData = async () => {
  try {
    const token = localStorage.getItem('token');
    const response = await fetch('http://localhost:5266/api/resource', {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      const error = await response.text();
      console.error('Error:', response.status, error);
      return;
    }
    
    const data = await response.json();
    setData(data);
  } catch (error) {
    console.error('Fetch error:', error);
  } finally {
    setLoading(false);
  }
};
```

**Documentación Completa**: Ver `/docs/implementations/features/Guardado.md` para guía detallada de troubleshooting.

## 🛠️ COMANDOS CLAUDE CODE DISPONIBLES
- `create-section [nombre]` - Crear nueva sección del builder
- `create-module [nombre]` - Crear módulo completo back office
- `debug-builder [error]` - Debuggear website builder
- `optimize-performance [área]` - Optimizar performance
- `/document-implementation [feature]` - Documentar implementación y troubleshooting

## 🗄️ MIGRACIONES DE BASE DE DATOS - PROCESO CRÍTICO
**⚠️ IMPORTANTE**: Las migraciones se ejecutan desde PowerShell en el entorno WSL.

### ✅ PROCESO CORRECTO DE MIGRACIONES:

#### Paso 1: Claude Code PREPARA el modelo
```csharp
// Claude Code crea/modifica:
// - Models/NuevoModelo.cs
// - Data/ApplicationDbContext.cs (agregar DbSet)
// - DTOs si son necesarios
// - NO crea archivos de migración manualmente
```

#### Paso 2: Claude Code PREGUNTA al usuario
```markdown
## 🗄️ MIGRACIÓN REQUERIDA

**Nombre de migración:** `AddNombreFeature`
**Contexto:** `ApplicationDbContext`

**Cambios que incluye:**
- ✅ Tabla Companies con campos X, Y, Z
- ✅ Relación con tabla Usuarios
- ✅ Índices en campos de búsqueda

¿La ejecuto por PowerShell? [S/N]
```

#### Paso 3: SI el usuario confirma, Claude Code EJECUTA
```bash
# COMANDO CORRECTO desde WSL:
powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'; dotnet ef migrations add AddNombreFeature --context ApplicationDbContext"

# Luego aplicar a la base de datos:
powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'; dotnet ef database update --context ApplicationDbContext"
```

### ⚠️ RECORDATORIO PARA CLAUDE CODE:
- **SIEMPRE** usar `powershell.exe -Command` desde WSL
- **NUNCA** intentar usar `dotnet` directamente (no está en Linux)
- **NUNCA** decir "no puedo ejecutar migraciones"
- **SIEMPRE** preguntar "¿La ejecuto por PowerShell?" después de preparar los modelos

### 📋 EJEMPLO DE FLUJO COMPLETO:

**Claude Code prepara los modelos y luego:**

```markdown
## 🗄️ MIGRACIÓN REQUERIDA

**Nombre de migración:** `AddSecurityFields`
**Contexto:** `ApplicationDbContext`

**Cambios que incluye:**
- ✅ Nuevos campos en tabla Customers: RecoveryEmail, SessionTimeoutMinutes
- ✅ Nueva tabla CustomerSecurityQuestions
- ✅ Índices y foreign keys correspondientes

¿La ejecuto por PowerShell? [S/N]
```

**Si el usuario responde "S" o "Sí", Claude Code ejecuta automáticamente:**
```bash
powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'; dotnet ef migrations add AddSecurityFields --context ApplicationDbContext"

powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI'; dotnet ef database update --context ApplicationDbContext"
```

### 🔴 PROBLEMAS COMUNES Y SOLUCIONES:

#### Problema: Migraciones duplicadas (CS0111)
**Causa:** Claude Code creó migración manual + Visual Studio auto-generó otra
**Solución:** 
1. Eliminar archivo de migración creado por Claude Code
2. Mantener solo el auto-generado por Visual Studio
3. Recompilar proyecto

#### Problema: El contexto no está especificado
**Causa:** Múltiples DbContext en el proyecto
**Solución:** SIEMPRE usar `-Context ApplicationDbContext`

### 📝 CHECKLIST DE MIGRACIÓN:
- [ ] Claude Code preparó modelos y DbContext
- [ ] Claude Code NO creó archivos .cs de migración
- [ ] Claude Code proporcionó nombre descriptivo de migración
- [ ] Claude Code especificó el contexto correcto
- [ ] Usuario ejecutó Add-Migration en Visual Studio
- [ ] Usuario ejecutó Update-Database
- [ ] Usuario confirmó éxito

## 📝 CHECKLIST ANTES DE CADA CAMBIO
- [ ] ¿Estoy separando correctamente habitaciones de productos?
- [ ] ¿Las secciones permiten múltiples instancias?
- [ ] ¿El drag & drop tiene validaciones?
- [ ] ¿Estoy usando cache diferenciado?
- [ ] ¿Los productos tienen sistema de variantes?
- [ ] ¿Implementé undo/redo para esta acción?
- [ ] ¿Incluí los 5 tipos de páginas estándar?
- [ ] ¿Los archivos tienen menos de 300 líneas?
- [ ] ¿Estoy siguiendo la arquitectura limpia?

## ⚠️ RECORDATORIOS CRÍTICOS
1. **NUNCA** mezclar lógica de reservas con e-commerce
2. **SIEMPRE** validar permisos multi-tenant
3. **SIEMPRE** usar TypeScript estricto en frontend
4. **NUNCA** crear archivos de más de 300 líneas
5. **SIEMPRE** implementar tests unitarios
6. **NUNCA** hacer push directo a main
7. **SIEMPRE** documentar APIs con Swagger
8. **SIEMPRE** manejar errores apropiadamente

## 🧩 COMPONENTES RESPONSIVE REUTILIZABLES

### 📦 Componentes Core Disponibles

#### 1. ResponsiveTabs
Automáticamente renderiza pestañas verticales en móvil y horizontales en desktop.
```typescript
import { ResponsiveTabs } from '@/components/responsive/ResponsiveTabs';

// Uso:
<ResponsiveTabs
  tabs={[
    { id: 'overview', label: 'Overview', icon: '📊' },
    { id: 'security', label: 'Security', icon: '🔒' }
  ]}
  activeTab={activeTab}
  onTabChange={setActiveTab}
  primaryColor={primaryColor}
/>
```

#### 2. ResponsiveTable
Automáticamente convierte tablas en cards apiladas en móvil.
```typescript
import { ResponsiveTable } from '@/components/responsive/ResponsiveTable';

// Uso:
<ResponsiveTable
  data={customers}
  columns={[
    { key: 'name', label: 'Name', priority: 'high' },
    { key: 'email', label: 'Email', priority: 'medium' },
    { key: 'status', label: 'Status', priority: 'high' }
  ]}
  onRowClick={handleRowClick}
  primaryColor={primaryColor}
/>
```

#### 3. MobileActionBar
Maneja botones de acción con layout optimizado para móvil.
```typescript
import { MobileActionBar } from '@/components/mobile/MobileActionBar';

// Uso:
<MobileActionBar
  actions={[
    { id: 'save', label: 'Save', variant: 'primary', onClick: handleSave },
    { id: 'cancel', label: 'Cancel', variant: 'secondary', onClick: handleCancel }
  ]}
  primaryColor={primaryColor}
  position="fixed" // fixed | sticky | relative
/>

// ⚠️ IMPORTANTE: Si usas position="fixed", agrega padding al contenedor:
<div className="pb-24 md:pb-0"> // Evita que el contenido se tape
  {/* Tu contenido */}
</div>
```

### 🎯 USO OBLIGATORIO

**SIEMPRE usar estos componentes cuando implementes:**
- Páginas con pestañas → `ResponsiveTabs`
- Listas/tablas de datos → `ResponsiveTable`
- Botones de acción → `MobileActionBar`

### 📋 Checklist al Recibir un Diseño

Cuando recibas un diseño nuevo, SIEMPRE:

1. **Identificar componentes responsive necesarios:**
   - [ ] ¿Tiene pestañas? → Usar ResponsiveTabs
   - [ ] ¿Tiene tabla? → Usar ResponsiveTable
   - [ ] ¿Tiene botones de acción? → Usar MobileActionBar
   - [ ] ¿Tiene formulario? → Aplicar patrones de inputs w-11/12
   - [ ] ¿Tiene métricas? → Usar grid 2x2

2. **Implementar versión dual desde el inicio:**
   ```typescript
   // NO hacer esto:
   <div className="flex gap-4">  // Solo desktop
   
   // SIEMPRE hacer esto:
   <div className="flex flex-col sm:flex-row gap-3 sm:gap-4">  // Mobile + Desktop
   ```

3. **Verificar en viewports móviles:**
   - 320px (iPhone SE)
   - 375px (iPhone standard)
   - 414px (iPhone Plus)

## 🚦 CRITERIOS DE ÉXITO
- Los 9 problemas originales están resueltos
- Performance < 2s carga inicial
- 100% responsive (mobile-first)
- Sin errores de TypeScript
- Cobertura de tests > 80%
- Documentación actualizada
- Deploy automatizado funcionando

# 📋 PROJECT WORKFLOW RULES

## Blueprint & Progress Management
- Always use PROJECT-PROGRESS.md as the single source of truth for project status
- Before starting any task, read both the original blueprint and PROJECT-PROGRESS.md
- Update PROJECT-PROGRESS.md before and after each task completion

## Task Execution Rules
- **CRITICAL**: Work on only ONE task at a time from the progress tracker
- When you complete a single task, STOP immediately and say "TASK COMPLETED - Ready for next instruction"
- This allows for context window management and work review between tasks
- Wait for explicit instruction before proceeding to the next task

## Git & Database Safety Rules
- **NEVER commit to GitHub without explicit permission**
- Before any git commit, ask: "Ready to commit these changes to GitHub? [Y/N]"
- **NEVER run database migrations without explicit permission**
- Before any migration command (dotnet ef database update, etc.), ask: "Ready to apply database migration? [Y/N]"
- Always show what will be committed/migrated before asking for permission
- If permission denied, continue with other tasks and note pending changes in PROJECT-PROGRESS.md

## Database Migration Workflow Rules
- **Claude Code PREPARES models only** - create/modify models, update DbContext, create DTOs
- **Claude Code NEVER creates migration files** - no .cs migration files, Visual Studio generates them
- **Human EXECUTES migrations** - user runs Add-Migration and Update-Database in Visual Studio
- **Claude Code MUST provide:**
  1. Descriptive migration name (e.g., `AddShippingFeature`, `UpdateCompanyModel`)
  2. Exact commands with context: `Add-Migration MigrationName -Context ApplicationDbContext`
  3. List of changes the migration will include
  4. Both Package Manager Console and CLI alternatives
- **Workflow steps:**
  1. Claude prepares models and DbContext changes
  2. Claude provides migration instructions with specific name
  3. User executes Add-Migration in Visual Studio
  4. User executes Update-Database
  5. User confirms completion

## Modified Permission Rules
- Remove any automatic database update permissions
- Claude CANNOT create migration .cs files (Visual Studio generates them)
- Always provide descriptive migration name and exact commands
- Document model changes and migration instructions separately in progress tracker

## Documentation Standards
- Always document completed tasks with:
  - Files created/modified
  - Commands executed
  - Decisions made
  - Any blockers encountered
  - Pending commits/migrations awaiting approval
- Keep implementation notes detailed but concise

## Session Handoff
- At session start, read PROJECT-PROGRESS.md and give status summary
- Always identify the next logical task before beginning work
- Check for any pending commits or migrations from previous sessions
- Maintain continuity between sessions using the progress tracker

## Error Handling
- If any command fails, document the error in PROJECT-PROGRESS.md
- Suggest solutions but don't auto-retry critical operations
- Always ask before making system-level changes

Remember: Quality and safety over speed. Better to complete one task perfectly with proper approvals than rush through multiple risky operations.

## 📚 DOCUMENTATION STANDARDS

### 🗂️ Documentation Structure
```
docs/
├── implementations/          # Implementation documentation
│   ├── auth/                # Authentication implementations
│   ├── api/                 # API feature implementations
│   ├── features/            # Business feature implementations
│   └── infrastructure/     # Infrastructure setup docs
├── troubleshooting/         # Problem-solution documentation
│   ├── auth/               # Authentication issues
│   ├── api/                # API integration issues
│   ├── database/           # Database & migration issues
│   ├── frontend/           # Next.js & UI issues
│   └── general/            # General development issues
└── documentation-templates/ # Standard templates
```

### 📋 Troubleshooting Documentation Rules
1. **File size limit**: Maximum 800 lines per .md file
2. **Structure**: Use modular approach in `/docs/troubleshooting/`
3. **Naming convention**: `category-##-descriptive-name.md`
4. **Required sections**:
   - Problem Summary (affects, frequency, severity)
   - Symptoms (checklist format with exact errors)
   - Root Causes (numbered with verification steps)
   - Solutions (Quick Fix < 5min, Step-by-Step, Alternatives)
   - Prevention (best practices, configuration templates)
   - Related Issues (cross-references)
   - Search Keywords
5. **Navigation**: Always include breadcrumb navigation and cross-references
6. **Indices**: Update ALL relevant index files when adding new problems:
   - Master index (`00-troubleshooting-index.md`)
   - Category index (`category-00-index.md`)

### 📄 Implementation Documentation Rules
1. **Create implementation docs** for major features in `/docs/implementations/`
2. **Naming convention**: `YYYY-MM-feature-name.md` (e.g., `2025-08-login-implementation.md`)
3. **Required sections**:
   ```markdown
   # Feature Name Implementation
   
   ## Overview
   - **Purpose**: Why this feature exists
   - **Scope**: What it includes/excludes
   - **Dependencies**: Required packages/services
   - **Date Implemented**: YYYY-MM-DD
   
   ## Architecture Decisions
   - **Pattern Used**: (e.g., Repository, Service Layer)
   - **Technology Choices**: Why X over Y
   - **Security Considerations**: Auth, validation, etc.
   
   ## Implementation Details
   ### Backend
   - Models created/modified
   - API endpoints
   - Services & repositories
   - Database changes
   
   ### Frontend
   - Components created
   - State management
   - API integration
   - UI/UX decisions
   
   ## Configuration
   - Environment variables
   - appsettings.json changes
   - Package installations
   
   ## Testing
   - Unit tests location
   - Integration test approach
   - Manual testing checklist
   
   ## Known Issues & Limitations
   - Current limitations
   - Future improvements
   - Performance considerations
   
   ## Troubleshooting
   - Common problems (link to troubleshooting docs)
   - Debug tips
   
   ## References
   - Related documentation
   - External resources
   ```

### 📝 When to Document
1. **Always document** when:
   - Implementation took >30 minutes to solve
   - Multiple attempts were needed
   - Non-obvious solution was required
   - Architecture decision was made

2. **Major features requiring docs**:
   - Authentication & Authorization
   - API integrations (payment, email, etc.)
   - Complex business logic (multi-tenancy, etc.)
   - Infrastructure setup (Docker, CI/CD)
   - Database design decisions

3. **Complex debugging** requiring troubleshooting docs:
   - Any error that took >15 minutes to resolve
   - Configuration issues
   - Integration problems
   - Performance bottlenecks

### 🔄 Documentation Workflow
1. **During Implementation**:
   - Take notes of decisions and problems
   - Capture exact error messages
   - Note configuration changes

2. **After Completion**:
   - Run `/document-implementation [feature-name]`
   - Follow prompts for:
     - Implementation type (auth/api/feature/infra)
     - Problems encountered (Y/N)
     - Key decisions made
     - Testing approach

3. **Documentation Creation**:
   - Creates implementation doc in correct folder
   - Creates troubleshooting docs if problems occurred
   - Updates all index files automatically
   - Adds references to PROJECT-PROGRESS.md

4. **Quality Check**:
   - Verify all sections completed
   - Check cross-references work
   - Ensure code examples are accurate
   - Confirm file size < 800 lines

### 📐 Documentation Templates Usage
- **Implementation**: Use `/docs/documentation-templates/implementation-template.md`
- **Troubleshooting**: Use `/docs/documentation-templates/troubleshooting-template.md`
- **Quick Reference**: Check `/docs/documentation-templates/documentation-checklist.md`

### 🎯 Documentation Examples
When I say:
- "Document the login implementation" → Create full implementation doc + any troubleshooting docs
- "Document this error" → Create troubleshooting doc with full problem-solution format
- "Update documentation" → Check what's missing and complete it

### ⚡ Quick Documentation Commands
```bash
# Full implementation + troubleshooting
/document-implementation login

# Just troubleshooting for current issue
/document-troubleshooting auth network-error

# Update indices after manual doc creation
/update-doc-indices
```

### 📊 Documentation Metrics
Track in PROJECT-PROGRESS.md:
- Features with complete documentation
- Troubleshooting issues documented
- Documentation coverage percentage
- Last documentation update date

## 🖼️ IMAGEN READING PROTOCOL - WSL ENVIRONMENT

### 📁 Ruta de Imágenes - Windows to Linux Mapping
**IMPORTANTE**: Este proyecto funciona en WSL (Windows Subsystem for Linux).

**Cuando el usuario proporciona rutas de Windows:**
- Usuario da: `C:\Users\hp\Desktop\imagen.png`
- **SIEMPRE convertir a**: `/mnt/c/Users/hp/Desktop/imagen.png`
- **NO cuestionar** la ruta o decir que "no se puede leer"
- **APLICAR directamente** la conversión y usar Read tool

### 🔧 Protocolo de Lectura
1. **Conversión automática**: `C:\` → `/mnt/c/`
2. **Leer inmediatamente** con la ruta convertida
3. **Si falla**, entonces buscar el archivo con bash/ls
4. **Nunca asumir** que la ruta es incorrecta por ser de Windows

### ✅ Ejemplos Correctos
```bash
# Usuario: C:\Users\hp\Desktop\calendario.png
# Claude: /mnt/c/Users/hp/Desktop/calendario.png ✅

# Usuario: C:\Users\hp\Documents\imagen.jpg  
# Claude: /mnt/c/Users/hp/Documents/imagen.jpg ✅
```

**RECORDATORIO**: Hemos leído cientos de imágenes de esta manera exitosamente. Mantener consistencia.