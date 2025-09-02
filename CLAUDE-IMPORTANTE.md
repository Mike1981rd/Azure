# ⚠️ REGLAS CRÍTICAS PARA CLAUDE Y CUALQUIER IA ⚠️

## 🚨 PROBLEMA DETECTADO: 
El 2 de septiembre 2025 se detectó que había DOS copias del proyecto:
1. **CORRECTA**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI` (Windows)
2. **PROBLEMÁTICA**: `~/websitebuilder-admin` (WSL) - AHORA DESACTIVADA

Esto causó que los archivos se BORRARAN durante la noche por conflictos de file watchers.

## ✅ REGLAS OBLIGATORIAS:

### 1. SIEMPRE TRABAJAR EN:
```bash
cd /mnt/c/Users/hp/Documents/Visual\ Studio\ 2022/Projects/WebsiteBuilderAPI
```

### 2. NUNCA HACER:
- ❌ NO crear copias en `~/` o `/home/plaska/`
- ❌ NO ejecutar `npm install` o `npm run dev` desde WSL home
- ❌ NO usar rutas relativas desde WSL
- ❌ NO trabajar en `~/websitebuilder-admin-OLD-NO-USE`

### 3. COMANDOS SEGUROS:
```bash
# Para ir al proyecto principal
cdweb

# Para ir al frontend
cdwebadmin

# Para ejecutar dev server
npmdev
```

### 4. ANTES DE TERMINAR SESIÓN:
```bash
# Siempre cerrar procesos node
pkill node
```

## 📅 INFORMACIÓN CRÍTICA:
- **Proyecto iniciado**: Hace 45 días
- **Deadline**: 3 días desde el 2 de septiembre 2025
- **Progreso**: 90% completado
- **NO ARRIESGAR**: El proyecto está casi listo, ser EXTREMADAMENTE cuidadoso

## 🔒 BACKUP:
- Token de Vercel guardado en: `/tmp/env-local-wsl-backup-20250902.txt`
- Carpeta vieja desactivada en: `~/websitebuilder-admin-OLD-NO-USE/`

---
**ÚLTIMA ACTUALIZACIÓN**: 2 de septiembre 2025
**RAZÓN**: Prevenir pérdida de archivos por conflictos WSL/Windows