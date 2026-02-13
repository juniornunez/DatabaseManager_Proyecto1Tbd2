using System.Data;

namespace DatabaseManager.Services
{
    public class TableViewerService
    {
        private readonly ConnectionService _conn;

        public TableViewerService(ConnectionService conn)
        {
            _conn = conn;
        }

        public DataTable GetTableData(string schema, string table, int top = 200)
        {
            string sql = $@"
                SELECT TOP ({top}) *
                FROM {Quote(schema)}.{Quote(table)};";

            return _conn.ExecuteSelect(sql);
        }

        public DataTable GetTopRows(string schema, string table, int top = 200)
        {
            return GetTableData(schema, table, top);
        }


        private string Quote(string name) => $"[{name.Replace("]", "]]")}]";
        private string Literal(string value) => $"'{value.Replace("'", "''")}'";
    }
}