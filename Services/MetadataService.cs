using System;
using System.Data;

namespace DatabaseManager.Services
{
    public class MetadataService
    {
        private readonly ConnectionService _conn;

        public MetadataService(ConnectionService conn)
        {
            _conn = conn;
        }

        public DataTable GetTables()
        {
            return _conn.ExecuteSelect(@"
                SELECT s.name AS SchemaName, t.name AS ObjectName
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE t.is_ms_shipped = 0
                  AND s.name <> 'sys'
                  AND t.name NOT LIKE 'sys%'
                  AND t.name NOT LIKE 'MS%'
                ORDER BY s.name, t.name;");
        }

        public DataTable GetViews()
        {
            return _conn.ExecuteSelect(@"
                SELECT s.name AS SchemaName, v.name AS ObjectName
                FROM sys.views v
                INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
                WHERE v.is_ms_shipped = 0
                  AND s.name <> 'sys'
                ORDER BY s.name, v.name;");
        }

        public DataTable GetProcedures()
        {
            return _conn.ExecuteSelect(@"
                SELECT s.name AS SchemaName, p.name AS ObjectName
                FROM sys.procedures p
                INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
                WHERE p.is_ms_shipped = 0
                  AND s.name <> 'sys'
                  AND p.name NOT LIKE 'sp_%'
                ORDER BY s.name, p.name;");
        }

        public DataTable GetFunctions()
        {
            return _conn.ExecuteSelect(@"
                SELECT s.name AS SchemaName, o.name AS ObjectName
                FROM sys.objects o
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                WHERE o.type IN ('FN','IF','TF')
                  AND o.is_ms_shipped = 0
                  AND s.name <> 'sys'
                ORDER BY s.name, o.name;");
        }

        public DataTable GetTriggers()
        {
            return _conn.ExecuteSelect(@"
                SELECT 
                    OBJECT_SCHEMA_NAME(t.object_id) AS SchemaName, 
                    t.name AS ObjectName
                FROM sys.triggers t
                WHERE t.is_ms_shipped = 0
                  AND OBJECT_SCHEMA_NAME(t.object_id) <> 'sys'
                  AND t.parent_class = 1
                ORDER BY OBJECT_SCHEMA_NAME(t.object_id), t.name;");
        }

        public DataTable GetIndexes()
        {
            return _conn.ExecuteSelect(@"
                SELECT 
                    OBJECT_SCHEMA_NAME(i.object_id) AS SchemaName,
                    i.name AS ObjectName
                FROM sys.indexes i
                INNER JOIN sys.objects o ON i.object_id = o.object_id
                WHERE o.is_ms_shipped = 0
                  AND i.name IS NOT NULL
                  AND i.type > 0
                  AND OBJECT_SCHEMA_NAME(i.object_id) <> 'sys'
                ORDER BY OBJECT_SCHEMA_NAME(i.object_id), i.name;");
        }

        public DataTable GetSequences()
        {
            return _conn.ExecuteSelect(@"
                SELECT 
                    s.name AS SchemaName,
                    seq.name AS ObjectName
                FROM sys.sequences seq
                INNER JOIN sys.schemas s ON seq.schema_id = s.schema_id
                WHERE s.name <> 'sys'
                ORDER BY s.name, seq.name;");
        }

        public DataTable GetUsers()
        {
            return _conn.ExecuteSelect(@"
                SELECT 
                    name AS UserName,
                    type_desc AS UserType,
                    create_date AS CreateDate
                FROM sys.database_principals
                WHERE type IN ('S', 'U', 'G')
                  AND name NOT IN ('guest', 'INFORMATION_SCHEMA', 'sys')
                  AND name NOT LIKE 'db_%'
                  AND name NOT LIKE '##%'
                ORDER BY name;");
        }

        public DataTable GetColumns(string schema, string objectName)
        {
            string sql = $@"
                SELECT 
                    c.column_id AS ColumnId,
                    c.name AS ColumnName,
                    t.name AS DataType,
                    c.max_length AS MaxLength,
                    c.precision AS [Precision],
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable,
                    c.is_identity AS IsIdentity,
                    ISNULL(dc.definition, '') AS DefaultValue
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                WHERE c.object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(objectName)}')
                ORDER BY c.column_id;";

            return _conn.ExecuteSelect(sql);
        }

        private string EscapeSql(string value)
        {
            return value?.Replace("'", "''") ?? "";
        }
    }
}