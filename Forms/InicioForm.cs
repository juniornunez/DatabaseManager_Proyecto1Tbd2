using System;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;

namespace DatabaseManager.Forms
{
    public class InicioForm : Form
    {
        TextBox txtNombre;
        TextBox txtServer;
        TextBox txtDatabase;
        CheckBox chkWindowsAuth;
        TextBox txtUser;
        TextBox txtPass;
        Button btnProbar;
        Button btnConectar;
        Label lblEstado;

        private readonly ConnectionService _conn = new ConnectionService();

        public InicioForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Inicio - Conexión a SQL Server";
            Size = new Size(650, 380);
            StartPosition = FormStartPosition.CenterScreen;

            // Solo un panel (antes era el rightPanel)
            var panel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(610, 320),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblDetails = new Label
            {
                Text = "Setup de Conexion",
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            int y = 45;

            panel.Controls.Add(MkLabel("Nombre:", 10, y));
            txtNombre = MkTextBox(140, y);
            txtNombre.Text = "Conexion1";
            panel.Controls.Add(txtNombre);
            y += 35;

            panel.Controls.Add(MkLabel("Servidor:", 10, y));
            txtServer = MkTextBox(140, y);
            txtServer.Text = "DESKTOP-S4DNI64";
            panel.Controls.Add(txtServer);
            y += 35;

            panel.Controls.Add(MkLabel("Base de datos:", 10, y));
            txtDatabase = MkTextBox(140, y);
            txtDatabase.Text = "ReplicacionDemo";
            panel.Controls.Add(txtDatabase);
            y += 35;

            chkWindowsAuth = new CheckBox
            {
                Text = "Windows Authentication",
                Location = new Point(140, y),
                AutoSize = true,
                Checked = true
            };
            chkWindowsAuth.CheckedChanged += (_, __) => ToggleAuthFields();
            panel.Controls.Add(chkWindowsAuth);
            y += 35;

            panel.Controls.Add(MkLabel("Usuario:", 10, y));
            txtUser = MkTextBox(140, y);
            panel.Controls.Add(txtUser);
            y += 35;

            panel.Controls.Add(MkLabel("Contraseña:", 10, y));
            txtPass = MkTextBox(140, y);
            txtPass.UseSystemPasswordChar = true;
            panel.Controls.Add(txtPass);
            y += 45;

            btnProbar = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(140, y),
                Size = new Size(140, 32)
            };
            btnProbar.Click += BtnProbar_Click;

            btnConectar = new Button
            {
                Text = "Conectar",
                Location = new Point(295, y),
                Size = new Size(120, 32)
            };
            btnConectar.Click += BtnConectar_Click;

            lblEstado = new Label
            {
                Text = "Estado: listo",
                Location = new Point(10, 285),
                AutoSize = true
            };

            panel.Controls.Add(btnProbar);
            panel.Controls.Add(btnConectar);
            panel.Controls.Add(lblEstado);
            panel.Controls.Add(lblDetails);

            Controls.Add(panel);

            ToggleAuthFields();
        }

        private void ToggleAuthFields()
        {
            bool win = chkWindowsAuth.Checked;
            txtUser.Enabled = !win;
            txtPass.Enabled = !win;
        }

        private void BuildConnectionString()
        {
            string server = txtServer.Text.Trim();
            string db = txtDatabase.Text.Trim();

            if (string.IsNullOrWhiteSpace(server)) throw new Exception("Servidor vacío.");
            if (string.IsNullOrWhiteSpace(db)) throw new Exception("Base de datos vacía.");

            if (chkWindowsAuth.Checked)
            {
                _conn.ConfigureWindowsAuth(server, db);
            }
            else
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text;
                if (string.IsNullOrWhiteSpace(user)) throw new Exception("Usuario vacío.");
                _conn.ConfigureSqlAuth(server, db, user, pass);
            }
        }

        private void BtnProbar_Click(object sender, EventArgs e)
        {
            try
            {
                BuildConnectionString();
                _conn.TestConnection();
                lblEstado.Text = "Estado: conexión OK";
                MessageBox.Show("Conexión exitosa.");
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Estado: error";
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                BuildConnectionString();
                _conn.TestConnection();
                lblEstado.Text = "Estado: conectado";

                var main = new MainForm(_conn);
                main.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Estado: error al conectar";
                MessageBox.Show(ex.Message);
            }
        }

        private Label MkLabel(string text, int x, int y)
            => new Label { Text = text, Location = new Point(x, y + 4), AutoSize = true };

        private TextBox MkTextBox(int x, int y)
            => new TextBox { Location = new Point(x, y), Size = new Size(350, 23) };
    }
}

