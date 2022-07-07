using System;
using System.Windows.Forms;

namespace MsgBox
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
