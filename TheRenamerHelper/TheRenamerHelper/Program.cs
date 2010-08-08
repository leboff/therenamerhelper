using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TheRenamerHelper
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
            HelperTray ht = new HelperTray();
            Application.Run();
        }
    }
}
