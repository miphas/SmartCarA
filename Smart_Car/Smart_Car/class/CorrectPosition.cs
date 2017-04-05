using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGVproject;
using Smart_Car;

namespace AGVproject.Class
{
    class CorrectPosition
    {
        ////////////////////////////////////////// public attribute ///////////////////////////////////////////////

        public KeyPoint currPoint;
        public struct KeyPoint { public double UrgK, UrgB; }

        ////////////////////////////////////////// private attribute ///////////////////////////////////////////////

        private KeyPoint prevPoint;

        private List<UrgPoint> urgPoints;
        private List<List<UrgPoint>> urgGroups;
        private PID_PARAMETER PID_parameter;
        private CONTROL_PARAMETER CON_parameter;

        private struct UrgPoint { public double X; public double Y; }
        private struct PID_PARAMETER
        {
            public double Kp;
            public double Ki;
            public double Kd;

            public double Error2;
            public double Error1;
            public double Error0;

            public double SumError;
        }
        private struct CONTROL_PARAMETER
        {
            public bool FirstKeyPoint;
            public bool LeftWall;

            public double Fit_Error; // 拟合允许误差
            public double Fit_Percent; // 要求拟合数据的靠近程度

            public double A_Error; // 角度调整允许误差 单位：0.01 度
            public double X_Error; // X 轴调整允许误差 单位：mm
            public double Y_Error; // Y 轴调整允许误差 单位：mm
        }

        ////////////////////////////////////////// public method ///////////////////////////////////////////////

        public void Start(Point point)
        {
            TH_SendCommand.StopReceiveUltraSonic();

            TH_RefreshUrgData.Open();
            TH_SendCommand.Open();

            while (TH_RefreshUrgData.TH_data.distance == null) ;

            if (TH_SendCommand.IsClose || TH_RefreshUrgData.IsClose) { return; }

            CON_parameter.FirstKeyPoint = false;
            prevPoint.UrgK = point.UrgK;
            prevPoint.UrgB = point.UrgB;

            // 粗调
            CON_parameter.Fit_Percent = 0.05;
            CON_parameter.A_Error = 50;
            CON_parameter.X_Error = 10;
            CON_parameter.Y_Error = 5;

            Adjust_A(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);
            Adjust_X(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);
            //Adjust_Y(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);

            // 细调
            CON_parameter.Fit_Percent = 0.02;
            CON_parameter.A_Error = 20;
            CON_parameter.X_Error = 5;
            CON_parameter.Y_Error = 2;

            Adjust_A(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);
            Adjust_X(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);
            //Adjust_Y(); TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);

            TH_RefreshUrgData.Close();
            TH_SendCommand.Close();

            while (TH_RefreshUrgData.TH_data.TH_running) ;
            while (TH_SendCommand.TH_data.TH_running) ;
        }
        public double[] GetUrg_K_B()
        {
            TH_RefreshUrgData.Open();

            while (TH_RefreshUrgData.TH_data.distance == null) ;

            System.Threading.Thread.Sleep(1000);

            CON_parameter.Fit_Percent = 0.01;
            GetUrgKB();

            TH_RefreshUrgData.Close();

            if (currPoint.UrgB > 1200) { return new double[3] { 0, 0, 0 }; }

            return new double[3] { 1, currPoint.UrgK, currPoint.UrgB };
        }

        ////////////////////////////////////////// private method ///////////////////////////////////////////////

        private void GetSonicData()
        {
            //TH_SendCommand.MeasureUltraSonic_0x86();

            //TH_SendCommand.TH_data.IsGetting = true;
            //while (TH_SendCommand.TH_data.IsSetting) ;

            //currPoint.UltraSonicL = TH_SendCommand.TH_data.Tail_L_Y;
            //currPoint.UltraSonicR = TH_SendCommand.TH_data.Tail_R_Y;
            //TH_SendCommand.TH_data.IsGetting = false;

            //TH_command.StopSendCommand_Sonic_0x86();
        }
        private void GetUrgKB()
        {
            // 取得数据
            GetUrgData_Head();

            // 分割成组
            urgGroups = new List<List<UrgPoint>>();
            CutGroup_UrgPoint(urgPoints);

            // 挑选出正前方数据，进行拟合
            List<UrgPoint> linePoints = GetHeadGroup_UrgPoint();
            double[] KB = Fit_UrgPoint(linePoints);
            currPoint.UrgK = KB[0];
            currPoint.UrgB = KB[1];

            // 拟合旁边墙的数据
            //if (!CON_parameter.FirstKeyPoint) { return; }

            //linePoints = GetSideGroup_UrgPoint();
            //KB = Fit_UrgPoint(linePoints);
            //currPoint.UrgExtraK = KB[0];
            //currPoint.UrgExtraB = KB[1];
        }

        private void GetUrgData_Head()
        {
            // 只取 30 度夹角的数据。
            int BG = (int)((75 - TH_RefreshUrgData.TH_data.AngleStart) / TH_RefreshUrgData.TH_data.AnglePace);
            int ED = (int)((105 - TH_RefreshUrgData.TH_data.AngleStart) / TH_RefreshUrgData.TH_data.AnglePace);

            TH_RefreshUrgData.TH_data.IsGetting = true;
            while (TH_RefreshUrgData.TH_data.IsSetting) ;
            
            urgPoints = new List<UrgPoint>();
            double sumDistance = 0;
            int N = 0;
            for (int i = BG; i <= ED; i++)
            {
                if (TH_RefreshUrgData.TH_data.distance[i] == 0) { continue; }

                UrgPoint ipoint = new UrgPoint();
                ipoint.X = TH_RefreshUrgData.TH_data.x[i];
                ipoint.Y = TH_RefreshUrgData.TH_data.y[i];

                urgPoints.Add(ipoint);
                N++;
                sumDistance += TH_RefreshUrgData.TH_data.distance[i];
            }

            TH_RefreshUrgData.TH_data.IsGetting = false;

            CON_parameter.Fit_Error = sumDistance / N * CON_parameter.Fit_Percent;
        }
        private void CutGroup_UrgPoint(List<UrgPoint> points)
        {
            // 点的数量不够
            if (points.Count == 0) { return; }
            if (points.Count == 1 || points.Count == 2) { urgGroups.Add(points); return; }

            // 基本参数
            double MaxDis = 0.0;
            int indexofmax = 0;

            // 直线参数
            double x1 = points[0].X, y1 = points[0].Y;
            double x2 = points[points.Count - 1].X, y2 = points[points.Count - 1].Y;

            double A = y2 - y1, B = -(x2 - x1), C = (x2 - x1) * y1 - (y2 - y1) * x1;

            // 寻找最大距离
            for (int i = 0; i < points.Count; i++)
            {
                double iDis = Math.Abs(A * points[i].X + B * points[i].Y + C) / Math.Sqrt(A * A + B * B);
                if (MaxDis > iDis) { continue; }

                indexofmax = i; MaxDis = iDis;
            }

            // 分割直线
            if (MaxDis <= CON_parameter.Fit_Error) { urgGroups.Add(points); return; }

            List<UrgPoint> newLine = new List<UrgPoint>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            CutGroup_UrgPoint(newLine);

            newLine = new List<UrgPoint>();
            for (int i = indexofmax; i < points.Count; i++) { newLine.Add(points[i]); }
            CutGroup_UrgPoint(newLine);
        }
        private List<UrgPoint> GetHeadGroup_UrgPoint()
        {
            // 挑选直线
            for (int i = 0; i < urgGroups.Count; i++)
            {
                double bgX = urgGroups[i][0].X;
                double edX = urgGroups[i][urgGroups[i].Count - 1].X;

                if (bgX > 0 && edX < 0) { return urgGroups[i]; }
                if (bgX < 0 && edX > 0) { return urgGroups[i]; }

                if (bgX == 0)
                {
                    if (i == 0) { return urgGroups[i]; }
                    if (urgGroups[i].Count >= urgGroups[i - 1].Count) { return urgGroups[i]; }
                    return urgGroups[i - 1];
                }
                if (edX == 0)
                {
                    if (i == urgGroups.Count) { return urgGroups[i]; }
                    if (urgGroups[i].Count >= urgGroups[i + 1].Count) { return urgGroups[i]; }
                    return urgGroups[i + 1];
                }
            }
            if (urgGroups.Count != 0) { return urgGroups[0]; }
            return new List<UrgPoint>();
        }
        private List<UrgPoint> GetSideGroup_UrgPoint()
        {
            // 挑选直线
            for (int i = 0; i < urgGroups.Count; i++)
            {
                double bgY = urgGroups[i][0].Y;
                double edY = urgGroups[i][urgGroups[i].Count - 1].Y;
                double X = urgGroups[i][0].Y;

                if (X > 0 && bgY <= 0 && edY >= 0) { if (!CON_parameter.LeftWall) { return urgGroups[i]; } }
                if (X < 0 && bgY >= 0 && edY <= 0) { if (CON_parameter.LeftWall) { return urgGroups[i]; } }
            }
            return urgGroups[0];
        }
        private double[] Fit_UrgPoint(List<UrgPoint> points)
        {
            double sumX = 0, sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0;
            int N = points.Count;

            for (int i = 0; i < N; i++)
            {
                sumX += points[i].X;
                sumY += points[i].Y;

                sumXX += points[i].X * points[i].X;
                sumXY += points[i].X * points[i].Y;
                sumYY += points[i].Y * points[i].Y;
            }

            double denominator = N * sumXX - sumX * sumX;
            if (denominator == 0) { denominator = 0.01; }

            double UrgK = (N * sumXY - sumX * sumY) / denominator;
            double UrgB = (sumXX * sumY - sumX * sumXY) / denominator;

            UrgK = Math.Atan(UrgK) * 180 / Math.PI;
            return new double[2] { UrgK, UrgB };
        }

        private void Adjust_A()
        {
            Initial_PID_parameter(1.0, 0.0, 0.0);
            GetUrgKB();

            while (true)
            {
                // 比较精度
                double current = currPoint.UrgK * 100;
                double target = prevPoint.UrgK * 100;
                if (Math.Abs(current - target) <= CON_parameter.A_Error) { return; }

                // 获取控制速度
                double adjustA = PIDcontroller1(current, target);
                int aSpeed = (int)(-adjustA * 100 / TH_SendCommand.TH_data.TimeForControl);
                if (aSpeed == 0) { return; }

                // 限幅
                if (aSpeed > 200) { aSpeed = 200; }
                if (aSpeed < -200) { aSpeed = -200; }

                // 控制
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);

                // 更新数据
                GetUrgKB();
            }
        }
        private void Adjust_X()
        {
            Initial_PID_parameter(1.0, 0.0, 0.0);

            while (true)
            {
                // 比较精度
                double current = currPoint.UrgB;
                double target = prevPoint.UrgB;
                if (Math.Abs(current - target) < CON_parameter.X_Error) { return; }

                // 获得输出
                double adjustX = PIDcontroller1(current, target);
                int xSpeed = (int)(-adjustX * 100 / TH_SendCommand.TH_data.TimeForControl);
                if (xSpeed == 0) { return; }

                // 限幅
                if (xSpeed > 200) { xSpeed = 200; }
                if (xSpeed < -200) { xSpeed = -200; }

                // 控制
                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, 0, 0);

                // 更新数据
                GetUrgKB();
            }
        }
        private void Adjust_Y()
        {
            //Initial_PID_parameter(1.0, 0.0, 0.0);

            //GetSonicData(TH_command);

            //while (true)
            //{
            //    // 比较精度
            //    bool NearLeft = currPoint.UltraSonicL <= currPoint.UltraSonicR;
            //    double current = NearLeft ? currPoint.UltraSonicL : currPoint.UltraSonicR;
            //    double target = NearLeft ? prevPoint.UltraSonicL : prevPoint.UltraSonicR;
            //    if (Math.Abs(current - target) < CON_parameter.Y_Error) { break; }

            //    // 得到调整量
            //    double adjustY = PIDcontroller1(current, target);

            //    if (NearLeft) { adjustY = -adjustY; }

            //    int ySpeed = (int)(-adjustY * 100 / TH_SendCommand.TH_data.TimeForControl / 2);
            //    if (ySpeed == 0) { break; }

            //    // 限幅
            //    if (ySpeed > 100) { ySpeed = 100; }
            //    if (ySpeed < -100) { ySpeed = -100; }

            //    // 控制
            //    TH_command.AGV_MoveControl_0x70(0, ySpeed, 0);

            //    // 更新数据
            //    GetSonicData(TH_command);
            //}

            //TH_command.StopSendCommand_Sonic_0x86();
        }
        
        private void Initial_PID_parameter(double Kp, double Ki, double Kd)
        {
            PID_parameter.Kp = Kp;
            PID_parameter.Ki = Ki;
            PID_parameter.Kd = Kd;

            PID_parameter.Error0 = 0;
            PID_parameter.Error1 = 0;
            PID_parameter.Error2 = 0;

            PID_parameter.SumError = 0;
        }
        private double PIDcontroller1(double current, double target) // 位置式
        {
            PID_parameter.Error2 = PID_parameter.Error1;
            PID_parameter.Error1 = PID_parameter.Error0;
            PID_parameter.Error0 = current - target;

            PID_parameter.SumError += PID_parameter.Error0;

            double pControl = PID_parameter.Kp * PID_parameter.Error0;
            double iControl = PID_parameter.Ki * PID_parameter.SumError;
            double dControl = PID_parameter.Kd * (PID_parameter.Error0 - PID_parameter.Error1);

            return -(pControl + iControl + dControl);
        }
        private double PIDcontroller2(double current, double target) // 增量式
        {
            PID_parameter.Error2 = PID_parameter.Error1;
            PID_parameter.Error1 = PID_parameter.Error0;
            PID_parameter.Error0 = current - target;

            double pControl = PID_parameter.Kp * (PID_parameter.Error0 - PID_parameter.Error1);
            double iControl = PID_parameter.Ki * PID_parameter.Error0;
            double dControl = PID_parameter.Kd * (PID_parameter.Error0 - 2 * PID_parameter.Error1 + PID_parameter.Error2);

            return -(pControl + iControl + dControl);
        }
    }
}
