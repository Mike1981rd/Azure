# Script PowerShell para crear usuario de prueba en PostgreSQL

Write-Host "🔄 Creando usuario de prueba en PostgreSQL..." -ForegroundColor Yellow

# Intentar con psql si está disponible
$psqlPath = "C:\Program Files\PostgreSQL\*\bin\psql.exe"
$psql = Get-ChildItem -Path $psqlPath -ErrorAction SilentlyContinue | Select-Object -First 1

if ($psql) {
    Write-Host "✅ Encontrado psql en: $($psql.FullName)" -ForegroundColor Green
    
    $env:PGPASSWORD = "123456"
    
    # Crear usuario de prueba
    $sql = @"
-- Verificar conexión
SELECT current_database() as db, current_user as usuario;

-- Crear o actualizar usuario de prueba
DO `$`$
BEGIN
    -- Verificar si el usuario existe
    IF EXISTS (SELECT 1 FROM users WHERE email = 'test@websitebuilder.com') THEN
        -- Actualizar password (BCrypt hash de '123456')
        UPDATE users 
        SET password_hash = '`$2a`$11`$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW',
            updated_at = NOW()
        WHERE email = 'test@websitebuilder.com';
        
        RAISE NOTICE 'Usuario actualizado: test@websitebuilder.com';
    ELSE
        -- Crear nuevo usuario
        INSERT INTO users (email, password_hash, first_name, last_name, is_active, email_confirmed, created_at, updated_at)
        VALUES (
            'test@websitebuilder.com',
            '`$2a`$11`$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW',
            'Test',
            'User',
            true,
            true,
            NOW(),
            NOW()
        );
        
        RAISE NOTICE 'Usuario creado: test@websitebuilder.com';
    END IF;
END`$`$;

-- Mostrar usuarios existentes
SELECT id, email, first_name, last_name 
FROM users 
ORDER BY id DESC 
LIMIT 5;
"@
    
    # Ejecutar SQL
    $sql | & $psql.FullName -h localhost -U postgres -d websitebuilder
    
    Write-Host "`n✅ Usuario de prueba listo:" -ForegroundColor Green
    Write-Host "   Email: test@websitebuilder.com" -ForegroundColor Cyan
    Write-Host "   Password: 123456" -ForegroundColor Cyan
    
} else {
    Write-Host "❌ No se encontró psql.exe" -ForegroundColor Red
    Write-Host "Instalando módulo PostgreSQL para PowerShell..." -ForegroundColor Yellow
    
    # Alternativa: Usar .NET directamente
    Write-Host "`nIntentando con .NET..." -ForegroundColor Yellow
    
    $code = @'
Add-Type -AssemblyName System.Data
    
$connString = "Host=localhost;Port=5432;Database=websitebuilder;Username=postgres;Password=123456"
$conn = New-Object Npgsql.NpgsqlConnection($connString)

try {
    $conn.Open()
    Write-Host "✅ Conexión exitosa a PostgreSQL" -ForegroundColor Green
    
    # Verificar si el usuario existe
    $checkCmd = $conn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM users WHERE email = 'test@websitebuilder.com'"
    $exists = $checkCmd.ExecuteScalar()
    
    if ($exists -gt 0) {
        # Actualizar password
        $updateCmd = $conn.CreateCommand()
        $updateCmd.CommandText = "UPDATE users SET password_hash = '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW', updated_at = NOW() WHERE email = 'test@websitebuilder.com'"
        $updateCmd.ExecuteNonQuery()
        Write-Host "✅ Password actualizado para test@websitebuilder.com" -ForegroundColor Green
    } else {
        # Crear usuario
        $insertCmd = $conn.CreateCommand()
        $insertCmd.CommandText = @"
INSERT INTO users (email, password_hash, first_name, last_name, is_active, email_confirmed, created_at, updated_at)
VALUES ('test@websitebuilder.com', '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW', 'Test', 'User', true, true, NOW(), NOW())
"@
        $insertCmd.ExecuteNonQuery()
        Write-Host "✅ Usuario creado: test@websitebuilder.com" -ForegroundColor Green
    }
    
    Write-Host "`n✅ Usuario de prueba listo:" -ForegroundColor Green
    Write-Host "   Email: test@websitebuilder.com" -ForegroundColor Cyan
    Write-Host "   Password: 123456" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
} finally {
    $conn.Close()
}
'@
    
    # Verificar si Npgsql.dll existe en el proyecto
    $npgsqlPath = "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI\bin\Debug\net8.0\Npgsql.dll"
    if (Test-Path $npgsqlPath) {
        Add-Type -Path $npgsqlPath
        Invoke-Expression $code
    } else {
        Write-Host "❌ No se encontró Npgsql.dll" -ForegroundColor Red
        Write-Host "Por favor, ejecuta este SQL manualmente en pgAdmin o cualquier cliente PostgreSQL:" -ForegroundColor Yellow
        Write-Host @"

-- Crear o actualizar usuario de prueba
UPDATE users 
SET password_hash = '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW'
WHERE email = 'test@websitebuilder.com';

-- Si no existe, crearlo:
INSERT INTO users (email, password_hash, first_name, last_name, is_active, email_confirmed)
VALUES ('test@websitebuilder.com', '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW', 'Test', 'User', true, true);

"@ -ForegroundColor Cyan
    }
}