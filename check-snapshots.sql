-- Verificar si existen snapshots en la tabla
SELECT 
    COUNT(*) as total_snapshots,
    COUNT(DISTINCT "PageId") as unique_pages,
    COUNT(DISTINCT "CompanyId") as companies_with_snapshots,
    MAX("Version") as max_version,
    MIN("PublishedAt") as first_snapshot,
    MAX("PublishedAt") as last_snapshot
FROM "PublishedSnapshots";

-- Ver los últimos 5 snapshots generados
SELECT 
    "Id",
    "CompanyId",
    "PageId",
    "PageSlug",
    "PageType",
    "Version",
    "IsStale",
    "PublishedAt",
    LENGTH("SnapshotData") as data_size
FROM "PublishedSnapshots"
ORDER BY "PublishedAt" DESC
LIMIT 5;

-- Verificar snapshots por compañía
SELECT 
    "CompanyId",
    COUNT(*) as snapshot_count,
    COUNT(DISTINCT "PageId") as page_count,
    MAX("PublishedAt") as last_update
FROM "PublishedSnapshots"
WHERE "IsStale" = false
GROUP BY "CompanyId";

-- Verificar si hay snapshots para compañía 1
SELECT 
    ps."PageId",
    ps."PageSlug",
    ps."PageType",
    ps."Version",
    ps."PublishedAt",
    wp."Name" as page_name,
    wp."IsPublished"
FROM "PublishedSnapshots" ps
LEFT JOIN "WebsitePages" wp ON ps."PageId" = wp."Id"
WHERE ps."CompanyId" = 1
ORDER BY ps."PublishedAt" DESC;