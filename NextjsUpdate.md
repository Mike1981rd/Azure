# 📦 PLAN DE ACTUALIZACIÓN: Next.js 14.2.31 → 15.5.2

## 📅 Información del Plan
- **Fecha creación**: 31 de agosto 2025
- **Tiempo estimado**: 30-60 minutos
- **Nivel de riesgo**: BAJO
- **Archivos afectados**: 1 crítico + cambios menores

## 🎯 OBJETIVO
Actualizar el frontend a Next.js 15.5.2 para entregar al cliente con la última versión estable, aprovechando mejoras de rendimiento y características modernas.

## ✅ PRE-REQUISITOS

1. **Frontend ya subido a GitHub**:
   - Repo: https://github.com/Mike1981rd/WebsiteBuilder-Frontend
   - Branch: main
   - Estado: ✅ Listo

2. **Backup del código actual**:
   - Local: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/websitebuilder-admin`
   - GitHub: Commit inicial ya creado

3. **Desarrollo pausado**:
   - Detener servidor local antes de actualizar

## 📊 ANÁLISIS DE IMPACTO

### Archivos que requieren cambios:

**1. CRÍTICO - API Asíncrona**:
- `/src/lib/supabase-server.ts`
  - Línea 5: `const cookieStore = cookies()` → `const cookieStore = await cookies()`
  - Requiere hacer la función async

**2. OPCIONALES - Dynamic Routes** (verificar después del codemod):
- `/src/app/dashboard/*/[id]/*.tsx` - Varios archivos usan `params.id`
- Estos pueden necesitar await si son Server Components

### Cambios automáticos por codemod:
- Package.json dependencies
- Configuración de ESLint
- Imports deprecados
- APIs asíncronas básicas

## 🚀 PLAN DE EJECUCIÓN

### PASO 1: Preparación (5 min)
```bash
# 1. Detener servidor desarrollo
# Ctrl+C en terminal de Next.js

# 2. Crear branch para la actualización
cd /tmp/frontend-clean
git checkout -b feature/nextjs-15-upgrade

# 3. Verificar estado limpio
git status
```

### PASO 2: Actualización automática con Codemod (10 min)
```bash
# 1. Ejecutar codemod de Next.js
cd /tmp/frontend-clean
npx @next/codemod@latest upgrade latest

# El codemod preguntará:
# - ¿Actualizar dependencies? → YES
# - ¿Migrar a React 19? → YES
# - ¿Actualizar ESLint? → YES
# - ¿Aplicar codemods? → YES
```

### PASO 3: Correcciones manuales (15 min)

#### A. Actualizar supabase-server.ts:
```typescript
// /src/lib/supabase-server.ts
import { cookies } from 'next/headers'

export async function createServerClient() {  // Agregar async
  const cookieStore = await cookies()  // Agregar await
  
  // resto del código...
}
```

#### B. Verificar archivos con params dinámicos:
```bash
# Buscar archivos que puedan necesitar async
grep -r "params\." src/app --include="*.tsx" | grep "page.tsx"

# Para cada archivo encontrado, si es Server Component:
# - Agregar async a la función
# - Agregar await antes de params
```

### PASO 4: Actualizar dependencias adicionales (5 min)
```bash
# 1. Actualizar todas las dependencias
npm update

# 2. Verificar vulnerabilidades
npm audit fix

# 3. Limpiar cache
rm -rf .next node_modules/.cache
```

### PASO 5: Testing local (10 min)
```bash
# 1. Iniciar servidor desarrollo
npm run dev

# 2. Verificar en browser:
# - http://localhost:3000 (redirect a login)
# - http://localhost:3000/login (página de login)
# - Console sin errores

# 3. Test build producción
npm run build

# Si build exitoso, continuar. Si hay errores, corregir.
```

### PASO 6: Commit y Push (5 min)
```bash
# 1. Commit cambios
git add -A
git commit -m "feat: Upgrade to Next.js 15.5.2 with React 19

- Updated all async APIs (cookies, headers)
- Migrated to React 19
- Updated ESLint configuration
- Applied Next.js 15 codemods
- Fixed TypeScript compatibility"

# 2. Push branch
git push origin feature/nextjs-15-upgrade

# 3. Merge a main
git checkout main
git merge feature/nextjs-15-upgrade
git push origin main
```

### PASO 7: Deploy a Vercel (10 min)
```bash
# Opción A: Desde CLI
vercel --prod

# Opción B: Automático
# Vercel detectará el push y desplegará automáticamente
```

## 🐛 TROUBLESHOOTING

### Error: "Cannot find module 'next/headers'"
**Solución**: Reinstalar Next.js
```bash
npm uninstall next
npm install next@latest
```

### Error: "cookies() is not a function"
**Solución**: Verificar import correcto
```typescript
import { cookies } from 'next/headers'  // Correcto
// NO: import cookies from 'next/headers'
```

### Error: Build falla con "Type error"
**Solución**: Actualizar tipos
```bash
npm install --save-dev @types/react@latest @types/react-dom@latest
```

### Error: "Hydration mismatch"
**Solución**: Limpiar cache y localStorage
```bash
rm -rf .next
# En browser: Abrir DevTools > Application > Clear Storage
```

## ✅ CHECKLIST POST-ACTUALIZACIÓN

- [ ] Server development funciona sin errores
- [ ] Login page carga correctamente
- [ ] Editor funciona (crear/editar secciones)
- [ ] Build de producción exitoso
- [ ] No hay errores en console del browser
- [ ] Tests de Playwright pasan (si existen)
- [ ] Vercel deploy exitoso
- [ ] Verificar en producción: https://app.test1hotelwebsite.online

## 📈 BENEFICIOS DE LA ACTUALIZACIÓN

1. **Rendimiento**:
   - Turbopack estable (desarrollo 10x más rápido)
   - Mejor tree-shaking
   - Menor bundle size

2. **Características nuevas**:
   - Partial Prerendering
   - Mejor manejo de errores
   - React 19 features

3. **Mantenimiento**:
   - Soporte activo
   - Actualizaciones de seguridad
   - Compatibilidad futura

## 🔄 ROLLBACK (si necesario)

Si algo sale mal:
```bash
# 1. Volver al commit anterior
cd /tmp/frontend-clean
git checkout main
git reset --hard HEAD~1

# 2. Force push
git push origin main --force

# 3. Vercel automáticamente volverá a la versión anterior
```

## 📝 NOTAS IMPORTANTES

1. **NO actualizar** si hay features críticas en desarrollo
2. **Hacer backup** antes de empezar
3. **Probar localmente** antes de deployar
4. **Comunicar al equipo** antes de actualizar producción
5. **Documentar cualquier issue** encontrado

## 🎯 RESULTADO ESPERADO

- Next.js 15.5.2 funcionando
- React 19 instalado
- Sin breaking changes en funcionalidad
- Mejor rendimiento en desarrollo
- Cliente recibe versión más actual

---

**Última revisión**: 31 de agosto 2025
**Estado**: LISTO PARA EJECUTAR
**Responsable**: Equipo de desarrollo