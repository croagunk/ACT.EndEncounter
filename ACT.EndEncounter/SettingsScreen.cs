using System;
using System.Windows.Forms;

namespace ACT.EndEncounter
{
    public partial class SettingsScreen : UserControl
    {
        public SettingsScreen()
        {
            InitializeComponent();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void SettingsScreen_Click(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
    }
}
