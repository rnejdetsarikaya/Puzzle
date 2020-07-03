using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yazlap2_1
{
    public partial class Form2 : Form
    {
        public static PuzzleForm form;
        public Form2(PuzzleForm form1)
        {
            InitializeComponent();
            form = form1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form.SaveScore(textBox3.Text);
            form.Enabled = true;
            this.Close();
        }
    }
}