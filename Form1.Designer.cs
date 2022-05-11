
namespace Emulatore_Pdp8
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RAM = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CompilerButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.LOG = new System.Windows.Forms.TextBox();
            this.REGISTERS = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.CleanLog = new System.Windows.Forms.Button();
            this.plus = new System.Windows.Forms.Button();
            this.mines = new System.Windows.Forms.Button();
            this.nSteps = new System.Windows.Forms.TextBox();
            this.Slabel_nSteps = new System.Windows.Forms.Label();
            this.checkBox_StepByStep = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // RAM
            // 
            this.RAM.BackColor = System.Drawing.SystemColors.WindowText;
            this.RAM.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RAM.ForeColor = System.Drawing.SystemColors.Window;
            this.RAM.Location = new System.Drawing.Point(12, 49);
            this.RAM.Multiline = true;
            this.RAM.Name = "RAM";
            this.RAM.Size = new System.Drawing.Size(680, 377);
            this.RAM.TabIndex = 0;
            this.RAM.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "RAM";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // CompilerButton
            // 
            this.CompilerButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CompilerButton.Location = new System.Drawing.Point(617, 537);
            this.CompilerButton.Name = "CompilerButton";
            this.CompilerButton.Size = new System.Drawing.Size(75, 34);
            this.CompilerButton.TabIndex = 2;
            this.CompilerButton.Text = "Compile";
            this.CompilerButton.UseVisualStyleBackColor = true;
            this.CompilerButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 452);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "LOG";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // LOG
            // 
            this.LOG.BackColor = System.Drawing.SystemColors.WindowText;
            this.LOG.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LOG.ForeColor = System.Drawing.SystemColors.Window;
            this.LOG.Location = new System.Drawing.Point(12, 475);
            this.LOG.Multiline = true;
            this.LOG.Name = "LOG";
            this.LOG.Size = new System.Drawing.Size(583, 228);
            this.LOG.TabIndex = 4;
            this.LOG.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // REGISTERS
            // 
            this.REGISTERS.BackColor = System.Drawing.SystemColors.WindowText;
            this.REGISTERS.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.REGISTERS.ForeColor = System.Drawing.SystemColors.Window;
            this.REGISTERS.Location = new System.Drawing.Point(733, 49);
            this.REGISTERS.Multiline = true;
            this.REGISTERS.Name = "REGISTERS";
            this.REGISTERS.Size = new System.Drawing.Size(394, 377);
            this.REGISTERS.TabIndex = 5;
            this.REGISTERS.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(729, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "REGISTERS";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(617, 669);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 34);
            this.button1.TabIndex = 7;
            this.button1.Text = "Exit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(617, 475);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 56);
            this.button2.TabIndex = 8;
            this.button2.Text = "Add Source";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // CleanLog
            // 
            this.CleanLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CleanLog.Location = new System.Drawing.Point(12, 709);
            this.CleanLog.Name = "CleanLog";
            this.CleanLog.Size = new System.Drawing.Size(94, 29);
            this.CleanLog.TabIndex = 9;
            this.CleanLog.Text = "Log Del";
            this.CleanLog.UseVisualStyleBackColor = true;
            this.CleanLog.Click += new System.EventHandler(this.button3_Click);
            // 
            // plus
            // 
            this.plus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.plus.Location = new System.Drawing.Point(824, 538);
            this.plus.Name = "plus";
            this.plus.Size = new System.Drawing.Size(34, 34);
            this.plus.TabIndex = 11;
            this.plus.Text = "+";
            this.plus.UseVisualStyleBackColor = true;
            this.plus.Click += new System.EventHandler(this.plus_Click);
            // 
            // mines
            // 
            this.mines.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mines.Location = new System.Drawing.Point(733, 538);
            this.mines.Name = "mines";
            this.mines.Size = new System.Drawing.Size(34, 34);
            this.mines.TabIndex = 12;
            this.mines.Text = "-";
            this.mines.UseVisualStyleBackColor = true;
            this.mines.Click += new System.EventHandler(this.mines_Click);
            // 
            // nSteps
            // 
            this.nSteps.BackColor = System.Drawing.SystemColors.WindowText;
            this.nSteps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nSteps.ForeColor = System.Drawing.SystemColors.Window;
            this.nSteps.Location = new System.Drawing.Point(773, 538);
            this.nSteps.Multiline = true;
            this.nSteps.Name = "nSteps";
            this.nSteps.Size = new System.Drawing.Size(45, 34);
            this.nSteps.TabIndex = 13;
            this.nSteps.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nSteps.TextChanged += new System.EventHandler(this.nSteps_TextChanged);
            // 
            // Slabel_nSteps
            // 
            this.Slabel_nSteps.AutoSize = true;
            this.Slabel_nSteps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Slabel_nSteps.Location = new System.Drawing.Point(769, 515);
            this.Slabel_nSteps.Name = "Slabel_nSteps";
            this.Slabel_nSteps.Size = new System.Drawing.Size(60, 20);
            this.Slabel_nSteps.TabIndex = 14;
            this.Slabel_nSteps.Text = "nSteps";
            // 
            // checkBox_StepByStep
            // 
            this.checkBox_StepByStep.AutoSize = true;
            this.checkBox_StepByStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox_StepByStep.Location = new System.Drawing.Point(733, 476);
            this.checkBox_StepByStep.Name = "checkBox_StepByStep";
            this.checkBox_StepByStep.Size = new System.Drawing.Size(122, 24);
            this.checkBox_StepByStep.TabIndex = 15;
            this.checkBox_StepByStep.Text = "Step By Step";
            this.checkBox_StepByStep.UseVisualStyleBackColor = true;
            this.checkBox_StepByStep.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(1273, 771);
            this.Controls.Add(this.checkBox_StepByStep);
            this.Controls.Add(this.Slabel_nSteps);
            this.Controls.Add(this.nSteps);
            this.Controls.Add(this.mines);
            this.Controls.Add(this.plus);
            this.Controls.Add(this.CleanLog);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.REGISTERS);
            this.Controls.Add(this.LOG);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CompilerButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RAM);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox RAM;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CompilerButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox LOG;
        private System.Windows.Forms.TextBox REGISTERS;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button CleanLog;
        private System.Windows.Forms.Button plus;
        private System.Windows.Forms.Button mines;
        private System.Windows.Forms.TextBox nSteps;
        private System.Windows.Forms.Label Slabel_nSteps;
        private System.Windows.Forms.CheckBox checkBox_StepByStep;
    }
}

