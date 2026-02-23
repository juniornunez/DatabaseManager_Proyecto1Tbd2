using System;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;

namespace DatabaseManager.Forms
{
    public class CreateViewForm : Form
    {
        private TextBox txtSchema;
        private TextBox txtViewName;
        private TextBox txtSQL;
        private Button btnGenerateSQL;
        private Button btnCreate;
        private Button btnCancel;
        private TextBox txtPreview;
        private ConnectionService _conn;

        public string GeneratedSQL { get; private set; }

        public CreateViewForm(ConnectionService conn)
        {
            _conn = conn;
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Crear Vista";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "Crear Nueva Vista",
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

            var lblViewName = new Label
            {
                Text = "Nombre Vista:",
                Location = new Point(350, 70),
                Size = new Size(100, 20)
            };

            txtViewName = new TextBox
            {
                Location = new Point(460, 68),
                Size = new Size(300, 25)
            };

            var lblSQL = new Label
            {
                Text = "SELECT Statement:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 110),
                Size = new Size(400, 20)
            };

            var lblHelp = new Label
            {
                Text = "Escribe solo el SELECT (sin CREATE VIEW)",
                ForeColor = Color.Gray,
                Location = new Point(20, 135),
                Size = new Size(860, 20)
            };

            txtSQL = new TextBox
            {
                Location = new Point(20, 160),
                Size = new Size(860, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10),
                AcceptsTab = true,
                Text = "SELECT \n    Column1,\n    Column2\nFROM [dbo].[TableName]\nWHERE Condition = 1"
            };

            btnGenerateSQL = new Button
            {
                Text = "Generar SQL Completo",
                Location = new Point(680, 370),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerateSQL.FlatAppearance.BorderSize = 0;
            btnGenerateSQL.Click += BtnGenerateSQL_Click;

            var lblPreview = new Label
            {
                Text = "SQL Completo Generado:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 420),
                Size = new Size(300, 20)
            };

            txtPreview = new TextBox
            {
                Location = new Point(20, 450),
                Size = new Size(860, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            btnCreate = new Button
            {
                Text = "Crear Vista",
                Location = new Point(730, 565),
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
                Location = new Point(580, 565),
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
            Controls.Add(lblViewName);
            Controls.Add(txtViewName);
            Controls.Add(lblSQL);
            Controls.Add(lblHelp);
            Controls.Add(txtSQL);
            Controls.Add(btnGenerateSQL);
            Controls.Add(lblPreview);
            Controls.Add(txtPreview);
            Controls.Add(btnCreate);
            Controls.Add(btnCancel);
        }

        private void BtnGenerateSQL_Click(object sender, EventArgs e)
        {
            try
            {
                string schema = txtSchema.Text.Trim();
                string viewName = txtViewName.Text.Trim();
                string selectStatement = txtSQL.Text.Trim();

                if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(viewName))
                {
                    MessageBox.Show("Schema y Nombre de Vista son requeridos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectStatement))
                {
                    MessageBox.Show("Debes escribir el SELECT statement.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (selectStatement.ToUpper().Contains("CREATE VIEW"))
                {
                    MessageBox.Show("Solo escribe el SELECT, no incluyas CREATE VIEW.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!selectStatement.ToUpper().TrimStart().StartsWith("SELECT"))
                {
                    MessageBox.Show(
                        "El SELECT statement debe empezar con SELECT.\n\n" +
                        "Ejemplo:\n" +
                        "SELECT Column1, Column2\n" +
                        "FROM [dbo].[TableName]\n" +
                        "WHERE Condition = 1",
                        "Error de Validación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                selectStatement = selectStatement.TrimEnd(';', ' ', '\n', '\r');

                var sql = new System.Text.StringBuilder();
                sql.AppendLine($"CREATE VIEW [{schema}].[{viewName}]");
                sql.AppendLine("AS");
                sql.AppendLine(selectStatement);
                sql.AppendLine(";");

                txtPreview.Text = sql.ToString();
                GeneratedSQL = sql.ToString();
                btnCreate.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar SQL:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                MessageBox.Show("Vista creada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                btnCreate.Enabled = true;
                btnCreate.Text = "Crear Vista";
                MessageBox.Show($"Error al crear vista:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}