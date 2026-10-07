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
            // DPI-unaware: the form is always laid out at 100% (96 DPI), exactly as
            // in the Designer, and Windows stretches it on scaled screens (sharp on a
            // 100% monitor, slightly blurry on e.g. a 250% laptop screen). This was
            // the .NET Framework behavior. PerMonitorV2 was tried: re-scaling the
            // live form when it moved from a 250% to a 100% monitor broke the layout.
            // The Designer must also run DPI-unaware (VS 'devenv /noScale') so the
            // layout stays stored at 100%. Must be called before any window is created.
            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);

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
