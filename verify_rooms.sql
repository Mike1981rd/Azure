-- Script simplificado para verificar datos de habitaciones
-- Ejecutar desde pgAdmin o psql

-- Ver las últimas 5 habitaciones actualizadas con los nuevos campos
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
    "UpdatedAt"
FROM "Rooms"
ORDER BY "UpdatedAt" DESC
LIMIT 5;

-- Ver campos JSONB
SELECT 
    "Id",
    "Name",
    "SleepingArrangements",
    "HouseRules",
    "CancellationPolicy",
    "CheckInExperience",
    "Accessibility",
    "StandoutAmenities",
    "SafetyAmenities"
FROM "Rooms"
ORDER BY "UpdatedAt" DESC
LIMIT 3;

-- Estadísticas
SELECT 
    COUNT(*) as "Total_Rooms",
    COUNT("HostId") as "Rooms_With_Host",
    COUNT("StreetAddress") as "Rooms_With_Address",
    COUNT(CASE WHEN "SleepingArrangements" IS NOT NULL AND "SleepingArrangements"::text != '{}' THEN 1 END) as "With_SleepingArrangements",
    COUNT(CASE WHEN "HouseRules" IS NOT NULL AND "HouseRules"::text != '{}' THEN 1 END) as "With_HouseRules"
FROM "Rooms";