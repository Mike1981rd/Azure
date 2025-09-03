# CLAUDE.md - CONFIGURACIÓN CRÍTICA

## ⛔ REGLAS ABSOLUTAS
• **NUNCA** decir "ejecuta este comando" → Claude SIEMPRE ejecuta directamente con `powershell.exe -Command`
• **NUNCA** usar git push/pull → SIEMPRE usar GitHub CLI (`gh`)
• **PostgreSQL Azure**: Si login falla → ejecutar: `ssh azureuser@20.169.209.166 "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '123456';\""`

## 📦 REPOSITORIOS
**Backend**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI`
• Remote: `Mike1981rd/Render.git` → push a `render/main`

**Frontend**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin`
• Remote: `Mike1981rd/Vercel-Frontend.git` → branch `whatsapp`, merge a `main` para producción

## 🔑 SERVICIOS
**Render**: `websitebuilder-api-staging.onrender.com`
**Vercel**: `websitebuilder-admin.vercel.app`
**Neon DB**: 
• Host: `ep-withered-paper-ad3yhgct.c-2.us-east-1.aws.neon.tech`
• User: `neondb_owner` / Pass: `npg_U1xrFkcz8PDC`

## 🛑 REGLAS DESARROLLO
• **Max 300 líneas/archivo** → Si >250 líneas, crear nuevo archivo
• **Arquitectura modular**: `/components/editor/modules/[ModuleName]/`
• **EditorPreview.tsx CONGELADO** → NO modificar (1,763 líneas)
• **i18n**: SIEMPRE actualizar `/src/lib/i18n/translations/es.json` y `en.json`

## 🌐 WSL → WINDOWS
• **NO usar localhost** → Obtener IP: `ip route | grep default` (típicamente `172.25.64.1`)
• Playwright/API: usar `http://172.25.64.1:3000` NO `http://localhost:3000`

## 🎯 COMANDO /init-session
Leer OBLIGATORIAMENTE:
1. `CLAUDE.md` → Este archivo de configuración
2. `docs/implementations/features/2025-01-live-preview.md` → Live Preview implementación
3. `docs/WEBSITE-BUILDER-MODULE-GUIDE.md` → Guía módulos Website Builder
4. `.claude-rules-enforcement.md`
5. `blueprint1.md` → 9 problemas críticos
6. `blueprint2.md` → Arquitectura técnica
7. `blueprint3.md` → UI/UX frontend
8. `CLAUDEBK1.md` → Reglas base
9. `CLAUDEBK2.md` → Patterns y troubleshooting
10. `logs.md` → Sistema logging

## 🏗️ COMANDO /init-websitebuilder
Leer:
• `websitebuilderprogress.md`
• `blueprintwebsite.md`
• `docs/WEBSITE-BUILDER-ARCHITECTURE.md`
• `docs/WEBSITE-BUILDER-TROUBLESHOOTING.md`

## 🗺️ ARCHIVOS CRÍTICOS WEBSITE BUILDER

**Editor** (`/editor`):
• Layout: `/src/app/editor/page.tsx`
• Sidebar: `/src/components/editor/EditorSidebar.tsx`
• Settings: `/src/components/editor/GlobalSettingsPanel.tsx`

**Dashboard** (`/dashboard/global-settings`):
• Page: `/src/app/dashboard/global-settings/page.tsx`

⚠️ SIEMPRE preguntar: "¿Estás en /editor o /dashboard?"

## 📄 LIVE PREVIEW - CONTRATO TÉCNICO

**Flujo de datos**:
1. Estructurales → `/api/structural-components/company/{id}/published`
2. Tema → `/api/global-theme-config/company/{id}/published`
3. Secciones → localStorage: `page_sections_{pageType}` → Backend: `/api/websitepages/company/{id}/slug/{handle}`

**Claves localStorage**: 
• SIEMPRE usar `page_sections_{pageType}` NO `page_sections_{id}`

**DeviceView**:
• NO forzar `|| 'desktop'`
• Usar patrón canónico de detección móvil

**Habitaciones/CUSTOM**:
• Slug: `habitaciones` (redirect 301 desde `/custom`)
• Módulos `room_*` SOLO en `pageType === CUSTOM`

## 🔐 PLAYWRIGHT CONFIG
**Credenciales**: `miguelnuez919@yahoo.com` / `123456`
**IP Host Windows**: verificar con `ip route | grep default`
**URL**: `http://172.25.64.1:3000/login`

## 🚀 DEPLOYS

**Vercel** (Claude ejecuta):
```bash
cd /mnt/c/Users/hp/Documents/Visual\ Studio\ 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin
npx vercel --prod --yes --token wdnjcPrnirBnWbEXu1zZGIiR
```

**Backend detener**:
```powershell
powershell.exe -Command "Get-Process dotnet | Stop-Process -Force"
```

## 🐘 SUPABASE CLI
```bash
export PATH="$HOME/.local/bin:$PATH"
export SUPABASE_ACCESS_TOKEN="sbp_6d79604906628971ef0bcfdd98219a2f2418972a"
```
• Project ID: `gvxqatvwkjmkvaslbevh`
• DB Pass: `AllisoN@1710.#`
• Pooler: `aws-1-us-east-1.pooler.supabase.com:6543`

## ⚠️ CHECKLIST PRE-ACCIÓN
□ ¿Voy a decir "ejecuta"? → EJECUTAR YO MISMO
□ ¿Es build/compile? → `powershell.exe -Command`
□ ¿Es deploy? → Ejecutar comando documentado
□ ¿Estoy ordenando? → ERROR CRÍTICO

**Última actualización**: 2025-09-03
**Stack**: ASP.NET Core 8 + Next.js 14 + PostgreSQL + Neon/Supabase