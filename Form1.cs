using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emulatore_Pdp8
{
    public partial class Form1 : Form
    {
        /*reference: https://stackoverflow.com/questions/13505248/how-to-make-autoscroll-multiline-textbox-in-winforms
                     https://stackoverflow.com/questions/19011948/how-to-add-scrollbars-in-c-sharp-form
         */

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.WordWrap = false;
        }
    }
}
