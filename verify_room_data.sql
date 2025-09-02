-- Script para verificar los datos de las habitaciones en la base de datos
-- Ejecutar con: PGPASSWORD=postgres psql -h localhost -U postgres -d websitebuilder -f verify_room_data.sql

-- 1. Verificar la estructura de la tabla Rooms
\echo '========================================='
\echo '1. ESTRUCTURA DE LA TABLA ROOMS'
\echo '========================================='
\d "Rooms";

-- 2. Listar todas las habitaciones con los nuevos campos
\echo ''
\echo '========================================='
\echo '2. DATOS DE HABITACIONES (CAMPOS BÁSICOS + UBICACIÓN)'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    "RoomCode",
    "StreetAddress",
    "City",
    "State",
    "Country",
    "PostalCode",
    "Neighborhood",
    "Latitude",
    "Longitude",
    "HostId",
    "CreatedAt",
    "UpdatedAt"
FROM "Rooms"
ORDER BY "Id" DESC
LIMIT 5;

-- 3. Verificar campos JSONB
\echo ''
\echo '========================================='
\echo '3. CAMPOS JSONB DE HABITACIONES'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    CASE 
        WHEN "SleepingArrangements" IS NULL THEN 'NULL'
        WHEN "SleepingArrangements"::text = '{}' THEN 'EMPTY OBJECT'
        ELSE substring("SleepingArrangements"::text, 1, 50) || '...'
    END as "SleepingArrangements_Preview",
    CASE 
        WHEN "HouseRules" IS NULL THEN 'NULL'
        WHEN "HouseRules"::text = '{}' THEN 'EMPTY OBJECT'
        ELSE substring("HouseRules"::text, 1, 50) || '...'
    END as "HouseRules_Preview",
    CASE 
        WHEN "CancellationPolicy" IS NULL THEN 'NULL'
        WHEN "CancellationPolicy"::text = '{}' THEN 'EMPTY OBJECT'
        ELSE substring("CancellationPolicy"::text, 1, 50) || '...'
    END as "CancellationPolicy_Preview"
FROM "Rooms"
ORDER BY "Id" DESC
LIMIT 5;

-- 4. Ver detalles completos de la habitación más reciente
\echo ''
\echo '========================================='
\echo '4. DETALLES COMPLETOS DE LA HABITACIÓN MÁS RECIENTE'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    "Description",
    "RoomCode",
    "BasePrice",
    "MaxOccupancy",
    "StreetAddress",
    "City",
    "State",
    "Country",
    "PostalCode",
    "Neighborhood",
    "Latitude",
    "Longitude",
    "HostId"
FROM "Rooms"
ORDER BY "UpdatedAt" DESC
LIMIT 1;

-- 5. Ver contenido JSONB de la habitación más reciente
\echo ''
\echo '========================================='
\echo '5. CONTENIDO JSONB DE LA HABITACIÓN MÁS RECIENTE'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    jsonb_pretty("SleepingArrangements") as "SleepingArrangements",
    jsonb_pretty("HouseRules") as "HouseRules",
    jsonb_pretty("CancellationPolicy") as "CancellationPolicy"
FROM "Rooms"
ORDER BY "UpdatedAt" DESC
LIMIT 1;

-- 6. Verificar campos adicionales JSONB
\echo ''
\echo '========================================='
\echo '6. CAMPOS JSONB ADICIONALES'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    CASE 
        WHEN "CheckInExperience" IS NULL THEN 'NULL'
        ELSE jsonb_pretty("CheckInExperience")
    END as "CheckInExperience",
    CASE 
        WHEN "Accessibility" IS NULL THEN 'NULL'
        ELSE jsonb_pretty("Accessibility")
    END as "Accessibility",
    CASE 
        WHEN "StandoutAmenities" IS NULL THEN 'NULL'
        ELSE jsonb_pretty("StandoutAmenities")
    END as "StandoutAmenities"
FROM "Rooms"
ORDER BY "UpdatedAt" DESC
LIMIT 1;

-- 7. Verificar si hay Hosts en la base de datos
\echo ''
\echo '========================================='
\echo '7. HOSTS DISPONIBLES'
\echo '========================================='
SELECT 
    "Id",
    "Name",
    "Email",
    "IsSuperHost",
    "ResponseRate",
    "CreatedAt"
FROM "Hosts"
ORDER BY "Id"
LIMIT 10;

-- 8. Verificar relación Room-Host
\echo ''
\echo '========================================='
\echo '8. HABITACIONES CON HOST ASIGNADO'
\echo '========================================='
SELECT 
    r."Id" as "RoomId",
    r."Name" as "RoomName",
    r."HostId",
    h."Name" as "HostName",
    h."Email" as "HostEmail"
FROM "Rooms" r
LEFT JOIN "Hosts" h ON r."HostId" = h."Id"
WHERE r."HostId" IS NOT NULL
ORDER BY r."Id" DESC
LIMIT 5;

-- 9. Estadísticas generales
\echo ''
\echo '========================================='
\echo '9. ESTADÍSTICAS GENERALES'
\echo '========================================='
SELECT 
    COUNT(*) as "Total_Rooms",
    COUNT("HostId") as "Rooms_With_Host",
    COUNT("StreetAddress") as "Rooms_With_Address",
    COUNT("Latitude") as "Rooms_With_Coordinates",
    COUNT("SleepingArrangements") as "Rooms_With_SleepingArrangements",
    COUNT("HouseRules") as "Rooms_With_HouseRules",
    COUNT("CancellationPolicy") as "Rooms_With_CancellationPolicy"
FROM "Rooms";

-- 10. Verificar la última actualización por campo
\echo ''
\echo '========================================='
\echo '10. ÚLTIMA ACTUALIZACIÓN POR CAMPO'
\echo '========================================='
SELECT 
    MAX(CASE WHEN "StreetAddress" IS NOT NULL AND "StreetAddress" != '' THEN "UpdatedAt" END) as "Last_Address_Update",
    MAX(CASE WHEN "HostId" IS NOT NULL THEN "UpdatedAt" END) as "Last_Host_Assignment",
    MAX(CASE WHEN "SleepingArrangements" IS NOT NULL THEN "UpdatedAt" END) as "Last_SleepingArrangements_Update",
    MAX(CASE WHEN "HouseRules" IS NOT NULL THEN "UpdatedAt" END) as "Last_HouseRules_Update"
FROM "Rooms";