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
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Table [{schema}].[{tableName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                sb.AppendLine($"CREATE TABLE [{schema}].[{tableName}] (");

                var columns = GetTableColumns(schema, tableName);
                for (int i = 0; i < columns.Rows.Count; i++)
                {
                    var col = columns.Rows[i];
                    sb.Append("    ");
                    sb.Append(FormatColumnDefinition(col));

                    if (i < columns.Rows.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }

                sb.AppendLine(");");
                sb.AppendLine();

                string pkDdl = GetPrimaryKeyDDL(schema, tableName);
                if (!string.IsNullOrEmpty(pkDdl))
                {
                    sb.AppendLine(pkDdl);
                    sb.AppendLine();
                }

                string uniqueDdl = GetUniqueConstraintsDDL(schema, tableName);
                if (!string.IsNullOrEmpty(uniqueDdl))
                {
                    sb.AppendLine(uniqueDdl);
                    sb.AppendLine();
                }

                string fkDdl = GetForeignKeysDDL(schema, tableName);
                if (!string.IsNullOrEmpty(fkDdl))
                {
                    sb.AppendLine(fkDdl);
                    sb.AppendLine();
                }

                string checkDdl = GetCheckConstraintsDDL(schema, tableName);
                if (!string.IsNullOrEmpty(checkDdl))
                {
                    sb.AppendLine(checkDdl);
                    sb.AppendLine();
                }

                string indexDdl = GetIndexesDDL(schema, tableName);
                if (!string.IsNullOrEmpty(indexDdl))
                {
                    sb.AppendLine(indexDdl);
                    sb.AppendLine();
                }

                string defaultDdl = GetDefaultConstraintsDDL(schema, tableName);
                if (!string.IsNullOrEmpty(defaultDdl))
                {
                    sb.AppendLine(defaultDdl);
                    sb.AppendLine();
                }

                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de tabla:\n-- {ex.Message}\n-- {ex.StackTrace}";
            }
        }

        public string GetViewDDL(string schema, string viewName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for View [{schema}].[{viewName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT OBJECT_DEFINITION(OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(viewName)}')) AS Definition;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Definition"] != DBNull.Value)
                {
                    string definition = dt.Rows[0]["Definition"].ToString();
                    sb.AppendLine(definition.Trim());
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener la definición de la vista");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de vista:\n-- {ex.Message}";
            }
        }

        
        public string GetProcedureDDL(string schema, string procName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Stored Procedure [{schema}].[{procName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT OBJECT_DEFINITION(OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(procName)}')) AS Definition;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Definition"] != DBNull.Value)
                {
                    string definition = dt.Rows[0]["Definition"].ToString();
                    sb.AppendLine(definition.Trim());
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener la definición del procedimiento");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de procedimiento:\n-- {ex.Message}";
            }
        }

        public string GetFunctionDDL(string schema, string functionName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Function [{schema}].[{functionName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT OBJECT_DEFINITION(OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(functionName)}')) AS Definition;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Definition"] != DBNull.Value)
                {
                    string definition = dt.Rows[0]["Definition"].ToString();
                    sb.AppendLine(definition.Trim());
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener la definición de la función");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de función:\n-- {ex.Message}";
            }
        }

        public string GetTriggerDDL(string schema, string triggerName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Trigger [{schema}].[{triggerName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();
                
                string sqlInfo = $@"
                    SELECT 
                        t.name AS TriggerName,
                        OBJECT_NAME(t.parent_id) AS TableName,
                        OBJECT_SCHEMA_NAME(t.parent_id) AS TableSchema,
                        t.is_disabled,
                        t.is_instead_of_trigger
                    FROM sys.triggers t
                    WHERE t.name = '{EscapeSql(triggerName)}'
                      AND OBJECT_SCHEMA_NAME(t.object_id) = '{EscapeSql(schema)}';";

                var dtInfo = _conn.ExecuteSelect(sqlInfo);

                if (dtInfo.Rows.Count > 0)
                {
                    var info = dtInfo.Rows[0];
                    string tableName = info["TableName"].ToString();
                    string tableSchema = info["TableSchema"].ToString();
                    bool isDisabled = Convert.ToBoolean(info["is_disabled"]);

                    sb.AppendLine($"-- Trigger on table: [{tableSchema}].[{tableName}]");
                    sb.AppendLine($"-- Status: {(isDisabled ? "DISABLED" : "ENABLED")}");
                    sb.AppendLine();
                }

                string sql = $@"
                    SELECT OBJECT_DEFINITION(OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(triggerName)}')) AS Definition;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Definition"] != DBNull.Value)
                {
                    string definition = dt.Rows[0]["Definition"].ToString();
                    sb.AppendLine(definition.Trim());
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener la definición del trigger");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de trigger:\n-- {ex.Message}";
            }
        }

        
        private DataTable GetTableColumns(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    c.column_id,
                    c.name AS ColumnName,
                    t.name AS DataType,
                    c.max_length,
                    c.precision,
                    c.scale,
                    c.is_nullable,
                    c.is_identity,
                    c.is_computed,
                    cc.definition AS ComputedFormula,
                    ISNULL(dc.definition, '') AS DefaultValue,
                    ISNULL(dc.name, '') AS DefaultConstraintName
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id 
                    AND c.column_id = cc.column_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                WHERE c.object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                ORDER BY c.column_id;";

            return _conn.ExecuteSelect(sql);
        }

       
        private string FormatColumnDefinition(DataRow col)
        {
            var sb = new StringBuilder();

            string colName = col["ColumnName"].ToString();
            string dataType = col["DataType"].ToString();
            int maxLength = Convert.ToInt32(col["max_length"]);
            byte precision = Convert.ToByte(col["precision"]);
            byte scale = Convert.ToByte(col["scale"]);
            bool isNullable = Convert.ToBoolean(col["is_nullable"]);
            bool isIdentity = Convert.ToBoolean(col["is_identity"]);
            bool isComputed = Convert.ToBoolean(col["is_computed"]);
            string computedFormula = col["ComputedFormula"].ToString();
            string defaultValue = col["DefaultValue"].ToString();

            sb.Append($"[{colName}] ");

            if (isComputed)
            {
                sb.Append($"AS {computedFormula}");
            }
            else
            {
                sb.Append(FormatDataType(dataType, maxLength, precision, scale));

                if (isIdentity)
                {
                    sb.Append(" IDENTITY(1,1)");
                }

                sb.Append(isNullable ? " NULL" : " NOT NULL");

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    sb.Append($" DEFAULT {defaultValue}");
                }
            }

            return sb.ToString();
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

                case "float":
                    return precision == 53 ? dataType : $"{dataType}({precision})";

                default:
                    return dataType;
            }
        }

        private string GetPrimaryKeyDDL(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    kc.name AS ConstraintName,
                    i.type_desc AS IndexType,
                    STRING_AGG(
                        CASE WHEN ic.is_descending_key = 1 
                        THEN QUOTENAME(c.name) + ' DESC'
                        ELSE QUOTENAME(c.name) + ' ASC'
                        END, 
                        ', '
                    ) WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
                FROM sys.key_constraints kc
                INNER JOIN sys.indexes i ON kc.parent_object_id = i.object_id 
                    AND kc.unique_index_id = i.index_id
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id 
                    AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id 
                    AND ic.column_id = c.column_id
                WHERE kc.type = 'PK'
                  AND kc.parent_object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                GROUP BY kc.name, i.type_desc;";

            var dt = _conn.ExecuteSelect(sql);

            if (dt.Rows.Count > 0)
            {
                string pkName = dt.Rows[0]["ConstraintName"].ToString();
                string columns = dt.Rows[0]["Columns"].ToString();
                string indexType = dt.Rows[0]["IndexType"].ToString();

                return $"ALTER TABLE [{schema}].[{tableName}]\n" +
                       $"    ADD CONSTRAINT [{pkName}] PRIMARY KEY {indexType} ({columns});";
            }

            return string.Empty;
        }

        private string GetUniqueConstraintsDDL(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    kc.name AS ConstraintName,
                    STRING_AGG(QUOTENAME(c.name), ', ') 
                        WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
                FROM sys.key_constraints kc
                INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id 
                    AND kc.unique_index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id 
                    AND ic.column_id = c.column_id
                WHERE kc.type = 'UQ'
                  AND kc.parent_object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                GROUP BY kc.name;";

            var dt = _conn.ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("-- Unique Constraints");

            foreach (DataRow row in dt.Rows)
            {
                string constraintName = row["ConstraintName"].ToString();
                string columns = row["Columns"].ToString();

                sb.AppendLine($"ALTER TABLE [{schema}].[{tableName}]");
                sb.AppendLine($"    ADD CONSTRAINT [{constraintName}] UNIQUE ({columns});");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetForeignKeysDDL(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    fk.name AS FKName,
                    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS RefSchema,
                    OBJECT_NAME(fk.referenced_object_id) AS RefTable,
                    STRING_AGG(QUOTENAME(c1.name), ', ') 
                        WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS FKColumns,
                    STRING_AGG(QUOTENAME(c2.name), ', ') 
                        WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS RefColumns,
                    fk.delete_referential_action_desc AS DeleteAction,
                    fk.update_referential_action_desc AS UpdateAction
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id 
                    AND fkc.parent_column_id = c1.column_id
                INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id 
                    AND fkc.referenced_column_id = c2.column_id
                WHERE fk.parent_object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                GROUP BY 
                    fk.name, 
                    fk.referenced_object_id,
                    fk.delete_referential_action_desc,
                    fk.update_referential_action_desc;";

            var dt = _conn.ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("-- Foreign Keys");

            foreach (DataRow row in dt.Rows)
            {
                string fkName = row["FKName"].ToString();
                string refSchema = row["RefSchema"].ToString();
                string refTable = row["RefTable"].ToString();
                string fkColumns = row["FKColumns"].ToString();
                string refColumns = row["RefColumns"].ToString();
                string deleteAction = row["DeleteAction"].ToString();
                string updateAction = row["UpdateAction"].ToString();

                sb.AppendLine($"ALTER TABLE [{schema}].[{tableName}]");
                sb.AppendLine($"    ADD CONSTRAINT [{fkName}] FOREIGN KEY ({fkColumns})");
                sb.AppendLine($"    REFERENCES [{refSchema}].[{refTable}] ({refColumns})");

                if (deleteAction != "NO_ACTION")
                    sb.AppendLine($"    ON DELETE {deleteAction.Replace("_", " ")}");

                if (updateAction != "NO_ACTION")
                    sb.AppendLine($"    ON UPDATE {updateAction.Replace("_", " ")}");

                sb.AppendLine(";");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetCheckConstraintsDDL(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    cc.name AS ConstraintName,
                    cc.definition AS CheckDefinition
                FROM sys.check_constraints cc
                WHERE cc.parent_object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                ORDER BY cc.name;";

            var dt = _conn.ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("-- Check Constraints");

            foreach (DataRow row in dt.Rows)
            {
                string constraintName = row["ConstraintName"].ToString();
                string definition = row["CheckDefinition"].ToString();

                sb.AppendLine($"ALTER TABLE [{schema}].[{tableName}]");
                sb.AppendLine($"    ADD CONSTRAINT [{constraintName}] CHECK {definition};");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetIndexesDDL(string schema, string tableName)
        {
            string sql = $@"
                SELECT 
                    i.name AS IndexName,
                    i.type_desc AS IndexType,
                    i.is_unique,
                    STRING_AGG(
                        CASE WHEN ic.is_descending_key = 1 
                        THEN QUOTENAME(c.name) + ' DESC'
                        ELSE QUOTENAME(c.name) + ' ASC'
                        END, 
                        ', '
                    ) WITHIN GROUP (ORDER BY ic.key_ordinal) AS KeyColumns,
                    STRING_AGG(
                        CASE WHEN ic.is_included_column = 1 
                        THEN QUOTENAME(c.name)
                        ELSE NULL
                        END, 
                        ', '
                    ) AS IncludeColumns
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id 
                    AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id 
                    AND ic.column_id = c.column_id
                WHERE i.object_id = OBJECT_ID('{EscapeSql(schema)}.{EscapeSql(tableName)}')
                  AND i.is_primary_key = 0
                  AND i.is_unique_constraint = 0
                  AND i.type > 0  -- No incluir HEAP
                GROUP BY i.name, i.type_desc, i.is_unique
                ORDER BY i.name;";

            var dt = _conn.ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("-- Indexes");

            foreach (DataRow row in dt.Rows)
            {
                string indexName = row["IndexName"].ToString();
                string indexType = row["IndexType"].ToString();
                bool isUnique = Convert.ToBoolean(row["is_unique"]);
                string keyColumns = row["KeyColumns"].ToString();
                string includeColumns = row["IncludeColumns"]?.ToString() ?? "";

                sb.Append($"CREATE ");
                if (isUnique) sb.Append("UNIQUE ");
                sb.Append($"{indexType} INDEX [{indexName}]");
                sb.AppendLine($" ON [{schema}].[{tableName}] ({keyColumns})");

                if (!string.IsNullOrWhiteSpace(includeColumns))
                {
                    sb.AppendLine($"    INCLUDE ({includeColumns})");
                }

                sb.AppendLine(";");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetDefaultConstraintsDDL(string schema, string tableName)
        {

            return string.Empty;
        }

        private string EscapeSql(string value)
        {
            return value?.Replace("'", "''") ?? "";
        }
    }
}