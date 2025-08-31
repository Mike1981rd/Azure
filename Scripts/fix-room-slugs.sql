-- Script para arreglar slugs mal formateados
-- Convierte "Suite Presidencial" -> "suite-presidencial"

UPDATE "Rooms" 
SET "Slug" = LOWER(
    TRIM(
        REGEXP_REPLACE(
            REGEXP_REPLACE(
                REGEXP_REPLACE(
                    REGEXP_REPLACE(
                        REGEXP_REPLACE(
                            REGEXP_REPLACE(
                                REGEXP_REPLACE(
                                    REGEXP_REPLACE(
                                        "Name",
                                        '[áàäâ]', 'a', 'g'
                                    ),
                                    '[éèëê]', 'e', 'g'
                                ),
                                '[íìïî]', 'i', 'g'
                            ),
                            '[óòöô]', 'o', 'g'
                        ),
                        '[úùüû]', 'u', 'g'
                    ),
                    'ñ', 'n', 'g'
                ),
                '[^a-zA-Z0-9\s-]', '', 'g'
            ),
            '\s+', '-', 'g'
        ),
        '-'
    )
);

-- Verificar los resultados
SELECT "Id", "Name", "Slug" 
FROM "Rooms" 
ORDER BY "Id";