using System;
using System.Windows.Forms;
using DatabaseManager.Forms;

namespace DatabaseManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new InicioForm());
        }
    }
}

