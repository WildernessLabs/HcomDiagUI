using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HcomDiagUi
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // .NET changed the WinForms default font to Segoe UI 9pt. Keep the
            // .NET Framework default so the form (designed with AutoScaleMode.Font)
            // lays out as it did before.
            Application.SetDefaultFont(new Font(new FontFamily("Microsoft Sans Serif"), 8.25f));

            Application.Run(new HcomDiagUiForm());
        }
    }
}
