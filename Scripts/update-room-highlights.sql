-- Script para actualizar las habitaciones existentes con Highlights por defecto
-- Ejecutar este script después de aplicar la migración AddHighlightsToRooms

-- Actualizar todas las habitaciones activas con highlights por defecto
UPDATE "Rooms" 
SET "Highlights" = '[
  {
    "icon": "map-pin",
    "title": "Great location",
    "description": "90% of recent guests gave the location a 5-star rating",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "icon": "key",
    "title": "Self check-in",
    "description": "Check yourself in with the keypad",
    "displayOrder": 2,
    "isActive": true
  },
  {
    "icon": "wifi",
    "title": "Fast WiFi",
    "description": "Perfect for remote work with 100 Mbps",
    "displayOrder": 3,
    "isActive": true
  },
  {
    "icon": "car",
    "title": "Free parking",
    "description": "Free parking space on premises",
    "displayOrder": 4,
    "isActive": false
  },
  {
    "icon": "sparkles",
    "title": "Enhanced cleaning",
    "description": "This host committed to enhanced cleaning process",
    "displayOrder": 5,
    "isActive": false
  }
]'::jsonb
WHERE "Highlights" IS NULL 
   OR "Highlights" = '[]'::jsonb
   OR "Highlights" = 'null'::jsonb;

-- Verificar el resultado
SELECT "Id", "Name", jsonb_array_length("Highlights") as highlights_count
FROM "Rooms"
WHERE "IsActive" = true
LIMIT 5;