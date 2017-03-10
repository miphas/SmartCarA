namespace Smart_Car
{
    partial class Serial_Config_Window
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.urg_port_com = new System.Windows.Forms.ComboBox();
            this.urg_port_baud = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.con_port_baud = new System.Windows.Forms.ComboBox();
            this.con_port_com = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dr_port_baud = new System.Windows.Forms.ComboBox();
            this.dr_port_com = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cam_port_baud = new System.Windows.Forms.ComboBox();
            this.cam_port_com = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.Save_Serial_Config = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "串口号：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "波特率：";
            // 
            // urg_port_com
            // 
            this.urg_port_com.FormattingEnabled = true;
            this.urg_port_com.Location = new System.Drawing.Point(82, 28);
            this.urg_port_com.Name = "urg_port_com";
            this.urg_port_com.Size = new System.Drawing.Size(97, 20);
            this.urg_port_com.TabIndex = 2;
            // 
            // urg_port_baud
            // 
            this.urg_port_baud.FormattingEnabled = true;
            this.urg_port_baud.Location = new System.Drawing.Point(82, 75);
            this.urg_port_baud.Name = "urg_port_baud";
            this.urg_port_baud.Size = new System.Drawing.Size(97, 20);
            this.urg_port_baud.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.urg_port_baud);
            this.groupBox1.Controls.Add(this.urg_port_com);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(14, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(199, 111);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "激光雷达串口";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.con_port_baud);
            this.groupBox2.Controls.Add(this.con_port_com);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(237, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(199, 111);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "控制串口";
            // 
            // con_port_baud
            // 
            this.con_port_baud.FormattingEnabled = true;
            this.con_port_baud.Location = new System.Drawing.Point(82, 75);
            this.con_port_baud.Name = "con_port_baud";
            this.con_port_baud.Size = new System.Drawing.Size(97, 20);
            this.con_port_baud.TabIndex = 3;
            // 
            // con_port_com
            // 
            this.con_port_com.FormattingEnabled = true;
            this.con_port_com.Location = new System.Drawing.Point(82, 28);
            this.con_port_com.Name = "con_port_com";
            this.con_port_com.Size = new System.Drawing.Size(97, 20);
            this.con_port_com.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "波特率：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "串口号：";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dr_port_baud);
            this.groupBox3.Controls.Add(this.dr_port_com);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Location = new System.Drawing.Point(14, 149);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(199, 111);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "编码器串口";
            // 
            // dr_port_baud
            // 
            this.dr_port_baud.FormattingEnabled = true;
            this.dr_port_baud.Location = new System.Drawing.Point(82, 75);
            this.dr_port_baud.Name = "dr_port_baud";
            this.dr_port_baud.Size = new System.Drawing.Size(97, 20);
            this.dr_port_baud.TabIndex = 3;
            // 
            // dr_port_com
            // 
            this.dr_port_com.FormattingEnabled = true;
            this.dr_port_com.Location = new System.Drawing.Point(82, 28);
            this.dr_port_com.Name = "dr_port_com";
            this.dr_port_com.Size = new System.Drawing.Size(97, 20);
            this.dr_port_com.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "波特率：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "串口号：";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cam_port_baud);
            this.groupBox4.Controls.Add(this.cam_port_com);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(237, 149);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(199, 111);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "摄像头串口";
            // 
            // cam_port_baud
            // 
            this.cam_port_baud.FormattingEnabled = true;
            this.cam_port_baud.Location = new System.Drawing.Point(82, 75);
            this.cam_port_baud.Name = "cam_port_baud";
            this.cam_port_baud.Size = new System.Drawing.Size(97, 20);
            this.cam_port_baud.TabIndex = 3;
            // 
            // cam_port_com
            // 
            this.cam_port_com.FormattingEnabled = true;
            this.cam_port_com.Location = new System.Drawing.Point(82, 28);
            this.cam_port_com.Name = "cam_port_com";
            this.cam_port_com.Size = new System.Drawing.Size(97, 20);
            this.cam_port_com.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 78);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 1;
            this.label7.Text = "波特率：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "串口号：";
            // 
            // Save_Serial_Config
            // 
            this.Save_Serial_Config.Location = new System.Drawing.Point(298, 269);
            this.Save_Serial_Config.Name = "Save_Serial_Config";
            this.Save_Serial_Config.Size = new System.Drawing.Size(97, 23);
            this.Save_Serial_Config.TabIndex = 8;
            this.Save_Serial_Config.Text = "保存配置";
            this.Save_Serial_Config.UseVisualStyleBackColor = true;
            this.Save_Serial_Config.Click += new System.EventHandler(this.Save_Serial_Config_Click);
            // 
            // Serial_Config_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 304);
            this.Controls.Add(this.Save_Serial_Config);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Serial_Config_Window";
            this.Text = "串口配置";
            this.Load += new System.EventHandler(this.Serial_Config_Window_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox urg_port_com;
        private System.Windows.Forms.ComboBox urg_port_baud;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox con_port_baud;
        private System.Windows.Forms.ComboBox con_port_com;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox dr_port_baud;
        private System.Windows.Forms.ComboBox dr_port_com;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox cam_port_baud;
        private System.Windows.Forms.ComboBox cam_port_com;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button Save_Serial_Config;
    }
}