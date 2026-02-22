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

        /// <summary>
        /// Obtiene datos de una tabla o vista
        /// </summary>
        public DataTable GetTableData(string schema, string objectName, int top = 200)
        {
            string sql = $@"
                SELECT TOP ({top}) *
                FROM {Quote(schema)}.{Quote(objectName)};";

            return _conn.ExecuteSelect(sql);
        }

        /// <summary>
        /// Alias del método anterior (por compatibilidad)
        /// </summary>
        public DataTable GetTopRows(string schema, string objectName, int top = 200)
        {
            return GetTableData(schema, objectName, top);
        }

        private string Quote(string name) => $"[{name.Replace("]", "]]")}]";
        private string Literal(string value) => $"'{value.Replace("'", "''")}'";
    }
}