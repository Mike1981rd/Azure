# 🚀 Plan de Migración: Azure → Railway+Supabase

## Fase 1: Preparación (30 minutos)

### 1.1 Backup de Azure
```bash
# Exportar base de datos actual
pg_dump "Host=azure-db.postgres.database.azure.com;..." > backup_azure_$(date +%Y%m%d).sql

# Guardar variables de entorno
az webapp config appsettings list --name websitebuilder-api > azure_env_backup.json
```

### 1.2 Verificar Railway+Supabase
```bash
# Según CLAUDE.md ya tienes configurado:
export RAILWAY_TOKEN=3752c685-6cfc-4d23-876b-6b543206212b
railway status --service c2be9027-a4bc-4674-94e4-1090b78d6753
```

## Fase 2: Migración de Datos (1 hora)

### 2.1 Importar a Supabase
```bash
# Conectar a Supabase
psql "postgresql://postgres.gvxqatvwkjmkvaslbevh:PcpBLRo5Mf5jBC4IQO_ineBHjVIj7npK9JhW5dJlUKI@aws-0-us-west-1.pooler.supabase.com:6543/postgres" < backup_azure_$(date +%Y%m%d).sql
```

### 2.2 Verificar Integridad
```sql
-- En Supabase SQL Editor
SELECT COUNT(*) FROM companies;
SELECT COUNT(*) FROM users;
SELECT COUNT(*) FROM global_theme_configs;
-- Comparar con Azure
```

## Fase 3: Actualización de Frontend (15 minutos)

### 3.1 Variables en Vercel
```bash
# En Vercel Dashboard o CLI
vercel env rm NEXT_PUBLIC_API_URL production
vercel env add NEXT_PUBLIC_API_URL production
# Valor: https://websitebuilderapi-production-production.up.railway.app/api

# Mantener Azure como fallback temporalmente
vercel env add NEXT_PUBLIC_API_FALLBACK production
# Valor: https://api.test1hotelwebsite.online/api
```

### 3.2 Implementar Fallback en Frontend
```typescript
// lib/api.ts
const API_URL = process.env.NEXT_PUBLIC_API_URL;
const API_FALLBACK = process.env.NEXT_PUBLIC_API_FALLBACK;

export const apiClient = {
  async fetch(endpoint: string, options?: RequestInit) {
    try {
      const response = await fetch(`${API_URL}${endpoint}`, options);
      if (!response.ok && API_FALLBACK) {
        // Fallback a Azure si Railway falla
        return fetch(`${API_FALLBACK}${endpoint}`, options);
      }
      return response;
    } catch (error) {
      if (API_FALLBACK) {
        return fetch(`${API_FALLBACK}${endpoint}`, options);
      }
      throw error;
    }
  }
};
```

## Fase 4: Testing en Producción (30 minutos)

### 4.1 Tests Críticos
- [ ] Login/Logout
- [ ] CRUD de Company
- [ ] Global Theme Config
- [ ] Structural Components
- [ ] Website Builder Editor
- [ ] Preview de páginas

### 4.2 Monitoreo
```javascript
// Agregar temporalmente en el frontend
window.addEventListener('load', () => {
  // Log del backend usado
  console.log('🔍 Backend:', process.env.NEXT_PUBLIC_API_URL);
  
  // Test de salud
  fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/health`)
    .then(r => console.log('✅ Railway OK'))
    .catch(e => console.error('❌ Railway Error:', e));
});
```

## Fase 5: Cut-over Final (15 minutos)

### 5.1 Confirmar Railway Estable
- Verificar logs últimas 2 horas
- Confirmar 0 errores CORS
- Performance aceptable (<500ms promedio)

### 5.2 Desactivar Azure
```bash
# Detener app service temporalmente (no eliminar aún)
az webapp stop --name websitebuilder-api --resource-group rg-aspnetcore-prod

# Remover fallback de Vercel
vercel env rm NEXT_PUBLIC_API_FALLBACK production
```

### 5.3 Actualizar DNS (si aplica)
- Apuntar api.test1hotelwebsite.online → Railway
- O simplemente dejar de usar ese dominio

## Fase 6: Rollback Plan (si necesario)

### Si algo sale mal:
```bash
# 1. Reactivar Azure inmediatamente
az webapp start --name websitebuilder-api --resource-group rg-aspnetcore-prod

# 2. Cambiar Vercel a Azure
vercel env add NEXT_PUBLIC_API_URL production
# Valor: https://api.test1hotelwebsite.online/api

# 3. Redeploy frontend
vercel --prod
```

## Timeline Estimado

| Fase | Tiempo | Riesgo | Rollback |
|------|--------|---------|----------|
| Preparación | 30 min | Bajo | N/A |
| Migración Datos | 1 hora | Medio | Restaurar backup |
| Update Frontend | 15 min | Bajo | Revertir env vars |
| Testing | 30 min | Bajo | N/A |
| Cut-over | 15 min | Alto | Script automático |
| **TOTAL** | **2.5 horas** | **Medio** | **< 5 min** |

## Checklist Pre-Migración

- [ ] Backup completo de Azure
- [ ] Railway funcionando (verificado con test)
- [ ] Supabase accesible
- [ ] Token Railway válido
- [ ] Acceso a Vercel dashboard
- [ ] Plan de comunicación a usuarios (si aplica)
- [ ] Ventana de mantenimiento definida

## Post-Migración (Después de 24h estable)

1. **Reducir costos Azure:**
   ```bash
   # Eliminar recursos no usados
   az group delete --name rg-aspnetcore-prod --yes
   ```

2. **Optimizar Railway:**
   - Configurar auto-scaling si necesario
   - Ajustar límites de memoria
   - Configurar alertas

3. **Documentar:**
   - Actualizar CLAUDE.md
   - Actualizar README
   - Documentar nuevos endpoints

## Contactos de Emergencia

- Railway Support: support@railway.app
- Supabase Support: support@supabase.io
- Azure Support: (tu plan actual)

---

**Última actualización:** 28 de enero 2025
**Decisión recomendada:** MIGRAR A RAILWAY+SUPABASE ✅