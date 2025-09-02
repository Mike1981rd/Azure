-- Script para limpiar duplicados de CustomerNotificationPreferences
-- Esto es necesario antes de aplicar la migración que crea el índice único

-- Primero, eliminar TODOS los registros existentes
-- (Los recrearemos con el formato correcto después)
DELETE FROM "CustomerNotificationPreferences";

-- Mensaje de confirmación
SELECT 'Todos los registros de CustomerNotificationPreferences han sido eliminados' AS resultado;