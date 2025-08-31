# 🚀 Guía de Comparación Azure vs Railway

## 📋 Configuración Rápida

### 1. Configurar Variables de Entorno

Crea un archivo `.env.local` en `/websitebuilder-admin/`:

```env
# Backend por defecto (azure | railway | local)
NEXT_PUBLIC_BACKEND_PROVIDER=azure

# URLs de cada backend
NEXT_PUBLIC_AZURE_API_URL=https://api.test1hotelwebsite.online/api
NEXT_PUBLIC_RAILWAY_API_URL=https://websitebuilderapi-production-production.up.railway.app/api
NEXT_PUBLIC_LOCAL_API_URL=http://172.25.64.1:5266/api

# Habilitar tracking de performance
NEXT_PUBLIC_ENABLE_PERFORMANCE_TRACKING=true
NEXT_PUBLIC_LOG_API_TIMES=true
```

### 2. Integrar el Backend Switcher

En tu layout principal (`/src/app/layout.tsx`), agrega:

```tsx
import { BackendSwitcher } from '@/components/BackendSwitcher';

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        {children}
        <BackendSwitcher />
      </body>
    </html>
  );
}
```

### 3. Actualizar la Configuración de API

En tu archivo `/src/lib/api.ts`, actualiza para usar el hook:

```typescript
// Opción A: Leer directamente de localStorage
export function getAPIUrl(): string {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('API_URL') || 
           process.env.NEXT_PUBLIC_AZURE_API_URL || 
           'https://api.test1hotelwebsite.online/api';
  }
  return process.env.NEXT_PUBLIC_AZURE_API_URL || 
         'https://api.test1hotelwebsite.online/api';
}

// Usar en tus llamadas:
const response = await fetch(`${getAPIUrl()}/endpoint`);
```

## 🧪 Métodos de Testing

### Método 1: Switch Visual en la UI

1. **Inicia el frontend**:
   ```bash
   cd websitebuilder-admin
   npm run dev
   ```

2. **Busca el botón flotante** en la esquina inferior derecha:
   - Muestra el backend actual (Azure/Railway/Local)
   - Click para abrir el menú de selección
   - Cambia entre backends sin reiniciar

3. **Monitorea las métricas** en tiempo real:
   - Click en "Show Performance Metrics"
   - Verás: Response time promedio, Total de requests, Requests fallidos

### Método 2: Script de PowerShell Automatizado

```powershell
# Ejecutar con tu token JWT
.\test-backend-performance.ps1 -Token "tu-jwt-token" -Iterations 20

# Sin autenticación (solo endpoints públicos)
.\test-backend-performance.ps1 -Iterations 10
```

El script:
- Prueba múltiples endpoints
- Compara tiempos de respuesta
- Genera un reporte JSON con resultados
- Muestra el ganador con porcentaje de mejora

### Método 3: Testing Manual con cURL

```bash
# Test Azure
time curl -H "Authorization: Bearer TOKEN" https://api.test1hotelwebsite.online/api/products

# Test Railway  
time curl -H "Authorization: Bearer TOKEN" https://websitebuilderapi-production-production.up.railway.app/api/products

# Comparar health endpoints
curl -w "@curl-format.txt" -o /dev/null -s https://api.test1hotelwebsite.online/api/health
curl -w "@curl-format.txt" -o /dev/null -s https://websitebuilderapi-production-production.up.railway.app/api/health
```

Crea `curl-format.txt`:
```
time_namelookup:  %{time_namelookup}s\n
time_connect:  %{time_connect}s\n
time_appconnect:  %{time_appconnect}s\n
time_pretransfer:  %{time_pretransfer}s\n
time_redirect:  %{time_redirect}s\n
time_starttransfer:  %{time_starttransfer}s\n
time_total:  %{time_total}s\n
```

## 📊 Métricas a Comparar

### 1. **Latencia (Response Time)**
- Primera respuesta (TTFB - Time to First Byte)
- Respuesta completa
- Consistencia (desviación estándar)

### 2. **Throughput**
- Requests por segundo
- Capacidad de carga concurrente

### 3. **Disponibilidad**
- Uptime
- Error rate
- Timeouts

### 4. **Costo-Beneficio**
- Azure: ~$30-50/mes (VM + IP + Storage)
- Railway: ~$5-20/mes (usage-based)

## 🎯 Test Scenarios Recomendados

### Scenario 1: Carga Normal
```javascript
// 10 usuarios concurrentes, 100 requests cada uno
for (let i = 0; i < 10; i++) {
  setTimeout(() => {
    testBackend('azure', 100);
    testBackend('railway', 100);
  }, i * 1000);
}
```

### Scenario 2: Picos de Tráfico
```javascript
// 50 requests simultáneos
const promises = [];
for (let i = 0; i < 50; i++) {
  promises.push(fetch(`${apiUrl}/api/products`));
}
await Promise.all(promises);
```

### Scenario 3: Operaciones Pesadas
- Upload de imágenes grandes
- Queries complejas con JOINs
- Exportación de reportes

## 📈 Tracking de Resultados

Los resultados se guardan automáticamente en:
- **Consola del navegador**: Logs en tiempo real con `📊` prefix
- **LocalStorage**: Métricas acumuladas por sesión
- **Archivos JSON**: Exportados por el script PowerShell

### Formato de Resultados:
```json
{
  "azure": {
    "averageResponseTime": 145.3,
    "minResponseTime": 89,
    "maxResponseTime": 523,
    "errorRate": 0.02,
    "totalRequests": 500
  },
  "railway": {
    "averageResponseTime": 112.7,
    "minResponseTime": 76,
    "maxResponseTime": 445,
    "errorRate": 0.01,
    "totalRequests": 500
  },
  "winner": "railway",
  "improvement": "22.4%"
}
```

## 🔍 Troubleshooting

### Problema: CORS errors al cambiar de backend
**Solución**: Asegúrate que ambos backends tengan CORS configurado:
```csharp
// En Program.cs de ambos backends
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(
            "http://localhost:3000",
            "https://websitebuilder-admin.vercel.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

### Problema: Railway responde 502 Bad Gateway
**Posibles causas**:
- El servicio está iniciándose (espera 1-2 minutos)
- Variables de entorno no configuradas
- Puerto incorrecto (debe ser $PORT en Railway)

### Problema: Azure más lento en primera llamada
**Causa**: Cold start de IIS/Azure App Service
**Solución**: Implementar Application Initialization o Always On

## 🏆 Criterios de Decisión

| Criterio | Azure | Railway | Ganador |
|----------|-------|---------|---------|
| Latencia promedio | Mayor control regional | CDN global incluido | Depende de ubicación |
| Escalabilidad | Manual (vertical) | Automática (horizontal) | Railway |
| Costo inicial | ~$30/mes mínimo | $0 (pay-as-you-go) | Railway |
| Control total | ✅ Full VM access | ❌ Platform-managed | Azure |
| Setup complexity | Alta | Baja | Railway |
| CI/CD | Manual o Azure DevOps | Automático con GitHub | Railway |

## 📝 Comandos Útiles

```bash
# Ver logs de Railway
railway logs --service c2be9027-a4bc-4674-94e4-1090b78d6753

# Ver métricas de Azure
az monitor metrics list --resource /subscriptions/.../vm-aspnetcore-prod --metric "Percentage CPU"

# Test de carga con Apache Bench
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" https://api.test1hotelwebsite.online/api/products
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" https://websitebuilderapi-production-production.up.railway.app/api/products
```

## 🎉 Resultado Esperado

Después de las pruebas deberías tener:
1. **Datos cuantitativos** de performance de ambos backends
2. **Experiencia cualitativa** del comportamiento bajo carga
3. **Decisión informada** sobre qué plataforma usar en producción

---

**Última actualización**: 2025-08-29
**Estado**: Sistema de comparación implementado y funcional