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
    public partial class Export : Form
    {
        Main_Form MF;
        CModel Model;

        public Export(object sender, object model)
        {
            MF = (Main_Form)sender;
            Model = (CModel)model;
            InitializeComponent();
            foreach (CModel.DProperty prop in Model.Props)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                row.Cells[0].Value = prop.Name;
                row.Cells[1].Value = prop.Name + "_out.txt";
                dataGridView1.Rows.Add(row);
            }
        }
    }
}
