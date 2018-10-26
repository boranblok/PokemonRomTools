using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EndianSwapper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerateREF_Click(object sender, EventArgs e)
        {
            var ptr = txtPointer.Text;
            txtReference.Text = ptr.Substring(4, 2) + ptr.Substring(2, 2) + ptr.Substring(0, 2) + "08";
        }

        private void btnGeneratePointer_Click(object sender, EventArgs e)
        {
            var rfr = txtReference.Text;
            txtPointer.Text = rfr.Substring(4, 2) + rfr.Substring(2, 2) + rfr.Substring(0, 2);
        }
    }
}
