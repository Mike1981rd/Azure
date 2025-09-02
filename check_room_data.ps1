# Script de PowerShell para verificar los datos de habitaciones en PostgreSQL
# Ejecutar desde PowerShell con: .\check_room_data.ps1

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "VERIFICACIÓN DE DATOS DE HABITACIONES" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Configuración de conexión
$env:PGPASSWORD = "postgres"
$pgHost = "localhost"
$pgUser = "postgres"
$pgDatabase = "websitebuilder"

# Verificar si psql está disponible
$psqlVersion = psql --version 2>$null
if ($psqlVersion) {
    Write-Host "✓ PostgreSQL cliente encontrado: $psqlVersion" -ForegroundColor Green
} else {
    Write-Host "✗ PostgreSQL cliente no encontrado. Por favor instala PostgreSQL." -ForegroundColor Red
    Write-Host "  Puedes descargarlo de: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
    exit 1
}

# Función para ejecutar consultas SQL
function Execute-Query {
    param(
        [string]$Query,
        [string]$Description
    )
    
    Write-Host ""
    Write-Host "► $Description" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────" -ForegroundColor DarkGray
    
    $result = psql -h $pgHost -U $pgUser -d $pgDatabase -c $Query 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host $result
    } else {
        Write-Host "Error ejecutando consulta: $result" -ForegroundColor Red
    }
}

# 1. Verificar conexión
Write-Host "Verificando conexión a la base de datos..." -ForegroundColor Cyan
$testConnection = psql -h $pgHost -U $pgUser -d $pgDatabase -c "SELECT 1" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ No se pudo conectar a la base de datos" -ForegroundColor Red
    Write-Host "  Error: $testConnection" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Conexión exitosa" -ForegroundColor Green

# 2. Verificar estructura de la tabla
Execute-Query -Query "\d ""Rooms""" -Description "Estructura de la tabla Rooms"

# 3. Verificar últimas habitaciones creadas/actualizadas
Execute-Query -Query @"
SELECT 
    ""Id"",
    ""Name"",
    ""RoomCode"",
    ""StreetAddress"",
    ""City"",
    ""HostId"",
    ""UpdatedAt""
FROM ""Rooms""
ORDER BY ""UpdatedAt"" DESC
LIMIT 3
"@ -Description "Últimas 3 habitaciones actualizadas"

# 4. Ver detalles de ubicación
Execute-Query -Query @"
SELECT 
    ""Id"",
    ""Name"",
    ""StreetAddress"",
    ""City"",
    ""State"",
    ""Country"",
    ""PostalCode"",
    ""Neighborhood"",
    ""Latitude"",
    ""Longitude""
FROM ""Rooms""
WHERE ""StreetAddress"" IS NOT NULL 
   OR ""City"" IS NOT NULL
ORDER BY ""UpdatedAt"" DESC
LIMIT 3
"@ -Description "Habitaciones con datos de ubicación"

# 5. Ver campos JSONB
Execute-Query -Query @"
SELECT 
    ""Id"",
    ""Name"",
    CASE 
        WHEN ""SleepingArrangements"" IS NULL THEN 'NULL'
        WHEN ""SleepingArrangements""::text = '{}' THEN 'VACÍO'
        ELSE 'CON DATOS'
    END as ""SleepingArrangements"",
    CASE 
        WHEN ""HouseRules"" IS NULL THEN 'NULL'
        WHEN ""HouseRules""::text = '{}' THEN 'VACÍO'
        ELSE 'CON DATOS'
    END as ""HouseRules"",
    CASE 
        WHEN ""CancellationPolicy"" IS NULL THEN 'NULL'
        WHEN ""CancellationPolicy""::text = '{}' THEN 'VACÍO'
        ELSE 'CON DATOS'
    END as ""CancellationPolicy""
FROM ""Rooms""
ORDER BY ""UpdatedAt"" DESC
LIMIT 3
"@ -Description "Estado de campos JSONB"

# 6. Ver contenido detallado de JSONB de la última habitación
Execute-Query -Query @"
SELECT 
    ""Name"",
    jsonb_pretty(""SleepingArrangements"") as ""SleepingArrangements""
FROM ""Rooms""
WHERE ""SleepingArrangements"" IS NOT NULL 
  AND ""SleepingArrangements""::text != '{}'
ORDER BY ""UpdatedAt"" DESC
LIMIT 1
"@ -Description "Contenido de SleepingArrangements más reciente"

Execute-Query -Query @"
SELECT 
    ""Name"",
    jsonb_pretty(""HouseRules"") as ""HouseRules""
FROM ""Rooms""
WHERE ""HouseRules"" IS NOT NULL 
  AND ""HouseRules""::text != '{}'
ORDER BY ""UpdatedAt"" DESC
LIMIT 1
"@ -Description "Contenido de HouseRules más reciente"

# 7. Estadísticas generales
Execute-Query -Query @"
SELECT 
    COUNT(*) as ""Total_Habitaciones"",
    COUNT(""HostId"") as ""Con_Host"",
    COUNT(""StreetAddress"") as ""Con_Dirección"",
    COUNT(CASE WHEN ""SleepingArrangements"" IS NOT NULL AND ""SleepingArrangements""::text != '{}' THEN 1 END) as ""Con_SleepingArrangements"",
    COUNT(CASE WHEN ""HouseRules"" IS NOT NULL AND ""HouseRules""::text != '{}' THEN 1 END) as ""Con_HouseRules""
FROM ""Rooms""
"@ -Description "Estadísticas generales"

# 8. Verificar Hosts disponibles
Execute-Query -Query @"
SELECT COUNT(*) as ""Total_Hosts"" FROM ""Hosts""
"@ -Description "Total de Hosts en el sistema"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "VERIFICACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si los campos nuevos aparecen como NULL o vacíos, intenta:" -ForegroundColor Yellow
Write-Host "1. Editar una habitación y guardar con datos en los nuevos campos" -ForegroundColor White
Write-Host "2. Verificar que el backend esté ejecutándose correctamente" -ForegroundColor White
Write-Host "3. Revisar los logs del backend para ver si hay errores" -ForegroundColor White
Write-Host ""