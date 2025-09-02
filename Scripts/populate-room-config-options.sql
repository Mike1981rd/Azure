-- Script para poblar las opciones de configuración para Things to Know
-- Ejecutar este script para agregar todas las opciones necesarias al catálogo

-- ====================================
-- HOUSE RULES (Reglas de la Casa)
-- ====================================

-- Primero eliminar duplicados si existen
DELETE FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" != 'Mascotas';

-- Insertar opciones de reglas de la casa
INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
VALUES 
-- Opciones básicas de permisos
('house_rules', 'smokingAllowed', 'Se permite fumar', 'Smoking allowed', '🚬', 'emoji', 'permisos', 1, 0, true, false, true),
('house_rules', 'petsAllowed', 'Se permiten mascotas', 'Pets allowed', '🐕', 'emoji', 'permisos', 2, 0, true, false, true),
('house_rules', 'eventsAllowed', 'Se permiten eventos', 'Events allowed', '🎉', 'emoji', 'permisos', 3, 0, true, false, true),
('house_rules', 'partiesAllowed', 'Se permiten fiestas', 'Parties allowed', '🎊', 'emoji', 'permisos', 4, 0, true, false, true),
('house_rules', 'childrenAllowed', 'Se permiten niños', 'Children allowed', '👶', 'emoji', 'permisos', 5, 0, true, false, true),
('house_rules', 'visitorsAllowed', 'Se permiten visitantes', 'Visitors allowed', '👥', 'emoji', 'permisos', 6, 0, true, false, true),
('house_rules', 'loudMusicAllowed', 'Se permite música alta', 'Loud music allowed', '🎵', 'emoji', 'permisos', 7, 0, true, false, true),
('house_rules', 'commercialPhotoAllowed', 'Se permiten fotos comerciales', 'Commercial photos allowed', '📸', 'emoji', 'permisos', 8, 0, true, false, true),

-- Horarios (estos son campos de texto, pero los agregamos como referencia)
('house_rules', 'checkInTime', 'Hora de check-in', 'Check-in time', '🕐', 'emoji', 'horarios', 9, 0, true, false, true),
('house_rules', 'checkOutTime', 'Hora de check-out', 'Check-out time', '🕑', 'emoji', 'horarios', 10, 0, true, false, true),
('house_rules', 'quietHours', 'Horario de silencio', 'Quiet hours', '🤫', 'emoji', 'horarios', 11, 0, true, false, true),

-- Restricciones adicionales
('house_rules', 'maxGuests', 'Máximo de huéspedes', 'Maximum guests', '👨‍👩‍👧‍👦', 'emoji', 'restricciones', 12, 0, true, false, true),
('house_rules', 'minimumAge', 'Edad mínima', 'Minimum age', '🔞', 'emoji', 'restricciones', 13, 0, true, false, true),
('house_rules', 'additionalRules', 'Reglas adicionales', 'Additional rules', '📋', 'emoji', 'restricciones', 14, 0, true, false, true)
ON CONFLICT ("Type", "Value") DO UPDATE SET
  "LabelEs" = EXCLUDED."LabelEs",
  "LabelEn" = EXCLUDED."LabelEn",
  "Icon" = EXCLUDED."Icon",
  "IconType" = EXCLUDED."IconType",
  "Category" = EXCLUDED."Category",
  "IsActive" = true;

-- ====================================
-- SAFETY & PROPERTY (Seguridad y Propiedad)
-- ====================================

-- Primero eliminar duplicados si existen
DELETE FROM "ConfigOptions" WHERE "Type" = 'safety_property' AND "Value" != 'Smoked Alarm';

-- Insertar opciones de seguridad y propiedad
INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
VALUES 
-- Detectores y alarmas
('safety_property', 'smokeDetector', 'Detector de humo', 'Smoke detector', '🚨', 'emoji', 'detectores', 1, 0, true, false, true),
('safety_property', 'carbonMonoxideDetector', 'Detector de monóxido de carbono', 'Carbon monoxide detector', '⚠️', 'emoji', 'detectores', 2, 0, true, false, true),
('safety_property', 'fireExtinguisher', 'Extintor', 'Fire extinguisher', '🧯', 'emoji', 'detectores', 3, 0, true, false, true),
('safety_property', 'firstAidKit', 'Botiquín de primeros auxilios', 'First aid kit', '🏥', 'emoji', 'detectores', 4, 0, true, false, true),

-- Seguridad
('safety_property', 'securityCameras', 'Cámaras de seguridad', 'Security cameras', '📹', 'emoji', 'seguridad', 5, 0, true, false, true),
('safety_property', 'alarmSystem', 'Sistema de alarma', 'Alarm system', '🔔', 'emoji', 'seguridad', 6, 0, true, false, true),
('safety_property', 'safe', 'Caja fuerte', 'Safe', '🔒', 'emoji', 'seguridad', 7, 0, true, false, true),
('safety_property', 'lockOnBedroom', 'Cerradura en habitación', 'Lock on bedroom', '🔐', 'emoji', 'seguridad', 8, 0, true, false, true),
('safety_property', 'deadbolt', 'Cerrojo', 'Deadbolt', '🔓', 'emoji', 'seguridad', 9, 0, true, false, true),
('safety_property', 'doorman', 'Portero', 'Doorman', '👮', 'emoji', 'seguridad', 10, 0, true, false, true),

-- Emergencias
('safety_property', 'emergencyExit', 'Salida de emergencia', 'Emergency exit', '🚪', 'emoji', 'emergencias', 11, 0, true, false, true),
('safety_property', 'emergencyPhone', 'Teléfono de emergencia', 'Emergency phone', '📞', 'emoji', 'emergencias', 12, 0, true, false, true),
('safety_property', 'emergencyLighting', 'Iluminación de emergencia', 'Emergency lighting', '💡', 'emoji', 'emergencias', 13, 0, true, false, true),

-- Características de la propiedad
('safety_property', 'pool', 'Piscina', 'Pool', '🏊', 'emoji', 'propiedad', 14, 0, true, false, true),
('safety_property', 'hotTub', 'Jacuzzi', 'Hot tub', '💆', 'emoji', 'propiedad', 15, 0, true, false, true),
('safety_property', 'balcony', 'Balcón', 'Balcony', '🏠', 'emoji', 'propiedad', 16, 0, true, false, true),
('safety_property', 'heights', 'Alturas peligrosas', 'Dangerous heights', '⛰️', 'emoji', 'propiedad', 17, 0, true, false, true),
('safety_property', 'stairs', 'Escaleras', 'Stairs', '🪜', 'emoji', 'propiedad', 18, 0, true, false, true),
('safety_property', 'unfencedPool', 'Piscina sin cerca', 'Unfenced pool', '🏊‍♂️', 'emoji', 'propiedad', 19, 0, true, false, true),
('safety_property', 'nearbyWater', 'Agua cercana', 'Nearby water', '🌊', 'emoji', 'propiedad', 20, 0, true, false, true)
ON CONFLICT ("Type", "Value") DO UPDATE SET
  "LabelEs" = EXCLUDED."LabelEs",
  "LabelEn" = EXCLUDED."LabelEn",
  "Icon" = EXCLUDED."Icon",
  "IconType" = EXCLUDED."IconType",
  "Category" = EXCLUDED."Category",
  "IsActive" = true;

-- ====================================
-- CANCELLATION POLICIES (Políticas de Cancelación)
-- ====================================

-- Primero eliminar duplicados si existen
DELETE FROM "ConfigOptions" WHERE "Type" = 'cancellation_policies' AND "Value" NOT IN ('Aviso', 'Refund');

-- Insertar opciones de políticas de cancelación
INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
VALUES 
-- Políticas de cancelación gratuita
('cancellation_policies', 'freeCancel24h', 'Cancelación gratuita 24h antes', 'Free cancellation 24h before', '✅', 'emoji', 'gratuita', 1, 0, true, false, true),
('cancellation_policies', 'freeCancel48h', 'Cancelación gratuita 48h antes', 'Free cancellation 48h before', '✅', 'emoji', 'gratuita', 2, 0, true, false, true),
('cancellation_policies', 'freeCancel72h', 'Cancelación gratuita 72h antes', 'Free cancellation 72h before', '✅', 'emoji', 'gratuita', 3, 0, true, false, true),
('cancellation_policies', 'freeCancel7days', 'Cancelación gratuita 7 días antes', 'Free cancellation 7 days before', '✅', 'emoji', 'gratuita', 4, 0, true, false, true),
('cancellation_policies', 'freeCancel14days', 'Cancelación gratuita 14 días antes', 'Free cancellation 14 days before', '✅', 'emoji', 'gratuita', 5, 0, true, false, true),
('cancellation_policies', 'freeCancel30days', 'Cancelación gratuita 30 días antes', 'Free cancellation 30 days before', '✅', 'emoji', 'gratuita', 6, 0, true, false, true),

-- Políticas de reembolso
('cancellation_policies', 'fullRefund', 'Reembolso completo', 'Full refund', '💰', 'emoji', 'reembolso', 7, 0, true, false, true),
('cancellation_policies', 'partialRefund', 'Reembolso parcial disponible', 'Partial refund available', '💸', 'emoji', 'reembolso', 8, 0, true, false, true),
('cancellation_policies', 'noRefund', 'Sin reembolso', 'No refund', '❌', 'emoji', 'reembolso', 9, 0, true, false, true),
('cancellation_policies', 'refund50', 'Reembolso del 50%', '50% refund', '💵', 'emoji', 'reembolso', 10, 0, true, false, true),
('cancellation_policies', 'refund75', 'Reembolso del 75%', '75% refund', '💵', 'emoji', 'reembolso', 11, 0, true, false, true),
('cancellation_policies', 'cleaningFeeRefund', 'Reembolso de tarifa de limpieza', 'Cleaning fee refund', '🧹', 'emoji', 'reembolso', 12, 0, true, false, true),

-- Otras opciones
('cancellation_policies', 'creditFuture', 'Crédito para futuras reservas', 'Credit for future bookings', '🎫', 'emoji', 'otras', 13, 0, true, false, true),
('cancellation_policies', 'modificationAllowed', 'Se permite modificación de fechas', 'Date modification allowed', '📅', 'emoji', 'otras', 14, 0, true, false, true),
('cancellation_policies', 'transferable', 'Reserva transferible', 'Transferable booking', '🔄', 'emoji', 'otras', 15, 0, true, false, true),
('cancellation_policies', 'insurance', 'Seguro de cancelación disponible', 'Cancellation insurance available', '🛡️', 'emoji', 'otras', 16, 0, true, false, true),
('cancellation_policies', 'weatherException', 'Excepción por clima', 'Weather exception', '🌧️', 'emoji', 'otras', 17, 0, true, false, true),
('cancellation_policies', 'emergencyException', 'Excepción por emergencia', 'Emergency exception', '🚨', 'emoji', 'otras', 18, 0, true, false, true),

-- Tipos de política
('cancellation_policies', 'flexible', 'Política flexible', 'Flexible policy', '🟢', 'emoji', 'tipos', 19, 0, true, false, true),
('cancellation_policies', 'moderate', 'Política moderada', 'Moderate policy', '🟡', 'emoji', 'tipos', 20, 0, true, false, true),
('cancellation_policies', 'strict', 'Política estricta', 'Strict policy', '🟠', 'emoji', 'tipos', 21, 0, true, false, true),
('cancellation_policies', 'superStrict', 'Política súper estricta', 'Super strict policy', '🔴', 'emoji', 'tipos', 22, 0, true, false, true),
('cancellation_policies', 'nonRefundable', 'No reembolsable', 'Non-refundable', '⛔', 'emoji', 'tipos', 23, 0, true, false, true)
ON CONFLICT ("Type", "Value") DO UPDATE SET
  "LabelEs" = EXCLUDED."LabelEs",
  "LabelEn" = EXCLUDED."LabelEn",
  "Icon" = EXCLUDED."Icon",
  "IconType" = EXCLUDED."IconType",
  "Category" = EXCLUDED."Category",
  "IsActive" = true;

-- Mensaje de confirmación
SELECT 
  "Type",
  COUNT(*) as total_options
FROM "ConfigOptions"
WHERE "Type" IN ('house_rules', 'safety_property', 'cancellation_policies')
GROUP BY "Type"
ORDER BY "Type";