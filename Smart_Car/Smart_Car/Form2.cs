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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        ConPort con_port = new ConPort();
        private void Form2_Load(object sender, EventArgs e)
        {
            while (true)
            {
                con_port.getSonicDistance();
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
