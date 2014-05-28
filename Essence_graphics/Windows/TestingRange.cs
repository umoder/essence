using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Essence_graphics.Windows
{
    public partial class TestingRange : Form
    {
        public TestingRange(Bitmap BM)
        {
            InitializeComponent();
            pictureBox1.Image = BM;
        }
    }
}
