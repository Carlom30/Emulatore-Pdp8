using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emulatore_Pdp8;
using PrintSpace;
using Assembler;
using UtilityStuff;

namespace Emulatore_Pdp8
{
    public partial class Form1 : Form
    {
        /*reference: https://stackoverflow.com/questions/13505248/how-to-make-autoscroll-multiline-textbox-in-winforms
                     https://stackoverflow.com/questions/19011948/how-to-add-scrollbars-in-c-sharp-form
                     https://stackoverflow.com/questions/1302804/how-do-i-add-a-newline-to-a-windows-forms-textbox new line
                     
         */
        OpenFileDialog sourceFile = null;

        private void update()
        {
            Printf.inizializeBuffers();
            Program.source = Program.readSource(sourceFile);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //update();
            RAM.ScrollBars = ScrollBars.Both;
            RAM.WordWrap = false;
            RAM.ReadOnly = true;

            LOG.ScrollBars = ScrollBars.Both;
            LOG.WordWrap = false;
            LOG.ReadOnly = true;

            REGISTERS.ScrollBars = ScrollBars.Both;
            REGISTERS.WordWrap = false;
            REGISTERS.ReadOnly = true;
            //textBox1.Text = string.Join("\n", strings);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //RAM
            
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //LOG
            
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //REGISTERS
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //compile button
            if (sourceFile == null)
            {
                LOG.AppendText("no file selected, click 'add source' and select a .txt file" + Environment.NewLine);
                return;
            }

            update();
            Compiler.Compile(Program.source);
            CompilerData data = Compiler.getCompilerData();

            if (data == null || !data.completed)
            {
                string[] logBuffer = Printf.getLogBuffer();
                for(int i = 0; i < logBuffer.Length; i++)
                {
                    LOG.AppendText(logBuffer[i] + Environment.NewLine);
                }
            }

            else
            {
                string[] ramBuffer = Printf.getRamBuffer();
                RAM.Text = "";
                for(int i = 0; i < 0x1a; i++)
                {
                    RAM.AppendText(ramBuffer[i] + Environment.NewLine);
                }
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            //exit
            Close();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //a++;
            //REGISTERS.Text = a.ToString();
            sourceFile = new OpenFileDialog();
            if(sourceFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] Localsourcefile = File.ReadAllLines(sourceFile.FileName);
                    if (Localsourcefile.Length == 0)
                    {
                        LOG.AppendText("source can't be empty" + Environment.NewLine);
                        sourceFile = null;
                        return;
                    }

                    for (int j = 0; j < Localsourcefile.Length; j++)
                    {
                        LOG.AppendText(Localsourcefile[j] + Environment.NewLine);
                    }
                }

                catch
                {
                    Console.WriteLine("something went wrong");
                    Close();
                }
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            LOG.Text = "";
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

    }
}
