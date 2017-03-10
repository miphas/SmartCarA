using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Car
{
    public struct ComboBoxItem<TKey, TValue>
    {
        private TKey key;
        private TValue value;

        public ComboBoxItem(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public TKey Key
        {
            get { return key; }
        }

        public TValue Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public partial class Serial_Config_Window : Form
    {
        static XML xml_con = new XML();
        public Serial_Config_Window()
        {
            InitializeComponent();
        }
        private void Serial_Config_Window_Load(object sender, EventArgs e)
        {
            xml_con.read();//读入XML数据
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM1"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM2"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM3"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM4"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM5"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM6"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM7"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM8"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM9"));
            this.urg_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM10"));
            urg_port_com.Text = urg_port_com.Items[xml_con.data[0]].ToString();

            this.urg_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "9600"));
            this.urg_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "115200"));
            urg_port_baud.Text = urg_port_baud.Items[xml_con.data[1]].ToString();

            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM1"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM2"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM3"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM4"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM5"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM6"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM7"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM8"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM9"));
            this.con_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM10"));
            con_port_com.Text = con_port_com.Items[xml_con.data[2]].ToString();

            this.con_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "9600"));
            this.con_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "115200"));
            con_port_baud.Text = con_port_baud.Items[xml_con.data[3]].ToString();

            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM1"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM2"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM3"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM4"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM5"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM6"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM7"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM8"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM9"));
            this.dr_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM10"));
            dr_port_com.Text = dr_port_com.Items[xml_con.data[4]].ToString();

            this.dr_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "9600"));
            this.dr_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "115200"));
            dr_port_baud.Text = dr_port_baud.Items[xml_con.data[5]].ToString();

            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM1"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM2"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM3"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM4"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM5"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM6"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM7"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM8"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM9"));
            this.cam_port_com.Items.Add(new ComboBoxItem<int, string>(1, "COM10"));
            cam_port_com.Text = cam_port_com.Items[xml_con.data[6]].ToString();

            this.cam_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "9600"));
            this.cam_port_baud.Items.Add(new ComboBoxItem<int, string>(1, "115200"));
            cam_port_baud.Text = cam_port_baud.Items[xml_con.data[7]].ToString();
        }
        private void Save_Serial_Config_Click(object sender, EventArgs e)//保存串口配置
        {
            xml_con.data[0] = urg_port_com.SelectedIndex;
            xml_con.data[1] = urg_port_baud.SelectedIndex;
            xml_con.data[2] = con_port_com.SelectedIndex;
            xml_con.data[3] = con_port_baud.SelectedIndex;
            xml_con.data[4] = dr_port_com.SelectedIndex;
            xml_con.data[5] = dr_port_baud.SelectedIndex;
            xml_con.data[6] = cam_port_com.SelectedIndex;
            xml_con.data[7] = cam_port_baud.SelectedIndex;
            xml_con.write();
        }
    }
}
