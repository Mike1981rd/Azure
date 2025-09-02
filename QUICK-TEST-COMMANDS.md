# 🚀 COMANDOS RÁPIDOS PARA PROBAR AZURE vs RAILWAY

## ✅ TUS DOS SITIOS LISTOS:

### 1️⃣ PRODUCCIÓN (AZURE - Tu sitio actual)
**URL**: https://websitebuilder-admin.vercel.app
- Backend: Azure
- Este es tu sitio actual sin cambios

### 2️⃣ TEST (RAILWAY - Prueba nueva)
**URL**: https://websitebuilder-railway-test-hxa1z63b6.vercel.app
- Backend: Railway + Supabase
- Sitio de prueba configurado

## 🔥 PRUEBAS RÁPIDAS EN NAVEGADOR:

### Test 1: Abrir ambos en pestañas separadas
1. Abre Chrome
2. Pestaña 1: https://websitebuilder-admin.vercel.app (AZURE)
3. Pestaña 2: https://websitebuilder-railway-test-hxa1z63b6.vercel.app (RAILWAY)

### Test 2: Login en ambos
- Email: `miguelnuez919@yahoo.com`
- Password: `123456`

### Test 3: Ver consola (F12)
- Buscar errores CORS (texto rojo)
- Azure tiene CORS? Sí/No
- Railway tiene CORS? Sí/No

## 📊 COMANDOS PARA VERIFICAR BACKENDS:

```bash
# Test Azure Backend
curl -s https://api.test1hotelwebsite.online/api/health | head -20

# Test Railway Backend
curl -s https://websitebuilderapi-production-production.up.railway.app/api/health | head -20

# Comparar velocidad
time curl -s https://api.test1hotelwebsite.online/api/health > /dev/null
time curl -s https://websitebuilderapi-production-production.up.railway.app/api/health > /dev/null
```

## 🎯 DECISIÓN RÁPIDA:

| Criterio | Azure | Railway | Ganador |
|----------|-------|---------|---------|
| Login funciona | ? | ? | |
| Sin CORS | ? | ? | |
| Velocidad | ? | ? | |
| **RECOMENDACIÓN** | | | |

---

**ÚLTIMA ACTUALIZACIÓN**: 28 enero 2025 - 19:48
**ESTADO**: ✅ Ambos deployments listos para probar