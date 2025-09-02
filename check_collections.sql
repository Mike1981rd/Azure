-- Check if Collections tables exist
SELECT tablename 
FROM pg_tables 
WHERE schemaname = 'public' 
AND (tablename = 'Collections' OR tablename = 'CollectionProducts');

-- Check Collections table structure if exists
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' 
AND table_name = 'Collections'
ORDER BY ordinal_position;