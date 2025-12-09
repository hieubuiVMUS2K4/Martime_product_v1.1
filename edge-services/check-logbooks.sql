-- Check all logbook tables existence and structure
-- Created: 2025-12-04

-- 1. Check if logbook tables exist
SELECT 
    tablename,
    schemaname
FROM pg_tables 
WHERE schemaname = 'public' 
    AND tablename IN (
        'deck_log_books',
        'engine_log_books',
        'garbage_record_books',
        'ballast_water_record_books'
    )
ORDER BY tablename;

-- 2. Check deck_log_books structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
    AND table_name = 'deck_log_books'
ORDER BY ordinal_position;

-- 3. Check engine_log_books structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
    AND table_name = 'engine_log_books'
ORDER BY ordinal_position;

-- 4. Check garbage_record_books structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
    AND table_name = 'garbage_record_books'
ORDER BY ordinal_position;

-- 5. Check ballast_water_record_books structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_schema = 'public' 
    AND table_name = 'ballast_water_record_books'
ORDER BY ordinal_position;

-- 6. Check indexes on logbook tables
SELECT
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
    AND tablename IN (
        'deck_log_books',
        'engine_log_books',
        'garbage_record_books',
        'ballast_water_record_books'
    )
ORDER BY tablename, indexname;

-- 7. Check for any constraints conflicts
SELECT
    tc.table_schema,
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name
FROM information_schema.table_constraints tc
LEFT JOIN information_schema.key_column_usage kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
WHERE tc.table_schema = 'public'
    AND tc.table_name IN (
        'deck_log_books',
        'engine_log_books',
        'garbage_record_books',
        'ballast_water_record_books'
    )
ORDER BY tc.table_name, tc.constraint_type, tc.constraint_name;

-- 8. Check row counts in each logbook table
SELECT 'deck_log_books' as table_name, COUNT(*) as row_count FROM public.deck_log_books
UNION ALL
SELECT 'engine_log_books', COUNT(*) FROM public.engine_log_books
UNION ALL
SELECT 'garbage_record_books', COUNT(*) FROM public.garbage_record_books
UNION ALL
SELECT 'ballast_water_record_books', COUNT(*) FROM public.ballast_water_record_books;

-- 9. Check for column name conflicts with existing tables
SELECT 
    t1.table_name as table1,
    t2.table_name as table2,
    t1.column_name,
    t1.data_type as type1,
    t2.data_type as type2
FROM information_schema.columns t1
INNER JOIN information_schema.columns t2 
    ON t1.column_name = t2.column_name 
    AND t1.table_name != t2.table_name
WHERE t1.table_schema = 'public' 
    AND t2.table_schema = 'public'
    AND (
        t1.table_name IN ('deck_log_books', 'engine_log_books', 'garbage_record_books', 'ballast_water_record_books')
        OR t2.table_name IN ('deck_log_books', 'engine_log_books', 'garbage_record_books', 'ballast_water_record_books')
    )
    AND t1.data_type != t2.data_type
ORDER BY t1.column_name, t1.table_name;

-- 10. Check migration history
SELECT 
    migration_id,
    product_version
FROM public.__efmigrationshistory
WHERE migration_id LIKE '%Logbook%'
ORDER BY migration_id DESC;
