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

        public DataTable GetColumns(string schema, string tableName)
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
                INNER JOIN sys.tables tb ON c.object_id = tb.object_id
                INNER JOIN sys.schemas s ON tb.schema_id = s.schema_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                WHERE s.name = '{EscapeSql(schema)}' 
                  AND tb.name = '{EscapeSql(tableName)}'
                ORDER BY c.column_id;";

            return _conn.ExecuteSelect(sql);
        }

        private string EscapeSql(string value)
        {
            return value?.Replace("'", "''") ?? "";
        }
    }
}