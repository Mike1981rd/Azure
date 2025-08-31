# 🚀 PRUEBA SIMPLE DE RAILWAY - SIN COMPLICACIONES

## ✅ MÉTODO DIRECTO: Cambiar Backend Temporalmente en Vercel

### PASO 1: Cambiar Variable en Vercel (2 minutos)

**Opción A - Por Dashboard:**
1. Ve a: https://vercel.com/dashboard
2. Selecciona tu proyecto: `websitebuilder-admin`
3. Ve a Settings → Environment Variables
4. Edita `NEXT_PUBLIC_API_URL`
5. Cambia de:
   - Azure: `https://api.test1hotelwebsite.online/api`
6. A:
   - Railway: `https://websitebuilderapi-production-production.up.railway.app/api`
7. Click en Save
8. Redeploy (botón en la parte superior)

**Opción B - Por Comando (más rápido):**
```bash
# Cambiar a Railway
vercel env rm NEXT_PUBLIC_API_URL production
vercel env add NEXT_PUBLIC_API_URL production
# Pegar: https://websitebuilderapi-production-production.up.railway.app/api

# Redeploy
vercel --prod --yes
```

### PASO 2: Probar (5 minutos)

1. Espera 1-2 minutos al deploy
2. Abre: https://websitebuilder-admin.vercel.app
3. Prueba:
   - [ ] Login funciona
   - [ ] No hay errores CORS
   - [ ] Website Builder carga
   - [ ] Puedes guardar cambios

### PASO 3: Decisión

**Si Railway funciona bien:**
- ✅ Dejar Railway
- ✅ Apagar Azure para ahorrar

**Si Railway falla:**
- ❌ Volver a Azure inmediatamente:
```bash
vercel env rm NEXT_PUBLIC_API_URL production
vercel env add NEXT_PUBLIC_API_URL production
# Pegar: https://api.test1hotelwebsite.online/api
vercel --prod --yes
```

---

## 🔄 ALTERNATIVA: Test con LocalStorage (Sin tocar Vercel)

Si prefieres NO cambiar Vercel aún, puedes probar así:

1. Abre tu app: https://websitebuilder-admin.vercel.app
2. Abre consola (F12)
3. Pega esto:
```javascript
// Activar Railway solo para ti
localStorage.setItem('API_OVERRIDE', 'https://websitebuilderapi-production-production.up.railway.app/api');
location.reload();
```

4. Para volver a Azure:
```javascript
localStorage.removeItem('API_OVERRIDE');
location.reload();
```

**Nota**: Esto requiere agregar 1 línea en tu código de API.

---

## 📊 COMPARACIÓN RÁPIDA

| Backend | URL | Estado | CORS |
|---------|-----|--------|------|
| Azure | https://api.test1hotelwebsite.online | ✅ Vivo | ❓ A veces |
| Railway | https://websitebuilderapi-production-production.up.railway.app | ✅ Vivo | ❓ Por probar |

---

## 🎯 MI RECOMENDACIÓN:

**Cambia la variable en Vercel directamente** (Opción 1)
- Es rápido (2 minutos)
- Reversible (2 minutos)
- Prueba real completa
- Si funciona, ya quedas con Railway

¿Qué prefieres hacer?