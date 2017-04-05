using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using AGVproject.Class;
using URG_data_pro_1;

namespace Smart_Car
{
    public partial class Map_Config_Window : Form
    {
        static Route route = new Route();
        static DrPort dr_port = new DrPort();
        static ConPort con_port = new ConPort();
        static UrgPort urg_port = new UrgPort();    //使用里面的行进位姿校准
        static Urg_pro_1.UrgPort urg_port_1 = new Urg_pro_1.UrgPort();   //使用里面的初始位姿校准
        static XML xml_con = new XML();
        static OperatingXML operating_xml = new OperatingXML();
        static CorrectPosition correct_pos = new CorrectPosition();  
        double size_rate = 1;//图像放缩比
        int select_line = -1;//选中线段
        int select_point = -1;//选中点
        Map int_map = new Map();//临时地图
        string mouse_Point = "";
        public Map_Config_Window()
        {
            InitializeComponent();
        }
        private void Map_Config_Window_Load(object sender, EventArgs e)
        {
            for (int i = 1; i <= 10; i++)//
            {
                string str = "../../Map/Route" + i.ToString() + ".xml";
                this.map_select1.Items.Add(new ComboBoxItem<int, string>(1, "Route" + i.ToString()));
                this.map_select2.Items.Add(new ComboBoxItem<int, string>(1, "Route" + i.ToString()));
            }

            List<Point> listPoint = new List<Point>();
            List<Line> listLine = new List<Line>();
            List<FixedRect> listFixedRect = new List<FixedRect>();
            int_map.listPoint = listPoint;
            int_map.listLine = listLine;
            int_map.listFixedRect = listFixedRect;
            //show_mouse_P.InitialDelay = 20;  //延时20ms显示鼠标位置坐标（x,y）
        }
        private void Map_Change(object sender, EventArgs e)//重新选择地图后重绘界面
        {
            string route_path = "../../Map/" + map_select1.Text.ToString() + ".xml";
            if (File.Exists(route_path))
            {
                route.map = operating_xml.readXML(route_path);
                Redraw_Panel1(route.map);
            }
        }
        private void Map_Config_Window_Shown(object sender, EventArgs e)
        {
            for (int i = 1; i <= 10; i++)//
            {
                string str = "../../Map/Route" + i.ToString() + ".xml";
                if (File.Exists(str))
                {
                    string str2 = "Route" + i.ToString();
                    map_select1.Text = str2;
                    string route_path = "../../Map/" + map_select1.Text.ToString() + ".xml";
                    if (File.Exists(route_path))
                    {
                        route.map = operating_xml.readXML(route_path);
                        Redraw_Panel1(route.map);
                    }
                    break;
                }

            }
        }
        private void Redraw_Panel1(Map map)//重绘界面函数
        {
            this.draw_map1.Refresh();
            Image route_start = Image.FromFile("../../ico/gps_map_128px_548366_easyicon.net.ico");
            Image route_end = Image.FromFile("../../ico/gps_map_128px_548391_easyicon.net.ico");
            Image route_point_com = Image.FromFile("../../ico/gps_map_128px_548373_easyicon.net.ico");
            Image route_point_urg = Image.FromFile("../../ico/gps_map_128px_548380_easyicon.net.ico");
            Graphics g = draw_map1.CreateGraphics();
            double p_left = 0, p_right = 0, p_top = 0, p_buttom = 0;//MAP四个角落值
            for (int i = 0; i < map.listPoint.Count(); i++)     //计算MAP四个角落值
            {
                if (i == 0)
                {
                    p_left = map.listPoint[i].x;
                    p_right = map.listPoint[i].x;
                    p_top = map.listPoint[i].y;
                    p_buttom = map.listPoint[i].y;
                }
                else
                {
                    if (p_left > map.listPoint[i].x)
                        p_left = map.listPoint[i].x;
                    if (p_right < map.listPoint[i].x)
                        p_right = map.listPoint[i].x;
                    if (p_top > map.listPoint[i].y)
                        p_top = map.listPoint[i].y;
                    if (p_buttom < map.listPoint[i].y)
                        p_buttom = map.listPoint[i].y;
                }
            }
            if ((p_buttom - p_top) / draw_map1.Size.Height > (p_right - p_left) / draw_map1.Size.Width)
                size_rate = (p_buttom - p_top) / draw_map1.Size.Height * 100 / (size_bar1.Value * 8);
            else
                size_rate = (p_right - p_left) / draw_map1.Size.Width * 100 / (size_bar1.Value * 8);
            //size_rate = 1;
            int size_rate_r = (int)(1 / size_rate);

            if (map.listPoint.Count() <= 1)
            {
                if(map.listPoint.Count==1)   //显示第一个标记的关键点
                {
                    g.DrawImage(route_point_com, (float)(draw_map1.Size.Width / 2 - 15), (float)(draw_map1.Size.Height / 2 - 29), 30, 30);
                }
                return;
            }
            for (int i = 0; i < draw_map1.Width; i++)
            {
                if (i % size_rate_r == 0)
                {
                    Pen m_pen = new Pen(Brushes.Gray, 3);
                    m_pen.Width = 1;
                    g.DrawLine(m_pen, i, 0, i, draw_map1.Height);
                }
            }
            for (int i = 0; i < draw_map1.Height; i++)
            {
                if (i % size_rate_r == 0)
                {
                    Pen m_pen = new Pen(Brushes.Gray, 3);
                    m_pen.Width = 1;
                    g.DrawLine(m_pen, 0, i, draw_map1.Width, i);
                }
            }

            for (int i = 0; i < map.listLine.Count(); i++)
            {
                if (i == select_line)
                {
                    Pen m_pen = new Pen(Brushes.Red, 3);
                    m_pen.Width = 5;
                    AdjustableArrowCap lineCap = new AdjustableArrowCap(4, 6, true);    //带方向箭头
                    m_pen.CustomEndCap = lineCap;
                    g.DrawLine(m_pen, (float)((map.listLine[i].startpoint.x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2), (float)(draw_map1.Size.Height - ((map.listLine[i].startpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)), (float)((map.listLine[i].endpoint.x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2), (float)(draw_map1.Size.Height - ((map.listLine[i].endpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)));

                }
                else
                {
                    Pen m_pen = new Pen(Brushes.DeepSkyBlue, 3);
                    m_pen.Width = 3;
                    AdjustableArrowCap lineCap = new AdjustableArrowCap(4, 6, true);    //带方向箭头
                    m_pen.CustomEndCap = lineCap;
                    g.DrawLine(m_pen, (float)((map.listLine[i].startpoint.x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2), (float)(draw_map1.Size.Height - ((map.listLine[i].startpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)), (float)((map.listLine[i].endpoint.x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2), (float)(draw_map1.Size.Height - ((map.listLine[i].endpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)));
                }
                if (map.listLine[i].num > 0 && map.listLine[i].num < 31)
                {
                    string str = "../../ico/id/" + map.listLine[i].num.ToString() + ".bmp";
                    Image img_num = Image.FromFile(str);
                    g.DrawImage(img_num, (float)(((map.listLine[i].startpoint.x + map.listLine[i].endpoint.x) / 2 - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2) - 15, (float)(draw_map1.Size.Height - (((map.listLine[i].startpoint.y + map.listLine[i].endpoint.y) / 2 - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)) - 15, 30, 30);
                }
            }
            for (int i = 0; i < map.listPoint.Count(); i++)
            {
                if (i == select_point)
                    g.DrawImage(route_start, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2) - 20, (float)(draw_map1.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)) - 38, 40, 40);
                else if (map.listPoint[i].type == 0)
                {
                    if (map.listPoint[i].direc == false)
                        g.DrawImage(route_point_com, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2) - 15, (float)(draw_map1.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)) - 29, 30, 30);

                    else
                        g.DrawImage(route_point_urg, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2) - 15, (float)(draw_map1.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)) - 29, 30, 30);
                }
                else
                    g.DrawImage(route_start, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map1.Size.Width / 2) - 15, (float)(draw_map1.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map1.Size.Height / 2)) - 29, 30, 30);
            }
        }
        private void Clear_Route1_Click(object sender, EventArgs e)//清除地面数据
        {
            try
            {
                route.map.listLine.Clear();
                route.map.listPoint.Clear();
                route.map.listFixedRect.Clear();
                select_line = -1;
                select_point = -1;
                Redraw_Panel1(route.map);
            }
            catch
            {
                return;
            }
        }
        private void Clear_Route2_Click(object sender, EventArgs e)
        {
            route.map.listLine.Clear();
            route.map.listPoint.Clear();
            route.map.listFixedRect.Clear();
        }
        private void Collect_Route1_Click(object sender, EventArgs e)
        {
            if (Collect_Route1.Text == "初始位姿校准")
            {
                xml_con.read();
                Collect_Route1.BackColor = Color.LightGreen;
                string[] portName = new string[2];    //[0]:urg_port    [1]:con_port
                string[] portBaudrate = new string[2];
                for (int i = 0; i < 2; i++)
                {
                    switch (xml_con.data[i * 2])
                    {
                        case 0: portName[i] = "COM1"; break;
                        case 1: portName[i] = "COM2"; break;
                        case 2: portName[i] = "COM3"; break;
                        case 3: portName[i] = "COM4"; break;
                        case 4: portName[i] = "COM5"; break;
                        case 5: portName[i] = "COM6"; break;
                        case 6: portName[i] = "COM7"; break;
                        case 7: portName[i] = "COM8"; break;
                        case 8: portName[i] = "COM9"; break;
                        case 9: portName[i] = "COM10"; break;
                    }
                    switch (xml_con.data[i * 2 + 1])
                    {
                        case 0: portBaudrate[i] = "9600"; break;
                        case 1: portBaudrate[i] = "115200"; break;
                    }
                }
                if (!urg_port_1.OpenPort(portName[0], portBaudrate[0]) || !con_port.OpenPort(portName[1], portBaudrate[1]))
                {
                    if (!urg_port_1.OpenPort(portName[0], portBaudrate[0]))
                        MessageBox.Show("激光雷达串口未能打开，请配置串口！");
                    else
                        MessageBox.Show("控制串口未能打开，请配置串口！");
                    return;
                }
                bool Adj_ok = urg_port_1.Init_Pos_Adjust(con_port);
                if (Adj_ok == true)
                {
                    Collect_Route1.Text = "获取路径信息";
                    Collect_Route1.BackColor = SystemColors.Control;
                    MessageBox.Show("初始位姿校准完毕！");
                }
            }
            else if (Collect_Route1.Text == "获取路径信息")
            {
                Urg_Point.Visible = true;
                Com_Point.Visible = true;
                Delete_Point.Visible = true;
                groupBox1.Visible = true;
                xml_con.read();
                Collect_Route1.BackColor = Color.LightGreen;

                string[] portName = new string[2];    //[0]:urg_port    [1]:con_port
                string[] portBaudrate = new string[2];
                for (int i = 0; i < 2; i++)
                {
                    switch (xml_con.data[i*4])
                    {
                        case 0: portName[i] = "COM1"; break;
                        case 1: portName[i] = "COM2"; break;
                        case 2: portName[i] = "COM3"; break;
                        case 3: portName[i] = "COM4"; break;
                        case 4: portName[i] = "COM5"; break;
                        case 5: portName[i] = "COM6"; break;
                        case 6: portName[i] = "COM7"; break;
                        case 7: portName[i] = "COM8"; break;
                        case 8: portName[i] = "COM9"; break;
                        case 9: portName[i] = "COM10"; break;
                    }
                    switch (xml_con.data[i*4 + 1])
                    {
                        case 0: portBaudrate[i] = "9600"; break;
                        case 1: portBaudrate[i] = "115200"; break;
                    }
                }
                //if (urg_port.OpenPort(portName[0], portBaudrate[0]))
                //    MessageBox.Show("激光雷达串口未能打开，请配置串口！");
                if (!dr_port.OpenPort(portName[1], portBaudrate[1]))
                    MessageBox.Show("编码器串口未能打开，请配置串口！");
                
                Collect_Route1.Text = "结束路径信息";
                Collect_Route1.BackColor = Color.LightGreen;
                try
                {
                    route.map.listLine.Clear();
                    route.map.listPoint.Clear();
                    route.map.listFixedRect.Clear();
                    dr_port.clearData();
                    
                }
                catch
                { 
                    route.map = new Map();
                    route.map.listPoint = new List<Point>();
                    route.map.listLine = new List<Line>();
                    route.map.listFixedRect = new List<FixedRect>();
                };
                //route.map.listLine.Clear();
                //route.map.listPoint.Clear();
                //route.map.listFixedRect.Clear();
                //dr_port.clearData();
            }
            else
            {
                route.map.listLine.Clear();
                for (int i = 0; i < route.map.listPoint.Count() - 1; i++)
                {
                    Line r_line = new Line();
                    r_line.startpoint = route.map.listPoint[i];
                    r_line.endpoint = route.map.listPoint[i + 1];
                    route.map.listLine.Add(r_line);
                }
                dr_port.ClosePort();
                con_port.ClosePort();
                urg_port_1.ClosePort();
                Collect_Route1.Text = "初始位姿校准";
                Collect_Route1.BackColor = SystemColors.Control;
                Redraw_Panel1(route.map);

            }
        }
        private void Collect_Route2_Click(object sender, EventArgs e)
        {

        }
        private void Save_Route1_Click(object sender, EventArgs e)
        {            
            string route_path = "../../Map/" + map_select1.Text.ToString() + ".xml";
            operating_xml.writeXML(route.map,route_path);
            MessageBox.Show("保存成功！");
        }
        private void Deal_Message(object sender, MouseEventArgs e)//左右键消息处理函数
        {
            double p_left = 0, p_right = 0, p_top = 0, p_buttom = 0;//MAP四个角落值
            for (int i = 0; i < route.map.listPoint.Count(); i++)     //计算MAP四个角落值
            {
                if (i == 0)
                {
                    p_left = route.map.listPoint[i].x;
                    p_right = route.map.listPoint[i].x;
                    p_top = route.map.listPoint[i].y;
                    p_buttom = route.map.listPoint[i].y;
                }
                else
                {
                    if (p_left > route.map.listPoint[i].x)
                        p_left = route.map.listPoint[i].x;
                    if (p_right < route.map.listPoint[i].x)
                        p_right = route.map.listPoint[i].x;
                    if (p_top > route.map.listPoint[i].y)
                        p_top = route.map.listPoint[i].y;
                    if (p_buttom < route.map.listPoint[i].y)
                        p_buttom = route.map.listPoint[i].y;
                }
            }
            if ((p_buttom - p_top) / draw_map1.Size.Height > (p_right - p_left) / draw_map1.Size.Width)
                size_rate = (p_buttom - p_top) / draw_map1.Size.Height * 100 / (size_bar1.Value * 8);
            else
                size_rate = (p_right - p_left) / draw_map1.Size.Width * 100 / (size_bar1.Value * 8);

            double x = (e.X - draw_map1.Size.Width / 2) * size_rate + (p_right + p_left) / 2;
            double y = ((draw_map1.Size.Height - e.Y) - draw_map1.Size.Height / 2) * size_rate + (p_buttom + p_top) / 2;
            mouse_Point = "(" + x.ToString("#0.00") + "," + y.ToString("#0.00") + ")";   //鼠标实时坐标

            if (e.Button == MouseButtons.Left)//左键功能
            {
                if (select_point != -1)//已选中点情况
                {
                    //int_map = route.map;

                    Map_Copy(route.map, int_map);
                    double r_x = (e.X - draw_map1.Size.Width / 2) * size_rate + (p_right + p_left) / 2;
                    double r_y = ((draw_map1.Size.Height - e.Y) - draw_map1.Size.Height / 2) * size_rate + (p_buttom + p_top) / 2;
                    Point r_point = new Point();
                    for (int i = 0; i < int_map.listLine.Count(); i++)
                    {
                        if (route.map.listPoint[select_point].x == int_map.listLine[i].startpoint.x && route.map.listPoint[select_point].y == int_map.listLine[i].startpoint.y)
                        {
                            Line r_Line = new Line();
                            r_Line = int_map.listLine[i];
                            r_Line.startpoint.x = r_x;
                            r_Line.startpoint.y = r_y;
                            int_map.listLine[i] = r_Line;
                        }
                        if (route.map.listPoint[select_point].x == int_map.listLine[i].endpoint.x && route.map.listPoint[select_point].y == int_map.listLine[i].endpoint.y)
                        {
                            Line r_Line = new Line();
                            r_Line = int_map.listLine[i];
                            r_Line.endpoint.x = r_x;
                            r_Line.endpoint.y = r_y;
                            int_map.listLine[i] = r_Line;
                        }
                    }
                    r_point = route.map.listPoint[select_point];
                    r_point.x = r_x;
                    r_point.y = r_y;
                    int_map.listPoint[select_point] = r_point;
                    Redraw_Panel1(int_map);
                    return;
                }
                //未选中点
                select_point = -1;
                select_line = -1;
                for (int i = 0; i < route.map.listPoint.Count(); i++)
                {
                    if ((x - route.map.listPoint[i].x) * (x - route.map.listPoint[i].x) + (y - route.map.listPoint[i].y) * (y - route.map.listPoint[i].y) < size_rate * 10)
                    {
                        select_point = i;
                        break;
                    }
                }
                if (select_point == -1)
                {
                    for (int i = 0; i < route.map.listLine.Count(); i++)
                    {
                        double k = (x - route.map.listLine[i].startpoint.x) / (route.map.listLine[i].endpoint.x - x) - (y - route.map.listLine[i].startpoint.y) / (route.map.listLine[i].endpoint.y - y);
                        if ((x > route.map.listLine[i].startpoint.x || x > route.map.listLine[i].endpoint.x) && (x < route.map.listLine[i].startpoint.x || x < route.map.listLine[i].endpoint.x) && (y < route.map.listLine[i].startpoint.y || y < route.map.listLine[i].endpoint.y) && (y > route.map.listLine[i].startpoint.y || y > route.map.listLine[i].endpoint.y))
                        {
                            select_line = i;
                            break;
                        }
                    }
                }
                Redraw_Panel1(route.map);
            }
            if (e.Button == MouseButtons.Right)//右键功能
            {
                if (select_point != -1)
                {
                    point_set.Show(this, e.X, e.Y);
                }
                if (select_line != -1)
                {
                    line_set.Show(this, e.X, e.Y);
                }
            }
        }
        public void Map_Copy(Map res, Map dst)
        {
            dst.listPoint.Clear();
            dst.listLine.Clear();
            dst.listFixedRect.Clear();
            for (int i = 0; i < res.listPoint.Count(); i++)
                dst.listPoint.Add(res.listPoint[i]);
            for (int i = 0; i < res.listLine.Count(); i++)
                dst.listLine.Add(res.listLine[i]);
            for (int i = 0; i < res.listFixedRect.Count(); i++)
                dst.listFixedRect.Add(res.listFixedRect[i]);
        }
        private void 确定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Map_Copy(int_map, route.map);
                select_point = -1;
                Redraw_Panel1(route.map);
            }
        }
        private void 取消ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select_point = -1;
            Map_Copy(route.map, int_map);
            Redraw_Panel1(route.map);
        }
        private void 度转弯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Point r_point = new Point();
                r_point = int_map.listPoint[select_point];
                r_point.type = 0;
                int_map.listPoint[select_point] = r_point;
            }
            Redraw_Panel1(int_map);
        }
        private void 路径角度转弯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Point r_point = new Point();
                r_point = int_map.listPoint[select_point];
                r_point.type = 1;
                int_map.listPoint[select_point] = r_point;
            }
            Redraw_Panel1(int_map);
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 1;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 2;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 3;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 4;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 5;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 6;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 7;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 8;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 9;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 10;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 11;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 12;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 13;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 14;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 15;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 16;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 17;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 18;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 19;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 10;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 21;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 22;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 23;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 24;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 25;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 26;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 27;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 28;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem33_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 29;
            route.map.listLine[select_line] = r_line;
        }
        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 30;
            route.map.listLine[select_line] = r_line;
        }
        private void 取消ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Line r_line = new Line();
            r_line = route.map.listLine[select_line];
            r_line.num = 0;
            route.map.listLine[select_line] = r_line;
        }

        private void Com_Point_Click(object sender, EventArgs e)
        {
            Point r_point = new Point();
            double[] urg_k_b = new double[3];
            r_point = dr_port.getPoint();
            urg_k_b = correct_pos.GetUrg_K_B();  //获取小车前面拟合直线的信息  [0]:是否校准点  [1]:K  [2]:B
            r_point.direc = false;

            r_point.Can_Adj = urg_k_b[0];
            r_point.UrgK = urg_k_b[1];
            r_point.UrgB = urg_k_b[2];
            route.map.listPoint.Add(r_point);

            route.map.listLine.Clear();
            int num = 0;//为激光雷达路径编号
            for (int i = 0; i < route.map.listPoint.Count() - 1; i++)
            {
                Line r_line = new Line();
                r_line.startpoint = route.map.listPoint[i];
                r_line.endpoint = route.map.listPoint[i + 1];
                if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                {
                    r_line.num = ++num;
                }
                route.map.listLine.Add(r_line);
            }
            Redraw_Panel1(route.map);
        }

        private void Urg_Point_Click(object sender, EventArgs e)
        {
            Point r_point = new Point();
            double[] urg_k_b = new double[3];
            r_point = dr_port.getPoint();
            urg_k_b = correct_pos.GetUrg_K_B();  //获取小车前面拟合直线的信息  [0]:是否校准点  [1]:K  [2]:B
            r_point.direc = true;
            r_point.Can_Adj = urg_k_b[0];
            r_point.UrgK = urg_k_b[1];
            r_point.UrgB = urg_k_b[2];
            route.map.listPoint.Add(r_point);

            route.map.listLine.Clear();
            int num = 0;//为激光雷达路径编号
            for (int i = 0; i < route.map.listPoint.Count() - 1; i++)
            {
                Line r_line = new Line();
                r_line.startpoint = route.map.listPoint[i];
                r_line.endpoint = route.map.listPoint[i + 1];
                if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                {
                    r_line.num = ++num;
                }
                route.map.listLine.Add(r_line);
            }
            Redraw_Panel1(route.map);
        }

        private void Delete_Point_Click(object sender, EventArgs e)
        {
            route.map.listPoint.RemoveAt(route.map.listPoint.Count - 1);
            route.map.listLine.Clear();

            int num = 0;//为激光雷达路径编号
            for (int i = 0; i < route.map.listPoint.Count() - 1; i++)
            {
                Line r_line = new Line();
                r_line.startpoint = route.map.listPoint[i];
                r_line.endpoint = route.map.listPoint[i + 1];
                if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                {
                    r_line.num = ++num;
                }
                route.map.listLine.Add(r_line);
            }
            Redraw_Panel1(route.map);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        
        //当鼠标停留在画板控件上方时，实时显示鼠标的坐标值（x,y）
        private void draw_map1_MouseMove(object sender, MouseEventArgs e)
        {
                double p_left = 0, p_right = 0, p_top = 0, p_buttom = 0;//MAP四个角落值
                for (int i = 0; i < route.map.listPoint.Count(); i++)     //计算MAP四个角落值
                {
                    if (i == 0)
                    {
                        p_left = route.map.listPoint[i].x;
                        p_right = route.map.listPoint[i].x;
                        p_top = route.map.listPoint[i].y;
                        p_buttom = route.map.listPoint[i].y;
                    }
                    else
                    {
                        if (p_left > route.map.listPoint[i].x)
                            p_left = route.map.listPoint[i].x;
                        if (p_right < route.map.listPoint[i].x)
                            p_right = route.map.listPoint[i].x;
                        if (p_top > route.map.listPoint[i].y)
                            p_top = route.map.listPoint[i].y;
                        if (p_buttom < route.map.listPoint[i].y)
                            p_buttom = route.map.listPoint[i].y;
                    }
                }
                if ((p_buttom - p_top) / draw_map1.Size.Height > (p_right - p_left) / draw_map1.Size.Width)
                    size_rate = (p_buttom - p_top) / draw_map1.Size.Height * 100 / (size_bar1.Value * 8);
                else
                    size_rate = (p_right - p_left) / draw_map1.Size.Width * 100 / (size_bar1.Value * 8);

                double x = (e.X - draw_map1.Size.Width / 2) * size_rate + (p_right + p_left) / 2;
                double y = ((draw_map1.Size.Height - e.Y) - draw_map1.Size.Height / 2) * size_rate + (p_buttom + p_top) / 2;
                mouse_Point = "(" + x.ToString("#0.00") + "," + y.ToString("#0.00") + ")";   //鼠标实时坐标
                show_mouse_P.Show(mouse_Point, draw_map1);
            
        }

        private void 普通关键点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Map_Copy(route.map, int_map);
                Point r_point = new Point();
                r_point = int_map.listPoint[select_point];
                r_point.direc = false;
                int_map.listPoint[select_point] = r_point;

                int_map.listLine.Clear();
                int num = 0;//为激光雷达路径编号
                for (int i = 0; i < int_map.listPoint.Count() - 1; i++)
                {
                    Line r_line = new Line();
                    r_line.startpoint = int_map.listPoint[i];
                    r_line.endpoint = int_map.listPoint[i + 1];
                    if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                    {
                        r_line.num = ++num;
                    }
                    int_map.listLine.Add(r_line);
                }
                    Map_Copy(int_map, route.map);
                select_point = -1;
            }
            Redraw_Panel1(route.map);
        }

        private void 激光雷达关键点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Map_Copy(route.map, int_map);
                Point r_point = new Point();
                r_point = int_map.listPoint[select_point];
                r_point.direc = true;
                int_map.listPoint[select_point] = r_point;

                int_map.listLine.Clear();
                int num = 0;//为激光雷达路径编号
                for (int i = 0; i < int_map.listPoint.Count() - 1; i++)
                {
                    Line r_line = new Line();
                    r_line.startpoint = int_map.listPoint[i];
                    r_line.endpoint = int_map.listPoint[i + 1];
                    if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                    {
                        r_line.num = ++num;
                    }
                    int_map.listLine.Add(r_line);
                }
                Map_Copy(int_map, route.map);
                select_point = -1;
            }
            Redraw_Panel1(route.map);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select_point != -1)
            {
                Map_Copy(route.map, int_map);
                int_map.listPoint.RemoveAt(select_point);
                int_map.listLine.Clear();
                int num = 0;//为激光雷达路径编号
                for (int i = 0; i < int_map.listPoint.Count() - 1; i++)
                {
                    Line r_line = new Line();
                    r_line.startpoint = int_map.listPoint[i];
                    r_line.endpoint = int_map.listPoint[i + 1];
                    if (r_line.startpoint.direc == true && r_line.endpoint.direc == true)
                    {
                        r_line.num = ++num;
                    }
                    int_map.listLine.Add(r_line);
                }
                Map_Copy(int_map, route.map);
                select_point = -1;
            }
            Redraw_Panel1(route.map);
        }
    }
}
