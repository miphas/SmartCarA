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
using System.Globalization;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Smart_Car
{
    public partial class Form1 : Form
    {
        static DrPort dr_port = new DrPort();
        static ConPort con_port = new ConPort();
        static UrgPort urg_port = new UrgPort();
        static XML xml_con = new XML();
        static Route route = new Route();
        double size_rate;//图像放缩比
        static double p_left = 0, p_right = 0, p_top = 0, p_buttom = 0;//MAP四个角落值
        Point c_point = new Point();//当前点坐标
        static OperatingXML operating_xml = new OperatingXML();
        // Network to add
        static NetSender net_sender = new NetSender();
        public Form1()
        {
            InitializeComponent();
            // add netSender
            //net_sender.init(dr_port);
            //Thread myThread = new Thread(net_sender.startSender);
            //myThread.Start();
        }
        private void Form1_Load(object sender, EventArgs e)//窗口初始化，下拉菜单显示内容刷新和初始选择
        {
            this.alarm_list.Columns.Add("时间", 150, HorizontalAlignment.Left);
            this.alarm_list.Columns.Add("事件内容", 320, HorizontalAlignment.Left);

            for (int i = 1; i <= 10; i++)//
            {
                string str = "../../Map/Route" + i.ToString() + ".xml";
                if(File.Exists(str))
                {
                    this.map_select.Items.Add(new ComboBoxItem<int, string>(1, "Route" + i.ToString()));
                }
            }
            for (int i = 1; i <= 10; i++)//
            {
                string str = "../../Map/Route" + i.ToString() + ".xml";
                if (File.Exists(str))
                {
                    str = "Route" + i.ToString();
                    this.map_select.Text = str;
                    break;
                }
            }
        }
       
        private void Serial_Config(object sender, MouseEventArgs e)//串口配置函数;打开串口配置页面
        {
            Serial_Config_Window serial_config_window = new Serial_Config_Window();
            serial_config_window.ShowDialog();
        }
        private void Map_Config(object sender, MouseEventArgs e)//地图配置函数;打开地图配置页面
        {
            Map_Config_Window map_config_window = new Map_Config_Window();
            map_config_window.ShowDialog();
        }
        private void Start_Car(object sender, MouseEventArgs e)//小车启动函数
        {
            xml_con.read();
            Redraw_Panel(route.map);
            string portName = "";
            string portBaudrate = "";
            int flag = 0;
            this.alarm_list.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度 
            for (int i = 0; i < 3; i++)
            {
                switch (xml_con.data[i * 2])
                {
                    case 0: portName = "COM1"; break;
                    case 1: portName = "COM2"; break;
                    case 2: portName = "COM3"; break;
                    case 3: portName = "COM4"; break;
                    case 4: portName = "COM5"; break;
                    case 5: portName = "COM6"; break;
                    case 6: portName = "COM7"; break;
                    case 7: portName = "COM8"; break;
                    case 8: portName = "COM9"; break;
                    case 9: portName = "COM10"; break;
                }
                switch (xml_con.data[i * 2 + 1])
                {
                    case 0: portBaudrate = "9600"; break;
                    case 1: portBaudrate = "115200"; break;
                }
                switch (i)
                {
                    case 0:
                        if (urg_port.OpenPort(portName, portBaudrate) == false)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = DateTime.Now.ToString(); ;
                            lvi.SubItems.Add("激光雷达串口未打开");
                            this.alarm_list.Items.Add(lvi);
                            system_light.FillColor = System.Drawing.Color.Red;
                            system_label.Text = "系统状态：串口异常";
                            flag = 1;
                        }
                        break;
                    case 1:
                        if (con_port.OpenPort(portName, portBaudrate) == false)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = DateTime.Now.ToString(); ;
                            lvi.SubItems.Add("控制串口未打开");
                            this.alarm_list.Items.Add(lvi);
                            system_light.FillColor = System.Drawing.Color.Red;
                            system_label.Text = "系统状态：串口异常";
                            flag = 1;
                        }
                        break;
                    case 2:
                        if (dr_port.OpenPort(portName, portBaudrate) == false)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = DateTime.Now.ToString(); ;
                            lvi.SubItems.Add("编码器串口未打开");
                            this.alarm_list.Items.Add(lvi);
                            system_light.FillColor = System.Drawing.Color.Red;
                            system_label.Text = "系统状态：串口异常";
                            flag = 1;
                        }
                        break;
                }
            }
            this.alarm_list.EndUpdate();  //结束数据处理，UI界面一次性绘制。
            if (flag == 0)
            {
                Thread goMap = new Thread(new ThreadStart(this.goAlongMap));
                goMap.Start();

                //goAlongMap(route.map.listLine);
                system_light.FillColor = System.Drawing.Color.Blue;
                system_label.Text = "系统状态：正在运行";
            }
            system_light.FillColor = System.Drawing.Color.Gray;
            system_label.Text = "系统状态：运行完成";
        }
        
        private void Map_Change(object sender, EventArgs e)//重新选择地图后重绘界面
        {
            string route_path = "../../Map/" + map_select.Text.ToString() + ".xml";
            if (File.Exists(route_path))
            {
                route.map = operating_xml.readXML(route_path);
                Redraw_Panel(route.map);
            }
        }
        private void Redraw_Panel(Map map)//重绘界面函数
        {
            this.draw_map.Refresh();
            Image route_start = Image.FromFile("../../ico/gps_map_128px_548366_easyicon.net.ico");
            Image route_end = Image.FromFile("../../ico/gps_map_128px_548391_easyicon.net.ico");
            Image route_point_com = Image.FromFile("../../ico/gps_map_128px_548373_easyicon.net.ico");
            Image route_point_urg = Image.FromFile("../../ico/gps_map_128px_548380_easyicon.net.ico");
            Image current = Image.FromFile("../../ico/Marker_48px_582998_easyicon.net.ico");
            Graphics g = draw_map.CreateGraphics();
            //double p_left = 0, p_right = 0, p_top = 0, p_buttom = 0;//MAP四个角落值
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
            if ((p_buttom - p_top) / draw_map.Size.Height > (p_right - p_left) / draw_map.Size.Width)
                size_rate = (p_buttom - p_top) / draw_map.Size.Height * 100 / (size_bar.Value * 8);
            else
                size_rate = (p_right - p_left) / draw_map.Size.Width * 100 / (size_bar.Value * 8);
            int size_rate_r = (int)(1 / size_rate);
            for (int i = 0; i < draw_map.Width; i++)
            {
                if (i % size_rate_r == 0)
                {
                    Pen m_pen = new Pen(Brushes.Gray, 3);
                    m_pen.Width = 1;
                    g.DrawLine(m_pen, i, 0, i, draw_map.Height);
                }
            }
            for (int i = 0; i < draw_map.Height; i++)
            {
                if (i % size_rate_r == 0)
                {
                    Pen m_pen = new Pen(Brushes.Gray, 3);
                    m_pen.Width = 1;
                    g.DrawLine(m_pen, 0, i, draw_map.Width, i);
                }
            }

            for (int i = 0; i < map.listLine.Count(); i++)
            {
                    Pen m_pen = new Pen(Brushes.DeepSkyBlue, 3);
                    m_pen.Width = 3;
                    AdjustableArrowCap lineCap = new AdjustableArrowCap(4, 6, true);    //带方向箭头
                    m_pen.CustomEndCap = lineCap;
                    g.DrawLine(m_pen, (float)((map.listLine[i].startpoint.x - (p_right + p_left) / 2) / size_rate  + draw_map.Size.Width / 2), (float)(draw_map.Size.Height-((map.listLine[i].startpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)), (float)((map.listLine[i].endpoint.x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2), (float)(draw_map.Size.Height-((map.listLine[i].endpoint.y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)));

                    if (map.listLine[i].num > 0 && map.listLine[i].num < 31)
                    {
                        string str = "../../ico/id/" + map.listLine[i].num.ToString() + ".bmp";
                        Image img_num = Image.FromFile(str);
                        g.DrawImage(img_num, (float)(((map.listLine[i].startpoint.x + map.listLine[i].endpoint.x) / 2 - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height-(((map.listLine[i].startpoint.y + map.listLine[i].endpoint.y) / 2 - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 15, 30, 30);
                    }
            }
            for (int i = 0; i < map.listPoint.Count(); i++)
            {
                if (i == 0)
                    g.DrawImage(route_start, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 20, (float)(draw_map.Size.Height-((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 38, 40, 40);
                else if (i == map.listPoint.Count() - 1)
                    g.DrawImage(route_end, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 29, 30, 30);
                else if (map.listPoint[i].direc == false)
                    g.DrawImage(route_point_com, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 29, 30, 30);
                else
                    g.DrawImage(route_point_urg, (float)((map.listPoint[i].x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height - ((map.listPoint[i].y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 29, 30, 30);
            }
            //g.DrawImage(current, (float)((c_point.x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height - ((c_point.y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 15, 30, 30);
            
        }

        //重绘当前坐标点(新加的)
        private void Redraw_Curr_P(Point p)
        {
            this.draw_map.Refresh();
            Redraw_Panel(route.map);
            Image current = Image.FromFile("../../ico/Marker_48px_582998_easyicon.net.ico");
            Graphics g = draw_map.CreateGraphics();
            int size_rate_r = (int)(1 / size_rate);
            g.DrawImage(current, (float)((p.x - (p_right + p_left) / 2) / size_rate + draw_map.Size.Width / 2) - 15, (float)(draw_map.Size.Height - ((p.y - (p_buttom + p_top) / 2) / size_rate + draw_map.Size.Height / 2)) - 15, 30, 30);
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            string route_path = "../../Map/" + map_select.Text.ToString() + ".xml";
            if (File.Exists(route_path))
            {
                //MessageBox.Show(map_select.Text.ToString());
                route.map = operating_xml.readXML(route_path);
                Redraw_Panel(route.map);
                //System.Threading.Thread.Sleep(3000);
            }
        }
        private void goAlongMap()//小车行进执行函数
        {
            List<Line> map = route.map.listLine;
            double todayTime = (DateTime.Today.ToUniversalTime().Ticks - 621355968000000000) / 10000000;   //Unix时间戳计算（基准时间）
            DateTime file_time = DateTime.Now;
            string year=file_time.Year.ToString(), month="", day="", hour="", minute="";
            if (file_time.Month < 10)
                month = "0"+file_time.Month.ToString();
            if (file_time.Day < 10)
                day = "0" + file_time.Day.ToString();
            if (file_time.Hour < 10)
                hour = "0" + file_time.Hour.ToString();
            if (file_time.Minute < 10)
                minute = "0" + file_time.Minute.ToString();
            string file_name = "../../road_sign/" + year + month + day + hour + minute + ".txt";

            /////摄像头////
            //new CameraControl().cameraLeading(dr_port, con_port);
            dr_port.clearData();

            for (int i = 0; i < map.Count; ++i)
            {
                //showPosition();
                string str = "";
                if (map[i].num != 0)//开始时间
                {
                    if (map[i].num < 10)
                    {
                        str += '0';
                    }
                    str += map[i].num.ToString();
                    str += ' ';
                    double epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000-todayTime;
                    str += epoch.ToString();
                }

                c_point = map[i].startpoint;
                Redraw_Panel(route.map);
                if (!map[i].startpoint.direc || !map[i].endpoint.direc)
                {
                    if (i == 0) {
                        dr_port.clearData();
                    }
                    if (map[i].startpoint.direc && map[i].endpoint.direc)
                    {
                        DrPort.distance.Sx = map[i].startpoint.x;
                        DrPort.distance.Sy = map[i].startpoint.y;
                        DrPort.distance.Sw = map[i].startpoint.w;
                    }
                    goWithPP(map[i].startpoint, map[i].endpoint, con_port, dr_port, urg_port);
                    //goWithPP(map[i].startpoint, map[i].endpoint, con_port, dr_port, urg_port);
                }
                else
                {
                    // setSpeed
                    int setSpeed = 50;
                    goWithRadar(map[i].startpoint, map[i].endpoint, con_port, dr_port, urg_port, setSpeed);
                }
                if (map[i].num != 0)//结束时间
                {
                    str += ' ';
                    double epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000-todayTime;
                    str += epoch.ToString();
                    FileStream syxstream = new FileStream(file_name,FileMode.Append,FileAccess.Write);
                    StreamWriter sw = new StreamWriter(syxstream);
                    sw.WriteLine(str);
                    sw.Close();


                }
                // showPosition();
            }
        }
        private void goWithPP(Point res, Point des, ConPort myConPort, DrPort myDrPort, UrgPort myUrgPort)//点对点行走
        {
            //Thread.Sleep(600);
            // set speed value
            int goSpeed = 0;
            int shiftSpeed = 0;
            int rotateSpeed = 3;
            int setGo = 60;
            int setShift = 40;
            int setRot = 20;
            // set begin point and now point
            Point save = myDrPort.getPoint();
            Point now = myDrPort.getPoint();
            //if (res.x == 0 && res.y == 0) {
            //    now.w = 0;
            //    des.w = 0;
            //}
            Redraw_Curr_P(now);  //绘出当前坐标点
            // first rotate to correct direction
            // relative
            //double toRotate = des.w - res.w;
            //while (Math.Abs((now.w - save.w) - toRotate) > 0.01){
            //    if (toRotate > 0){
            //        myConPort.controlDirectAGV(goSpeed, shiftSpeed, rotateSpeed);
            //    }
            //    else{
            //        myConPort.controlDirectAGV(goSpeed, shiftSpeed, -rotateSpeed);
            //    }
            //    System.Threading.Thread.Sleep(100);
            //    now = myDrPort.getPoint();
            //    Redraw_Curr_P(now);  //绘出当前坐标点
            //}
            // absolute

            // change des to res
            while (Math.Abs(now.w - des.w) > 0.008) {
                int curRotSpeed = rotateSpeed;
                if (Math.Abs(now.w - des.w) < 0.1) {
                    curRotSpeed  = (int)(rotateSpeed * 0.2);
                    if (curRotSpeed == 0) {
                        curRotSpeed = 1;
                    }
                }
                if (des.w > now.w) {
                    myConPort.controlDirectAGV(goSpeed, shiftSpeed, curRotSpeed);
                } else {
                    myConPort.controlDirectAGV(goSpeed, shiftSpeed, -curRotSpeed);
                }
                System.Threading.Thread.Sleep(100);
                now = myDrPort.getPoint();
            }

            // second judge how to move to correct location
            rotateSpeed = 0;
            double tmpW = des.w + Math.PI / 2.0;
            double length = Math.Sqrt(Math.Pow(res.x - des.x, 2) + Math.Pow(res.y - des.y, 2));
            if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW) - des.x, 2))
                        + Math.Pow(res.y + length * Math.Sin(tmpW) - des.y, 2)) < length)
            {
                goSpeed = setGo;
            }
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW + Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(tmpW + Math.PI / 2) - des.y, 2)) < length)
            {
                shiftSpeed = setShift;
            }
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW - Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(tmpW - Math.PI / 2) - des.y, 2)) < length)
            {
                shiftSpeed = -setShift;
            }
            else
            {
                goSpeed = -setGo;
            }
            // third move!
            Thread.Sleep(1000);
            double toMoveX = des.x - res.x;
            double toMoveY = des.y - res.y;
            // relative
            //if (Math.Abs(toMoveX) < Math.Abs(toMoveY)) {
            //    while (Math.Abs(now.y - save.y - toMoveY) > 0.05) {
            //        myUrgPort.GetAngleAndDis();
            //        if (myUrgPort.CanGo()) {
            //            myConPort.controlDirectAGV(goSpeed, shiftSpeed, rotateSpeed);
            //        }
            //        myConPort.controlDirectAGV(goSpeed, shiftSpeed, rotateSpeed);
            //        System.Threading.Thread.Sleep(100);
            //        now = myDrPort.getPoint();
            //    }
            //} else {
            //    while (Math.Abs(now.x - save.x - toMoveX) > 0.05) {
            //        myUrgPort.GetAngleAndDis();
            //        if (myUrgPort.CanGo()) {
            //            myConPort.controlDirectAGV(goSpeed, shiftSpeed, rotateSpeed);
            //        }
            //        myConPort.controlDirectAGV(goSpeed, shiftSpeed, rotateSpeed);
            //        System.Threading.Thread.Sleep(100);
            //        now = myDrPort.getPoint();
            //    }
            //}

            // absolute
            int curGoSpeed = goSpeed;
            int curShSpeed = shiftSpeed;
            if (Math.Abs(toMoveX) < Math.Abs(toMoveY)) {
                while (Math.Abs(des.y - now.y) > 0.0075) {
                    if (Math.Abs(des.y - now.y) < 0.08) {
                        curGoSpeed = (int)(goSpeed * 0.3);
                        curShSpeed = (int)(shiftSpeed * 0.3);
                    }
                    if (myUrgPort.CanGo()) {
                        myConPort.controlDirectAGV(curGoSpeed, curShSpeed, rotateSpeed);
                    }
                    System.Threading.Thread.Sleep(100);
                    now = myDrPort.getPoint();
                    Redraw_Curr_P(now);  //绘出当前坐标点
                }

            } else {
                while (Math.Abs(des.x - now.x) > 0.0075) {
                    if (Math.Abs(des.x - now.x) < 0.08) {
                        curGoSpeed = (int)(goSpeed * 0.3);
                        curShSpeed = (int)(shiftSpeed * 0.3);
                    }
                    if (myUrgPort.CanGo()) {
                        myConPort.controlDirectAGV(curGoSpeed, curShSpeed, rotateSpeed);
                    }
                    System.Threading.Thread.Sleep(100);
                    now = myDrPort.getPoint();
                    Redraw_Curr_P(now);  //绘出当前坐标点
                }
            }

        }


        private enum Direction {
            front, left, right, behind
        }
        /// <summary>
        /// X方向是主偏差时
        /// </summary>
        /// <param name="p1">目标位置 - 当前位置偏差 为负true</param>
        /// <param name="p2">当前位置左转90度Cos值 为负true</param>
        /// <returns></returns>
        private Direction getDirectionX(bool p1, bool p2) {
            if (p1) {
                return (p2 ? Direction.left : Direction.right);
            }
            else {
                return (p2 ? Direction.right : Direction.left);
            }
        }
        /// <summary>
        /// Y方向是主偏差时
        /// </summary>
        /// <param name="p1">目标位置 - 当前位置偏差 为负true</param>
        /// <param name="p2">当前位置左转90度Sin值 为负true</param>
        /// <returns></returns>
        private Direction getDirectionY(bool p1, bool p2) {
            if (p1) {
                return (p2 ? Direction.front : Direction.behind);
            }
            else {
                return (p2 ? Direction.behind : Direction.front);
            }
        }
        private void goWithPP2(Point res, Point des, ConPort myConPort, DrPort myDrPort, UrgPort myUrgPort)//点对点行走
        {
            // 设置行进方向最小速度和最大速速度
            int minGo = 10, minShift = 10, minRot = 2;
            int maxGo = 60, maxShift = 40, maxRot = 20;
            // 设置角度、距离误差范围
            double angRound = 0.008;
            double disRound = 0.0075;
            // 角度、距离比例控制
            double angP = 120;   // 10/0.008  = 250
            double disP = 650;   // 60/0.0075 = 13333
            // 当前位置点信息
            Point nowPos = myDrPort.getPoint();

            //
            // 行进控制部分开始
            //
            // 1.旋转AGV对准方向
            //
            while (Math.Abs(nowPos.w - des.w) > angRound) {
                // 比例控制速度
                int curRot = (int)(angP * (des.w - nowPos.w));
                // 限制速度在合理范围
                curRot = Math.Min(curRot, maxRot);
                curRot = Math.Max(curRot, minRot);
                // 执行转弯
                myConPort.controlDirectAGV(0, 0, curRot);
                Thread.Sleep(100);
                nowPos = myDrPort.getPoint();
            }
            //
            // 2.判断行进方向
            //
            Direction direct = Direction.front;
            // 前方向角度以及两点距离
            double frontAngle = des.w + Math.PI / 2.0;
            double length = Math.Sqrt(Math.Pow(res.x - des.x, 2) + Math.Pow(res.y - des.y, 2));
            // 假设向前移动一段距离，若距离终点变近则向前移动
            if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(frontAngle) - des.x, 2))
                        + Math.Pow(res.y + length * Math.Sin(frontAngle) - des.y, 2)) < length) {
                direct = Direction.front;
            }
            // 同上，判断是否向左
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(frontAngle + Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(frontAngle + Math.PI / 2) - des.y, 2)) < length) {
                direct = Direction.left;
            }
            // 同上，判断是否向右
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(frontAngle - Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(frontAngle - Math.PI / 2) - des.y, 2)) < length) {
                direct = Direction.right;
            }
            // 方向为向后
            else {
                direct = Direction.behind;
            }
            //
            // 3.进行移动
            //
            double toMoveX = des.x - res.x;
            double toMoveY = des.y - res.y;
            // 主偏移方向为Y
            if (Math.Abs(toMoveX) < Math.Abs(toMoveY)) {
                // Y方向已经走的长度占整个长度的比例
                double rate = Math.Abs((nowPos.y - res.y) / (des.y - res.y));
                // 此时小车x方向应在的位置
                double xPos = res.x + rate * toMoveX;

            }
            // 主偏移方向为X
            else {

            }


            /*
            //Thread.Sleep(600);
            // set speed value
            int goSpeed = 0;
            int shiftSpeed = 0;
            int rotateSpeed = 3;
            int setGo = 60;
            int setShift = 40;
            int setRot = 20;
            // set begin point and now point
            Point save = myDrPort.getPoint();
            Point now = myDrPort.getPoint();

            Redraw_Curr_P(now);  //绘出当前坐标点
            // first rotate to correct direction
            // absolute

            // change des to res
            while (Math.Abs(now.w - des.w) > 0.008) {
                int curRotSpeed = rotateSpeed;
                if (Math.Abs(now.w - des.w) < 0.1) {
                    curRotSpeed = (int)(rotateSpeed * 0.2);
                    if (curRotSpeed == 0) {
                        curRotSpeed = 1;
                    }
                }
                if (des.w > now.w) {
                    myConPort.controlDirectAGV(goSpeed, shiftSpeed, curRotSpeed);
                }
                else {
                    myConPort.controlDirectAGV(goSpeed, shiftSpeed, -curRotSpeed);
                }
                System.Threading.Thread.Sleep(100);
                now = myDrPort.getPoint();
            }

            // second judge how to move to correct location
            rotateSpeed = 0;
            int direction = 1;
            double tmpW = des.w + Math.PI / 2.0;
            double length = Math.Sqrt(Math.Pow(res.x - des.x, 2) + Math.Pow(res.y - des.y, 2));
            if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW) - des.x, 2))
                        + Math.Pow(res.y + length * Math.Sin(tmpW) - des.y, 2)) < length) {
                goSpeed = setGo;
                direction = 1;
            }
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW + Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(tmpW + Math.PI / 2) - des.y, 2)) < length) {
                shiftSpeed = setShift;
                direction = 2;
            }
            else if (Math.Sqrt((Math.Pow(res.x + length * Math.Cos(tmpW - Math.PI / 2) - des.x, 2))
                      + Math.Pow(res.y + length * Math.Sin(tmpW - Math.PI / 2) - des.y, 2)) < length) {
                shiftSpeed = -setShift;
                direction = 3;
            }
            else {
                goSpeed = -setGo;
                direction = 4;
            }
            // third move!
            Thread.Sleep(1000);
            double toMoveX = des.x - res.x;
            double toMoveY = des.y - res.y;
            // relative

            // absolute
            int curGoSpeed = goSpeed;
            int curShSpeed = shiftSpeed;
            if (Math.Abs(toMoveX) < Math.Abs(toMoveY)) {
                while (Math.Abs(des.y - now.y) > 0.0075) {
                    if (Math.Abs(des.y - now.y) < 0.08) {
                        curGoSpeed = (int)(goSpeed * 0.3);
                        curShSpeed = (int)(shiftSpeed * 0.3);
                    }
                    if (myUrgPort.CanGo()) {
                        myConPort.controlDirectAGV(curGoSpeed, curShSpeed, rotateSpeed);
                    }
                    System.Threading.Thread.Sleep(100);
                    now = myDrPort.getPoint();
                    Redraw_Curr_P(now);  //绘出当前坐标点
                }

            }
            else {
                while (Math.Abs(des.x - now.x) > 0.0075) {
                    if (Math.Abs(des.x - now.x) < 0.08) {
                        curGoSpeed = (int)(goSpeed * 0.3);
                        curShSpeed = (int)(shiftSpeed * 0.3);
                    }
                    if (myUrgPort.CanGo()) {
                        myConPort.controlDirectAGV(curGoSpeed, curShSpeed, rotateSpeed);
                    }
                    System.Threading.Thread.Sleep(100);
                    now = myDrPort.getPoint();
                    Redraw_Curr_P(now);  //绘出当前坐标点
                }
            }
             * */

        }
        private void goWithRadar(Point res, Point des, ConPort myConPort, DrPort myDrPort, UrgPort myUrgPort, int speed)//激光雷达行走
        {
            double toMove = 0;
            bool flagX = false;
            bool flagY = false;
            Point start = myDrPort.getPoint();
            Point now = myDrPort.getPoint();
            Redraw_Curr_P(now);  //绘出当前坐标点
            if (Math.Abs(des.x - res.x) < Math.Abs(des.y - res.y))
            {
                toMove = des.y - res.y;
                flagY = true;
            }
            else
            {
                toMove = des.x - res.x;
                flagX = true;
            }
            while ((flagX && Math.Abs(toMove - (now.x - start.x)) > 0.1) ||
                   (flagY && Math.Abs(toMove - (now.y - start.y)) > 0.1))
            {
                myUrgPort.GetAngleAndDis();
                // add 20151113 for stop directly
                if (myUrgPort.CanGo()) {
                    myConPort.UrgControl(speed, 90 - myUrgPort.angle, 0 - myUrgPort.dis);
                } 
                System.Threading.Thread.Sleep(100);
                now = myDrPort.getPoint();
                Redraw_Curr_P(now);  //绘出当前坐标点
            }

            //while (Math.Abs(now.w - des.w) > 0.01)
            //{
            //    int rotateS = 40;
            //    if (des.w > now.w)
            //    {
            //        myConPort.controlDirectAGV(0, 0, rotateS);
            //    }
            //    else
            //    {
            //        myConPort.controlDirectAGV(0, 0, -rotateS);
            //    }
            //    System.Threading.Thread.Sleep(100);
            //    now = myDrPort.getPoint();
            //    Redraw_Curr_P(now);  //绘出当前坐标点
            //}
        }
        private void map_select_DropDown(object sender, EventArgs e)//点击地图下拉时刷新选择内容
        {
            this.map_select.Items.Clear();
            for (int i = 1; i <= 10; i++)//
            {
                string str = "../../Map/Route" + i.ToString() + ".xml";
                if (File.Exists(str))
                {
                    this.map_select.Items.Add(new ComboBoxItem<int, string>(1, "Route" + i.ToString()));
                }
            }
        }
    }
}
