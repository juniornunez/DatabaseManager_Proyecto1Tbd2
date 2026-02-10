using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DatabaseManager.Services
{
    public class ConnectionService
    {
        public string ConnectionString { get; private set; } = "";

        public void ConfigureWindowsAuth(string server, string database)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                Encrypt = false
            };
            ConnectionString = csb.ConnectionString;
        }

        public void ConfigureSqlAuth(string server, string database, string user, string pass)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                IntegratedSecurity = false,
                UserID = user,
                Password = pass,
                TrustServerCertificate = true,
                Encrypt = false
            };
            ConnectionString = csb.ConnectionString;
        }

        public void TestConnection()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("No hay ConnectionString configurada.");

            using var cn = new SqlConnection(ConnectionString);
            cn.Open();
        }

        public DataTable ExecuteSelect(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            using var da = new SqlDataAdapter(cmd);

            var dt = new DataTable();
            cn.Open();
            da.Fill(dt);
            return dt;
        }

        public int ExecuteNonQuery(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
