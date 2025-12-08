-- List all tables in SQL Server database
SELECT 
    t.name AS TableName,
    s.name AS SchemaName
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'dbo'
ORDER BY t.name;

-- Check if TaskChecklistItems exists
SELECT 
    CASE WHEN EXISTS (
        SELECT 1 FROM sys.tables WHERE name = 'TaskChecklistItems'
    ) THEN 'TaskChecklistItems table EXISTS'
    ELSE 'TaskChecklistItems table NOT FOUND'
    END AS Result;

-- Get column details for TaskChecklistItems
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('TaskChecklistItems')
ORDER BY c.column_id;
