using System.Data;
using DatabaseManager.Services;

namespace DatabaseManager.Services
{
    public class TableViewerService
    {
        private readonly ConnectionService _conn;

        public TableViewerService(ConnectionService conn)
        {
            _conn = conn;
        }

        public DataTable GetTopRows(string schema, string table, int top = 200)
        {
            string sql = $@"
                SELECT TOP ({top}) *
                FROM {Quote(schema)}.{Quote(table)};";

            return _conn.ExecuteSelect(sql);
        }

        public DataTable GetColumns(string schema, string table)
        {
            string sql = $@"
                SELECT 
                    c.column_id AS ColumnId,
                    c.name AS ColumnName,
                    t.name AS DataType,
                    c.max_length AS MaxLength,
                    c.precision AS [Precision],
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                INNER JOIN sys.tables tb ON c.object_id = tb.object_id
                INNER JOIN sys.schemas s ON tb.schema_id = s.schema_id
                WHERE s.name = {Literal(schema)} AND tb.name = {Literal(table)}
                ORDER BY c.column_id;";

            return _conn.ExecuteSelect(sql);
        }

        private string Quote(string name) => $"[{name.Replace("]", "]]")}]";
        private string Literal(string value) => $"'{value.Replace("'", "''")}'";
    }
}
