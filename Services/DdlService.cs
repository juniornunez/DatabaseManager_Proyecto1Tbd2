using System;
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

        public string GetTableDDL(string schema, string tableName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- DDL for Table [{schema}].[{tableName}]");
                sb.AppendLine();
                sb.AppendLine($"CREATE TABLE [{schema}].[{tableName}] (");

                string sqlColumns = $@"
                    SELECT 
                        c.column_id,
                        c.name AS ColumnName,
                        t.name AS DataType,
                        c.max_length,
                        c.precision,
                        c.scale,
                        c.is_nullable,
                        c.is_identity,
                        ISNULL(dc.definition, '') AS DefaultValue
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                    WHERE c.object_id = OBJECT_ID('{schema}.{tableName}')
                    ORDER BY c.column_id;";

                var dtColumns = _conn.ExecuteSelect(sqlColumns);

                for (int i = 0; i < dtColumns.Rows.Count; i++)
                {
                    var row = dtColumns.Rows[i];
                    string colName = row["ColumnName"].ToString();
                    string dataType = row["DataType"].ToString();
                    int maxLength = Convert.ToInt32(row["max_length"]);
                    byte precision = Convert.ToByte(row["precision"]);
                    byte scale = Convert.ToByte(row["scale"]);
                    bool isNullable = Convert.ToBoolean(row["is_nullable"]);
                    bool isIdentity = Convert.ToBoolean(row["is_identity"]);
                    string defaultValue = row["DefaultValue"].ToString();

                    sb.Append($"    [{colName}] {FormatDataType(dataType, maxLength, precision, scale)}");

                    if (isIdentity)
                        sb.Append(" IDENTITY(1,1)");

                    sb.Append(isNullable ? " NULL" : " NOT NULL");

                    if (!string.IsNullOrEmpty(defaultValue))
                        sb.Append($" DEFAULT {defaultValue}");

                    if (i < dtColumns.Rows.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }

                string sqlPK = $@"
                    SELECT 
                        kc.name AS ConstraintName,
                        STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
                    FROM sys.key_constraints kc
                    INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id 
                        AND kc.unique_index_id = ic.index_id
                    INNER JOIN sys.columns c ON ic.object_id = c.object_id 
                        AND ic.column_id = c.column_id
                    WHERE kc.type = 'PK'
                      AND kc.parent_object_id = OBJECT_ID('{schema}.{tableName}')
                    GROUP BY kc.name;";

                var dtPK = _conn.ExecuteSelect(sqlPK);
                if (dtPK.Rows.Count > 0)
                {
                    string pkName = dtPK.Rows[0]["ConstraintName"].ToString();
                    string pkColumns = dtPK.Rows[0]["Columns"].ToString();
                    sb.AppendLine($"    CONSTRAINT [{pkName}] PRIMARY KEY ({pkColumns})");
                }

                sb.AppendLine(");");
                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL:\n-- {ex.Message}";
            }
        }

        public string GetModuleDDL(string schema, string objectName)
        {
            try
            {
                string sql = $@"
                    SELECT OBJECT_DEFINITION(OBJECT_ID('{schema}.{objectName}')) AS Definition;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Definition"] != DBNull.Value)
                {
                    string definition = dt.Rows[0]["Definition"].ToString();
                    return string.IsNullOrWhiteSpace(definition)
                        ? $"-- No se pudo obtener la definición de [{schema}].[{objectName}]"
                        : definition;
                }

                return $"-- No se encontró definición para [{schema}].[{objectName}]";
            }
            catch (Exception ex)
            {
                return $"-- Error al obtener DDL:\n-- {ex.Message}";
            }
        }

        private string FormatDataType(string dataType, int maxLength, byte precision, byte scale)
        {
            switch (dataType.ToLower())
            {
                case "varchar":
                case "char":
                case "varbinary":
                case "binary":
                    return maxLength == -1 ? $"{dataType}(MAX)" : $"{dataType}({maxLength})";

                case "nvarchar":
                case "nchar":
                    return maxLength == -1 ? $"{dataType}(MAX)" : $"{dataType}({maxLength / 2})";

                case "decimal":
                case "numeric":
                    return $"{dataType}({precision},{scale})";

                case "datetime2":
                case "time":
                case "datetimeoffset":
                    return scale > 0 ? $"{dataType}({scale})" : dataType;

                default:
                    return dataType;
            }
        }
    }
}