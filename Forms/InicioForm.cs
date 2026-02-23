using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;
using DatabaseManager.Models;

namespace DatabaseManager.Forms
{
    public class InicioForm : Form
    {
        private Panel pnlLeft;
        private Panel pnlRight;
        private ListBox lstConnections;
        private Label lblSavedConnections;
        private Button btnNewConnection;

        private TextBox txtNombre;
        private TextBox txtServer;
        private TextBox txtDatabase;
        private RadioButton rbWindowsAuth;
        private RadioButton rbSqlAuth;
        private Panel pnlSqlAuth;
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnProbar;
        private Button btnConectar;
        private Button btnSaveConnection;
        private Label lblEstado;

        private readonly ConnectionService _conn = new ConnectionService();
        private List<SavedConnection> _savedConnections;
        private SavedConnection _selectedConnection;

        public InicioForm()
        {
            _savedConnections = ConnectionManager.LoadConnections();
            BuildUI();
            LoadConnectionsList();
        }

        private void BuildUI()
        {
            Text = "Database Manager - Conexión";
            Size = new Size(950, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;

            pnlLeft = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(280, 500),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblSavedConnections = new Label
            {
                Text = "Conexiones Guardadas",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 15),
                Size = new Size(260, 25),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            lstConnections = new ListBox
            {
                Location = new Point(10, 50),
                Size = new Size(258, 380),
                Font = new Font("Segoe UI", 9),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 60,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstConnections.DrawItem += LstConnections_DrawItem;
            lstConnections.SelectedIndexChanged += LstConnections_SelectedIndexChanged;
            lstConnections.MouseDown += LstConnections_MouseDown;

            btnNewConnection = new Button
            {
                Text = "Nueva Conexión",
                Location = new Point(10, 440),
                Size = new Size(258, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNewConnection.FlatAppearance.BorderSize = 0;
            btnNewConnection.Click += BtnNewConnection_Click;

            pnlLeft.Controls.Add(lblSavedConnections);
            pnlLeft.Controls.Add(lstConnections);
            pnlLeft.Controls.Add(btnNewConnection);

            pnlRight = new Panel
            {
                Location = new Point(280, 0),
                Size = new Size(670, 500),
                BackColor = Color.White
            };

            BuildRightPanel();

            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);
        }

        private void BuildRightPanel()
        {
            var lblTitle = new Label
            {
                Text = "Detalles de la Conexión",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(630, 30),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            int y = 65;

            pnlRight.Controls.Add(MkLabel("Nombre:", 20, y));
            txtNombre = MkTextBox(180, y);
            txtNombre.Text = "ReplicacionDemo";
            pnlRight.Controls.Add(txtNombre);
            y += 40;

            pnlRight.Controls.Add(MkLabel("Servidor:", 20, y));
            txtServer = MkTextBox(180, y);
            txtServer.Text = "DESKTOP-S4DNI64";
            pnlRight.Controls.Add(txtServer);

            var btnServerHelp = new Button
            {
                Text = "?",
                Location = new Point(540, y - 2),
                Size = new Size(25, 25),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230),
                Cursor = Cursors.Help
            };
            btnServerHelp.FlatAppearance.BorderSize = 1;
            btnServerHelp.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnServerHelp.Click += (s, e) => MessageBox.Show(
                "Ejemplos de servidores:\n\n" +
                "• localhost\n" +
                "• .\n" +
                "• (local)\n" +
                "• NOMBRE-PC\n" +
                "• NOMBRE-PC\\SQLEXPRESS\n" +
                "• 127.0.0.1",
                "Ayuda - Servidor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            pnlRight.Controls.Add(btnServerHelp);
            y += 40;

            pnlRight.Controls.Add(MkLabel("Base de Datos:", 20, y));
            txtDatabase = MkTextBox(180, y);
            txtDatabase.Text = "ReplicacionDemo";
            pnlRight.Controls.Add(txtDatabase);
            y += 50;

            var separator1 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Location = new Point(20, y),
                Size = new Size(630, 2)
            };
            pnlRight.Controls.Add(separator1);
            y += 15;

            var lblAuthTitle = new Label
            {
                Text = "Tipo de Autenticación:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            pnlRight.Controls.Add(lblAuthTitle);
            y += 30;

            rbWindowsAuth = new RadioButton
            {
                Text = "Windows Authentication",
                Location = new Point(180, y),
                Size = new Size(300, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };
            rbWindowsAuth.CheckedChanged += (_, __) => ToggleAuthFields();
            pnlRight.Controls.Add(rbWindowsAuth);
            y += 30;

            rbSqlAuth = new RadioButton
            {
                Text = "SQL Server Authentication",
                Location = new Point(180, y),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 9)
            };
            rbSqlAuth.CheckedChanged += (_, __) => ToggleAuthFields();
            pnlRight.Controls.Add(rbSqlAuth);
            y += 35;

            pnlSqlAuth = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(670, 90),
                BackColor = Color.FromArgb(245, 245, 245),
                Visible = false
            };

            pnlSqlAuth.Controls.Add(MkLabel("Usuario:", 20, 10));
            txtUser = MkTextBox(180, 10);
            txtUser.Text = "sa";
            pnlSqlAuth.Controls.Add(txtUser);

            pnlSqlAuth.Controls.Add(MkLabel("Contraseña:", 20, 50));
            txtPass = MkTextBox(180, 50);
            txtPass.UseSystemPasswordChar = true;
            pnlSqlAuth.Controls.Add(txtPass);

            pnlRight.Controls.Add(pnlSqlAuth);
            y += 100;

            var separator2 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Location = new Point(20, y),
                Size = new Size(630, 2)
            };
            pnlRight.Controls.Add(separator2);
            y += 20;

            btnProbar = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(180, y),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnProbar.FlatAppearance.BorderSize = 0;
            btnProbar.Click += BtnProbar_Click;

            btnSaveConnection = new Button
            {
                Text = "Guardar",
                Location = new Point(345, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 180, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSaveConnection.FlatAppearance.BorderSize = 0;
            btnSaveConnection.Click += BtnSaveConnection_Click;

            btnConectar = new Button
            {
                Text = "Conectar",
                Location = new Point(460, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConectar.FlatAppearance.BorderSize = 0;
            btnConectar.Click += BtnConectar_Click;

            lblEstado = new Label
            {
                Text = "● Listo para conectar",
                Location = new Point(20, y + 10),
                Size = new Size(630, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            pnlRight.Controls.Add(lblTitle);
            pnlRight.Controls.Add(btnProbar);
            pnlRight.Controls.Add(btnSaveConnection);
            pnlRight.Controls.Add(btnConectar);
            pnlRight.Controls.Add(lblEstado);
        }

        private void LoadConnectionsList()
        {
            lstConnections.Items.Clear();
            foreach (var conn in _savedConnections)
            {
                lstConnections.Items.Add(conn);
            }
        }

        private void LstConnections_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var conn = (SavedConnection)lstConnections.Items[e.Index];
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            e.Graphics.FillRectangle(
                new SolidBrush(isSelected ? Color.FromArgb(230, 240, 250) : Color.White),
                e.Bounds
            );

            int padding = 8;
            int textX = e.Bounds.Left + padding;
            int textY = e.Bounds.Top + padding;

            e.Graphics.DrawString(
                conn.Name,
                new Font("Segoe UI", 9, FontStyle.Bold),
                Brushes.Black,
                textX,
                textY
            );

            textY += 18;
            string details = $"{conn.Server} → {conn.Database}";
            e.Graphics.DrawString(
                details,
                new Font("Segoe UI", 8),
                Brushes.Gray,
                textX,
                textY
            );

            textY += 16;
            string authType = conn.UseWindowsAuth ? "Windows Auth" : $"SQL Auth ({conn.Username})";
            e.Graphics.DrawString(
                authType,
                new Font("Segoe UI", 7),
                Brushes.DarkGray,
                textX,
                textY
            );

            int closeSize = 16;
            int closeX = e.Bounds.Right - closeSize - padding;
            int closeY = e.Bounds.Top + (e.Bounds.Height - closeSize) / 2;
            Rectangle closeRect = new Rectangle(closeX, closeY, closeSize, closeSize);

            e.Graphics.FillRectangle(
                new SolidBrush(Color.FromArgb(220, 80, 80)),
                closeRect
            );

            using (Pen whitePen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawLine(whitePen,
                    closeX + 4, closeY + 4,
                    closeX + closeSize - 4, closeY + closeSize - 4);
                e.Graphics.DrawLine(whitePen,
                    closeX + closeSize - 4, closeY + 4,
                    closeX + 4, closeY + closeSize - 4);
            }

            e.Graphics.DrawRectangle(Pens.LightGray, e.Bounds);
        }

        private void LstConnections_MouseDown(object sender, MouseEventArgs e)
        {
            int index = lstConnections.IndexFromPoint(e.Location);
            if (index < 0) return;

            Rectangle itemRect = lstConnections.GetItemRectangle(index);
            int closeSize = 16;
            int padding = 8;
            int closeX = itemRect.Right - closeSize - padding;
            int closeY = itemRect.Top + (itemRect.Height - closeSize) / 2;
            Rectangle closeRect = new Rectangle(closeX, closeY, closeSize, closeSize);

            if (closeRect.Contains(e.Location))
            {
                var conn = (SavedConnection)lstConnections.Items[index];
                var result = MessageBox.Show(
                    $"¿Eliminar la conexión '{conn.Name}'?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ConnectionManager.DeleteConnection(conn.Id);
                    _savedConnections = ConnectionManager.LoadConnections();
                    LoadConnectionsList();
                    ClearForm();
                }
            }
        }

        private void LstConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstConnections.SelectedIndex < 0) return;

            _selectedConnection = (SavedConnection)lstConnections.SelectedItem;
            LoadConnectionToForm(_selectedConnection);
        }

        private void LoadConnectionToForm(SavedConnection conn)
        {
            txtNombre.Text = conn.Name;
            txtServer.Text = conn.Server;
            txtDatabase.Text = conn.Database;
            rbWindowsAuth.Checked = conn.UseWindowsAuth;
            rbSqlAuth.Checked = !conn.UseWindowsAuth;
            txtUser.Text = conn.Username ?? "sa";
            txtPass.Text = "";
        }

        private void BtnNewConnection_Click(object sender, EventArgs e)
        {
            lstConnections.SelectedIndex = -1;
            _selectedConnection = null;
            ClearForm();
        }

        private void ClearForm()
        {
            txtNombre.Text = "Nueva Conexión";
            txtServer.Text = "DESKTOP-S4DNI64";
            txtDatabase.Text = "ReplicacionDemo";
            rbWindowsAuth.Checked = true;
            txtUser.Text = "sa";
            txtPass.Text = "";
        }

        private void ToggleAuthFields()
        {
            bool useSqlAuth = rbSqlAuth.Checked;
            pnlSqlAuth.Visible = useSqlAuth;
            Height = 500;

            txtUser.Enabled = useSqlAuth;
            txtPass.Enabled = useSqlAuth;

            if (useSqlAuth)
            {
                txtUser.Focus();
            }
        }

        private void BuildConnectionString()
        {
            string server = txtServer.Text.Trim();
            string db = txtDatabase.Text.Trim();

            if (string.IsNullOrWhiteSpace(server))
                throw new Exception("El campo Servidor es requerido.");

            if (string.IsNullOrWhiteSpace(db))
                throw new Exception("El campo Base de Datos es requerido.");

            if (rbWindowsAuth.Checked)
            {
                _conn.ConfigureWindowsAuth(server, db);
            }
            else
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text;

                if (string.IsNullOrWhiteSpace(user))
                    throw new Exception("El campo Usuario es requerido.");

                _conn.ConfigureSqlAuth(server, db, user, pass);
            }
        }

        private void BtnSaveConnection_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtNombre.Text.Trim();
                string server = txtServer.Text.Trim();
                string db = txtDatabase.Text.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("El campo Nombre es requerido.");

                if (string.IsNullOrWhiteSpace(server))
                    throw new Exception("El campo Servidor es requerido.");

                if (string.IsNullOrWhiteSpace(db))
                    throw new Exception("El campo Base de Datos es requerido.");

                var newConnection = new SavedConnection
                {
                    Name = name,
                    Server = server,
                    Database = db,
                    UseWindowsAuth = rbWindowsAuth.Checked,
                    Username = rbSqlAuth.Checked ? txtUser.Text.Trim() : null
                };

                if (_selectedConnection != null)
                {
                    newConnection.Id = _selectedConnection.Id;
                }

                ConnectionManager.AddConnection(newConnection);
                _savedConnections = ConnectionManager.LoadConnections();
                LoadConnectionsList();

                lblEstado.Text = "● Conexión guardada";
                lblEstado.ForeColor = Color.Green;

                MessageBox.Show(
                    "La conexión se ha guardado correctamente.",
                    "Conexión Guardada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnProbar_Click(object sender, EventArgs e)
        {
            try
            {
                lblEstado.Text = "● Probando conexión...";
                lblEstado.ForeColor = Color.Orange;
                btnProbar.Enabled = false;
                btnConectar.Enabled = false;
                Application.DoEvents();

                BuildConnectionString();
                _conn.TestConnection();

                lblEstado.Text = "● Conexión exitosa";
                lblEstado.ForeColor = Color.Green;

                MessageBox.Show(
                    "La conexión se estableció correctamente.",
                    "Conexión Exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblEstado.Text = "● Error de conexión";
                lblEstado.ForeColor = Color.Red;

                string errorMsg = ex.Message;

                if (errorMsg.Contains("network-related") || errorMsg.Contains("transport-level"))
                {
                    errorMsg = "No se pudo conectar al servidor.\n\n" +
                               "Verifica que:\n" +
                               "• El nombre del servidor sea correcto\n" +
                               "• SQL Server esté ejecutándose\n" +
                               "• El firewall permita la conexión";
                }
                else if (errorMsg.Contains("Login failed"))
                {
                    errorMsg = "Error de autenticación.\n\n" +
                               "Verifica que:\n" +
                               "• El usuario y contraseña sean correctos\n" +
                               "• SQL Server Authentication esté habilitado (Mixed Mode)\n" +
                               "• El usuario tenga permisos en la base de datos";
                }
                else if (errorMsg.Contains("Cannot open database"))
                {
                    errorMsg = "No se pudo abrir la base de datos.\n\n" +
                               "Verifica que:\n" +
                               "• El nombre de la base de datos sea correcto\n" +
                               "• La base de datos exista en el servidor\n" +
                               "• Tengas permisos para acceder a ella";
                }

                MessageBox.Show(
                    errorMsg,
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnProbar.Enabled = true;
                btnConectar.Enabled = true;
            }
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                lblEstado.Text = "● Conectando...";
                lblEstado.ForeColor = Color.Orange;
                btnProbar.Enabled = false;
                btnConectar.Enabled = false;
                Application.DoEvents();

                BuildConnectionString();
                _conn.TestConnection();

                lblEstado.Text = "● Conectado";
                lblEstado.ForeColor = Color.Green;

                if (_selectedConnection != null)
                {
                    ConnectionManager.UpdateLastUsed(_selectedConnection.Id);
                }

                var main = new MainForm(_conn);
                main.FormClosed += (s, args) => Application.Exit();
                main.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblEstado.Text = "● Error al conectar";
                lblEstado.ForeColor = Color.Red;
                btnProbar.Enabled = true;
                btnConectar.Enabled = true;

                string errorMsg = ex.Message;

                if (errorMsg.Contains("network-related") || errorMsg.Contains("transport-level"))
                {
                    errorMsg = "No se pudo conectar al servidor.\n\n" +
                               "Verifica que:\n" +
                               "• El nombre del servidor sea correcto\n" +
                               "• SQL Server esté ejecutándose\n" +
                               "• El firewall permita la conexión";
                }
                else if (errorMsg.Contains("Login failed"))
                {
                    errorMsg = "Error de autenticación.\n\n" +
                               "Verifica que:\n" +
                               "• El usuario y contraseña sean correctos\n" +
                               "• SQL Server Authentication esté habilitado (Mixed Mode)\n" +
                               "• El usuario tenga permisos en la base de datos";
                }

                MessageBox.Show(
                    errorMsg,
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private Label MkLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 4),
                Size = new Size(150, 20),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9)
            };
        }

        private TextBox MkTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 9)
            };
        }
    }
}