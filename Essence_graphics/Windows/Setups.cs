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
    public partial class Setups : Form
    {
        private CModel Model;

        public Setups(object sender)
        {
            InitializeComponent();
            Model = (CModel)sender;
            List<string> temp=new List<string>();
            foreach (string str in Model.gridsToLook)
                temp.Add(str);
            textBox1.Lines = temp.ToArray();
        }
    }
}
