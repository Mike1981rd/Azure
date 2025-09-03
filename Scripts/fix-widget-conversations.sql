-- Script para corregir conversaciones de widget que no tienen Source configurado correctamente
-- Identificamos conversaciones de widget por CustomerPhone o BusinessPhone = 'widget'

UPDATE "WhatsAppConversations"
SET "Source" = 'widget',
    "UpdatedAt" = CURRENT_TIMESTAMP
WHERE ("CustomerPhone" = 'widget' OR "BusinessPhone" = 'widget')
  AND ("Source" IS NULL OR "Source" != 'widget');

-- Verificar cuántas conversaciones fueron actualizadas
SELECT COUNT(*) as conversations_fixed
FROM "WhatsAppConversations"
WHERE ("CustomerPhone" = 'widget' OR "BusinessPhone" = 'widget')
  AND "Source" = 'widget';