namespace Essence_graphics.Windows
{
    partial class Reduce_Request
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
            this.M = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dis = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.freq = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.mincoef = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.maxcoef = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // M
            // 
            this.M.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.M.Location = new System.Drawing.Point(107, 15);
            this.M.Margin = new System.Windows.Forms.Padding(4);
            this.M.Name = "M";
            this.M.Size = new System.Drawing.Size(112, 26);
            this.M.TabIndex = 1;
            this.M.Text = "0.5";
            this.M.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputChecker);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(12, 193);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 5;
            this.button1.Text = "Ok";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(120, 193);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(19, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "M";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 57);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "dis";
            // 
            // dis
            // 
            this.dis.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dis.Location = new System.Drawing.Point(107, 50);
            this.dis.Margin = new System.Windows.Forms.Padding(4);
            this.dis.Name = "dis";
            this.dis.Size = new System.Drawing.Size(112, 26);
            this.dis.TabIndex = 9;
            this.dis.Text = "0.5";
            this.dis.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputChecker);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 92);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "Freq";
            // 
            // freq
            // 
            this.freq.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.freq.Location = new System.Drawing.Point(107, 86);
            this.freq.Margin = new System.Windows.Forms.Padding(4);
            this.freq.Name = "freq";
            this.freq.Size = new System.Drawing.Size(112, 26);
            this.freq.TabIndex = 11;
            this.freq.Text = "0.5";
            this.freq.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputChecker);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 128);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "MinCoef";
            // 
            // mincoef
            // 
            this.mincoef.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.mincoef.Location = new System.Drawing.Point(107, 122);
            this.mincoef.Margin = new System.Windows.Forms.Padding(4);
            this.mincoef.Name = "mincoef";
            this.mincoef.Size = new System.Drawing.Size(112, 26);
            this.mincoef.TabIndex = 13;
            this.mincoef.Text = "-1";
            this.mincoef.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputChecker);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 164);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 17);
            this.label5.TabIndex = 14;
            this.label5.Text = "MaxCoef";
            // 
            // maxcoef
            // 
            this.maxcoef.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.maxcoef.Location = new System.Drawing.Point(107, 158);
            this.maxcoef.Margin = new System.Windows.Forms.Padding(4);
            this.maxcoef.Name = "maxcoef";
            this.maxcoef.Size = new System.Drawing.Size(112, 26);
            this.maxcoef.TabIndex = 15;
            this.maxcoef.Text = "-1";
            this.maxcoef.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputChecker);
            // 
            // Reduce_Request
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(231, 229);
            this.ControlBox = false;
            this.Controls.Add(this.maxcoef);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.mincoef);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.freq);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dis);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.M);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Reduce_Request";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Reduce coefficients";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.TextBox M;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox dis;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox freq;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox mincoef;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox maxcoef;
    }
}