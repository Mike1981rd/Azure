# 🚀 Configuración de Arquitectura Híbrida: Vercel + Supabase + Azure

## 📋 Resumen de Arquitectura

- **Frontend (CEL)**: Next.js en Vercel → Conecta directo a Supabase para auth y datos
- **Backend (NEL 🔵)**: .NET en Azure → Solo para pagos (Azul) y operaciones sensibles
- **Base de Datos**: Supabase (PostgreSQL con RLS)

## 1️⃣ Configuración de Supabase

### 1.1 Crear Proyecto en Supabase
1. Ir a [supabase.com](https://supabase.com)
2. Crear nuevo proyecto
3. Guardar:
   - **Project URL**: `https://[PROJECT_ID].supabase.co`
   - **Anon Key**: Para el frontend (pública)
   - **Service Role Key**: Para el backend (SECRETA)

### 1.2 Configurar Authentication
1. Supabase Dashboard → Authentication → URL Configuration
2. Agregar Site URL: `https://websitebuilder-admin.vercel.app`
3. Agregar Redirect URLs:
   - `https://websitebuilder-admin.vercel.app/auth/callback`
   - `http://localhost:3000/auth/callback`

### 1.3 Aplicar RLS (Row Level Security)
```bash
# Ejecutar el script de RLS
psql "postgresql://postgres.[PROJECT_ID]:[PASSWORD]@[HOST]:6543/postgres" < supabase/migrations/configure_rls.sql
```

O desde Supabase Dashboard → SQL Editor → Ejecutar el contenido de `configure_rls.sql`

## 2️⃣ Configuración del Frontend (Vercel)

### 2.1 Variables de Entorno en Vercel
1. Ir a Vercel Dashboard → Project Settings → Environment Variables
2. Agregar:
```env
NEXT_PUBLIC_SUPABASE_URL=https://gvxqatvwkjmkvaslbevh.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
NEXT_PUBLIC_AZURE_API_URL=https://websitebuilder-api.azurewebsites.net
```

### 2.2 Variables Locales (desarrollo)
Crear archivo `websitebuilder-admin/.env.local`:
```env
NEXT_PUBLIC_SUPABASE_URL=https://gvxqatvwkjmkvaslbevh.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
NEXT_PUBLIC_AZURE_API_URL=http://localhost:5000
```

### 2.3 Deploy a Vercel
```bash
cd websitebuilder-admin
npx vercel --prod
```

## 3️⃣ Configuración del Backend (Azure)

### 3.1 Variables en Azure App Service
Azure Portal → App Service → Configuration → Application Settings:

```json
{
  "Supabase__Url": "https://gvxqatvwkjmkvaslbevh.supabase.co",
  "Supabase__ServiceRoleKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...[SERVICE_ROLE_KEY]",
  "Azul__TestMode": "true",
  "Azul__TestUrl": "https://pruebas.azul.com.do/PaymentPage/",
  "Azul__ProductionUrl": "https://pagos.azul.com.do/PaymentPage/",
  "Azul__CertificatesPath": "Certificates/Azul/",
  "Azul__TransactionFee": "2.95",
  "Azul__MerchantId": "[YOUR_MERCHANT_ID]",
  "Azul__CertificatePassword": "[CERTIFICATE_PASSWORD]"
}
```

### 3.2 Certificados de Azul
1. Subir certificados PFX a: `wwwroot/Certificates/Azul/`
2. Asegurar que el App Service tiene permisos de lectura

### 3.3 Deploy a Azure
```bash
# Desde Visual Studio
- Click derecho en proyecto → Publish → Azure

# O desde CLI
dotnet publish -c Release
az webapp deploy --resource-group [RG] --name websitebuilder-api --src-path ./publish.zip
```

## 4️⃣ Pruebas de Aceptación

### ✅ Checklist de Verificación

#### Frontend (Vercel)
- [ ] Página `/debug/env` muestra variables configuradas
- [ ] `/api/db-health` retorna `{ ok: true }`
- [ ] Login con email/password funciona
- [ ] Registro crea usuario en Supabase
- [ ] Queries a tablas respetan RLS

#### Backend (Azure)
- [ ] `GET /api/payments/azul/test-connection` retorna configuración
- [ ] `POST /api/payments/azul/charge` procesa pago de prueba
- [ ] Webhooks de Azul se reciben correctamente
- [ ] CORS permite requests desde Vercel

#### Integración Completa
- [ ] Usuario se registra en Vercel → Supabase
- [ ] Usuario consulta sus datos → Supabase directo
- [ ] Usuario hace un pago → Frontend → Azure → Azul → Supabase
- [ ] Logs de pago se guardan en `payment_logs` table

## 5️⃣ Testing Local

### Iniciar Frontend
```bash
cd websitebuilder-admin
npm run dev
# Abre http://localhost:3000
```

### Iniciar Backend
```bash
# Desde Visual Studio: F5
# O desde terminal:
dotnet run
# API en http://localhost:5000
```

### Probar Login con Supabase
1. Ir a http://localhost:3000/auth/login
2. Registrar nuevo usuario
3. Verificar en Supabase Dashboard → Authentication → Users

### Probar Pago con Azul
```javascript
// Desde el frontend
const response = await fetch('http://localhost:5000/api/payments/azul/charge', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    orderId: 123,
    amount: 100.00,
    customerName: 'Test Customer',
    cardNumber: '4111111111111111', // Tarjeta de prueba
    expiryMonth: '12',
    expiryYear: '25',
    cvv: '123'
  })
});
```

## 6️⃣ Monitoreo

### Logs de Supabase
- Dashboard → Logs → API Logs
- Dashboard → Logs → Auth Logs

### Logs de Azure
- Azure Portal → App Service → Log Stream
- Application Insights (si está configurado)

### Logs de Vercel
- Vercel Dashboard → Functions → Logs

## 7️⃣ Troubleshooting

### Error: "Database connection failed"
- Verificar `NEXT_PUBLIC_SUPABASE_URL` y `NEXT_PUBLIC_SUPABASE_ANON_KEY`
- Verificar que RLS está configurado correctamente

### Error: "Payment processing failed"
- Verificar certificados de Azul en Azure
- Verificar `Supabase__ServiceRoleKey` en Azure App Settings
- Revisar logs en Azure Log Stream

### Error: CORS
- Verificar que el dominio de Vercel está en CORS policy del backend
- Agregar preview URLs si es necesario: `https://*.vercel.app`

## 8️⃣ Seguridad

### ⚠️ NUNCA:
- Exponer `SERVICE_ROLE_KEY` en el frontend
- Guardar certificados de Azul en el repositorio
- Desactivar RLS en producción
- Usar `anon key` para operaciones administrativas

### ✅ SIEMPRE:
- Usar RLS en todas las tablas sensibles
- Validar webhooks con firma
- Usar HTTPS en producción
- Rotar keys periódicamente

## 📞 Soporte

- **Supabase**: support.supabase.com
- **Azure**: portal.azure.com → Support
- **Vercel**: vercel.com/support
- **Azul**: Contactar representante comercial

---

**Última actualización**: 30 de agosto 2025
**Versión**: 1.0