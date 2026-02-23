using System;
using System.Windows.Forms;
using DatabaseManager.Forms;

namespace DatabaseManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool continuar = true;

            while (continuar)
            {
                using (var inicioForm = new InicioForm())
                {
                    var resultado = inicioForm.ShowDialog();

                    if (resultado == DialogResult.OK)
                    {
                        var conn = inicioForm.GetConnectionService();
                        using (var mainForm = new MainForm(conn))
                        {
                            mainForm.ShowDialog();
                        }
                    }
                    else
                    {
                        continuar = false;
                    }
                }
            }
        }
    }
}