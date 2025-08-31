-- Script 1: Poblar opciones de HOUSE RULES
-- Ejecutar: PGPASSWORD=postgres psql -h 172.25.64.1 -U postgres -d websitebuilder -f "Scripts/populate-house-rules.sql"

-- Primero verificar qué opciones ya existen
SELECT "Value", "LabelEs" FROM "ConfigOptions" WHERE "Type" = 'house_rules';

-- Insertar nuevas opciones (ignorará si ya existen por el valor)
INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
SELECT 'house_rules', 'smokingAllowed', 'Se permite fumar', 'Smoking allowed', '🚬', 'emoji', 'permisos', 1, 0, true, false, true
WHERE NOT EXISTS (SELECT 1 FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" = 'smokingAllowed');

INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
SELECT 'house_rules', 'petsAllowed', 'Se permiten mascotas', 'Pets allowed', '🐕', 'emoji', 'permisos', 2, 0, true, false, true
WHERE NOT EXISTS (SELECT 1 FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" = 'petsAllowed');

INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
SELECT 'house_rules', 'eventsAllowed', 'Se permiten eventos', 'Events allowed', '🎉', 'emoji', 'permisos', 3, 0, true, false, true
WHERE NOT EXISTS (SELECT 1 FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" = 'eventsAllowed');

INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
SELECT 'house_rules', 'partiesAllowed', 'Se permiten fiestas', 'Parties allowed', '🎊', 'emoji', 'permisos', 4, 0, true, false, true
WHERE NOT EXISTS (SELECT 1 FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" = 'partiesAllowed');

INSERT INTO "ConfigOptions" ("Type", "Value", "LabelEs", "LabelEn", "Icon", "IconType", "Category", "SortOrder", "UsageCount", "IsActive", "IsCustom", "IsDefault")
SELECT 'house_rules', 'childrenAllowed', 'Se permiten niños', 'Children allowed', '👶', 'emoji', 'permisos', 5, 0, true, false, true
WHERE NOT EXISTS (SELECT 1 FROM "ConfigOptions" WHERE "Type" = 'house_rules' AND "Value" = 'childrenAllowed');

-- Verificar que se insertaron
SELECT "Value", "LabelEs", "Icon" 
FROM "ConfigOptions" 
WHERE "Type" = 'house_rules'
ORDER BY "SortOrder";