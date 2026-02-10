using System.Data;
using System.Text;

namespace DatabaseManager.Services
{
    public class DdlService
    {
        private readonly ConnectionService _conn;

        public DdlService(ConnectionService conn)
        {
            _conn = conn;
        }

        public string GetTableDDL(string schema, string table)
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
    ic.is_identity AS IsIdentity,
    ic.seed_value AS SeedValue,
    ic.increment_value AS IncrementValue,
    dc.definition AS DefaultDefinition
FROM sys.columns c
INNER JOIN sys.tables tb ON c.object_id = tb.object_id
INNER JOIN sys.schemas s ON tb.schema_id = s.schema_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN sys.identity_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE s.name = {Literal(schema)} AND tb.name = {Literal(table)}
ORDER BY c.column_id;";

            DataTable dt = _conn.ExecuteSelect(sql);

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(Quote(schema));
            sb.Append(".");
            sb.Append(Quote(table));
            sb.AppendLine(" (");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow r = dt.Rows[i];

                string colName = r["ColumnName"].ToString();
                string dataType = r["DataType"].ToString();
                int maxLen = ToInt(r["MaxLength"]);
                int precision = ToInt(r["Precision"]);
                int scale = ToInt(r["Scale"]);
                bool nullable = ToBool(r["IsNullable"]);
                bool isIdentity = ToBool(r["IsIdentity"]);
                string seed = r["SeedValue"] == DBNull.Value ? "" : r["SeedValue"].ToString();
                string inc = r["IncrementValue"] == DBNull.Value ? "" : r["IncrementValue"].ToString();
                string def = r["DefaultDefinition"] == DBNull.Value ? "" : r["DefaultDefinition"].ToString();

                sb.Append("    ");
                sb.Append(Quote(colName));
                sb.Append(" ");
                sb.Append(FormatType(dataType, maxLen, precision, scale));

                if (isIdentity && seed.Length > 0 && inc.Length > 0)
                {
                    sb.Append(" IDENTITY(");
                    sb.Append(seed);
                    sb.Append(",");
                    sb.Append(inc);
                    sb.Append(")");
                }

                if (!string.IsNullOrWhiteSpace(def))
                {
                    sb.Append(" DEFAULT ");
                    sb.Append(def.Trim());
                }

                sb.Append(nullable ? " NULL" : " NOT NULL");

                if (i < dt.Rows.Count - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        public string GetModuleDDL(string schema, string name)
        {
            string sql = $@"
SELECT 
    m.definition AS Definition
FROM sys.sql_modules m
INNER JOIN sys.objects o ON m.object_id = o.object_id
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE s.name = {Literal(schema)} AND o.name = {Literal(name)};";

            DataTable dt = _conn.ExecuteSelect(sql);
            if (dt.Rows.Count == 0) return "";

            string def = dt.Rows[0]["Definition"] == DBNull.Value ? "" : dt.Rows[0]["Definition"].ToString();
            return def;
        }

        private string FormatType(string typeName, int maxLen, int precision, int scale)
        {
            string t = typeName.ToLowerInvariant();

            if (t == "varchar" || t == "char" || t == "nvarchar" || t == "nchar" || t == "varbinary" || t == "binary")
            {
                int len = maxLen;
                if (t == "nvarchar" || t == "nchar") len = maxLen / 2;
                if (len < 0) return $"{typeName}(MAX)";
                return $"{typeName}({len})";
            }

            if (t == "decimal" || t == "numeric")
            {
                return $"{typeName}({precision},{scale})";
            }

            if (t == "datetime2" || t == "time" || t == "datetimeoffset")
            {
                return $"{typeName}({scale})";
            }

            return typeName;
        }

        private int ToInt(object v)
        {
            if (v == DBNull.Value) return 0;
            return System.Convert.ToInt32(v);
        }

        private bool ToBool(object v)
        {
            if (v == DBNull.Value) return false;
            return System.Convert.ToBoolean(v);
        }

        private string Quote(string name) => $"[{name.Replace("]", "]]")}]";
        private string Literal(string value) => $"'{value.Replace("'", "''")}'";
    }
}
