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

        /// <summary>
        /// Obtiene el DDL completo de una tabla con todas sus columnas, constraints, índices, etc.
        /// </summary>
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

                // 1. CREATE TABLE statement
                sb.AppendLine($"CREATE TABLE [{schema}].[{tableName}] (");

                // Obtener columnas
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

                // 2. Primary Key
                string pkDdl = GetPrimaryKeyDDL(schema, tableName);
                if (!string.IsNullOrEmpty(pkDdl))
                {
                    sb.AppendLine(pkDdl);
                    sb.AppendLine();
                }

                // 3. Unique Constraints
                string uniqueDdl = GetUniqueConstraintsDDL(schema, tableName);
                if (!string.IsNullOrEmpty(uniqueDdl))
                {
                    sb.AppendLine(uniqueDdl);
                    sb.AppendLine();
                }

                // 4. Foreign Keys
                string fkDdl = GetForeignKeysDDL(schema, tableName);
                if (!string.IsNullOrEmpty(fkDdl))
                {
                    sb.AppendLine(fkDdl);
                    sb.AppendLine();
                }

                // 5. Check Constraints
                string checkDdl = GetCheckConstraintsDDL(schema, tableName);
                if (!string.IsNullOrEmpty(checkDdl))
                {
                    sb.AppendLine(checkDdl);
                    sb.AppendLine();
                }

                // 6. Indexes (non-clustered, non-PK)
                string indexDdl = GetIndexesDDL(schema, tableName);
                if (!string.IsNullOrEmpty(indexDdl))
                {
                    sb.AppendLine(indexDdl);
                    sb.AppendLine();
                }

                // 7. Default Constraints (si no se incluyeron en la definición de columna)
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

        /// <summary>
        /// Obtiene el DDL de una vista
        /// </summary>
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

        /// <summary>
        /// Obtiene el DDL de un stored procedure
        /// </summary>
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

        /// <summary>
        /// Obtiene el DDL de una función
        /// </summary>
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

        /// <summary>
        /// Obtiene el DDL de un trigger
        /// </summary>
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

                // Obtener información del trigger
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

                // Obtener definición del trigger
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

        // ==================== MÉTODOS PRIVADOS AUXILIARES ====================

        /// <summary>
        /// Obtiene las columnas de una tabla con toda su información
        /// </summary>
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

        /// <summary>
        /// Formatea una definición de columna completa
        /// </summary>
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
                // Columna computada
                sb.Append($"AS {computedFormula}");
            }
            else
            {
                // Tipo de dato
                sb.Append(FormatDataType(dataType, maxLength, precision, scale));

                // IDENTITY
                if (isIdentity)
                {
                    sb.Append(" IDENTITY(1,1)");
                }

                // NULL / NOT NULL
                sb.Append(isNullable ? " NULL" : " NOT NULL");

                // DEFAULT (si existe y no es columna computada)
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    sb.Append($" DEFAULT {defaultValue}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formatea el tipo de dato con su tamaño/precisión
        /// </summary>
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

        /// <summary>
        /// Obtiene el DDL de la Primary Key
        /// </summary>
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

        /// <summary>
        /// Obtiene los DDL de UNIQUE constraints
        /// </summary>
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

        /// <summary>
        /// Obtiene los DDL de Foreign Keys
        /// </summary>
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

        /// <summary>
        /// Obtiene los DDL de Check Constraints
        /// </summary>
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

        /// <summary>
        /// Obtiene los DDL de Indexes (que no sean PK)
        /// </summary>
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

        /// <summary>
        /// Obtiene los DDL de Default Constraints (standalone)
        /// </summary>
        private string GetDefaultConstraintsDDL(string schema, string tableName)
        {
            return string.Empty;
        }

        public string GetIndexDDL(string schema, string indexName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Index [{schema}].[{indexName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT 
                        i.name AS IndexName,
                        OBJECT_SCHEMA_NAME(i.object_id) AS SchemaName,
                        OBJECT_NAME(i.object_id) AS TableName,
                        i.type_desc AS IndexType,
                        i.is_unique,
                        i.is_primary_key,
                        i.is_unique_constraint,
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
                    WHERE i.name = '{EscapeSql(indexName)}'
                      AND OBJECT_SCHEMA_NAME(i.object_id) = '{EscapeSql(schema)}'
                    GROUP BY 
                        i.name, 
                        i.object_id,
                        i.type_desc, 
                        i.is_unique,
                        i.is_primary_key,
                        i.is_unique_constraint;";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    string tableName = row["TableName"].ToString();
                    string schemaName = row["SchemaName"].ToString();
                    string indexType = row["IndexType"].ToString();
                    bool isUnique = Convert.ToBoolean(row["is_unique"]);
                    bool isPrimaryKey = Convert.ToBoolean(row["is_primary_key"]);
                    bool isUniqueConstraint = Convert.ToBoolean(row["is_unique_constraint"]);
                    string keyColumns = row["KeyColumns"].ToString();
                    string includeColumns = row["IncludeColumns"]?.ToString() ?? "";

                    if (isPrimaryKey)
                    {
                        sb.AppendLine($"-- This is a PRIMARY KEY constraint");
                        sb.AppendLine($"ALTER TABLE [{schemaName}].[{tableName}]");
                        sb.AppendLine($"    ADD CONSTRAINT [{indexName}] PRIMARY KEY {indexType} ({keyColumns});");
                    }
                    else if (isUniqueConstraint)
                    {
                        sb.AppendLine($"-- This is a UNIQUE constraint");
                        sb.AppendLine($"ALTER TABLE [{schemaName}].[{tableName}]");
                        sb.AppendLine($"    ADD CONSTRAINT [{indexName}] UNIQUE ({keyColumns});");
                    }
                    else
                    {
                        sb.Append($"CREATE ");
                        if (isUnique) sb.Append("UNIQUE ");
                        sb.Append($"{indexType} INDEX [{indexName}]");
                        sb.AppendLine($" ON [{schemaName}].[{tableName}] ({keyColumns})");

                        if (!string.IsNullOrWhiteSpace(includeColumns))
                        {
                            sb.AppendLine($"    INCLUDE ({includeColumns})");
                        }

                        sb.AppendLine(";");
                    }
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener información del índice");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de índice:\n-- {ex.Message}";
            }
        }

        public string GetSequenceDDL(string schema, string sequenceName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for Sequence [{schema}].[{sequenceName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT 
                        seq.name AS SequenceName,
                        s.name AS SchemaName,
                        TYPE_NAME(seq.user_type_id) AS DataType,
                        seq.start_value AS StartValue,
                        seq.increment AS Increment,
                        seq.minimum_value AS MinValue,
                        seq.maximum_value AS MaxValue,
                        seq.is_cycling AS IsCycling,
                        seq.current_value AS CurrentValue
                    FROM sys.sequences seq
                    INNER JOIN sys.schemas s ON seq.schema_id = s.schema_id
                    WHERE seq.name = '{EscapeSql(sequenceName)}'
                      AND s.name = '{EscapeSql(schema)}';";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    string dataType = row["DataType"].ToString();
                    long startValue = Convert.ToInt64(row["StartValue"]);
                    long increment = Convert.ToInt64(row["Increment"]);
                    long minValue = Convert.ToInt64(row["MinValue"]);
                    long maxValue = Convert.ToInt64(row["MaxValue"]);
                    bool isCycling = Convert.ToBoolean(row["IsCycling"]);

                    sb.AppendLine($"CREATE SEQUENCE [{schema}].[{sequenceName}]");
                    sb.AppendLine($"    AS {dataType}");
                    sb.AppendLine($"    START WITH {startValue}");
                    sb.AppendLine($"    INCREMENT BY {increment}");
                    sb.AppendLine($"    MINVALUE {minValue}");
                    sb.AppendLine($"    MAXVALUE {maxValue}");
                    sb.AppendLine($"    {(isCycling ? "CYCLE" : "NO CYCLE")};");
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener información de la secuencia");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de secuencia:\n-- {ex.Message}";
            }
        }

        public string GetUserDDL(string userName)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"-- =============================================");
                sb.AppendLine($"-- DDL for User [{userName}]");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- =============================================");
                sb.AppendLine();

                string sql = $@"
                    SELECT 
                        dp.name AS UserName,
                        dp.type_desc AS UserType,
                        dp.create_date AS CreateDate,
                        dp.default_schema_name AS DefaultSchema,
                        sl.name AS LoginName
                    FROM sys.database_principals dp
                    LEFT JOIN sys.server_principals sl ON dp.sid = sl.sid
                    WHERE dp.name = '{EscapeSql(userName)}';";

                var dt = _conn.ExecuteSelect(sql);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    string userType = row["UserType"].ToString();
                    string defaultSchema = row["DefaultSchema"]?.ToString() ?? "dbo";
                    string loginName = row["LoginName"]?.ToString() ?? "";

                    sb.AppendLine($"-- User Type: {userType}");
                    sb.AppendLine($"-- Created: {row["CreateDate"]}");
                    sb.AppendLine();

                    if (!string.IsNullOrEmpty(loginName))
                    {
                        sb.AppendLine($"CREATE USER [{userName}]");
                        sb.AppendLine($"    FOR LOGIN [{loginName}]");
                        sb.AppendLine($"    WITH DEFAULT_SCHEMA = [{defaultSchema}];");
                    }
                    else
                    {
                        sb.AppendLine($"CREATE USER [{userName}]");
                        sb.AppendLine($"    WITHOUT LOGIN");
                        sb.AppendLine($"    WITH DEFAULT_SCHEMA = [{defaultSchema}];");
                    }

                    sb.AppendLine();

                    string sqlRoles = $@"
                        SELECT r.name AS RoleName
                        FROM sys.database_role_members drm
                        INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
                        INNER JOIN sys.database_principals u ON drm.member_principal_id = u.principal_id
                        WHERE u.name = '{EscapeSql(userName)}';";

                    var dtRoles = _conn.ExecuteSelect(sqlRoles);

                    if (dtRoles.Rows.Count > 0)
                    {
                        sb.AppendLine($"-- Role memberships:");
                        foreach (DataRow roleRow in dtRoles.Rows)
                        {
                            string roleName = roleRow["RoleName"].ToString();
                            sb.AppendLine($"ALTER ROLE [{roleName}] ADD MEMBER [{userName}];");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"-- No se pudo obtener información del usuario");
                }

                sb.AppendLine();
                sb.AppendLine("GO");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Error al generar DDL de usuario:\n-- {ex.Message}";
            }
        }

        private string EscapeSql(string value)
        {
            return value?.Replace("'", "''") ?? "";
        }
    }
}