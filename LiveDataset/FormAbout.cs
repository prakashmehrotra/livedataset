// About form

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace LiveDataset
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            lblVersion.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void FormAbout_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
