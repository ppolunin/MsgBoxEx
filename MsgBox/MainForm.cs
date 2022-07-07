using System;
using System.Windows.Forms;

namespace MsgBox
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Icon = MessageBoxEx.IconResource.Application.ToIcon();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBoxEx.Show("text", null, MessageBoxButtonsEx.OK, AppRes.LoadIcon(2));
        }
    }
}
