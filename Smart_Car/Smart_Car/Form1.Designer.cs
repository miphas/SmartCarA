namespace Smart_Car
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menu_interface = new System.Windows.Forms.MenuStrip();
            this.串口配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.地图配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.数据显示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.alarm_list = new System.Windows.Forms.ListView();
            this.draw_map = new System.Windows.Forms.Panel();
            this.system_label = new System.Windows.Forms.Label();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.system_light = new Microsoft.VisualBasic.PowerPacks.OvalShape();
            this.label1 = new System.Windows.Forms.Label();
            this.map_select = new System.Windows.Forms.ComboBox();
            this.size_bar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.menu_interface.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.size_bar)).BeginInit();
            this.SuspendLayout();
            // 
            // menu_interface
            // 
            this.menu_interface.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.串口配置ToolStripMenuItem,
            this.地图配置ToolStripMenuItem,
            this.数据显示ToolStripMenuItem});
            this.menu_interface.Location = new System.Drawing.Point(0, 0);
            this.menu_interface.Name = "menu_interface";
            this.menu_interface.Size = new System.Drawing.Size(810, 25);
            this.menu_interface.TabIndex = 0;
            this.menu_interface.Text = "menuStrip1";
            // 
            // 串口配置ToolStripMenuItem
            // 
            this.串口配置ToolStripMenuItem.Name = "串口配置ToolStripMenuItem";
            this.串口配置ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.串口配置ToolStripMenuItem.Text = "串口配置";
            this.串口配置ToolStripMenuItem.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Serial_Config);
            // 
            // 地图配置ToolStripMenuItem
            // 
            this.地图配置ToolStripMenuItem.Name = "地图配置ToolStripMenuItem";
            this.地图配置ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.地图配置ToolStripMenuItem.Text = "地图配置";
            this.地图配置ToolStripMenuItem.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Map_Config);
            // 
            // 数据显示ToolStripMenuItem
            // 
            this.数据显示ToolStripMenuItem.Name = "数据显示ToolStripMenuItem";
            this.数据显示ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.数据显示ToolStripMenuItem.Text = "数据显示";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Location = new System.Drawing.Point(664, 350);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(114, 102);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Start_Car);
            // 
            // alarm_list
            // 
            this.alarm_list.Location = new System.Drawing.Point(13, 477);
            this.alarm_list.Name = "alarm_list";
            this.alarm_list.Size = new System.Drawing.Size(785, 86);
            this.alarm_list.TabIndex = 2;
            this.alarm_list.UseCompatibleStateImageBehavior = false;
            this.alarm_list.View = System.Windows.Forms.View.Details;
            // 
            // draw_map
            // 
            this.draw_map.BackColor = System.Drawing.SystemColors.Info;
            this.draw_map.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.draw_map.Location = new System.Drawing.Point(16, 30);
            this.draw_map.Name = "draw_map";
            this.draw_map.Size = new System.Drawing.Size(626, 423);
            this.draw_map.TabIndex = 3;
            // 
            // system_label
            // 
            this.system_label.AutoSize = true;
            this.system_label.Location = new System.Drawing.Point(689, 43);
            this.system_label.Name = "system_label";
            this.system_label.Size = new System.Drawing.Size(89, 12);
            this.system_label.TabIndex = 4;
            this.system_label.Text = "系统状态：等待";
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.system_light});
            this.shapeContainer1.Size = new System.Drawing.Size(810, 575);
            this.shapeContainer1.TabIndex = 5;
            this.shapeContainer1.TabStop = false;
            // 
            // system_light
            // 
            this.system_light.BackColor = System.Drawing.Color.Gray;
            this.system_light.FillColor = System.Drawing.Color.Gray;
            this.system_light.FillStyle = Microsoft.VisualBasic.PowerPacks.FillStyle.Solid;
            this.system_light.Location = new System.Drawing.Point(664, 41);
            this.system_light.Name = "system_light";
            this.system_light.Size = new System.Drawing.Size(15, 14);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(662, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "路径选择：";
            // 
            // map_select
            // 
            this.map_select.FormattingEnabled = true;
            this.map_select.Location = new System.Drawing.Point(722, 83);
            this.map_select.Name = "map_select";
            this.map_select.Size = new System.Drawing.Size(77, 20);
            this.map_select.TabIndex = 7;
            this.map_select.DropDown += new System.EventHandler(this.map_select_DropDown);
            this.map_select.SelectedIndexChanged += new System.EventHandler(this.Map_Change);
            // 
            // size_bar
            // 
            this.size_bar.Location = new System.Drawing.Point(664, 152);
            this.size_bar.Maximum = 20;
            this.size_bar.Minimum = 5;
            this.size_bar.Name = "size_bar";
            this.size_bar.Size = new System.Drawing.Size(134, 45);
            this.size_bar.TabIndex = 8;
            this.size_bar.Value = 10;
            this.size_bar.Scroll += new System.EventHandler(this.Map_Change);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(662, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "尺寸调节：";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 575);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.size_bar);
            this.Controls.Add(this.map_select);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.system_label);
            this.Controls.Add(this.draw_map);
            this.Controls.Add(this.alarm_list);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menu_interface);
            this.Controls.Add(this.shapeContainer1);
            this.MainMenuStrip = this.menu_interface;
            this.Name = "Form1";
            this.Text = "SmartCar 1.0";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.menu_interface.ResumeLayout(false);
            this.menu_interface.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.size_bar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu_interface;
        private System.Windows.Forms.ToolStripMenuItem 串口配置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 地图配置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 数据显示ToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListView alarm_list;
        private System.Windows.Forms.Panel draw_map;
        private System.Windows.Forms.Label system_label;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.OvalShape system_light;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox map_select;
        private System.Windows.Forms.TrackBar size_bar;
        private System.Windows.Forms.Label label2;
    }
}

