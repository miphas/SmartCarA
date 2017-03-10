using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Ports;
using SCIP_library;

namespace Smart_Car
{
    class UrgPort : Port{
        struct MyPointer
        {
            public double x;
            public double y;
            public double dis;
        }
        //激光雷达状态
        bool UrgState;
        //起始和终止位置
        const int startStep = 0;
        const int endStep = 760;
        const int startData = 44;
        //数据记录
        long timeStamp;
        List<long> distance = new List<long>();
        public int[] distanceInt = new int[800]; //4-14
        //转换为X、Y坐标
        double[] radarX = new double[800];
        double[] radarY = new double[800];
        MyPointer[] pointers = new MyPointer[201];
        //记录中心线的中点和角度
        double angleMid;
        MyPointer pointMid;
        //左右两边的中点和角度
        double angleLeft;
        MyPointer pointLeft;
        double angleRight;
        MyPointer pointRight;

        public double angle;
        public double dis;


        //构造函数
        public UrgPort()
        {
            //数据初始化
            UrgState = false;
        }
        //析构函数
        ~UrgPort() { }

        //打开串口
        public override bool OpenPort(string portName, string portBundrate) {
            if (!UrgState)
            {
                UrgState = true;
                int bundrate = int.Parse(portBundrate);
                port = new SerialPort(portName, bundrate);
                port.NewLine = "\n\n";
                try
                {
                    port.Open();
                }
                catch(Exception)
                {
                    UrgState = false;
                    return false;
                }
                port.Write(SCIP_Writer.SCIP2());
                port.ReadLine();
                port.Write(SCIP_Writer.MD(startStep, endStep));
                port.ReadLine();
                return true;
            }
            return false;
        }
        //关闭串口
        public bool ClosePort() {
            if (UrgState)
            {
                UrgState = false;
                port.Write(SCIP_Writer.QT());
                port.ReadLine();
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

        //获取激光雷达的探测通道内角度和距离信息
        public void GetAngleAndDis( )
        {
            clearData();
            //获取数据以及存储
            getUrgData();
            RadarDataTrans(ref distance, ref radarX, ref radarY);
            //滤波处理
            midFilter(ref distanceInt);

            getAngleAndCenter(ref angleRight, ref pointRight, 25, 50);  // 20  60
            getAngleAndCenter(ref angleLeft, ref pointLeft, 150, 175);  // 140 180
            getMid();

            getDisError(33, 53, 147, 167);


            //test if car can go  20151113
            TestIfCanGo();
        }
        private void clearData()
        {
            port.DiscardInBuffer();
        }
        //接收数据
        private bool getUrgData()
        {
            string receiveData = port.ReadLine();
            if (!SCIP_Reader.MD(receiveData, ref timeStamp, ref distance))
            {
                Console.WriteLine(receiveData);
                return false;
            }
            if (distance.Count == 0)
            {
                Console.WriteLine(receiveData);
                return false;
            }
            return true;
        }

        void RadarDataTrans(ref List<long> ditance, ref double[] radarX, ref double[] radarY)
        {
            double angle;
            for (int i = startData; i < distance.Count; ++i)
            {
                distanceInt[i] = Convert.ToInt32(distance[i]);
                angle = -30 + (i - 44) * 6 / 17;
                radarX[i] = distance[i] * (Math.Cos(angle * 3.1415 / 180)); //distance[i] / 10
                radarY[i] = distance[i] * (Math.Sin(angle * 3.1415 / 180)); //distance[i] / 10
            }
        }
        private void midFilter(ref int[] d)
        {
            int disMid;
            for (int i = 83; i < 685; i += 3)
            {
                disMid = d[i];
                if ((disMid < d[i + 1]) && (disMid < d[i + 2]))
                {
                    if (d[i + 1] < d[i + 2])
                        disMid = d[i + 1];
                    else
                        disMid = d[i + 2];
                }
                else if ((disMid > d[i + 1]) && (disMid > d[i + 2]))
                {
                    if (d[i + 1] > d[i + 2])
                        disMid = d[i + 1];
                    else
                        disMid = d[i + 2];
                }
                double angle = -30 + (i - 44) * 6 / 17;
                transToPoint(ref disMid, (i - 83) / 3, angle);
            }
        }
        private void transToPoint(ref int dis, int i, double angle)
        {
            pointers[i].x = dis * (Math.Cos(angle * 3.14159 / 180));
            pointers[i].y = dis * (Math.Sin(angle * 3.14159 / 180));
            pointers[i].dis = dis;
        }

        private void getAngleAndCenter(ref double angle, ref MyPointer point, int start, int end)
        {
            double resultX = 0, tempX = 0;
            double resultY = 0, tempY = 0;
            int count = 0, tempCount = 0;
            //获取线段中心点
            for (int i = start; i < end; ++i)
            {
                resultX += pointers[i].x;
                resultY += pointers[i].y;
                ++count;
            }
            point.x = resultX / count;
            point.y = resultY / count;
            //角度计算
            count = 0;
            resultX = resultY = 0;
            for (int i = start; i < end; ++i)
            {
                if (point.y < pointers[i].y)
                {
                    ++count;
                    resultX += pointers[i].x - point.x;
                    resultY += pointers[i].y - point.y;
                }
                else if (point.y > pointers[i].y)
                {
                    ++tempCount;
                    tempX += pointers[i].x - point.x;
                    tempY += pointers[i].y - point.y;
                }
            }

            if ((count == 0 && tempCount > 0) ||
                ((count != 0 && tempCount != 0) && (Math.Abs(tempX / tempCount) < Math.Abs(resultX / count))))
            {
                count = tempCount;
                resultX = -tempX;
                resultY = -tempY;
            }
            //4-22
            if (count == 0)
                ++count;
            if (Math.Abs(resultX) < 0.0000001)
                resultX = 1;
            //end 4-22
            resultX /= count;
            resultY /= count;
            angle = Math.Atan(resultY / resultX);
            if (angle < 0)
                angle += 3.14159;
        }


        private void getMid()
        {
            double tempLength;
            MyPointer tempPoint = new MyPointer();

            if (pointLeft.y < pointRight.y)
            {
                tempLength = pointRight.y - pointLeft.y;
                tempPoint.x = pointRight.x - tempLength * Math.Tan(angleRight * 3.14159 / 180);
                tempPoint.y = pointLeft.y;
                pointMid.x = (tempPoint.x + pointLeft.x) / 2;
                pointMid.y = (tempPoint.y + pointLeft.y) / 2;

            }
            else
            {
                tempLength = pointLeft.y - pointRight.y;
                tempPoint.x = pointLeft.x - tempLength * Math.Tan(angleLeft * 3.14159 / 180);
                tempPoint.y = pointRight.y;
                pointMid.x = (tempPoint.x + pointRight.x) / 2;
                pointMid.y = (tempPoint.y + pointRight.y) / 2;

            }


            // original
            //angleMid = (angleLeft + angleRight) / 2;
            //angle = angleMid * 180 / 3.1415;

            if (Math.Abs(angleLeft - angleRight) < 0.15) {   // 两者相差小于9度
                angleMid = (angleLeft + angleRight) / 2;
                angle = angleMid * 180 / 3.1415;
            } else if (Math.Abs(angleLeft - 1.57) <= Math.Abs(angleRight - 1.57)
                    && Math.Abs(angleLeft - 1.57) < 0.2) {
                angleMid = angleLeft;
                angle = angleMid * 180 / 3.1415;
            } else if (Math.Abs(angleLeft - 1.57) > Math.Abs(angleRight - 1.57)
                    && Math.Abs(angleRight - 1.57) < 0.2) {
                angleMid = angleRight;
                angle = angleMid * 180 / 3.1415;
            } else {
                angle = 90;
            }

        }

        void getDisError(int rightStart, int rightEnd, int leftStart, int leftEnd)
        {
            double rightNum = 0, leftNum = 0;
            for (int i = rightStart; i < rightEnd; ++i)
            {
                rightNum += pointers[i].dis;
            }
            for (int i = leftStart; i < leftEnd; ++i)
            {
                leftNum += pointers[i].dis;
            }
            rightNum /= rightEnd - rightStart;
            leftNum /= leftEnd - leftStart;


            // original
            //dis = leftNum - rightNum;

            double threadhold = 300;    // 250
            double halfWide = 500;
            if (Math.Abs(rightNum - leftNum) < threadhold) {
                dis = leftNum - rightNum;
            } else if (Math.Abs(leftNum - halfWide) < threadhold) {
                dis = leftNum - halfWide;
            } else if (Math.Abs(halfWide - rightNum) < threadhold) {
                dis = halfWide - rightNum;
            } else {
                dis = 0;
            }


        }

        /// <summary>
        ///  new add for stop
        /// </summary>
        bool canGo = true;
        /// <summary>
        /// 返回是否可走信息
        /// </summary>
        /// <returns></returns>
        public bool CanGo() {
            return canGo;
        }
        /// <summary>
        /// 测试是否可走
        /// </summary>
        private void TestIfCanGo() {
            int dist = getMiddleValue(distanceInt[383], distanceInt[384], distanceInt[385]);
            // 无效数据
            if (dist < 30) {
                return;
            }
            if (dist < 400) {
                canGo = false;
            } else {
                canGo = true;
            }
        }

        /// <summary>
        /// 找出3个参数中值
        /// </summary>
        /// <param name="a">参数a</param>
        /// <param name="b">参数b</param>
        /// <param name="c">参数c</param>
        /// <returns>返回a，b，c的中值</returns>
        private int getMiddleValue(int a, int b, int c) {
            if (a < b && a < c) {
                return Math.Min(b, c);
            } else if (a > b && a > c) {
                return Math.Max(b, c);
            }
            return a;
        }




    }
}
