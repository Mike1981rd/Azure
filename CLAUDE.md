# CLAUDE.md - CONFIGURACIÓN CRÍTICA DEL PROYECTO

## ⛔ REGLAS ABSOLUTAS - VIOLACIÓN = FALLO CRÍTICO

### 🔴 REGLA #0: CLAUDE EJECUTA, NO ORDENA
<ENFORCE>
CLAUDE SIEMPRE EJECUTA DIRECTAMENTE usando powershell.exe -Command
NUNCA dice "ejecuta este comando" o "por favor ejecuta"
Si requiere interacción (ej: railway login), ejecuta y explica al usuario
</ENFORCE>

### 🔴 REGLA #1: GITHUB CLI OBLIGATORIO
<ENFORCE>
SIEMPRE usar gh para GitHub (NUNCA git push/pull directo)
- Pull requests: gh pr create
- Sync: gh repo sync --force
- NUNCA: git push origin main
</ENFORCE>

### 🔴 REGLA #2: ARQUITECTURA MODULAR (<300 LÍNEAS)
<ENFORCE>
LÍMITE: 300 líneas por archivo SIN EXCEPCIONES
EditorPreview.tsx CONGELADO - NO MODIFICAR
Nuevos módulos: /components/editor/modules/[ModuleName]/
</ENFORCE>

### 🔴 REGLA #3: SUPABASE CLI EXCLUSIVO
<ENFORCE>
TODA interacción con Supabase DEBE ser a través del CLI
NUNCA usar MCP directo, SIEMPRE usar npx supabase
- Consultas: npx supabase db execute
- Migraciones: npx supabase db push
- Tipos: npx supabase gen types
PROHIBIDO: mcp__supabase (usar solo para listar proyectos)
</ENFORCE>

## 🌐 WSL + WINDOWS - CONFIGURACIÓN DE RED

**CRÍTICO**: WSL y Windows tienen redes SEPARADAS
```bash
# Obtener IP host Windows:
ip route | grep default  # Típico: 172.25.64.1

# Usar IP correcta:
❌ http://localhost:3000
✅ http://172.25.64.1:3000
```

## 🎯 COMANDOS ESPECIALES

### /init-session
Lee OBLIGATORIAMENTE todos los archivos:
1. .claude-rules-enforcement.md
2. blueprint1.md (9 problemas críticos)
3. blueprint2.md (ASP.NET Core 8)
4. blueprint3.md (Next.js 14)
5. CLAUDEBK1.md, CLAUDEBK2.md
6. logs.md

### /init-websitebuilder
Carga progreso del Website Builder:
- websitebuilderprogress.md
- blueprintwebsite.md
- docs/WEBSITE-BUILDER-ARCHITECTURE.md
- docs/WEBSITE-BUILDER-TROUBLESHOOTING.md

## 🚂 RAILWAY - CONFIGURACIÓN ACTUAL

**Proyecto**: superb-stillness (def4e9c8-0104-4cb1-ad7b-0dc415887110)
**Servicio**: c2be9027-a4bc-4674-94e4-1090b78d6753
**URL**: https://websitebuilderapi-production-production.up.railway.app
**Token**: 3752c685-6cfc-4d23-876b-6b543206212b

### Comandos (SIEMPRE con --service):
```bash
export RAILWAY_TOKEN=3752c685-6cfc-4d23-876b-6b543206212b
railway variables --service c2be9027-a4bc-4674-94e4-1090b78d6753
railway redeploy --service c2be9027-a4bc-4674-94e4-1090b78d6753 -y
```

## 🚀 RENDER - CONFIGURACIÓN COMPLETA

**Fecha configuración**: 30 de agosto 2025
**Email cuenta**: miguelgarcia80@gmail.com
**API Token**: `rnd_tyxAUfs2cgBOksWhtY6u9FqHmT6J`
**Workspace ID**: `tea-d2odvlq4d50c739r2du0`
**Workspace Name**: Miguel's workspace
**CLI Version**: 2.1.5

### Instalación Render CLI (WSL/Ubuntu):
```bash
# 1. Descargar Render CLI v2.1.5 directamente (el script oficial requiere unzip)
cd /tmp
wget https://github.com/render-oss/cli/releases/download/v2.1.5/cli_2.1.5_linux_amd64.zip -O render.zip

# 2. Extraer usando Python (si no tienes unzip)
python3 -m zipfile -e render.zip render-cli/

# 3. Instalar en ~/.local/bin
chmod +x render-cli/cli_v2.1.5
mkdir -p ~/.local/bin
mv render-cli/cli_v2.1.5 ~/.local/bin/render

# 4. Agregar al PATH permanentemente
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
export PATH="$HOME/.local/bin:$PATH"

# 5. Verificar instalación
render --version  # Debe mostrar: render version 2.1.5
```

### Configuración de credenciales:
```bash
# 1. Configurar API Token (OBLIGATORIO)
export RENDER_API_KEY="rnd_tyxAUfs2cgBOksWhtY6u9FqHmT6J"
echo 'export RENDER_API_KEY="rnd_tyxAUfs2cgBOksWhtY6u9FqHmT6J"' >> ~/.bashrc

# 2. Configurar Workspace (OBLIGATORIO - sin esto no funciona)
render workspace set tea-d2odvlq4d50c739r2du0 --output json

# 3. Verificar autenticación
render workspace current --output json
# Debe mostrar: {"email":"miguelgarcia80@gmail.com","id":"tea-d2odvlq4d50c739r2du0","name":"Miguel's workspace","type":"team"}
```

### Comandos útiles Render CLI:
```bash
# IMPORTANTE: Render CLI requiere modo no-interactivo en WSL
# SIEMPRE usar --output json o --output text

# Listar servicios (actualmente vacío)
render services list --output json

# Crear nuevo servicio web
render services create --output json

# Ver logs de un servicio
render logs [service-name] --output text

# Hacer deploy manual
render deploys create [service-name] --output json

# Ver información del servicio
render services info [service-name] --output json

# Listar deploys
render deploys list [service-name] --output json
```

### API REST de Render (alternativa al CLI):
```bash
# Headers requeridos
AUTH_HEADER="Authorization: Bearer rnd_tyxAUfs2cgBOksWhtY6u9FqHmT6J"

# Verificar autenticación
curl -H "$AUTH_HEADER" https://api.render.com/v1/owners

# Listar servicios (array vacío si no hay)
curl -H "$AUTH_HEADER" https://api.render.com/v1/services

# Crear servicio web (ejemplo)
curl -X POST -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  https://api.render.com/v1/services \
  -d '{
    "type": "web_service",
    "name": "websitebuilder-api",
    "ownerId": "tea-d2odvlq4d50c739r2du0",
    "repo": "https://github.com/Mike1981rd/Azure",
    "branch": "main",
    "buildCommand": "dotnet publish -c Release",
    "startCommand": "dotnet WebsiteBuilderAPI.dll"
  }'
```

### Troubleshooting Render CLI:
```bash
# Error: "panic: Failed to initialize interface"
# Solución: Usar --output json/text (modo no-interactivo)

# Error: "no workspace set"
# Solución: Ejecutar primero:
export RENDER_API_KEY="rnd_tyxAUfs2cgBOksWhtY6u9FqHmT6J"
render workspace set tea-d2odvlq4d50c739r2du0 --output json

# Error: "command not found: render"
# Solución: Agregar al PATH:
export PATH="$HOME/.local/bin:$PATH"
```

### GitHub Repository para Render:
- **URL**: https://github.com/Mike1981rd/Azure
- **Branch**: main
- **Contenido**: Backend ASP.NET Core 8.0 (sin frontend)
- **Tamaño**: ~190MB (solo código fuente)

## 📦 GITHUB AZURE REPOSITORY - BACKEND PRODUCTION

**Repositorio**: https://github.com/Mike1981rd/Azure
**Branch principal**: main
**Creado**: 30 de agosto 2025
**Contenido**: Solo backend ASP.NET Core 8.0 (frontend va en Vercel)

### Estructura del repositorio:
```
Azure/
├── Controllers/        (48 archivos - todos los endpoints)
├── Services/          (70 archivos - lógica de negocio)
├── Models/            (62 archivos - modelos de datos)
├── Migrations/        (108 archivos - historial DB)
├── DTOs/              (todos los DTOs)
├── Program.cs         (configuración principal)
├── appsettings.json   (configuración)
├── WebsiteBuilderAPI.csproj
└── WebsiteBuilderAPI.sln
```

### Cómo hacer push de cambios:
```bash
# 1. Desde el repositorio principal (con todo el historial pesado)
cd "/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI"

# 2. Hacer cambios y commit local
git add .
git commit -m "feat: descripción del cambio"

# 3. Crear archivo limpio para Azure (sin historial)
git archive --format=tar HEAD | gzip > cambios.tar.gz

# 4. En el repositorio Azure limpio
cd /tmp/azure-clean

# 5. Extraer cambios
tar -xzf "/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/cambios.tar.gz"

# 6. Commit y push
git add -A
git commit -m "feat: descripción del cambio"
git push origin main
```

### Método alternativo - Push directo desde repo limpio:
```bash
# Repositorio limpio ya configurado en:
cd /tmp/azure-clean

# Verificar remote
git remote -v
# Debe mostrar: origin https://github.com/Mike1981rd/Azure.git

# Hacer cambios directamente aquí si es necesario
# O copiar archivos específicos:
cp "/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/Controllers/NuevoController.cs" Controllers/

# Commit y push
git add .
git commit -m "feat: agregar nuevo controller"
git push origin main
```

### Sincronización completa (cuando hay muchos cambios):
```bash
# 1. Eliminar repo temporal anterior
rm -rf /tmp/azure-clean

# 2. Crear nuevo repo limpio
mkdir /tmp/azure-clean && cd /tmp/azure-clean
git init
git branch -m main
git remote add origin https://Mike1981rd:[REDACTED]@github.com/Mike1981rd/Azure.git

# 3. Crear archivo con código actualizado
cd "/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI"
git archive --format=tar HEAD | gzip > /tmp/source-code.tar.gz

# 4. Extraer y push
cd /tmp/azure-clean
tar -xzf /tmp/source-code.tar.gz
git add -A
git commit -m "sync: actualización completa del backend"
git push origin main --force
```

### Archivos excluidos del repositorio Azure:
- ❌ websitebuilder-admin/ (frontend - va en Vercel)
- ❌ node_modules/ (dependencias Node.js)
- ❌ .next/ (cache de Next.js)
- ❌ bin/ y obj/ (binarios compilados)
- ❌ *.dll, *.pdb, *.exe (archivos binarios)
- ❌ publish.tgz (archivo de deployment)

### Verificar estado del repositorio:
```bash
# Ver último commit en GitHub
curl -s https://api.github.com/repos/Mike1981rd/Azure/commits/main | python3 -c "import json,sys; d=json.load(sys.stdin); print(f\"Último commit: {d['commit']['message']} - {d['commit']['author']['date']}\")"

# Ver tamaño del repo
curl -s https://api.github.com/repos/Mike1981rd/Azure | python3 -c "import json,sys; d=json.load(sys.stdin); print(f\"Tamaño: {d.get('size', 'N/A')} KB\")"
```

### Token GitHub (para push):
```bash
# Token con permisos de escritura
GH_TOKEN="[REDACTED]"

# Configurado en remote URL:
https://Mike1981rd:[REDACTED]@github.com/Mike1981rd/Azure.git
```

## 🐘 SUPABASE - CONEXIÓN FUNCIONAL

**Project ID**: gvxqatvwkjmkvaslbevh
**Project Name**: WebsiteBuilder
**Host Pooler**: aws-1-us-east-1.pooler.supabase.com:6543
**Usuario**: postgres.gvxqatvwkjmkvaslbevh
**Password**: AllisoN@1710.# (escapar # como %23 en URLs)
**CLI Version**: 2.39.2
**Prerequisito**: Docker Desktop debe estar corriendo

### Connection String Railway:
```
Host=aws-0-us-west-1.pooler.supabase.com;Database=postgres;Username=postgres.gvxqatvwkjmkvaslbevh;Password=PcpBLRo5Mf5jBC4IQO_ineBHjVIj7npK9JhW5dJlUKI;Port=6543;Pooling=true;MinPoolSize=5;MaxPoolSize=20
```

### ⚠️ SOLUCIÓN TIMEOUT SUPABASE - USAR CONEXIÓN DIRECTA:
```
# PROBLEMA: El pooler de Supabase causa timeouts con Entity Framework
# Pooler (NO USAR): aws-1-us-east-1.pooler.supabase.com:6543

# ✅ SOLUCIÓN: Usar conexión directa en appsettings.json:
Host=db.gvxqatvwkjmkvaslbevh.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=AllisoN@1710.#;Timeout=30;Command Timeout=30;Pooling=true;MinPoolSize=5;MaxPoolSize=20

# Diferencias clave:
# - Host: db.gvxqatvwkjmkvaslbevh.supabase.co (conexión directa)
# - Puerto: 5432 (no 6543)
# - Usuario: postgres (sin el prefijo del proyecto)
# - Timeouts: Agregar Timeout=30;Command Timeout=30
```

### Conexión CLI - PROCESO COMPLETO Y VERIFICADO:
```bash
# 0. PREREQUISITO: Docker debe estar corriendo
docker --version  # Verificar Docker disponible

# 1. Configurar token de acceso (ejecutar siempre primero)
export SUPABASE_ACCESS_TOKEN="sbp_6d79604906628971ef0bcfdd98219a2f2418972a"

# 2. Conectar al proyecto (requiere Docker)
npx supabase link --project-ref gvxqatvwkjmkvaslbevh --password "AllisoN@1710.#"

# 3. Verificar conexión exitosa
SUPABASE_ACCESS_TOKEN="sbp_6d79604906628971ef0bcfdd98219a2f2418972a" npx supabase projects list
# Debe mostrar: ● LINKED | gvxqatvwkjmkvaslbevh | WebsiteBuilder

# 4. Comandos del CLI de Supabase - SOLO PARA ESQUEMAS:
npx supabase db pull                    # Sincronizar esquema local
npx supabase db push --db-url "postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres"  # Aplicar migraciones
npx supabase gen types typescript       # Generar tipos TypeScript
npx supabase db dump --data-only       # Backup de datos (puede timeout)
```

### 🔴 EJECUTAR SQL EN SUPABASE - MÉTODO CORRECTO CON DOCKER:
```bash
# ✅ MÉTODO CORRECTO - Usar Docker con psql para CUALQUIER operación SQL:
docker run --rm postgres:latest psql \
  "postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres" \
  -c "YOUR SQL QUERY HERE"

# Ejemplos:
# Ver estructura de tabla:
docker run --rm postgres:latest psql "postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres" -c "\d public.\"Users\""

# Insertar datos:
docker run --rm postgres:latest psql "postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres" -c "INSERT INTO public.\"Users\" (...) VALUES (...);"

# Consultar datos:
docker run --rm postgres:latest psql "postgresql://postgres.gvxqatvwkjmkvaslbevh:AllisoN%401710.%23@aws-1-us-east-1.pooler.supabase.com:6543/postgres" -c "SELECT * FROM public.\"Users\";"
```

### ❌ COMANDOS INCORRECTOS - NUNCA USAR:
```bash
# ❌ NO FUNCIONA - db execute no existe para remoto:
npx supabase db execute --db-url "..." --command "SQL"

# ❌ NO FUNCIONA - db push es solo para migraciones de esquema, no datos:
npx supabase db push  # (para insertar datos)

# ❌ NO FUNCIONA - inspect es solo para estadísticas:
npx supabase inspect db table-stats --db-url "..."  # (para ejecutar SQL)

# ❌ EVITAR - MCP está prohibido según reglas:
mcp__supabase__execute_sql

# ❌ NO FUNCIONA - psql no está instalado localmente:
psql -h aws-1-us-east-1.pooler.supabase.com ...

# ❌ PROBLEMÁTICO - rutas con espacios sin comillas:
docker run -v /mnt/c/Users/hp/Documents/Visual Studio 2022/...  # Falla

# ✅ CORRECTO - rutas con espacios entre comillas:
docker run -v "/mnt/c/Users/hp/Documents/Visual Studio 2022/..."
```

### 📝 RESUMEN DE USO CORRECTO:
- **Para migraciones de esquema**: `npx supabase db push --db-url "..."`
- **Para CUALQUIER operación SQL (INSERT, UPDATE, SELECT, etc.)**: `docker run --rm postgres:latest psql "postgresql://..." -c "SQL"`
- **Para generar tipos**: `npx supabase gen types typescript`
- **NUNCA usar**: `db execute`, `mcp__supabase`, `psql` local

### 🔧 MIGRACIONES ENTITY FRAMEWORK CON SUPABASE:
```bash
# IMPORTANTE: Las migraciones de EF Core requieren conexión DIRECTA (no pooler)
# Usar db.gvxqatvwkjmkvaslbevh.supabase.co en puerto 5432

# 1. Crear migración (usa connection string de appsettings.json):
cd "/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI"
powershell.exe -Command "C:\Users\hp\.dotnet\tools\dotnet-ef.exe migrations add NombreMigracion"

# 2. Aplicar migración a Supabase (DEBE usar conexión directa, NO el pooler):
powershell.exe -Command "C:\Users\hp\.dotnet\tools\dotnet-ef.exe database update --connection 'Host=db.gvxqatvwkjmkvaslbevh.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=AllisoN@1710.#;Timeout=60;Command Timeout=60'"

# ⚠️ NOTA: El pooler (aws-1-us-east-1.pooler.supabase.com:6543) causa timeouts con EF migrations
# ✅ SIEMPRE usar la conexión directa para migraciones: db.gvxqatvwkjmkvaslbevh.supabase.co:5432
```

## 🚀 VERCEL - DEPLOYMENT

<ENFORCE>NO MODIFICAR CONFIGURACIÓN - ESTÁ CORRECTA</ENFORCE>

```bash
# CLAUDE EJECUTA DIRECTAMENTE:
cd /mnt/c/Users/hp/Documents/Visual\ Studio\ 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin
npx vercel --prod --yes --token wdnjcPrnirBnWbEXu1zZGIiR
```

## 🔐 PLAYWRIGHT - CREDENCIALES

```javascript
const TEST_CREDENTIALS = {
  email: "miguelnuez919@yahoo.com",
  password: "123456",
  role: "SuperAdmin"
};

// Usar IP del host Windows:
await page.goto('http://172.25.64.1:3000/login');
```

## 🚨 AZURE - CONFIGURACIÓN Y DESPLIEGUE

### Azure CLI - Conexión:
```bash
# 1. Login con Azure CLI (ya está configurado, NO usar --use-device-code)
az login  # Ya autenticado, no requiere interacción

# 2. Verificar suscripción activa
az account show

# 3. Resource Group y VM correctos:
# - Resource Group: rg-aspnetcore-prod
# - VM Name: vm-aspnetcore-prod
# - IP Pública: 20.169.209.166
```

### Azure VM - Despliegue del Backend:
```bash
# 1. Compilar proyecto (desde Windows PowerShell)
powershell.exe -Command "cd 'C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI' ; dotnet publish WebsiteBuilderAPI.csproj -c Release"

# 2. Archivos publicados están en:
# /mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/bin/Release/net8.0/publish/

# 3. Sincronizar a VM
rsync -avz '/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/bin/Release/net8.0/publish/' azureuser@20.169.209.166:/home/azureuser/websitebuilder-app/

# 4. Configurar connection string de Supabase (IMPORTANTE: escapar # como %23)
ssh azureuser@20.169.209.166 "sudo sed -i 's|\"DefaultConnection\": \".*\"|\"DefaultConnection\": \"Host=aws-1-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.gvxqatvwkjmkvaslbevh;Password=AllisoN@1710.%23;Pooling=true;MinPoolSize=5;MaxPoolSize=20\"|' /home/azureuser/websitebuilder-app/appsettings.json"

# 5. Reiniciar servicio
ssh azureuser@20.169.209.166 "sudo systemctl restart websitebuilder"

# 6. Verificar funcionamiento
curl -s https://api.test1hotelwebsite.online/api/health
```

### IMPORTANTE - Connection String Supabase:
- **Host**: aws-1-us-east-1.pooler.supabase.com (NO aws-0-us-west-1)
- **Password**: AllisoN@1710.%23 (el # DEBE estar escapado como %23)
- **Puerto**: 6543
- **Usuario**: postgres.gvxqatvwkjmkvaslbevh

<ENFORCE>
Si usuario reporta "credenciales incorrectas" → EJECUTAR INMEDIATAMENTE:
ssh azureuser@20.169.209.166 "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '123456';\""
NO debuggear código de autenticación
</ENFORCE>

## 🛑 DETENER BACKEND WINDOWS

```powershell
# Desde WSL:
powershell.exe -Command "Get-Process dotnet | Stop-Process -Force"
```

## 📄 WEBSITE BUILDER - GUÍAS CRÍTICAS

### Live Preview - Contrato Técnico:
1. **localStorage keys**: `page_sections_{pageType}` (NO por ID)
2. **deviceView**: NUNCA coalescer a desktop
3. **CUSTOM/Habitaciones**: slug = `habitaciones`, redirect 301 desde `/custom`
4. **room_* modules**: SOLO en pageType === CUSTOM

### Archivos según contexto:
- **Editor** (/editor): `/components/editor/`
- **Dashboard** (/dashboard): `/app/dashboard/`

### i18n - Traducciones:
- SIEMPRE agregar a es.json Y en.json
- NUNCA duplicar secciones en JSON
- Verificar: `grep '"moduleName":' es.json`

## 📋 WORKFLOW TÍPICO

1. **Modificar código**: Respetar límite 300 líneas
2. **Actualizar Railway**: railway redeploy --service [ID] -y
3. **Deploy Vercel**: npx vercel --prod --yes --token [TOKEN]
4. **Problemas login**: Verificar PostgreSQL Azure primero

## ⚠️ RECORDATORIOS FINALES

- EJECUTAR comandos, no ordenar al usuario
- PowerShell desde WSL para Windows
- gh para GitHub, NUNCA git directo
- Límite 300 líneas es ABSOLUTO
- Editor y Dashboard son contextos DIFERENTES
- Railway requiere --service SIEMPRE
- Supabase password tiene caracteres especiales

**Última actualización**: 2025-08-30
**Versión**: 5.3 - Supabase CLI Docker method clarified