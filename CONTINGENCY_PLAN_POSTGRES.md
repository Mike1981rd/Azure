# 🚨 PLAN DE CONTINGENCIA - REVERTIR A POSTGRESQL LOCAL
## Fecha: 30 de Agosto 2025
## Deadline: 31 de Agosto 2025 - 5:00 PM

---

## ✅ CONFIGURACIÓN ACTUAL FUNCIONAL (PostgreSQL Local en Windows)

### 1. BASE DE DATOS
- **Host**: 172.25.64.1 (Windows host desde WSL)
- **Puerto**: 5432
- **Database**: websitebuilder
- **Usuario**: postgres
- **Password**: 123456
- **Servicio Windows**: postgresql-x64-17

### 2. CONNECTION STRING FUNCIONAL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=172.25.64.1;Port=5432;Database=websitebuilder;Username=postgres;Password=123456;Pooling=true;MinPoolSize=5;MaxPoolSize=20"
  }
}
```

### 3. CONFIGURACIÓN pg_hba.conf (Ya aplicada)
Ubicación: `C:\Program Files\PostgreSQL\17\data\pg_hba.conf`
```
# Allow connections from WSL
host    all             all             172.25.64.0/24          md5
```

### 4. USUARIOS CONFIRMADOS EN DB LOCAL
```sql
-- Usuario principal que FUNCIONA:
Email: miguelnuez919@yahoo.com
Password: [tu contraseña actual]

-- Usuario de prueba creado:
Email: test@test.com
Password: test123
Rol: SuperAdmin
```

---

## 🔄 PROCESO DE REVERSIÓN (Si Supabase falla)

### PASO 1: Detener la aplicación
```bash
# En PowerShell como Admin:
Stop-Process -Name "WebsiteBuilderAPI" -Force
```

### PASO 2: Cambiar Connection String
**Archivo**: `/mnt/c/Users/hp/Documents/Visual Studio 2022/Projects/WebsiteBuilderAPI/appsettings.json`

**Cambiar DE** (Supabase):
```json
"DefaultConnection": "Host=aws-1-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.gvxqatvwkjmkvaslbevh;Password=AllisoN@1710.#;Pooling=true"
```

**Cambiar A** (PostgreSQL Local):
```json
"DefaultConnection": "Host=172.25.64.1;Port=5432;Database=websitebuilder;Username=postgres;Password=123456;Pooling=true;MinPoolSize=5;MaxPoolSize=20"
```

### PASO 3: Verificar PostgreSQL está corriendo
```powershell
# En PowerShell:
Get-Service postgresql-x64-17

# Si no está corriendo:
Start-Service postgresql-x64-17
```

### PASO 4: Probar conexión
```bash
# Desde WSL:
export PGPASSWORD='123456'
psql -h 172.25.64.1 -p 5432 -U postgres -d websitebuilder -c "SELECT COUNT(*) FROM \"Users\";"

# Debe devolver: count = 3
```

### PASO 5: Ejecutar la aplicación
```powershell
# En PowerShell desde el directorio del proyecto:
cd "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI"
dotnet run --urls=http://localhost:5000
```

### PASO 6: Verificar funcionamiento
1. Abrir navegador
2. Ir a la aplicación
3. Login con: miguelnuez919@yahoo.com

---

## 📦 DEPLOYMENT PARA PRODUCCIÓN CON POSTGRESQL

### OPCIÓN A: Render con PostgreSQL Addon
1. En Render Dashboard → New PostgreSQL
2. Copiar connection string
3. Actualizar variable de entorno en Render:
   ```
   CONNECTION_STRING = [PostgreSQL de Render]
   ```

### OPCIÓN B: Azure Database for PostgreSQL
1. Crear Azure Database for PostgreSQL
2. Configurar firewall para permitir Render
3. Migrar datos con pg_dump/pg_restore
4. Actualizar connection string

### OPCIÓN C: Digital Ocean Managed Database
1. Crear PostgreSQL cluster en DO
2. Obtener connection string
3. Configurar trusted sources
4. Actualizar variables en Render

---

## 🔍 COMANDOS DE VERIFICACIÓN RÁPIDA

### Verificar tablas existen:
```bash
export PGPASSWORD='123456'
psql -h 172.25.64.1 -p 5432 -U postgres -d websitebuilder -c "\dt" | head -20
```

### Verificar usuarios:
```bash
export PGPASSWORD='123456'
psql -h 172.25.64.1 -p 5432 -U postgres -d websitebuilder -c "SELECT \"Email\", \"FirstName\" FROM \"Users\";"
```

### Verificar API health:
```bash
curl http://172.25.64.1:5000/health
```

---

## ⚠️ PROBLEMAS COMUNES Y SOLUCIONES

### Problema: "Connection refused"
**Solución**: PostgreSQL no está en localhost, usar IP 172.25.64.1

### Problema: "no pg_hba.conf entry"
**Solución**: Ya está configurado para 172.25.64.0/24

### Problema: "password authentication failed"
**Solución**: Password es '123456' (sin comillas)

### Problema: La aplicación no compila
**Solución**: 
```powershell
Stop-Process -Name "WebsiteBuilderAPI" -Force
dotnet build --no-incremental
```

---

## 📱 CONTACTOS DE EMERGENCIA

- **PostgreSQL local funciona**: ✅ CONFIRMADO 30/08/2025
- **Último login exitoso**: 30/08/2025 con miguelnuez919@yahoo.com
- **IP WSL actual**: 172.25.64.130
- **IP Windows host**: 172.25.64.1

---

## 🎯 CHECKLIST FINAL ANTES DE ENTREGAR

- [ ] PostgreSQL Windows está corriendo
- [ ] Connection string apunta a 172.25.64.1
- [ ] Login funciona con usuario principal
- [ ] API responde en http://172.25.64.1:5000
- [ ] Frontend puede conectar al backend
- [ ] Todas las tablas tienen datos

---

**NOTA IMPORTANTE**: Esta configuración está 100% probada y funcional al 30 de Agosto 2025.
Si necesitas revertir, sigue estos pasos EXACTAMENTE como están documentados.