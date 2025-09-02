-- Script para generar slugs para habitaciones existentes
-- Actualiza las habitaciones que no tienen slug

-- Función para generar slug desde el nombre
CREATE OR REPLACE FUNCTION generate_slug(input_text TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN LOWER(
        TRIM(
            REGEXP_REPLACE(
                REGEXP_REPLACE(
                    REGEXP_REPLACE(
                        REGEXP_REPLACE(
                            REGEXP_REPLACE(
                                REGEXP_REPLACE(
                                    REGEXP_REPLACE(
                                        input_text,
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
                '[^a-z0-9\s-]', '', 'g'
            ),
            '\s+', '-', 'g'
        ),
        '-'
    );
END;
$$ LANGUAGE plpgsql;

-- Actualizar habitaciones sin slug
UPDATE "Rooms" 
SET "Slug" = generate_slug("Name")
WHERE "Slug" IS NULL OR "Slug" = '';

-- Verificar resultados
SELECT "Id", "Name", "Slug" 
FROM "Rooms" 
WHERE "CompanyId" = 1 
ORDER BY "Id";

-- Limpiar función temporal
DROP FUNCTION IF EXISTS generate_slug(TEXT);