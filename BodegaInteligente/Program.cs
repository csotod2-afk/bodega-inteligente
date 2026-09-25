using System;
using System.Windows.Forms;

namespace BodegaInteligente
{
    static class Program
    {
        /// <summary>Punto de entrada de la aplicación.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
