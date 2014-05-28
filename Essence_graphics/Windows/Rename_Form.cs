using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Essence_graphics.Windows
{
    public partial class Rename_Form : Form
    {
        public Rename_Form()
        {
            InitializeComponent();
        }

        private void B_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void B_Ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
