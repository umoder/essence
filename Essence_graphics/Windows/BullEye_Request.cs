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
    public partial class BullEye_Request : Form
    {
        public BullEye_Request()
        {
            InitializeComponent();
        }

        private void InputChecker(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.OemPeriod:
                case Keys.Oemcomma:
                case Keys.Decimal:
                case Keys.OemMinus:
                case Keys.Subtract:
                case Keys.Left:
                case Keys.Right:
                case Keys.Back:
                case Keys.Delete:
                case Keys.Enter:
                case Keys.Escape:
                    e.SuppressKeyPress = false;
                    break;
                default:
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double mult;
            double add;
            if (!double.TryParse(Mult.Text, out mult))
            {
                MessageBox.Show("Wrong string in Multiply field");
                DialogResult = DialogResult.None;
                return;
            }
            if (!double.TryParse(Add.Text, out add))
            {
                MessageBox.Show("Wrong string in Addendum field");
                DialogResult = DialogResult.None;
                return;
            }
        }
    }
}
