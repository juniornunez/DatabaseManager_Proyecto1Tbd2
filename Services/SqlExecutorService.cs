using System.Data;
using Microsoft.Data.SqlClient;

namespace DatabaseManager.Services
{
    public class SqlExecutorService
    {
        private readonly ConnectionService _conn;

        public SqlExecutorService(ConnectionService conn)
        {
            _conn = conn;
        }

        public DataTable ExecuteQuery(string sql)
        {
            return _conn.ExecuteSelect(sql);
        }

        public int ExecuteNonQuery(string sql)
        {
            return _conn.ExecuteNonQuery(sql);
        }
    }
}