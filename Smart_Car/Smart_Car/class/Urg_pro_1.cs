using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using SCIP_library;
using System.Threading;
using Smart_Car;
using WinFormKalmanRadar;   //卡尔曼滤波
using URG_data_pro_1;

namespace URG_data_pro_1
{
    public class Urg_pro_1
    {
        //激光雷达数据结构
        public struct Urg_Point
        {
            public double X;
            public double Y;
        }
        public struct dis_set//块集合
        {
            public List<int> data;    //存放激光雷达距离数据的标号
            public Urg_Point start_point;
            public Urg_Point end_point;
        }
        public struct segment_set  //直线集合
        {
            public List<Urg_Point> datalist;
            public double k;//拟合出直线的K
            public double b;//拟合出直线的B
            public double p;//拟合出 直线对应的距离p
            public double q;//拟合出 直线对应的角度q（角度为单位）
            public double l;//拟合出直线的长度l
            public Urg_Point start_point;
            public Urg_Point end_point;
        }
        public struct line_type   //判断组成直角弯的直线类型  type=1 ：第一条线  type=2:第二条线
        {
            public segment_set s_s;
            public int type;
        }
        public class UrgPort : Port
        {

            //激光雷达状态
            bool UrgState;
            //起始和终止位置
            //const int startStep = 44;   //激光雷达距离数据的开始位置编号(修改前)
            //const int endStep = 725;    //...结束位置编号（修改前）
            const int startStep = 130;   //激光雷达距离数据的开始位置编号(修改后)
            const int endStep = 639;    //...结束位置编号（修改后）
            const double q_threshold = 30;  //判断组成直角的相邻两条直线的极径角与 90 度之间差值的阀值 
            const double dis_threshold = 50;    //判断组成直角的两条直线的首位坐标点之间的间距阀值
            const double l_threshold = 50;  //判断组成直角的两条直线长度差值的阀值

            const double Int_angle = 90;    //初始位姿设置的标准
            const double Int_X = 500;
            const double Int_Y = 600;

            const double T_X = 1200;  ////关键点位姿设置的标准
            const double T_Y = 400;
            //数据记录
            long timeStamp = 0;
            List<long> distancesInit = new List<long>();
            //public int[] distances = new int[682]; //注意初始化数组的范围,或者指定初值;（682=725-44+1）
            //int[] requre_data = new int[682];    //存放经过预处理后的激光雷达距离数据
            //int[] former_data=new int[682];   //历史数据（前一时刻数据）
            public int[] distances = new int[510]; //注意初始化数组的范围,或者指定初值;（682=725-44+1）
            int[] requre_data = new int[510];    //存放经过预处理后的激光雷达距离数据
            int[] former_data = new int[510];   //历史数据（前一时刻数据）
            bool Is_form_1 = false;     //判断former_data是否有数据
            int times = 0;
            const int lcount = 3;    //对lcount组采样数据进行滤波
            const double pi = 3.1415926;
            int[,] buff = new int[682, lcount];
            public Dictionary<int, List<segment_set>> lastResult = new Dictionary<int, List<segment_set>>();   //存储激光雷达原始数据经过聚类、合并、拟合之后的线段集合

            public List<segment_set> right_angle = new List<segment_set>();  //存储构成直角弯道线段集合
            public List<segment_set> former_line = new List<segment_set>();    //存储（组成直角）直线的历史数据
            public List<segment_set> T_angle = new List<segment_set>();   //存储构成T型角线段集合
            public List<segment_set> former_T = new List<segment_set>();    //存储构成T型角线段集合(历史数据)
            bool Adjust_flag = false;    //判断是否开始校准初始位姿

            //ConPort con_port;    //控制串口
            public List<KalmanSingle> k_man_list = new List<KalmanSingle>();
            //KalmanSingle k_man = new KalmanSingle();   //实例化卡尔曼滤波对象
            public UrgPort()
            {
                //数据初始化
                UrgState = false;
                //Init_K_man(682);
                Init_K_man(510);
            }
            //析构函数
            ~UrgPort() { }
            //打开串口
            public override bool OpenPort(string portName, string portBundrate)    //打开串口
            {
                if (!UrgState)
                {
                    UrgState = true;
                    int bundrate = int.Parse(portBundrate);
                    port = new SerialPort(portName, bundrate);
                    port.NewLine = "\n\n";
                    try
                    {
                        port.Open();
                        AGVproject.Class.TH_RefreshUrgData.urgport = port;
                        Form1.urg_port.port = port;
                    }
                    catch (Exception)
                    {
                        UrgState = false;
                        return false;
                    }
                    //port.Write(SCIP_Writer.SCIP2());
                    //port.ReadLine();
                    //port.Write(SCIP_Writer.MD(startStep, endStep));
                    //port.ReadLine();

                    port.Write(SCIP_Writer.SCIP2());
                    port.ReadLine();

                    return true;
                }
                return false;
            }
            //关闭串口
            public override bool ClosePort()    //关闭串口
            {
                if (UrgState)
                {
                    UrgState = false;
                    port.Write(SCIP_Writer.QT());
                    port.ReadLine();
                    Thread.Sleep(100);
                    try
                    {
                        port.Close();
                    }
                    catch (Exception)
                    {
                        UrgState = true;
                        return false;
                    }
                    return true;
                }
                return false;
            }
            private void clearData()   //清理串口缓冲区数据
            {
                port.DiscardInBuffer();
            }
            //接收数据
            public bool getUrgData()      //获取激光雷达数据
            {
                clearData();
                port.NewLine = "\n\n";
                port.Write(SCIP_Writer.MD(startStep, endStep));
                port.ReadLine();
                Thread.Sleep(20);
                clearData();
                string receiveData = port.ReadLine();

                if (!SCIP_Reader.MD(receiveData, ref timeStamp, ref distancesInit))
                {

                    return false;
                }
                if (distancesInit.Count == 0)
                {

                    return false;
                }
                for (int i = 0; i < distancesInit.Count; i++)
                {
                    distances[i] = Convert.ToInt32(distancesInit[i]);

                }
                //Thread.Sleep(10);
                return true;
            }
            public void Init_K_man(int n)
            {
                for (int i = 0; i < n; i++)
                    k_man_list.Add(new KalmanSingle());
            }    //卡尔曼滤波实例化对象
            public Dictionary<int, List<segment_set>> AfterDeal()     //对激光雷达数据首先进行区域分块
            {
                lastResult.Clear();
                getUrgData();
                //一：数据预处理
                //1:去除杂点
                for (int i = 0; i < 510; i++)
                {
                    if (distances[i] < 50 || distances[i] > 3000)   //更改接收的激光雷达数据的阀值
                        requre_data[i] = 0;           //requre_data装的是距离数据
                    else
                        requre_data[i] = distances[i];

                    //requre_data[i] = k_man_list[i].getDistance(requre_data[i]);    //卡尔曼滤波
                }
                //ArrayList list = new ArrayList();       //数据中位数滤波  取 lcount 个数
                //for (int i = 0; i < 682; i++)
                //{
                //    buff[i, times] = requre_data[i];
                //    if (times == lcount - 1)
                //        times = 0;
                //    for (int k = 0; k < lcount; k++)
                //        list.Add(buff[i, k]);
                //    list.Sort();
                //    requre_data[i] = buff[i, (lcount - 1) / 2];
                //    list.Clear();
                //    buff[i, lcount - 1] = requre_data[i];
                //}
                //times++;
                //former_data = requre_data;    //更新历史数据

                //二：整幅距离图像块分割
                //1.检测出数据中不为0的数据 完成分割 形成一个数据结构为list的dataset，里面每一个dis_set元素为一个集合，每个集合.data中的i即为对应的距离 
                List<dis_set> dataset = new List<dis_set>();//线段集合 每一list元素是距离数据
                dis_set d_s = new dis_set();
                List<int> datalist = new List<int>();//list元素,用来存放数据的标号（几号数据数据），范围是：0--681
                int compare = 0;
                double dis_theta = 0;//分割的距离阈值
                for (int i = 0; i < 510; i++)
                {
                    if (requre_data[i] != 0)
                    {
                        if (datalist.Count > 2)     //当第一个不为0的数据进来时候，不比较，直接添加到datalist里面去
                        {
                            //dis_theta = requre_data[i] * 0.01;//该阈值随着激光雷达距离数据的增加成比例关系
                            int pp = datalist[datalist.Count - 1];
                            dis_theta = 2 * (Math.Abs(requre_data[pp] - requre_data[pp - 1]) + Math.Abs(requre_data[pp - 1] - requre_data[pp - 2]));
                            if (dis_theta < 60)
                            {
                                dis_theta = 60;
                            }
                            double ll = Math.Abs(requre_data[i] - requre_data[datalist[datalist.Count - 1]]);
                            if (ll > dis_theta || i - compare > 10)//当读取的数据在距离上大于阈值或者在角度上大于阈值  就判断成为分割点
                            {
                                d_s.data = datalist.ToList<int>();
                                dataset.Add(d_s);
                                datalist.Clear();
                            }
                        }
                        datalist.Add(i);
                        compare = i;

                    }
                    if (i == 509)
                    {
                        if (datalist.Count > 0)
                        {
                            d_s.data = datalist.ToList<int>();
                            dataset.Add(d_s);
                            datalist.Clear();
                        }
                    }
                }
                for (int p = 0; p < dataset.Count; p++)     //如果集合中的数据少于10个，将该集合移除
                {
                    if (dataset[p].data.Count < 10)
                    {
                        dataset.Remove(dataset[p]);
                    }
                }
                //2.更新每个块的之间的起始坐标和结束坐标。
                if (dataset.Count > 0)
                {
                    for (int e = 0; e < dataset.Count; e++)
                    {
                        dataset[e] = UpdataBlock(dataset[e]);    //将起、始点极坐标转换成点坐标形式
                    }
                    //3.绘图加分割合并算法
                    // 对dataset集合中的每个元素进行分别处理，每个元素为dis_set数据结构，该结构中的data对应的距离数据即可以转换成直角坐标 然后处理
                    for (int i = 0; i < dataset.Count; i++)
                    {
                        lastResult.Add(i, DealSet(dataset[i].data));
                    }
                }
                return lastResult;
            }

            /// <summary>
            /// 根据 lastResult，初步筛选出组成直角弯的线段的集合，结果存入中right_angle中
            /// </summary>
            /// <param name="dataprocess"></param>
            /// <returns></returns>
            public List<segment_set> right_angle_find(Dictionary<int, List<segment_set>> AfterDeal)  //找出所有组成直角弯道的直线
            {
                right_angle = new List<segment_set>();
                for (int i = 0; i < AfterDeal.Keys.Count; i++)   //每个区域有多少块
                {
                    for (int j = 0; j < AfterDeal[i].Count - 1; j++)
                    {
                        double q1, q2, sign;
                        double q_q, dis_dis, l_l;
                        q1 = (AfterDeal[i])[j].q;
                        q2 = (AfterDeal[i])[j + 1].q;
                        sign = q1 * q2; //判断两条线的极径角是否同号
                        if (sign < 0)
                        {
                            if (q1 < 0)
                                q1 = q1 + 180;
                            else
                                q2 = q2 + 180;
                        }
                        q_q = Math.Abs(Math.Abs(q1 - q2) - 90);  //相邻两条直线的极径角 q 与 90 度之差
                        dis_dis = GetUrg_Pointdis((AfterDeal[i])[j].end_point, (AfterDeal[i])[j + 1].start_point);  //计算相邻两条直线的头尾坐标点之间的距离绝对值
                        l_l = Math.Abs((AfterDeal[i])[j].l - (AfterDeal[i])[j + 1].l);   //计算相邻两条直线间长度差值的绝对值
                        if (q_q < q_threshold && dis_dis < dis_threshold)   //判断夹角和点点之间的距离是否满足要求
                        {
                            //if(l_l>l_threshold)     //判断线段长度的差值是否满足要求
                            //{
                            if (right_angle.IndexOf((AfterDeal[i])[j]) == -1)
                                right_angle.Add((AfterDeal[i])[j]);
                            if (right_angle.IndexOf((AfterDeal[i])[j + 1]) == -1)
                                right_angle.Add((AfterDeal[i])[j + 1]);
                            //}
                        }
                    }
                }
                return right_angle;
            }
            public List<segment_set> T_angle_find(Dictionary<int, List<segment_set>> AfterDeal) //找出所有组成T型弯的直线
            {
                T_angle = new List<segment_set>();
                segment_set H_line = new segment_set();//前面，左边，右边直线
                segment_set L_line = new segment_set();
                segment_set R_line = new segment_set();
                for (int i = 0; i < AfterDeal.Keys.Count; i++)   //每个区域有多少块
                {
                    for (int j = 0; j < AfterDeal[i].Count - 1; j++)
                    {
                        if (Math.Abs((AfterDeal[i])[j].q - 90) < 30 && (AfterDeal[i])[j].l > 500)
                            H_line = (AfterDeal[i])[j];
                        else if (Math.Abs((AfterDeal[i])[j].q - 0) < 30 && (AfterDeal[i])[j].l > 200)
                            R_line = (AfterDeal[i])[j];
                        else if (Math.Abs((AfterDeal[i])[j].q - 180) < 30 && (AfterDeal[i])[j].l > 200)
                            L_line = (AfterDeal[i])[j];
                    }
                }
                T_angle.Add(H_line);
                T_angle.Add(L_line);
                T_angle.Add(R_line);
                if (T_angle.Count != 3)
                    T_angle = former_T;
                else
                    former_T = T_angle;
                return T_angle;
            }
            /// <summary>
            /// 初始位姿校准
            /// </summary>
            /// <param name="dataprocess"></param>
            /// <returns></returns>
            public bool Init_Pos_Adjust(ConPort con_port)   //初始位姿校准
            {
                int go_sp, shift_sp, rot_sp;
                int max_go = 60, min_go = 10;
                int max_sh = 40, min_sh = 10;
                int max_rot = 10, min_rot = 2;
                double go_rate = max_go / (51 - 1);    //60/(51-1)     51---value
                double sh_rate = max_sh / (61 - 1);   //40/(61-1)      61---value
                double rot_rate = max_rot / (45 - 1);    //10/(45-1)    45-----value
                if (con_port == null)
                {
                    con_port = ConPort.getInstance("A", "");
                }
                bool Is_angle_ok = false, Is_X_ok = false, Is_Y_ok = false;  //判断位姿校准的三个向量是否校准完毕
                List<line_type> line = new List<line_type>();

                segment_set ss = new segment_set();
                ss.p = 0;
                ss.q = 0;
                former_line.Add(ss);
                former_line.Add(ss);

                line = right_angle_2();
                Adjust_flag = true;
                for (int k = 0; k < 2; k++)  //循环校准两次
                {
                    while (Math.Abs(line[0].s_s.q - Int_angle) > 0.5)    //1.调整旋转方向
                    {
                        rot_sp = limit_value(line[0].s_s.q - Int_angle, rot_rate, max_rot, min_rot);
                        if (line[0].s_s.q < Int_angle)
                            con_port.controlDirectAGV(0, 0, -rot_sp);   //顺时针旋转
                        else if (line[0].s_s.q > Int_angle)
                            con_port.controlDirectAGV(0, 0, rot_sp);   //逆时针旋转
                        //Thread.Sleep(100);
                        line = right_angle_2();
                    }
                    //if(k==1)
                    Is_angle_ok = true;
                    Thread.Sleep(500);
                    while (Math.Abs(line[0].s_s.p - Int_X) > 5)     //2.调整前进方向
                    {
                        go_sp = limit_value(line[0].s_s.p - Int_X, go_rate, max_go, min_go);
                        if (line[0].s_s.p > Int_X)
                            con_port.controlDirectAGV(go_sp, 0, 0);
                        else if (line[0].s_s.p < Int_X)
                            con_port.controlDirectAGV(-go_sp, 0, 0);
                        //Thread.Sleep(100);
                        line = right_angle_2();
                        //line = new List<line_type>(right_angle_2());
                    }
                    //if(k==1)
                    Is_X_ok = true;
                    Thread.Sleep(500);
                    while (Math.Abs(line[1].s_s.p - Int_Y) > 5)     //3.调整平移方向
                    {
                        shift_sp = limit_value(line[1].s_s.p - Int_Y, sh_rate, max_sh, min_sh);
                        if (line[1].s_s.p > Int_Y)
                        {
                            if (line[1].type == 2)
                                con_port.controlDirectAGV(0, shift_sp, 0);
                            else if (line[1].type == 1)
                                con_port.controlDirectAGV(0, -shift_sp, 0);
                        }
                        else
                        {
                            if (line[1].type == 2)
                                con_port.controlDirectAGV(0, -shift_sp, 0);
                            else if (line[1].type == 1)
                                con_port.controlDirectAGV(0, shift_sp, 0);
                        }
                        //Thread.Sleep(100);
                        line = right_angle_2();
                        //line = new List<line_type>(right_angle_2());
                    }
                    //if(k==1)
                    Is_Y_ok = true;
                    //Thread.Sleep(500);
                }
                //this.ClosePort();   //校准完毕关闭串口
                if (Is_angle_ok && Is_X_ok && Is_Y_ok)
                {
                    //Adjust_flag=false;
                    return true;
                }
                else
                    return false;
            }
            public bool Key_point_Adjust(ConPort con_port)  //关键点位姿校准
            {
                int go_sp, shift_sp, rot_sp;
                const int max_go = 60, min_go = 10;
                const int max_sh = 40, min_sh = 10;
                const int max_rot = 10, min_rot = 2;
                double go_rate = max_go / (51 - 1);    //60/(51-1)     51---value
                double sh_rate = max_sh / (61 - 1);   //40/(61-1)      61---value
                double rot_rate = max_rot / (45 - 1);    //10/(45-1)    45-----value
                if (con_port == null)
                {
                    con_port = ConPort.getInstance("A", "");
                }
                bool Is_end = false;
                segment_set ss = new segment_set();
                ss.p = 0;
                ss.q = 0;
                former_T.Add(ss);
                former_T.Add(ss);
                former_T.Add(ss);

                List<segment_set> tmp = new List<segment_set>();
                tmp = T_angle_find(AfterDeal());

                while (Math.Abs(tmp[0].q - Int_angle) > 0.5)   //1.调整旋转方向
                {
                    rot_sp = limit_value(tmp[0].q - Int_angle, rot_rate, max_rot, min_rot);
                    if (tmp[0].q < Int_angle)
                        con_port.controlDirectAGV(0, 0, -rot_sp);   //顺时针旋转
                    else if (tmp[0].q > Int_angle)
                        con_port.controlDirectAGV(0, 0, rot_sp);   //逆时针旋转
                    Thread.Sleep(100);
                    tmp = T_angle_find(AfterDeal());
                }
                Thread.Sleep(500);
                while (Math.Abs(tmp[0].p - T_X) > 5)    //2.调整前进方向
                {
                    go_sp = limit_value(tmp[0].p - T_X, go_rate, max_go, min_go);
                    if (tmp[0].p > T_X)
                        con_port.controlDirectAGV(go_sp, 0, 0);
                    else if (tmp[0].p < T_X)
                        con_port.controlDirectAGV(-go_sp, 0, 0);
                    Thread.Sleep(100);
                    tmp = T_angle_find(AfterDeal());
                }
                Thread.Sleep(500);
                while (Math.Abs(tmp[1].p - T_Y) > 5)   //3.调整平移方向
                {
                    shift_sp = limit_value(tmp[1].p - T_Y, sh_rate, max_sh, min_sh);
                    if (tmp[1].p > T_Y)
                        con_port.controlDirectAGV(0, shift_sp, 0);
                    else
                        con_port.controlDirectAGV(0, -shift_sp, 0);
                    Thread.Sleep(100);
                    tmp = T_angle_find(AfterDeal());
                }
                Is_end = true;
                return Is_end;
            }
            int limit_value(double value, double rate, int max, int min)    // 将距离或角度差值（value）转化为控制速度，并限制上下界
            {
                value = Math.Abs(value);
                if (value * rate < min)
                    return min;
                else if (value * rate > max)
                    return max;
                return Convert.ToInt32(value * rate);
            }
            List<line_type> right_angle_2()    //分类出小车前面拟合直线与侧面拟合直线（直角弯）;line[0]:前面 line[1]:侧面
            {
                List<line_type> line = new List<line_type>();
                List<segment_set> tmp = new List<segment_set>();
                tmp = right_angle_find(AfterDeal());
                if (tmp.Count != 2)    //排除其他直线对组成直角的直线的干扰
                {
                    tmp = former_line;
                }
                double min_diff, tmp_diff;  //最小的差值
                int pos_head = 0, pos_side;   //最小差值的下标
                min_diff = Math.Abs(Math.Abs(tmp[0].q) - 90);
                tmp_diff = Math.Abs(Math.Abs(tmp[1].q) - 90);
                if (tmp_diff < min_diff)
                    pos_head = 1;
                pos_side = 1 - pos_head;

                for (int k = 0; k < 2; k++)    //判断第一条识别的直线是小车前面的还是侧面的 ，并对其标号 
                {
                    line_type l_type = new line_type();
                    l_type.s_s = tmp[Math.Abs(pos_head - k)];
                    l_type.type = (Math.Abs(pos_head - k) == 0) ? 1 : 2;
                    line.Add(l_type);
                }
                former_line = tmp;     //更新历史数据
                return line;
            }
            List<segment_set> DealSet(List<int> dataprocess)     //计算出每块区域中所有的直线
            {
                List<segment_set> setlist = new List<segment_set>();//集合list   里面一个元素即是    坐标集合
                segment_set set = new segment_set();//即集合   里面一个元素就是一个坐标 
                set.datalist = new List<Urg_Point>();
                Urg_Point Urg_Point = new Urg_Point();
                int num;
                for (int i = 0; i < dataprocess.Count; i++)
                {
                    num = dataprocess[i];
                    Urg_Point.X = (Math.Cos((0 + num * 0.3519) * Math.PI / 180) * requre_data[num]);  //0是x坐标
                    Urg_Point.Y = (Math.Sin((0 + num * 0.3519) * Math.PI / 180) * requre_data[num]);  //1是y坐标
                    set.datalist.Add(Urg_Point);
                }
                setlist.Add(set);  //  这个时候的集合列表中就一个元素
                //三：分裂合并算法
                //写分裂——合并算法
                //1.分裂算法
                double splite_dis = 0;
                double[] value = new double[2];

                for (int j = 0; j < setlist.Count; j++)
                {
                    segment_set frontset = new segment_set();//若分裂成2个集合，中的前面一个集合
                    frontset.datalist = new List<Urg_Point>();
                    segment_set backset = new segment_set();//若分裂成2个集合，中的后面一个集合
                    //分裂的过程是一个迭代的过程
                    backset.datalist = new List<Urg_Point>();

                    if (setlist[j].datalist.Count < 4)
                    {
                        setlist.Remove(setlist[j]);    //是不是少了一位  j=j-1;
                        j = j - 1;
                    }
                    else
                    {
                        value = Splitdis(setlist[j]);//调用求最大距离函数  返回值value[0]是最大距离在列表中的i值 ，value[1]是最大距离
                        //splite_dis = 0.1*requre_data[setlist[j].datalist[(int)value[0]]];
                        int hh = (int)value[0];
                        //阈值怎么取呢。。取该返回点 与 前后点加起来的距离和  setlist[j].datalist[int(value[0])] setlist[j].datalist[int(value[0])-1]
                        if (hh < setlist[j].datalist.Count - 1 && hh > 0)   //利用分割点距离其左右两边的距离之和作为动态的分割阀值
                        {
                            splite_dis = 2 * (GetUrg_Pointdis(setlist[j].datalist[hh], setlist[j].datalist[hh + 1]) + GetUrg_Pointdis(setlist[j].datalist[hh], setlist[j].datalist[hh - 1]));
                        }
                        else
                        {
                            splite_dis = 60;
                        }

                        if (splite_dis < 60)
                        {
                            splite_dis = 60;
                        }

                        if (value[1] > splite_dis)//当返回的最大距离大于阈值的时候以该点起 将set集合分割成两个集合   放到setlist里面去  
                        {
                            for (int i = 0; i < hh; i++)  //以返回的值为边界 将为set集合分割成frontset和backset 
                            {
                                frontset.datalist.Add(setlist[j].datalist[i]);

                            }
                            for (int i = hh; i < setlist[j].datalist.Count; i++)  //以返回的值为边界 将为set集合分割成frontset和backset 
                            {

                                backset.datalist.Add(setlist[j].datalist[i]);

                            }


                            setlist.Remove(setlist[j]);       //集合列表移除j号集合 
                            setlist.Insert(j, frontset);       //在第j出插入
                            setlist.Insert(j + 1, backset);        //在j+1出插入
                            j = j - 1;//分割后  将j-1，然后从开始 j 位置重新递归
                        }
                    }

                }


                //得到的setlist集合列表中将会是 一个一个集合


                //2.合并算法
                //首先去掉里面个数小于5个的点
                for (int k = 0; k < setlist.Count; k++)
                {
                    if (setlist[k].datalist.Count < 6)
                    {
                        setlist.Remove(setlist[k]);   //是不是少了一位？？？？  k=k-1
                        k = k - 1;
                    }
                }
                //然后 拟合出每条直线的k  b以及 p   β值          
                for (int l = 0; l < setlist.Count; l++)
                {
                    setlist[l] = UpdateLine(setlist[l]);

                }



                //最后 再合并
                //合并算法  寻找两个集合   如果P相差不大 Q相差不大 
                double merge_p_theta = 50, merge_q_theta = 7;
                for (int t = 1; t < setlist.Count; t++)
                {
                    segment_set mergetempt = new segment_set();
                    mergetempt.datalist = new List<Urg_Point>();
                    if (Math.Abs(setlist[t].p - setlist[t - 1].p) < merge_p_theta && Math.Abs(setlist[t].q - setlist[t - 1].q) < merge_q_theta)
                    {
                        //合并算法  合并的集合为setlist[t]  setlist[t-1]  合并成为setlist[t]
                        for (int i = 0; i < setlist[t - 1].datalist.Count; i++)
                        {
                            mergetempt.datalist.Add(setlist[t - 1].datalist[i]);
                        }
                        for (int i = 0; i < setlist[t].datalist.Count; i++)
                        {
                            mergetempt.datalist.Add(setlist[t].datalist[i]);
                        }
                        mergetempt = UpdateLine(mergetempt);//合并好了  
                        //然后放到源集合中去
                        setlist.Remove(setlist[t - 1]);   //移除一个元素，后面的元素会自动向前推进补位
                        setlist.Remove(setlist[t - 1]);
                        setlist.Insert(t - 1, mergetempt);
                        t = t - 1;//重新迭代    为了防止  t++  后，漏掉一位  
                    }
                }
                //得到了这个集合内的所有的线段

                return setlist;

            }
            segment_set UpdateLine(segment_set set)      //拟合出每条直线的k  b   L 以及 p   β  值   
            {
                double[] kb = new double[2];
                segment_set tempt;//临时变量 替换 list里面的值
                double xx, yy;//垂线交点的坐标在 一二象限  0-180   后面 是  -0-(-180)
                tempt = set;
                kb = TotalLeastSquares(tempt);   //存储每条线段的斜率和截距
                tempt.k = kb[0];    //斜率
                tempt.b = kb[1];    //截距
                tempt.p = Math.Abs(kb[1]) / Math.Sqrt(kb[0] * kb[0] + 1);    //点到直线的距离（直线的极径）
                xx = -kb[0] * kb[1] / (kb[0] * kb[0] + 1);     //令y=0,得到X轴上的截距（主要判断其正负号）
                yy = kb[1] / (1 + kb[0] * kb[0]);       //主要得到Y轴上的截距的正负号
                if (yy >= 0)     //计算极径的夹角
                {
                    if (xx >= 0)
                    {
                        tempt.q = 180 * Math.Atan(-1 / kb[0]) / Math.PI;        //计算结果是角度
                    }
                    else
                    {
                        tempt.q = 180 - Math.Abs(180 * Math.Atan(-1 / kb[0]) / Math.PI);
                    }
                }
                else
                {
                    if (xx >= 0)
                    {
                        tempt.q = 180 * Math.Atan(-1 / kb[0]) / Math.PI;
                    }
                    else
                    {
                        tempt.q = -(180 - 180 * Math.Atan(-1 / kb[0]) / Math.PI);
                    }
                }
                //更新起始点和结束点  直线y = kx+b    坐标点（m,n）  相交点为（kn+m-kb)/(kk+1）,(kkn+km-kkb)/(kk+1)+b
                tempt.start_point = CrossOver(set.datalist[0], kb[0], kb[1]);
                tempt.end_point = CrossOver(set.datalist[set.datalist.Count - 1], kb[0], kb[1]);
                tempt.l = GetUrg_Pointdis(tempt.start_point, tempt.end_point);
                return tempt;
            }
            dis_set UpdataBlock(dis_set set)    //将起始点的极坐标转换成点坐标
            {
                dis_set tempt = set;
                int fnum = set.data[0];
                int lnum = set.data[set.data.Count - 1];
                tempt.start_point.X = (Math.Cos((0 + fnum * 0.3519) * Math.PI / 180) * requre_data[fnum]);  //0.3519=360/1023  (激光雷达一圈的角度和一圈的光束线数) 
                tempt.start_point.Y = (Math.Sin((0 + fnum * 0.3519) * Math.PI / 180) * requre_data[fnum]);
                tempt.end_point.X = (Math.Cos((0 + lnum * 0.3519) * Math.PI / 180) * requre_data[lnum]);
                tempt.end_point.Y = (Math.Sin((0 + lnum * 0.3519) * Math.PI / 180) * requre_data[lnum]);
                return tempt;

            }
            Urg_Point CrossOver(Urg_Point Urg_Point, double k, double b)     //计算直线上的起始点坐标
            {
                Urg_Point Urg_Point_return;
                Urg_Point_return.X = (k * Urg_Point.Y + Urg_Point.X - k * b) / (k * k + 1);
                Urg_Point_return.Y = (k * k * Urg_Point.Y + k * Urg_Point.X - k * k * b) / (k * k + 1) + b;
                return Urg_Point_return;
            }
            double[] TotalLeastSquares(segment_set set)   //总体最小二乘法
            {
                //array[i,0]为x坐标   array[i,1]为y坐标
                int length = set.datalist.Count;
                double[] kb = new double[2];
                double X = 0, Y = 0, R = 0, S = 0, T = 0, M = 0;
                for (int i = 0; i < length; i++)
                {
                    X = X + set.datalist[i].X;
                    Y = Y + set.datalist[i].Y;
                    R = R + set.datalist[i].X * set.datalist[i].Y;
                    S = S + set.datalist[i].X * set.datalist[i].X;
                    T = T + set.datalist[i].Y * set.datalist[i].Y;
                }
                X = X / length;
                Y = Y / length;
                R = R / length;
                S = S / length;
                T = T / length;
                M = ((S - T) - (X * X - Y * Y)) / (R - X * Y);
                if (R > X * Y)
                {
                    kb[0] = (-M + Math.Sqrt(M * M + 4)) / 2;
                    kb[1] = Y + X * (M - Math.Sqrt(M * M + 4)) / 2;
                }
                else
                {
                    kb[0] = (-M - Math.Sqrt(M * M + 4)) / 2;
                    kb[1] = Y + X * (M + Math.Sqrt(M * M + 4)) / 2;
                }
                return kb;
            }
            double[] Splitdis(segment_set set) //以起点和终点拟合出一条直线  然后遍历求出各个点到直线的最大距离是多少
            {
                //若前后两点为（a1,b1）(a2,b2)  则直线方程为（b2-b1）X-（a2-a1）Y+b1*a2-b2*a1 = 0  
                //点到直线的距离公式为|Ax0+By0+C|/根号下A*A+B*B
                int length = set.datalist.Count - 1;//datalist所含的Urg_Point元素的个数

                double maxdis = 0;
                double num = 0;
                double[] return_value = new double[2];
                double A = set.datalist[length].Y - set.datalist[0].Y;
                double B = -(set.datalist[length].X - set.datalist[0].X);
                double C = set.datalist[0].Y * set.datalist[length].X - set.datalist[length].Y * set.datalist[0].X;
                for (int i = 1; i < set.datalist.Count - 1; i++)
                {
                    double dis = Math.Abs(A * set.datalist[i].X + B * set.datalist[i].Y + C) / Math.Sqrt(A * A + B * B);
                    if (dis > maxdis)
                    {
                        maxdis = dis;
                        num = i;
                    }
                }
                return_value[0] = num;
                return_value[1] = maxdis;
                return return_value;

            }

            double GetUrg_Pointdis(Urg_Point a, Urg_Point b)    //计算两个坐标点之间的距离
            {
                double dis = Math.Sqrt((a.Y - b.Y) * (a.Y - b.Y) + (a.X - b.X) * (a.X - b.X));
                return dis;
            }

        }
    }
}
