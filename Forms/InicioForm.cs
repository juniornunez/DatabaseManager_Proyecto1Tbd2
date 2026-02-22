using System;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;

namespace DatabaseManager.Forms
{
    public class InicioForm : Form
    {
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
        private Label lblEstado;

        private readonly ConnectionService _conn = new ConnectionService();

        public InicioForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Database Manager - Conexión";
            Size = new Size(650, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;

            var panel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(600, 440),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = "Detalles de la Conexión",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(580, 30),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            int y = 55;

            panel.Controls.Add(MkLabel("Nombre:", 10, y));
            txtNombre = MkTextBox(170, y);
            txtNombre.Text = "ReplicacionDemo";
            panel.Controls.Add(txtNombre);
            y += 40;

            panel.Controls.Add(MkLabel("Servidor:", 10, y));
            txtServer = MkTextBox(170, y);
            txtServer.Text = "DESKTOP-S4DNI64";
            panel.Controls.Add(txtServer);

            var btnServerHelp = new Button
            {
                Text = "?",
                Location = new Point(530, y - 2),
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
            panel.Controls.Add(btnServerHelp);
            y += 40;

            panel.Controls.Add(MkLabel("Base de Datos:", 10, y));
            txtDatabase = MkTextBox(170, y);
            txtDatabase.Text = "ReplicacionDemo";
            panel.Controls.Add(txtDatabase);
            y += 50;

            var separator1 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Location = new Point(10, y),
                Size = new Size(580, 2)
            };
            panel.Controls.Add(separator1);
            y += 15;

            var lblAuthTitle = new Label
            {
                Text = "Tipo de Autenticación:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, y),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panel.Controls.Add(lblAuthTitle);
            y += 30;

            rbWindowsAuth = new RadioButton
            {
                Text = "Windows Authentication",
                Location = new Point(170, y),
                Size = new Size(300, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };
            rbWindowsAuth.CheckedChanged += (_, __) => ToggleAuthFields();
            panel.Controls.Add(rbWindowsAuth);
            y += 30;

            rbSqlAuth = new RadioButton
            {
                Text = "SQL Server Authentication",
                Location = new Point(170, y),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 9)
            };
            rbSqlAuth.CheckedChanged += (_, __) => ToggleAuthFields();
            panel.Controls.Add(rbSqlAuth);
            y += 35;

            pnlSqlAuth = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(600, 90),
                BackColor = Color.FromArgb(245, 245, 245),
                Visible = false
            };

            pnlSqlAuth.Controls.Add(MkLabel("Usuario:", 10, 10));
            txtUser = MkTextBox(170, 10);
            txtUser.Text = "sa";
            pnlSqlAuth.Controls.Add(txtUser);

            pnlSqlAuth.Controls.Add(MkLabel("Contraseña:", 10, 50));
            txtPass = MkTextBox(170, 50);
            txtPass.UseSystemPasswordChar = true;
            pnlSqlAuth.Controls.Add(txtPass);

            panel.Controls.Add(pnlSqlAuth);
            y += 100;

            var separator2 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Location = new Point(10, y),
                Size = new Size(580, 2)
            };
            panel.Controls.Add(separator2);
            y += 20;

            btnProbar = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(170, y),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(100, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnProbar.FlatAppearance.BorderSize = 0;
            btnProbar.Click += BtnProbar_Click;

            btnConectar = new Button
            {
                Text = "Conectar",
                Location = new Point(335, y),
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
                Location = new Point(10, y + 10),
                Size = new Size(580, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(btnProbar);
            panel.Controls.Add(btnConectar);
            panel.Controls.Add(lblEstado);

            Controls.Add(panel);

            ToggleAuthFields();
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