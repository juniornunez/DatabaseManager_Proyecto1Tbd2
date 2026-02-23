using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;

namespace DatabaseManager.Forms
{
    public class CreateTableForm : Form
    {
        private TextBox txtSchema;
        private TextBox txtTableName;
        private DataGridView gridColumns;
        private Button btnAddColumn;
        private Button btnRemoveColumn;
        private Button btnGenerateSQL;
        private Button btnCreate;
        private Button btnCancel;
        private TextBox txtSQL;
        private ConnectionService _conn;

        public string GeneratedSQL { get; private set; }

        public CreateTableForm(ConnectionService conn)
        {
            _conn = conn;
            BuildUI();
            AddDefaultColumn();
        }

        private void BuildUI()
        {
            Text = "Crear Tabla";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "Crear Nueva Tabla",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(860, 30)
            };

            var lblSchema = new Label
            {
                Text = "Schema:",
                Location = new Point(20, 70),
                Size = new Size(100, 20)
            };

            txtSchema = new TextBox
            {
                Location = new Point(130, 68),
                Size = new Size(200, 25),
                Text = "dbo"
            };

            var lblTableName = new Label
            {
                Text = "Nombre Tabla:",
                Location = new Point(350, 70),
                Size = new Size(100, 20)
            };

            txtTableName = new TextBox
            {
                Location = new Point(460, 68),
                Size = new Size(300, 25)
            };

            var lblColumns = new Label
            {
                Text = "Columnas:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 110),
                Size = new Size(200, 20)
            };

            gridColumns = new DataGridView
            {
                Location = new Point(20, 140),
                Size = new Size(860, 250),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            gridColumns.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColumnName",
                HeaderText = "Nombre Columna",
                Width = 200
            });

            var cboType = new DataGridViewComboBoxColumn
            {
                Name = "DataType",
                HeaderText = "Tipo de Dato",
                Width = 150
            };
            cboType.Items.AddRange(new string[]
            {
                "int", "bigint", "smallint", "tinyint",
                "decimal", "numeric", "float", "real",
                "varchar", "nvarchar", "char", "nchar", "text", "ntext",
                "date", "datetime", "datetime2", "time",
                "bit", "uniqueidentifier", "xml", "varbinary"
            });
            gridColumns.Columns.Add(cboType);

            gridColumns.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Length",
                HeaderText = "Longitud",
                Width = 100
            });

            gridColumns.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsNullable",
                HeaderText = "Nullable",
                Width = 80
            });

            gridColumns.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsPrimaryKey",
                HeaderText = "PK",
                Width = 60
            });

            gridColumns.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsIdentity",
                HeaderText = "Identity",
                Width = 80
            });

            gridColumns.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DefaultValue",
                HeaderText = "Valor por Defecto",
                Width = 150
            });

            btnAddColumn = new Button
            {
                Text = "Agregar Columna",
                Location = new Point(20, 400),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddColumn.FlatAppearance.BorderSize = 0;
            btnAddColumn.Click += (s, e) => AddDefaultColumn();

            btnRemoveColumn = new Button
            {
                Text = "Eliminar Columna",
                Location = new Point(180, 400),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(220, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemoveColumn.FlatAppearance.BorderSize = 0;
            btnRemoveColumn.Click += BtnRemoveColumn_Click;

            btnGenerateSQL = new Button
            {
                Text = "Generar SQL",
                Location = new Point(730, 400),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerateSQL.FlatAppearance.BorderSize = 0;
            btnGenerateSQL.Click += BtnGenerateSQL_Click;

            var lblSQL = new Label
            {
                Text = "SQL Generado:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 450),
                Size = new Size(200, 20)
            };

            txtSQL = new TextBox
            {
                Location = new Point(20, 480),
                Size = new Size(860, 120),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            btnCreate = new Button
            {
                Text = "Crear Tabla",
                Location = new Point(730, 615),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;

            btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(580, 615),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblSchema);
            Controls.Add(txtSchema);
            Controls.Add(lblTableName);
            Controls.Add(txtTableName);
            Controls.Add(lblColumns);
            Controls.Add(gridColumns);
            Controls.Add(btnAddColumn);
            Controls.Add(btnRemoveColumn);
            Controls.Add(btnGenerateSQL);
            Controls.Add(lblSQL);
            Controls.Add(txtSQL);
            Controls.Add(btnCreate);
            Controls.Add(btnCancel);
        }

        private void AddDefaultColumn()
        {
            gridColumns.Rows.Add("Column" + (gridColumns.Rows.Count + 1), "int", "", true, false, false, "");
        }

        private void BtnRemoveColumn_Click(object sender, EventArgs e)
        {
            if (gridColumns.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in gridColumns.SelectedRows)
                {
                    if (!row.IsNewRow)
                        gridColumns.Rows.Remove(row);
                }
            }
        }

        private void BtnGenerateSQL_Click(object sender, EventArgs e)
        {
            try
            {
                string schema = txtSchema.Text.Trim();
                string tableName = txtTableName.Text.Trim();

                if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(tableName))
                {
                    MessageBox.Show("Schema y Nombre de Tabla son requeridos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (gridColumns.Rows.Count == 0)
                {
                    MessageBox.Show("Debes agregar al menos una columna.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var sql = new System.Text.StringBuilder();
                sql.AppendLine($"CREATE TABLE [{schema}].[{tableName}] (");

                var pkColumns = new List<string>();

                for (int i = 0; i < gridColumns.Rows.Count; i++)
                {
                    var row = gridColumns.Rows[i];

                    string colName = row.Cells["ColumnName"].Value?.ToString() ?? "";
                    string dataType = row.Cells["DataType"].Value?.ToString() ?? "int";
                    string length = row.Cells["Length"].Value?.ToString() ?? "";
                    bool isNullable = Convert.ToBoolean(row.Cells["IsNullable"].Value ?? false);
                    bool isPK = Convert.ToBoolean(row.Cells["IsPrimaryKey"].Value ?? false);
                    bool isIdentity = Convert.ToBoolean(row.Cells["IsIdentity"].Value ?? false);
                    string defaultValue = row.Cells["DefaultValue"].Value?.ToString() ?? "";

                    if (string.IsNullOrWhiteSpace(colName))
                    {
                        MessageBox.Show($"La columna en la fila {i + 1} no tiene nombre.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (isPK && isNullable)
                    {
                        MessageBox.Show(
                            $"La columna '{colName}' está marcada como PRIMARY KEY pero también como NULLABLE.\n\n" +
                            "Una PRIMARY KEY no puede ser nullable.\n\n" +
                            "Desmarca 'Nullable' para las columnas que sean Primary Key.",
                            "Error de Validación",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    sql.Append($"    [{colName}] {FormatDataType(dataType, length)}");

                    if (isIdentity)
                        sql.Append(" IDENTITY(1,1)");

                    sql.Append(isNullable ? " NULL" : " NOT NULL");

                    if (!string.IsNullOrWhiteSpace(defaultValue))
                        sql.Append($" DEFAULT {defaultValue}");

                    if (isPK)
                        pkColumns.Add(colName);

                    if (i < gridColumns.Rows.Count - 1 || pkColumns.Count > 0)
                        sql.AppendLine(",");
                    else
                        sql.AppendLine();
                }

                if (pkColumns.Count > 0)
                {
                    string pkCols = string.Join(", ", pkColumns.ConvertAll(c => $"[{c}]"));
                    sql.AppendLine($"    CONSTRAINT [PK_{tableName}] PRIMARY KEY ({pkCols})");
                }

                sql.AppendLine(");");

                txtSQL.Text = sql.ToString();
                GeneratedSQL = sql.ToString();
                btnCreate.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar SQL:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatDataType(string dataType, string length)
        {
            dataType = dataType.ToLower();

            if ((dataType == "varchar" || dataType == "nvarchar" || dataType == "char" || dataType == "nchar" || dataType == "varbinary") && !string.IsNullOrWhiteSpace(length))
            {
                if (length.ToLower() == "max")
                    return $"{dataType}(MAX)";
                else
                    return $"{dataType}({length})";
            }
            else if ((dataType == "decimal" || dataType == "numeric") && !string.IsNullOrWhiteSpace(length))
            {
                return $"{dataType}({length})";
            }

            return dataType;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GeneratedSQL))
                {
                    MessageBox.Show("Primero genera el SQL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnCreate.Enabled = false;
                btnCreate.Text = "Creando...";
                Application.DoEvents();

                _conn.ExecuteNonQuery(GeneratedSQL);

                MessageBox.Show("Tabla creada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                btnCreate.Enabled = true;
                btnCreate.Text = "Crear Tabla";
                MessageBox.Show($"Error al crear tabla:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}