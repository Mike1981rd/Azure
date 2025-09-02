-- Script de prueba para verificar que un producto puede estar en múltiples colecciones

-- 1. Ver todas las colecciones existentes
SELECT id, "Title", "IsActive", "ProductCount" 
FROM "Collections" 
WHERE "CompanyId" = 1;

-- 2. Ver todos los productos existentes
SELECT id, "Name", "IsActive", "Stock" 
FROM "Products" 
WHERE "CompanyId" = 1
LIMIT 5;

-- 3. Ver las relaciones producto-colección existentes
SELECT 
    cp."Id",
    cp."ProductId",
    p."Name" as "ProductName",
    cp."CollectionId",
    c."Title" as "CollectionTitle",
    cp."IsFeatured",
    cp."Position",
    cp."AddedAt"
FROM "CollectionProducts" cp
JOIN "Products" p ON cp."ProductId" = p."Id"
JOIN "Collections" c ON cp."CollectionId" = c."Id"
ORDER BY cp."ProductId", cp."CollectionId";

-- 4. Contar cuántas colecciones tiene cada producto
SELECT 
    p."Id",
    p."Name",
    COUNT(cp."Id") as "NumberOfCollections"
FROM "Products" p
LEFT JOIN "CollectionProducts" cp ON p."Id" = cp."ProductId"
WHERE p."CompanyId" = 1
GROUP BY p."Id", p."Name"
HAVING COUNT(cp."Id") > 0
ORDER BY COUNT(cp."Id") DESC;

-- 5. Ver productos que están en múltiples colecciones (más de una)
SELECT 
    p."Id",
    p."Name",
    STRING_AGG(c."Title", ', ') as "Collections",
    COUNT(cp."Id") as "CollectionCount"
FROM "Products" p
JOIN "CollectionProducts" cp ON p."Id" = cp."ProductId"
JOIN "Collections" c ON cp."CollectionId" = c."Id"
WHERE p."CompanyId" = 1
GROUP BY p."Id", p."Name"
HAVING COUNT(cp."Id") > 1
ORDER BY COUNT(cp."Id") DESC;

-- 6. Insertar un producto de prueba en múltiples colecciones (si no existe)
-- Primero verificar si existe un producto de prueba
DO $$
DECLARE
    test_product_id INT;
    collection1_id INT;
    collection2_id INT;
    collection3_id INT;
BEGIN
    -- Obtener o crear un producto de prueba
    SELECT "Id" INTO test_product_id 
    FROM "Products" 
    WHERE "Name" = 'Producto Multi-Colección Test' 
    AND "CompanyId" = 1
    LIMIT 1;
    
    IF test_product_id IS NULL THEN
        INSERT INTO "Products" (
            "CompanyId", "Name", "Description", "BasePrice", "Stock", 
            "IsActive", "CreatedAt", "HasVariants", "RequiresShipping", 
            "TrackQuantity", "ContinueSellingWhenOutOfStock"
        )
        VALUES (
            1, 'Producto Multi-Colección Test', 'Producto para probar múltiples colecciones', 
            99.99, 100, true, NOW(), false, true, true, false
        )
        RETURNING "Id" INTO test_product_id;
        
        RAISE NOTICE 'Producto de prueba creado con ID: %', test_product_id;
    ELSE
        RAISE NOTICE 'Producto de prueba ya existe con ID: %', test_product_id;
    END IF;
    
    -- Obtener IDs de las primeras 3 colecciones activas
    SELECT "Id" INTO collection1_id FROM "Collections" 
    WHERE "CompanyId" = 1 AND "IsActive" = true 
    ORDER BY "Id" LIMIT 1;
    
    SELECT "Id" INTO collection2_id FROM "Collections" 
    WHERE "CompanyId" = 1 AND "IsActive" = true AND "Id" != collection1_id
    ORDER BY "Id" LIMIT 1;
    
    SELECT "Id" INTO collection3_id FROM "Collections" 
    WHERE "CompanyId" = 1 AND "IsActive" = true 
    AND "Id" NOT IN (collection1_id, collection2_id)
    ORDER BY "Id" LIMIT 1;
    
    -- Limpiar asignaciones existentes para este producto
    DELETE FROM "CollectionProducts" WHERE "ProductId" = test_product_id;
    
    -- Asignar el producto a múltiples colecciones
    IF collection1_id IS NOT NULL THEN
        INSERT INTO "CollectionProducts" ("ProductId", "CollectionId", "Position", "IsFeatured", "AddedAt")
        VALUES (test_product_id, collection1_id, 1, true, NOW());
        RAISE NOTICE 'Producto asignado a colección ID: %', collection1_id;
    END IF;
    
    IF collection2_id IS NOT NULL THEN
        INSERT INTO "CollectionProducts" ("ProductId", "CollectionId", "Position", "IsFeatured", "AddedAt")
        VALUES (test_product_id, collection2_id, 2, false, NOW());
        RAISE NOTICE 'Producto asignado a colección ID: %', collection2_id;
    END IF;
    
    IF collection3_id IS NOT NULL THEN
        INSERT INTO "CollectionProducts" ("ProductId", "CollectionId", "Position", "IsFeatured", "AddedAt")
        VALUES (test_product_id, collection3_id, 3, false, NOW());
        RAISE NOTICE 'Producto asignado a colección ID: %', collection3_id;
    END IF;
END $$;

-- 7. Verificar el resultado de la prueba
SELECT 
    p."Name" as "Producto",
    c."Title" as "Colección",
    cp."IsFeatured" as "Destacado",
    cp."Position" as "Posición"
FROM "CollectionProducts" cp
JOIN "Products" p ON cp."ProductId" = p."Id"
JOIN "Collections" c ON cp."CollectionId" = c."Id"
WHERE p."Name" = 'Producto Multi-Colección Test'
ORDER BY cp."Position";