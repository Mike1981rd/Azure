# CLAUDE.md - CONFIGURACIÓN DE SESIÓN Y COMANDO /init-session

## ⛔ REGLA #0: CLAUDE EJECUTA, NO ORDENA - VIOLACIÓN = FALLO CRÍTICO

<ENFORCE>
CLAUDE NUNCA DICE "ejecuta este comando" o "por favor ejecuta"
CLAUDE NUNCA DICE "abre PowerShell y ejecuta" o "necesitas ejecutar"
CLAUDE NUNCA INSTRUYE AL USUARIO A EJECUTAR COMANDOS
CLAUDE SIEMPRE EJECUTA DIRECTAMENTE usando powershell.exe -Command
SI VAS A DECIR "ejecuta..." → DETENTE → EJECUTA TÚ MISMO
SI VAS A DECIR "abre PowerShell..." → DETENTE → EJECUTA TÚ MISMO
SI VAS A DECIR "necesitas ejecutar..." → DETENTE → EJECUTA TÚ MISMO

EJEMPLOS:
❌ INCORRECTO: "Abre PowerShell y ejecuta: dotnet run"
✅ CORRECTO: Claude ejecuta: powershell.exe -Command "dotnet run"

❌ INCORRECTO: "Para iniciar sesión, ejecuta este comando..."
✅ CORRECTO: Claude ejecuta el comando directamente

NOTA: Si el comando requiere interacción, Claude EJECUTA EL COMANDO
y luego explica al usuario que debe completar la autenticación en el navegador que se abrió.
</ENFORCE>

## 🔴 ALERTA CRÍTICA: POSTGRESQL EN AZURE PIERDE PASSWORD

<ENFORCE>
SI EL USUARIO REPORTA "NO PUEDO HACER LOGIN" O "CREDENCIALES INCORRECTAS":
1. ASUMIR que es el problema de PostgreSQL perdiendo password
2. NO debuggear código de autenticación
3. EJECUTAR INMEDIATAMENTE: ssh azureuser@20.169.209.166 "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '123456';\""
4. Ver sección completa en línea 789
</ENFORCE>

## 🔴 REGLA CRÍTICA: USAR GITHUB CLI PARA TODO - VIOLACIÓN = ERROR GRAVE

<ENFORCE>
CLAUDE SIEMPRE USA GITHUB CLI (gh) PARA CUALQUIER OPERACIÓN EN GITHUB
CLAUDE NUNCA USA git DIRECTAMENTE PARA OPERACIONES REMOTAS
CLAUDE NUNCA DICE "usa git push" o "ejecuta git pull"
CLAUDE SIEMPRE USA gh PARA:
- Crear pull requests: gh pr create
- Sincronizar repos: gh repo sync
- Ver issues: gh issue list
- Crear branches remotos: gh api
- Push/Pull: gh repo sync o gh pr
- Cualquier operación con GitHub

EJEMPLOS:
❌ INCORRECTO: git push origin main
✅ CORRECTO: gh repo sync --force

❌ INCORRECTO: git pull origin production
✅ CORRECTO: gh repo sync --source production

❌ INCORRECTO: Crear PR manualmente en GitHub
✅ CORRECTO: gh pr create --title "..." --body "..."

NOTA: Esta regla es ABSOLUTA. GitHub CLI (gh) es la ÚNICA herramienta permitida para GitHub.
</ENFORCE>

## 📦 REPOSITORIOS DEL PROYECTO (ACTUALIZADO)

### BACKEND (ASP.NET Core API):
- **Ubicación local**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI`
- Backend: `Mike1981rd/Render.git` (Render.com)
- Frontend: `Mike1981rd/Vercel-Frontend.git` (Vercel)
- **Rama para push**: `main` (SIEMPRE pushear a main en repositorio render)
- **Estrategia**: Siempre hacer push a `render/main` para deploys

### FRONTEND (Next.js):
- **Ubicación local**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin`
- Repositorio origin: `Mike1981rd/Vercel-Frontend.git` (Vercel)
- **Rama principal**: `main`
- Rama de trabajo actual: `whatsapp`
- **Estrategia**: Trabajar en rama whatsapp, hacer merge a main cuando esté completo

## 📝 GUÍA DE GITHUB - PROCEDIMIENTOS ESTÁNDAR

### 🚀 BACKEND - Push a Render
```bash
# Siempre desde el directorio del backend
cd /mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI

# 1. Verificar cambios
git status

# 2. Agregar cambios
git add .

# 3. Commit con mensaje descriptivo
git commit -m "feat: descripción del cambio"

# 4. Push SIEMPRE a render/main
git push render main

# NOTA: Si estás en otra rama, hacer merge a main primero
git checkout main
git merge tu-rama
git push render main
```

### 🎨 FRONTEND - Trabajo en rama WhatsApp
```bash
# Siempre desde el directorio del frontend
cd /mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin

# 1. Asegurarse de estar en la rama correcta
git checkout whatsapp || git checkout -b whatsapp

# 2. Verificar cambios
git status

# 3. Agregar cambios
git add .

# 4. Commit con mensaje descriptivo
git commit -m "feat: descripción del cambio"

# 5. Push a la rama WhatsApp
git push origin whatsapp

# CUANDO ESTÉ LISTO PARA PRODUCCIÓN:
# Hacer merge a main
git checkout main
git merge whatsapp
git push origin main
```

### ⚠️ REGLAS IMPORTANTES
1. **Backend**: SIEMPRE push a `render/main` para que Render haga el deploy automático
2. **Frontend**: Trabajar en `whatsapp` hasta completar el módulo de chat
3. **NO usar** comandos git con remotes antiguos como "azure" - ahora es "render"
4. **NO hacer** push directo a main en frontend mientras se trabaja en WhatsApp

### 🔄 Verificar configuración de remotes
```bash
# Backend - debe mostrar render, origin, production
git remote -v

# Si falta render o apunta a URL incorrecta:
git remote set-url render https://github.com/Mike1981rd/Render.git
# O si no existe:
git remote add render https://github.com/Mike1981rd/Render.git
```

## 🔑 CREDENCIALES DE SERVICIOS

### 📦 GitHub
- **Usuario**: Mike1981rd
- Token: [NO VERSIONAR] (usar autenticación local o GH CLI)
- Uso: nunca incluir tokens en remotes ni en archivos del repo

### 🚀 Render
- **Dashboard**: https://dashboard.render.com
- **Servicio**: `websitebuilder-api-staging`
- **URL API**: https://websitebuilder-api-staging.onrender.com
- **Deploy**: Automático al push a `render/main`

### ✅ Vercel
- Token: [NO VERSIONAR]
- **Proyecto**: `websitebuilder-admin`
- **URL Producción**: https://websitebuilder-admin.vercel.app
- **Deploy Command**:
```bash
npx vercel --prod --yes --token $VERCEL_TOKEN
```

### 🔮 Neon Database
- **Host**: `ep-withered-paper-ad3yhgct.c-2.us-east-1.aws.neon.tech`
- **Database**: `neondb`
- **Username**: `neondb_owner`
- **Password**: `npg_U1xrFkcz8PDC`
- **Port**: 5432
- **Project ID**: `proud-flower-27235934`
- **Dashboard**: https://console.neon.tech/app/projects/proud-flower-27235934
- **Connection String**:
```
Host=ep-withered-paper-ad3yhgct.c-2.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_U1xrFkcz8PDC;SSL Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30;Pooling=true;MinPoolSize=5;MaxPoolSize=20
```

## 🛑 REGLAS CRÍTICAS DE DESARROLLO - VIOLACIÓN = DETENCIÓN INMEDIATA

### 🚨 SISTEMA DE ENFORCEMENT AUTOMÁTICO ACTIVO

**ARCHIVOS DE CONTROL:**
- `.claude-rules-enforcement.md` - Sistema de validación y reglas
- `validate-module.sh` - Script de validación automática

### 🔴 REGLA #1: LÍMITE DE 300 LÍNEAS POR ARCHIVO - SIN EXCEPCIONES

**ANTES de escribir CUALQUIER código:**
1. EJECUTAR: `./validate-module.sh [archivo]`
2. Si el archivo actual tiene >250 líneas → CREAR NUEVO ARCHIVO
3. Si agregarías >50 líneas → CREAR COMPONENTE SEPARADO
4. Si es un preview → CREAR EN `/components/editor/modules/[Name]/`

**ARCHIVOS EN VIOLACIÓN (CONGELADOS - NO MODIFICAR):**
- ❌ EditorPreview.tsx con 1,763 líneas - **NO AGREGAR MÁS CÓDIGO**
- ❌ Para nuevos módulos usar: `/components/editor/modules/[ModuleName]/`

**ENFORCEMENT AUTOMÁTICO:**
```bash
# OBLIGATORIO ANTES DE CADA MODIFICACIÓN
./validate-module.sh [archivo]
# Si falla → STOP → Crear en /components/editor/modules/
```

### 🔴 REGLA #2: ARQUITECTURA MODULAR OBLIGATORIA

**NUEVA ESTRUCTURA OBLIGATORIA** para módulos del Website Builder:
```
components/
├── editor/
│   ├── modules/                    # 🆕 TODOS LOS NUEVOS MÓDULOS AQUÍ
│   │   ├── [ModuleName]/
│   │   │   ├── [ModuleName]Editor.tsx      (<300 líneas)
│   │   │   ├── [ModuleName]Preview.tsx     (<300 líneas)
│   │   │   ├── [ModuleName]Config.tsx      (<300 líneas)
│   │   │   ├── [ModuleName]Types.ts        (<100 líneas)
│   │   │   └── index.ts                    (exports)
```

**NO ESTÁ PERMITIDO:**
- Agregar NADA a EditorPreview.tsx (está CONGELADO)
- Crear archivos fuera de /modules/ para nuevas funcionalidades
- Crear archivos monolíticos de más de 300 líneas

### 🔴 REGLA #3: VERIFICACIÓN PRE-CÓDIGO OBLIGATORIA

**ANTES de escribir la PRIMERA línea de código, DEBES:**
```typescript
/**
 * PRE-CODE CHECKLIST - MUST BE TRUE:
 * [ ] File will be < 300 lines
 * [ ] Preview logic in separate file
 * [ ] Following modular architecture
 * [ ] Will update isDirty on changes
 * [ ] Will sync with props via useEffect
 * 
 * IF ANY FALSE → STOP → REDESIGN
 */
```

### 🔴 REGLA #4: AUTO-VERIFICACIÓN EN CADA ARCHIVO

**CADA archivo nuevo DEBE comenzar con:**
```typescript
/**
 * @file [NombreArchivo].tsx
 * @max-lines 300
 * @current-lines 0
 * @architecture modular
 * @validates-rules ✅
 */
```

### 🔴 REGLA #5: DETENCIÓN AUTOMÁTICA SI SE VIOLA

**Si Claude está por violar cualquier regla:**
1. DETENERSE inmediatamente
2. Decir: "⚠️ ALERTA: Esto violaría la regla de [X]. Propongo alternativa:"
3. Ofrecer solución modular que cumpla las reglas

## 🌐 ACCESO A SERVICIOS WINDOWS DESDE WSL - CRÍTICO

### ⚠️ CONFIGURACIÓN DE RED WSL2 + WINDOWS

**PROBLEMA**: WSL2 y Windows tienen interfaces de red SEPARADAS
- `localhost` en WSL apunta a WSL mismo, NO a Windows
- Los servicios corriendo en Windows NO son accesibles via localhost desde WSL

**SOLUCIÓN OBLIGATORIA**:
1. **Obtener IP del host Windows desde WSL**:
   ```bash
   ip route | grep default
   # Resultado típico: default via 172.25.64.1 dev eth0
   ```

2. **Usar IP del host para acceder a servicios Windows**:
   ```
   ❌ INCORRECTO: http://localhost:3000
   ✅ CORRECTO:   http://172.25.64.1:3000
   ```

### 📌 APLICACIÓN PRÁCTICA

**Para Playwright/Browser automation desde WSL**:
```javascript
// ❌ NO FUNCIONARÁ
await page.goto('http://localhost:3000/login');

// ✅ FUNCIONARÁ
await page.goto('http://172.25.64.1:3000/login');
```

**Para llamadas API desde WSL al backend en Windows**:
```bash
# ❌ NO FUNCIONARÁ
curl http://localhost:5266/api/endpoint

# ✅ FUNCIONARÁ  
curl http://172.25.64.1:5266/api/endpoint
```

**NOTA**: La IP del host puede cambiar tras reiniciar WSL. Siempre verificar con `ip route`.

## 🎯 COMANDO /init-session - CONFIGURACIÓN FORZADA

### ⚠️ IMPORTANTE: EJECUCIÓN OBLIGATORIA DEL COMANDO

Cuando el usuario ejecute `/init-session`, **DEBES OBLIGATORIAMENTE**:

1. **LEER TODOS LOS ARCHIVOS DE CONFIGURACIÓN** (no puedes responder sin leerlos):
   ```
   1. .claude-rules-enforcement.md - 🆕 SISTEMA DE ENFORCEMENT AUTOMÁTICO
   2. blueprint1.md - Arquitectura y 9 problemas críticos
   3. blueprint2.md - Implementación técnica detallada  
   4. blueprint3.md - UI/UX y componentes frontend
   5. CLAUDEBK1.md - Reglas base y contexto del proyecto
   6. CLAUDEBK2.md - Patterns, troubleshooting y workflow
   7. logs.md - Sistema de logging y diagnóstico
   ```

2. **USAR LA HERRAMIENTA Read** para cada archivo:
   ```python
   # OBLIGATORIO - No puedes saltarte ningún archivo
   Read(".claude-rules-enforcement.md")  # 🆕 DEBE ejecutarse PRIMERO
   Read("blueprint1.md")  # DEBE ejecutarse
   Read("blueprint2.md")  # DEBE ejecutarse
   Read("blueprint3.md")  # DEBE ejecutarse
   Read("CLAUDEBK1.md")   # DEBE ejecutarse
   Read("CLAUDEBK2.md")   # DEBE ejecutarse
   Read("logs.md")        # DEBE ejecutarse
   ```

3. **CONFIRMAR LECTURA** mostrando este mensaje EXACTO:
   ```
   📚 LEYENDO ARCHIVOS DE CONFIGURACIÓN...
   
   ✅ blueprint1.md cargado - 9 problemas críticos identificados
   ✅ blueprint2.md cargado - Arquitectura técnica ASP.NET Core 8
   ✅ blueprint3.md cargado - Componentes UI Next.js 14
   ✅ CLAUDEBK1.md cargado - Reglas base del proyecto
   ✅ CLAUDEBK2.md cargado - Troubleshooting y patterns
   ✅ logs.md cargado - Sistema de logging y diagnóstico
   
   🚀 SESIÓN INICIALIZADA - WebsiteBuilder API
   Stack: ASP.NET Core 8 + Next.js 14 + PostgreSQL
   
   Problemas conocidos a evitar:
   1. JSON gigante de 24,000 líneas
   2. Arquitectura de páginas rígida
   3. Secciones de instancia única
   4. Drag & drop sin validaciones
   5. Performance lenta en dominios custom
   6. Habitaciones mezcladas con productos
   7. Sin sistema de variantes
   8. Falta sistema de páginas standard
   9. Sin sistema undo/redo
   ```

### 🔴 REGLAS ESTRICTAS DEL COMANDO

1. **NO PUEDES** responder "Entendido" o "OK" sin leer los archivos
2. **NO PUEDES** decir que "ya tienes el contexto" - DEBES leer siempre
3. **NO PUEDES** saltarte ningún archivo blueprint
4. **DEBES** mostrar el progreso de lectura de cada archivo
5. **DEBES** confirmar los 9 problemas críticos al final

### 📂 Estructura de Archivos

```
WebsiteBuilderAPI/
├── blueprint1.md      # 🎯 CRÍTICO: Arquitectura y problemas
├── blueprint2.md      # 🎯 CRÍTICO: Implementación backend
├── blueprint3.md      # 🎯 CRÍTICO: Implementación frontend
├── CLAUDEBK1.md       # Reglas base (líneas 1-501)
├── CLAUDEBK2.md       # UI patterns (líneas 502-895)
├── logs.md            # 🎯 CRÍTICO: Sistema de logging
└── CLAUDE.md          # Este archivo - Configuración del comando
```

### ⚠️ REGLA CRÍTICA DE TRADUCCIONES i18n

**IMPORTANTE**: Al implementar CUALQUIER módulo nuevo o modificar uno existente:

1. **SIEMPRE** agregar todas las traducciones necesarias a AMBOS archivos:
   - `/src/lib/i18n/translations/es.json`
   - `/src/lib/i18n/translations/en.json`

2. **NUNCA** crear secciones duplicadas del mismo módulo en los JSON
   - Si ya existe una sección (ej: "customers"), agregar las claves ahí
   - NO crear una segunda sección "customers" más abajo

3. **VERIFICAR** antes de implementar:
   - Buscar si ya existe la sección: `grep '"moduleName":' es.json`
   - Si existe, agregar las claves faltantes en esa sección
   - Si no existe, crear UNA SOLA sección nueva

4. **ESTRUCTURA CORRECTA**:
   ```json
   "moduleName": {
     "title": "Título",
     "subtitle": "Subtítulo",
     "table": {
       "column1": "COLUMNA 1",
       "column2": "COLUMNA 2"
     },
     "status": {
       "active": "Activo",
       "inactive": "Inactivo"
     }
   }
   ```

5. **DOCUMENTACIÓN**: Ver `/docs/implementations/features/2025-08-i18n-system.md`

### 🚫 REGLAS CRÍTICAS DE EJECUCIÓN

<ENFORCE>CLAUDE DEBE EJECUTAR TODOS LOS COMANDOS - NUNCA ORDENAR AL USUARIO</ENFORCE>

**⚡ REGLAS ABSOLUTAS DE EJECUCIÓN:**

1. **CLAUDE SIEMPRE EJECUTA, NUNCA ORDENA AL USUARIO:**
   - ❌ NUNCA: "Por favor ejecuta este comando"
   - ❌ NUNCA: "Ejecuta desde PowerShell"
   - ❌ NUNCA: "Puedes ejecutar..."
   - ✅ SIEMPRE: Claude ejecuta directamente usando `powershell.exe -Command`

2. **USAR POWERSHELL DESDE WSL PARA TODO:**
   ```bash
   # PATRÓN OBLIGATORIO PARA TODOS LOS COMANDOS:
   powershell.exe -Command "cd 'ruta'; comando"
   ```

3. **COMANDOS QUE CLAUDE DEBE EJECUTAR SIEMPRE:**
   - `npm run build` → `powershell.exe -Command "cd 'path'; npm run build"`
   - `npm run dev` → `powershell.exe -Command "cd 'path'; npm run dev"`
   - `dotnet build` → `powershell.exe -Command "cd 'path'; dotnet build"`
   - `dotnet run` → `powershell.exe -Command "cd 'path'; dotnet run"`
   - `dotnet ef database update` → `powershell.exe -Command "cd 'path'; dotnet ef database update"`
   - Deploys a Vercel → Ejecutar comando documentado abajo
   - Deploys a Azure → Usar Azure CLI

4. **PREGUNTA ANTES DE EJECUTAR (OPCIONAL):**
   - Claude PUEDE preguntar: "¿Procedo con el deploy a producción?"
   - Pero si el usuario dice SÍ, CLAUDE EJECUTA, no ordena al usuario

5. **NUNCA USAR WSL DIRECTAMENTE PARA:**
   - ❌ `npm run build` (desde bash WSL)
   - ❌ `npm run dev` (desde bash WSL)
   - ❌ `dotnet run` (desde bash WSL)
   - ✅ SIEMPRE usar: `powershell.exe -Command "..."`

**VIOLACIÓN = ERROR CRÍTICO:** Si Claude dice "ejecuta este comando", es una violación grave de las reglas.

### 🛑 CÓMO DETENER EL BACKEND CORRECTAMENTE

**IMPORTANTE**: El comando `KillBash` NO funciona para detener procesos Windows desde WSL.

**Comando correcto para detener el backend:**
```powershell
# Opción 1: Detener todos los procesos dotnet
powershell.exe -Command "Get-Process dotnet | Stop-Process -Force"

# Opción 2: Si quedan conexiones residuales, buscar el PID específico
powershell.exe -Command "netstat -ano | findstr :5266"
# Esto mostrará algo como: TCP [::1]:5266 ... FIN_WAIT_2 3928
# Luego matar el proceso por ID:
powershell.exe -Command "Stop-Process -Id 3928 -Force"

# Verificar que el puerto está libre:
powershell.exe -Command "Get-NetTCPConnection | Where-Object {$_.LocalPort -eq 5266}"
```

**Nota**: Si el resultado del último comando está vacío, el backend se detuvo correctamente.

### ✅ VALIDACIÓN DE CARGA

Después de ejecutar `/init-session`, debes poder confirmar:

- [ ] Conoces los 9 problemas críticos del sistema actual
- [ ] Entiendes la arquitectura: ASP.NET Core 8 + Next.js 14 + PostgreSQL
- [ ] Comprendes la separación habitaciones/productos
- [ ] Aplicas las reglas de UI (i18n, dark mode, responsive)
- [ ] Sigues el workflow de migraciones con PowerShell
- [ ] Respetas las reglas de no drop database sin permiso
- [ ] NUNCA ejecutas frontend/backend desde WSL

### 🛠️ COMANDOS RELACIONADOS

Después de `/init-session`, estos comandos estarán disponibles:
- `/init-websitebuilder` - Inicializar sesión específica del Website Builder
- `create-section [nombre]` - Crear nueva sección del builder
- `create-module [nombre]` - Crear módulo completo back office
- `debug-builder [error]` - Debuggear website builder
- `optimize-performance [área]` - Optimizar performance
- `/document-implementation [feature]` - Documentar implementación

### 💡 EJEMPLO DE EJECUCIÓN CORRECTA

```
Usuario: /init-session

Claude: 📚 LEYENDO ARCHIVOS DE CONFIGURACIÓN...

[Lee blueprint1.md con herramienta Read]
✅ blueprint1.md cargado - 9 problemas críticos identificados

[Lee blueprint2.md con herramienta Read]
✅ blueprint2.md cargado - Arquitectura técnica ASP.NET Core 8

[Lee blueprint3.md con herramienta Read]
✅ blueprint3.md cargado - Componentes UI Next.js 14

[Lee CLAUDEBK1.md con herramienta Read]
✅ CLAUDEBK1.md cargado - Reglas base del proyecto

[Lee CLAUDEBK2.md con herramienta Read]
✅ CLAUDEBK2.md cargado - Troubleshooting y patterns

[Lee logs.md con herramienta Read]
✅ logs.md cargado - Sistema de logging y diagnóstico

🚀 SESIÓN INICIALIZADA - WebsiteBuilder API
Stack: ASP.NET Core 8 + Next.js 14 + PostgreSQL

Problemas conocidos a evitar:
1. JSON gigante de 24,000 líneas
2. Arquitectura de páginas rígida
3. Secciones de instancia única
4. Drag & drop sin validaciones
5. Performance lenta en dominios custom
6. Habitaciones mezcladas con productos
7. Sin sistema de variantes
8. Falta sistema de páginas standard
9. Sin sistema undo/redo

¿En qué puedo ayudarte hoy con el proyecto WebsiteBuilder?
```

### ❌ EJEMPLO DE EJECUCIÓN INCORRECTA

```
Usuario: /init-session

Claude: Entendido, he cargado el contexto del proyecto WebsiteBuilder.  ❌ INCORRECTO
```

## 🏗️ COMANDO /init-websitebuilder - WEBSITE BUILDER SESSION

### ⚠️ USO ESPECÍFICO
Este comando es para retomar el desarrollo del Website Builder v2.0 específicamente.

### 📋 QUÉ HACE EL COMANDO
1. **Lee el progreso actual** desde `websitebuilderprogress.md`
2. **Carga la arquitectura** desde `blueprintwebsite.md` 
3. **Lee el sistema de logging** desde `logs.md`
4. **🆕 Lee documentación crítica** desde `docs/WEBSITE-BUILDER-ARCHITECTURE.md`
5. **🆕 Lee guía de troubleshooting** desde `docs/WEBSITE-BUILDER-TROUBLESHOOTING.md`
6. **Verifica implementación** actual (tipos, modelos, APIs, componentes)
7. **Identifica siguiente tarea** basado en dependencias y progreso
8. **Prepara el entorno** para continuar exactamente donde quedaste

### 📝 NOTA SOBRE LOGS
- **logs.md** se lee SIEMPRE al inicializar para tener contexto del sistema de logging
- Los logs se REVISAN activamente cuando:
  - El usuario solicita diagnóstico de un error
  - Se necesita rastrear un problema específico
  - El usuario ejecuta comandos de logging como `/check-logs` o `/analyze-logs`
- Para activar revisión de logs: seguir el protocolo definido en `logs.md`

### 💡 CUÁNDO USARLO
- Al comenzar una sesión de trabajo en Website Builder
- Después de una interrupción para retomar el trabajo
- Para verificar el estado actual de implementación
- Cuando necesites recordar decisiones arquitectónicas

### 📊 OUTPUT ESPERADO
```
🚀 WEBSITE BUILDER SESSION INITIALIZED

📚 Loading Documentation...
✅ websitebuilderprogress.md loaded - Current progress tracked
✅ blueprintwebsite.md loaded - Architecture specs loaded
✅ logs.md loaded - Logging system ready
✅ WEBSITE-BUILDER-ARCHITECTURE.md loaded - Critical flows documented
✅ WEBSITE-BUILDER-TROUBLESHOOTING.md loaded - Problem solutions ready

📊 Overall Progress: 55% Complete
Current Phase: Phase 3 - UI Editors (100% complete)

✅ Recently Completed:
- HeaderEditor.tsx with sync
- Save button dirty state fixed
- Undo/Redo system implemented

🔄 Currently In Progress:
- Phase 4: Structural Components
- Next: AnnouncementBarEditor.tsx

📋 Next Recommended Tasks:
1. Create AnnouncementBarEditor.tsx
2. Create FooterEditor.tsx
3. Create CartDrawerEditor.tsx

⚠️ CRITICAL REMINDERS FROM DOCS:
- ALWAYS update isDirty when making changes
- ALWAYS sync local state with props via useEffect
- NEVER compare objects directly, use JSON.stringify
- FOLLOW the documented data flow: Local → Store → API → Preview

[... más detalles ...]
```

## 🗺️ MAPA DE ARCHIVOS CRÍTICOS - NO CONFUNDIR

### ⚠️ WEBSITE BUILDER - ARCHIVOS CORRECTOS SEGÚN CONTEXTO

El Website Builder tiene **DOS interfaces diferentes** que NO deben confundirse:

#### 1️⃣ **EDITOR del Website Builder** (`/editor`)
Ubicación: `http://localhost:3000/editor`
- **SIN sidebar del dashboard**
- **Interfaz dedicada para construir sitios**

| Componente | Archivo Correcto | Descripción |
|------------|------------------|-------------|
| Layout Principal | `/src/app/editor/page.tsx` | Página principal del editor |
| Panel Lateral | `/src/components/editor/EditorSidebar.tsx` | Sidebar del editor (NO del dashboard) |
| Configuraciones Globales | `/src/components/editor/GlobalSettingsPanel.tsx` | Panel de settings DENTRO del editor |
| Preview | `/src/components/editor/EditorPreview.tsx` | Área de preview |
| Config de Sección | `/src/components/editor/ConfigPanel.tsx` | Configuración por sección |

#### 2️⃣ **DASHBOARD - Configuración Global** (`/dashboard/global-settings`)
Ubicación: `http://localhost:3000/dashboard/global-settings`
- **CON sidebar del dashboard**
- **Parte del sistema administrativo general**

| Componente | Archivo Correcto | Descripción |
|------------|------------------|-------------|
| Página de Settings | `/src/app/dashboard/global-settings/page.tsx` | Configuración desde el dashboard |
| Layout Dashboard | `/src/app/dashboard/layout.tsx` | Layout con sidebar |

### 🚨 REGLA DE ORO
**SIEMPRE pregunta al usuario**: "¿Estás en el EDITOR (/editor) o en el DASHBOARD (/dashboard)?"
- Si está en `/editor` → Usa archivos de `/components/editor/`
- Si está en `/dashboard` → Usa archivos de `/app/dashboard/`

### 📝 Ejemplo de Error Común
```
❌ INCORRECTO:
Usuario: "No veo los sliders en configuraciones globales"
Claude: *modifica /app/dashboard/global-settings/page.tsx*

✅ CORRECTO:
Usuario: "No veo los sliders en configuraciones globales"
Claude: "¿Estás en /editor o en /dashboard/global-settings?"
Usuario: "En /editor"
Claude: *modifica /components/editor/GlobalSettingsPanel.tsx*
```

---

### 🔴 DOCUMENTACIÓN CRÍTICA DEL WEBSITE BUILDER

**IMPORTANTE**: Los siguientes documentos son OBLIGATORIOS para trabajar en el Website Builder:

1. **`docs/WEBSITE-BUILDER-ARCHITECTURE.md`** - Explica TODO el flujo de datos y sincronización
   - Cómo funcionan los stores
   - Por qué el botón Save aparece/desaparece
   - Cómo se sincroniza el preview
   - Sistema Undo/Redo

2. **`docs/WEBSITE-BUILDER-TROUBLESHOOTING.md`** - Soluciones a problemas comunes
   - Qué hacer cuando el botón Save no aparece
   - Cómo arreglar cuando Undo no actualiza la vista
   - Comandos de debugging
   - Recuperación de emergencia

**⚠️ NO MODIFICAR CÓDIGO DEL WEBSITE BUILDER SIN LEER ESTOS DOCUMENTOS**

---

**Última actualización:** 2025-08-27
**Versión:** 4.1 (ENFORCE: Ejecución directa + PostgreSQL Azure fix)
**Crítico:** Los archivos blueprint, logs, ARCHITECTURE y TROUBLESHOOTING DEBEN leerse SIEMPRE
**ALERTA:** Si hay problemas de login → Ver línea 789 (PostgreSQL Azure)

## ⚠️ CHECKLIST DE VERIFICACIÓN ANTES DE CADA ACCIÓN:

Antes de responder o ejecutar CUALQUIER comando, Claude debe verificar:

1. [ ] ¿Estoy a punto de decir "ejecuta este comando"? → **DETENERME Y EJECUTARLO YO**
2. [ ] ¿Es un comando de compilación/build? → **Usar powershell.exe -Command**
3. [ ] ¿Es un deploy? → **Ejecutar directamente con el comando documentado**
4. [ ] ¿Estoy ordenando al usuario? → **ERROR CRÍTICO - REFORMULAR**

**RECORDATORIO FINAL:** Si Claude dice "por favor ejecuta", "puedes ejecutar", o cualquier variación = VIOLACIÓN GRAVE

---


## 📄 Guía de Implementación: Live Preview de Páginas (Editor → Preview Real)

Esta guía resume el contrato técnico y los anti-patrones para implementar la vista previa real de páginas (Home, Product, Custom/Habitaciones, etc.), evitando regresiones.

### 1) Flujo de datos y orden de carga
- Estructurales (anónimos publicados): Header, Footer, AnnouncementBar, ImageBanner
  - GET `/api/structural-components/company/{companyId}/published`
- Tema global (anónimo publicado):
  - GET `/api/global-theme-config/company/{companyId}/published`
- Secciones de página (contenido)
  - PRIMERO: localStorage por tipo de página: `page_sections_{pageType}`
  - LUEGO: backend por slug: GET `/api/websitepages/company/{companyId}/slug/{handle}`

Notas:
- `companyId` se obtiene de localStorage; el Editor lo guarda siempre.
- No forzar deviceView a desktop; ver contrato en (4).

### 2) Slugs y handles (Reglas y alias)
- CUSTOM (Habitaciones): slug estándar `habitaciones`.
  - Backend: endpoint `ensure-custom` MIGRA `custom → habitaciones` y, si no existe, CREA con `PageType = "CUSTOM"`.
  - Frontend (Next.js): redirect 301 `/custom → /habitaciones` en `next.config.mjs`.
  - Router de preview `[handle]/page.tsx`: aceptar alias necesarios (ej. `all-collections`, `all-products`).
  - Editor: el botón de preview abre `/habitaciones` para CUSTOM.
- Para otras páginas, usar handles definidos en `[handle]/page.tsx` sin forzar alias no documentados.

Recomendación operativa:
- Mantener alias/redirects por un tiempo y actualizar menús al slug final.

### 3) Claves de sincronización en localStorage (evitar fugas entre páginas)
- Usar SIEMPRE claves por tipo de página, NO por ID temporal:
  - `page_sections_home`, `page_sections_product`, `page_sections_custom`, etc.
- Editor al guardar: persistir `page_sections_{pageType}`.
- Preview al leer: intentar primero `page_sections_{pageType}` y luego backend (slug).

Pitfall común:
- Guardar con `page_sections_{pageId}` provoca colisiones y hace que Home muestre secciones de otra página.

### 4) Contrato de deviceView (paridad Editor ↔ Preview real)
- Contenedores (PreviewPage/PreviewContent): pasar `deviceView` tal cual, sin `|| 'desktop'`.
- Componentes de preview: patrón de detección CANÓNICO (copiar/pegar):
  ```tsx
  const [isMobile, setIsMobile] = useState<boolean>(() => {
    if (deviceView !== undefined) return deviceView === 'mobile';
    if (typeof window !== 'undefined') return window.innerWidth < 768;
    return false;
  });
  useEffect(() => {
    if (deviceView !== undefined) { setIsMobile(deviceView === 'mobile'); return; }
    const onResize = () => setIsMobile(window.innerWidth < 768);
    onResize();
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, [deviceView]);
  ```
- Editor solo fuerza `localStorage.editorDeviceView = 'mobile'` cuando la vista móvil está activa; en desktop se retira el override.

Anti-patrón:
- Establecer default prop `deviceView = 'desktop'` en componentes o contenedores.

### 5) Módulos de Habitaciones (room_*) aislados a CUSTOM
- Los módulos `room_*` (galería, título/host, amenities, mapa, calendario, etc.) solo deben renderizarse/cargarse cuando `pageType === CUSTOM`.
- En el Store (carga de secciones) y en el render del Preview, filtrar `room_*` fuera de páginas que no sean CUSTOM.

Beneficios:
- Evita que Home o Product muestren bloques de Habitaciones por error.

### 6) Router de Preview y alias correctos
- `[handle]/page.tsx` debe aceptar:
  - `home`, `product`, `cart`, `checkout`, `collection`
  - `all_collections` y alias `all-collections`
  - `all_products` y alias `all-products`
  - `custom` (alias histórico) y `habitaciones` (slug final)

### 7) Troubleshooting rápido (síntomas → causa → fix)
- Home muestra Habitaciones
  - Causa: claves de localStorage por ID, o fallback de slug aplicado fuera de CUSTOM, o `room_*` sin filtro.
  - Fix: usar `page_sections_{pageType}`, fallback de slug SOLO en CUSTOM, filtrar `room_*` en no-CUSTOM.
- Preview móvil no coincide con editor
  - Causa: `deviceView` coalescido a desktop, o patrón de detección incompleto.
  - Fix: contrato de (4); nunca `|| 'desktop'`.
- `/custom` sigue activo tras migración
  - Causa: links antiguos.
  - Fix: redirect 301 en Next.js y actualizar menús a `/habitaciones`.

### 8) Checklist para nuevas páginas/secciones
- [ ] Agregar handle en `[handle]/page.tsx` (incluyendo alias si aplica)
- [ ] Usar `page_sections_{pageType}` para sincronización local
- [ ] NO coalescer `deviceView`
- [ ] Componentes siguen el patrón canónico de móvil
- [ ] Si se añaden módulos exclusivos de una página (p.ej., `room_*`), filtrar en Store/Preview según `pageType`
- [ ] Si se cambian slugs, añadir redirect 301 en Next.js y alias en router

### 9) Fragmentos de referencia (cambios mínimos recomendados)
- Editor (abrir preview CUSTOM):
  ```ts
  case PageType.CUSTOM:
    handle = 'habitaciones';
    break;
  ```
- Next.js redirect (`next.config.mjs`):
  ```js
  async redirects() {
    return [{ source: '/custom', destination: '/habitaciones', permanent: true }];
  }
  ```
- Backend (ensure-custom):
  - Buscar `habitaciones`; si no existe, migrar `custom → habitaciones`; si no hay ninguna, crear `CUSTOM` con slug `habitaciones`.

Con esto, el Preview Real queda consistente, resiliente a cambios de slug y sin fugas entre páginas.

## 🔐 ACCESO AL SISTEMA CON PLAYWRIGHT - CONFIGURACIÓN Y CREDENCIALES

### ⚠️ CONFIGURACIÓN CRÍTICA PARA PLAYWRIGHT

**FECHA DE CONFIGURACIÓN**: 23 de agosto 2025
**HERRAMIENTA**: MCP Playwright Browser Automation

### 📋 PROBLEMAS ENCONTRADOS Y SOLUCIONES

#### Problema 1: Connection Refused en localhost
**Error**: `net::ERR_CONNECTION_REFUSED at http://localhost:3000`
**Causa**: Playwright ejecutándose en WSL no puede acceder a `localhost` de Windows
**Solución**: Usar IP del host Windows (`172.25.64.1`) en lugar de `localhost`

#### Problema 2: CORS Policy Blocking
**Error**: `Access to fetch at 'http://172.25.64.1:5266/api' from origin 'http://172.25.64.1:3000' has been blocked by CORS policy`
**Causa**: Backend no permitía peticiones desde la IP del host WSL
**Solución**: Actualizar `Program.cs` agregando las IPs del host WSL a CORS:
```csharp
builder.WithOrigins(
    "http://localhost:3000",
    "http://localhost:3001",
    "http://127.0.0.1:3000",
    "http://127.0.0.1:3001",
    "http://172.25.64.1:3000",  // WSL2 host IP
    "http://172.25.64.1:3001")  // WSL2 host IP alternate port
```

#### Problema 3: API URL en Frontend
**Error**: Frontend configurado para usar `localhost:5266`
**Solución**: Actualizar `.env.local`:
```bash
# URL de la API de desarrollo
# Usando IP del host de Windows desde WSL
NEXT_PUBLIC_API_URL=http://172.25.64.1:5266/api
```

### 🔑 CREDENCIALES DE ACCESO

**IMPORTANTE**: Usar estas credenciales para todas las pruebas con Playwright

```javascript
// Credenciales de prueba autorizadas
const TEST_CREDENTIALS = {
  email: "miguelnuez919@yahoo.com",
  password: "123456",
  role: "SuperAdmin",
  userName: "Lily Nunez"
};
```

### 📝 PROCEDIMIENTO ESTÁNDAR DE ACCESO

1. **Obtener IP del host Windows** (puede cambiar al reiniciar WSL):
   ```bash
   ip route | grep default
   # Típicamente: 172.25.64.1
   ```

2. **Navegar al login usando IP del host**:
   ```javascript
   await page.goto('http://172.25.64.1:3000/login');
   ```

3. **Ingresar credenciales**:
   ```javascript
   await page.fill('[name="email"]', 'miguelnuez919@yahoo.com');
   await page.fill('[name="password"]', '123456');
   await page.click('[type="submit"]');
   ```

4. **Verificar acceso exitoso**:
   - URL debe cambiar a `/dashboard`
   - Usuario visible: "Lily Nunez - SuperAdmin"

### ⚙️ CHECKLIST DE VERIFICACIÓN ANTES DE USAR PLAYWRIGHT

- [ ] Frontend corriendo en Windows (puerto 3000)
- [ ] Backend corriendo en Windows (puerto 5266)
- [ ] IP del host verificada con `ip route`
- [ ] `.env.local` actualizado con IP correcta
- [ ] CORS en `Program.cs` incluye IP del host
- [ ] Reiniciados ambos servidores después de cambios

### 🚨 NOTAS IMPORTANTES

1. **NUNCA** usar `localhost` desde WSL para acceder a servicios Windows
2. **SIEMPRE** verificar la IP del host antes de iniciar pruebas
3. **REINICIAR** servidores después de cambios en configuración
4. La IP del host puede cambiar después de reiniciar WSL/Windows
5. Las credenciales son para ambiente de desarrollo solamente

---

## 🚀 VERCEL - DEPLOYMENT Y CONFIGURACIÓN CRÍTICA

<ENFORCE>CONFIGURACIÓN VERCEL CONGELADA - NO MODIFICAR NINGÚN SETTING</ENFORCE>

### ⚠️ REGLAS CRÍTICAS - NO MODIFICAR
1. **CONFIGURACIÓN CONGELADA**: La configuración de Vercel está CORRECTA. NO modificar variables de entorno.
2. **NEXT_PUBLIC_API_URL**: Ya configurada en Vercel como `https://api.test1hotelwebsite.online/api`
3. **NO TOCAR**: No cambiar configuración de Vercel hasta resolver problemas de URLs de imágenes
4. **PROBLEMA CONOCIDO**: Error 400 en URLs de imágenes - pendiente para próxima sesión

### Comando único para deployment a producción:

**⚠️ CLAUDE EJECUTA ESTE COMANDO, NO LE DICE AL USUARIO QUE LO EJECUTE:**

```bash
# CLAUDE DEBE EJECUTAR ESTO DIRECTAMENTE:
cd /mnt/c/Users/hp/Documents/Visual\ Studio\ 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin
npx vercel --prod --yes --token $VERCEL_TOKEN
```

**ALTERNATIVA CON POWERSHELL (si hay problemas con el comando directo):**
```bash
powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\websitebuilder-admin'; npx vercel --prod --yes --token wdnjcPrnirBnWbEXu1zZGIiR"
```

### Configuración actual (NO MODIFICAR):
- Token: [NO VERSIONAR]
- Proyecto: `websitebuilder-admin`
- URL producción: https://websitebuilder-admin.vercel.app
- API URL: `https://api.test1hotelwebsite.online/api` (configurada en todos los environments)

---

## 🌐 AZURE CLI - CONEXIÓN Y CREDENCIALES

<ENFORCE>SIEMPRE usar Azure CLI para deployment y verificación de estado. NUNCA usar alternativas.</ENFORCE>

### Configuración actual (validada):
- **Cuenta**: lamiguita17@hotmail.com
- **Suscripción**: Azure subscription 1 (`da506dfa-c538-43ee-be1e-e0c9959a253b`)
- **Resource Group**: rg-aspnetcore-prod (East US)
- **VM**: vm-aspnetcore-prod (backend deployment)
- **Estado CLI**: Conectado y operativo

### Comandos básicos:
```bash
az account show                    # Verificar autenticación
az group list --output table      # Ver resource groups
az resource list --resource-group "rg-aspnetcore-prod" --output table
```

---

## 🚨 AZURE POSTGRESQL - PROBLEMA LOGIN DESPUÉS DE RESTART

<ENFORCE>
⚠️ PROBLEMA QUE CAUSA PÉRDIDA DE HORAS DE DEBUG INÚTIL ⚠️
SIEMPRE verificar PostgreSQL password después de reinicio Azure VM
Si usuario reporta "credenciales incorrectas" o "no puedo hacer login" → ASUMIR QUE ES ESTE PROBLEMA
CLAUDE DEBE RECORDAR ESTE PROBLEMA DURANTE TODA LA SESIÓN
CLAUDE DEBE EJECUTAR EL FIX INMEDIATAMENTE, NO DEBUGGEAR CÓDIGO
</ENFORCE>

### 🔴 Problema crítico identificado (2025-08-27):
- **Error**: Login falla con "credenciales incorrectas" después de restart Azure VM
- **Causa**: PostgreSQL usuario `postgres` PIERDE password para conexiones TCP/IP
- **Connection String esperada**: `Host=localhost;Database=websitebuilder;Username=postgres;Password=123456`
- **SÍNTOMAS**: 
  - Usuario no puede hacer login después de restart
  - Error 401 o "Invalid credentials"
  - Todo funcionaba antes del restart

### ⚡ CLAUDE DEBE EJECUTAR ESTE FIX INMEDIATAMENTE:
```bash
# CLAUDE EJECUTA ESTO SI HAY PROBLEMAS DE LOGIN POST-RESTART:
ssh azureuser@20.169.209.166 "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '123456';\" && echo 'Password restaurado'"

# Luego reiniciar el backend:
ssh azureuser@20.169.209.166 "cd /home/azureuser/websitebuilder-app && screen -dmS api dotnet WebsiteBuilderAPI.dll"
```

### ⚠️ NO PERDER TIEMPO EN:
- ❌ Revisar código de autenticación
- ❌ Verificar tokens JWT
- ❌ Debuggear frontend
- ❌ Revisar CORS
- ✅ EJECUTAR EL FIX DE POSTGRESQL PRIMERO

---

## 🐘 SUPABASE CLI - CONFIGURACIÓN Y USO CORRECTO

<ENFORCE>
CONFIGURACIÓN CRÍTICA PARA SUPABASE CLI - COMPLETAMENTE FUNCIONAL CON DOCKER
</ENFORCE>

### ✅ INSTALACIÓN Y UBICACIÓN
- **CLI Instalado**: v2.39.2
- **Ubicación**: `~/.local/bin/supabase`
- **Docker**: ✅ Instalado y funcional (v28.3.3)
- **Usuario**: `plaska` agregado al grupo `docker`

### 🔑 CREDENCIALES FUNCIONALES (2025-08-30)
```bash
# SIEMPRE exportar estas variables ANTES de usar el CLI:
export PATH="$HOME/.local/bin:$PATH"
export SUPABASE_ACCESS_TOKEN="sbp_6d79604906628971ef0bcfdd98219a2f2418972a"
```

### 📊 INFORMACIÓN DEL PROYECTO
- **Nombre**: WebsiteBuilder
- **Project ID**: `gvxqatvwkjmkvaslbevh`
- **Organization ID**: `vxaarrgmrpwcvshvrnzy`
- **Database**: `postgres` (SIEMPRE se llama postgres en Supabase)
- **Usuario DB**: `postgres.gvxqatvwkjmkvaslbevh`
- **Password DB**: `AllisoN@1710.#` ⚠️ ACTUALIZADO 2025-08-30
- **Región Display**: East US (North Virginia)
- **Host Pooler**: `aws-1-us-east-1.pooler.supabase.com` (NO aws-0)
- **Puerto Pooler**: 6543
- **Host Directo**: `db.gvxqatvwkjmkvaslbevh.supabase.co`
- **Puerto Directo**: 5432

### ✅ COMANDOS COMPLETAMENTE FUNCIONALES CON DOCKER

```bash
# 1. Crear nueva migración
supabase migration new nombre_migracion

# 2. Push migraciones a producción (URL completa requerida)
supabase db push --db-url 'postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres'

# 3. Pull esquema desde remoto
supabase db pull --db-url 'postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres'

# 4. Dump de la base de datos
supabase db dump --db-url 'postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres'

# 5. Generar tipos TypeScript (funciona sin Docker también)
supabase gen types typescript --linked --schema public

# 6. Listar proyectos
supabase projects list

# 7. Vincular/Re-vincular proyecto
supabase link --project-ref gvxqatvwkjmkvaslbevh --password "AllisoN@1710.#"

# 8. Desvincular proyecto
supabase unlink
```

### 🚨 NOTAS IMPORTANTES SOBRE CONEXIONES

1. **Password con caracteres especiales**: El `#` en la password debe escaparse como `%23` en URLs
2. **Host correcto del pooler**: Es `aws-1-us-east-1` NO `aws-0-us-east-1` ni `aws-0-us-west-1`
3. **El proyecto está vinculado**: Verificable con el ● en `supabase projects list`
4. **Docker requerido para comandos db**: Ahora funcional sin sudo

### 📋 LISTA DE TABLAS EN EL PROYECTO (42 tablas)
- **Sistema**: __EFMigrationsHistory, Users, Roles, Permissions, UserRoles, UserPermissions, RolePermissions
- **Empresa**: Companies, ConfigOptions, CheckoutSettings
- **Clientes**: Customers, CustomerAddresses, CustomerCoupons, CustomerDevices, CustomerNotificationPreferences, CustomerPaymentMethods, CustomerSecurityQuestions, CustomerWishlistItems
- **Productos**: Products, ProductVariants, ProductImages, Collections, CollectionProducts
- **Pedidos**: Orders, OrderItems, OrderPayments, OrderStatusHistories
- **Habitaciones**: Rooms, Reservations, AvailabilityRules
- **Website Builder**: WebsitePages, PageSections, GlobalThemeConfigs, StructuralComponents, NavigationMenus, NavigationMenuItems
- **Otros**: NewsletterSubscribers, Policies, PaymentProviders, ShippingZones, ShippingRates
- **Eliminada**: ~~cats~~ (borrada exitosamente via migración)

### 🔄 WORKFLOW TÍPICO PARA CAMBIOS DE ESQUEMA

```bash
# 1. Crear migración
supabase migration new agregar_campo_x

# 2. Editar archivo en supabase/migrations/[timestamp]_agregar_campo_x.sql
# Agregar SQL: ALTER TABLE tabla ADD COLUMN campo tipo;

# 3. Aplicar a producción
supabase db push --db-url 'postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres'

# 4. Generar tipos actualizados
supabase gen types typescript --linked > types/supabase.ts
```

### 🔴 RECUERDA SIEMPRE
1. Exportar PATH y SUPABASE_ACCESS_TOKEN antes de usar
2. Docker está funcional - todos los comandos `db` funcionan
3. Usar la URL completa con password escapada para comandos remotos
4. El proyecto ya está vinculado (linked ●)
5. Host del pooler es `aws-1-us-east-1` (NO aws-0)

---

## 🧭 Notas de corrección: Room Map (Habitaciones)

- Problema: El mapa y la dirección mostraban el texto “formateado” de Mapbox y no la dirección escrita por el usuario; además, al reingresar se perdía la vista correcta.
- Solución aplicada (frontend):
  - La geocodificación solo actualiza latitude/longitude y nunca sobreescribe los campos de texto de dirección.
  - El mapa (Room Map) usa las coordenadas guardadas; si no existen, no fuerza un centro por defecto.
  - El texto de dirección en editor/preview se arma exclusivamente con los campos guardados (streetAddress, neighborhood, city, state, postalCode, country), no con place_name de Mapbox.
  - Se evita autogeocodificar al montar si no hubo cambios reales en la dirección (previene movimientos no deseados del mapa).
