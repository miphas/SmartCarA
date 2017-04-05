using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Smart_Car
{
    class TH_SendCommand
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public static bool IsOpen { get { return controlport != null && controlport.IsOpen; } }
        public static bool IsClose { get { return controlport == null || !controlport.IsOpen; } }

        public static TH_DATA TH_data;
        public struct TH_DATA
        {
            public string PortName;
            public int BaudRate;

            public bool IsGetting;
            public bool IsSetting;
            
            public bool TH_cmd_abort;
            public bool TH_running { get { return TH_control.ThreadState == System.Threading.ThreadState.Running; } }
            public bool TH_hanging;
            
            public int Head_L_X;
            public int Head_L_Y;
            public int Head_R_X;
            public int Head_R_Y;
            public int Tail_L_X;
            public int Tail_L_Y;
            public int Tail_R_X;
            public int Tail_R_Y;

            public uint posX;
            public uint posY;
            public double posA;

            public int TimeForControl;
        }

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static System.Threading.Thread TH_control = new System.Threading.Thread(SendCommand_100ms);

        public static SerialPort controlport;
        
        private static PORT_CONFIG portConfig;
        private struct PORT_CONFIG
        {
            public bool Sent0x86;
            public bool Sent0x84;
            public bool Sent0x70;
            public bool Receiving_0x86;
            public bool Receiving_0x84;
            public bool Receiving_0x70;

            public int ReceiveLength_0x86;
            public int ReceiveLength_0x84;
            public int ReceiveLength_0x70;

            public bool IsReading;
            public bool IsClosing;

            public bool IsSettingCommand;
            public bool IsGettingCommand;

            public byte[] ControlCommand;
            public byte[] SonicCommand;
            public byte[] DistanceCommand;

            public List<byte> Receive;
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        public static bool Open()
        {
            //if (IsOpen) { return true; }
            //if (CreatePort) { controlport = new SerialPort(TH_data.PortName, TH_data.BaudRate); }

            try
            {
                // 初始化线程
                Initial_TH_SendCommand();

                // 打开串口
                controlport.DataReceived -= portDataReceived;
                controlport.DataReceived += portDataReceived;
                //controlport.Open();

                // 打开线程
                TH_data.TH_cmd_abort = true;
                while (TH_control != null && TH_control.ThreadState == System.Threading.ThreadState.Running) { return false; } ;
                TH_data.TH_cmd_abort = false;
                TH_control = new System.Threading.Thread(SendCommand_100ms);
                TH_control.Start();

                return true;
            }
            catch { return false; }
        }
        public static bool Close()
        {
            controlport.DataReceived -= portDataReceived;
            TH_data.TH_cmd_abort = true;
            return true;
        }

        public static bool AGV_MoveControl_0x70(int xSpeed, int ySpeed, int aSpeed)
        {
            // 限幅
            aSpeed = (int)Math.Round(aSpeed * 3.14159 / 180);
            if (xSpeed > 800) { xSpeed = 800; }
            if (xSpeed < -800) { xSpeed = -800; }
            if (ySpeed > 800) { ySpeed = 800; }
            if (ySpeed < -800) { ySpeed = -800; }
            if (aSpeed < 0) { aSpeed = 128 - aSpeed; }
            if (aSpeed > 255) { aSpeed = 255; }
            if (aSpeed < 0) { aSpeed = 0; }
            
            int speed = 0, direction = 0, rotate = aSpeed;

            speed = (int)(Math.Sqrt(xSpeed * xSpeed + ySpeed * ySpeed));
            if (xSpeed == 0 && ySpeed >= 0)
            {
                direction = 0;
            }
            else if (xSpeed == 0 && ySpeed < 0)
            {
                direction = 180;
            }
            else if (xSpeed >= 0 && ySpeed == 0)
            {
                direction = 90;
            }
            else if (xSpeed < 0 && ySpeed == 0)
            {
                direction = 270;
            }
            else
            {
                double angle = Math.Atan((Math.Abs((double)xSpeed)) / Math.Abs((double)ySpeed));
                direction = (int)((angle) * 180 / Math.PI);


                if (xSpeed > 0 && ySpeed > 0) { }
                if (xSpeed > 0 && ySpeed < 0) { direction = 180 - direction; }
                if (xSpeed < 0 && ySpeed > 0) { direction = 360 - direction; }
                if (xSpeed < 0 && ySpeed < 0) { direction = 180 + direction; }
            }


            //if (xSpeed > 0) { speed = xSpeed; direction = 90; rotate = 0; }
            //if (xSpeed < 0) { speed = -xSpeed; direction = 270; rotate = 0; }
            //if (ySpeed > 0) { speed = ySpeed; direction = 0; rotate = 0; }
            //if (ySpeed < 0) { speed = -ySpeed; direction = 180; rotate = 0; }

            // 填充 0x70 命令
            byte[] ControlCommand = new byte[11];
            ControlCommand[0] = 0xf1;
            ControlCommand[1] = 0x70;
            ControlCommand[2] = (byte)(speed >> 8);
            ControlCommand[3] = (byte)(speed);
            ControlCommand[4] = (byte)(direction >> 8);
            ControlCommand[5] = (byte)(direction);
            ControlCommand[6] = (byte)(rotate);
            ControlCommand[7] = 0x00;
            Fill_CheckBytes(ref ControlCommand);
            
            // 写入
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;
            
            portConfig.Sent0x70 = true;
            portConfig.ControlCommand = ControlCommand;

            portConfig.IsSettingCommand = false;
            return true;
        }
        public static bool MeasureDistance_0x84(int waitTime = 0)
        {
            // 填充 0x84 命令
            byte[] DistanceCommand = new byte[4] { 0xf1, 0x84, 0x00, 0x00 };
            Fill_CheckBytes(ref DistanceCommand);

            // 写入
            portConfig.Receiving_0x84 = true;
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;

            portConfig.Sent0x84 = true;
            portConfig.DistanceCommand = DistanceCommand;

            portConfig.IsSettingCommand = false;
            
            // 等待结果
            if (waitTime <= 0) { while (portConfig.Receiving_0x84) ; }
            if (waitTime > 0) { System.Threading.Thread.Sleep(waitTime); }
            return !portConfig.Receiving_0x84;
        }
        public static void MeasureUltraSonic_0x86()
        {
            controlport.DataReceived -= portDataReceived;
            controlport.DataReceived += portDataReceived;
            Initial_TH_SendCommand();
            //while (TH_control != null && TH_control.ThreadState == System.Threading.ThreadState.Running) { retur; };
            TH_control.Start();

            // 填充命令
            byte[] SonicCommand = new byte[4] { 0xf1, 0x86, 0x00, 0x00 };
            Fill_CheckBytes(ref SonicCommand);

            // 写入
            portConfig.Receiving_0x86 = true;
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;

            portConfig.Sent0x86 = true;
            portConfig.SonicCommand = SonicCommand;

            portConfig.IsSettingCommand = false;
        }

        public static void StopReceiveUltraSonic() { portConfig.SonicCommand = null; }
        
        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void SendCommand_100ms()
        {
            while (true)
            {
                // 串口已关闭
                if (controlport == null && !controlport.IsOpen) { TH_control.Abort(); TH_data.TH_cmd_abort = false; return; }

                // 外部要求关闭线程
                if (TH_data.TH_cmd_abort) { TH_control.Abort(); TH_data.TH_cmd_abort = false; return; }

                // 外部要求挂起线程
                if (TH_data.TH_hanging) { continue; }
                
                // 写入失败，再次写入。
                try
                {
                    controlport.ReceivedBytesThreshold = 1;
                    controlport.DiscardOutBuffer();

                    portConfig.IsGettingCommand = true;
                    while (portConfig.IsSettingCommand) ;

                    //if (portConfig.Sent0x86) {  }
                    //if (portConfig.Sent0x84) { System.Threading.Thread.Sleep(5); controlport.Write(portConfig.DistanceCommand, 0, portConfig.DistanceCommand.Length); }
                    //if (portConfig.Sent0x70) { System.Threading.Thread.Sleep(20); controlport.Write(portConfig.ControlCommand, 0, portConfig.ControlCommand.Length); }

                    if (portConfig.SonicCommand != null)
                    {
controlport.Write(portConfig.SonicCommand, 0, portConfig.SonicCommand.Length);System.Threading.Thread.Sleep(50);
                    }
                    
                     controlport.Write(portConfig.ControlCommand, 0, portConfig.ControlCommand.Length);
       
                    portConfig.Sent0x84 = false;
                    //portConfig.Sent0x86 = false;
                    portConfig.IsGettingCommand = false;
                }
                catch { portConfig.IsGettingCommand = false; continue; }

                // 执行命令
                System.Threading.Thread.Sleep(150);//TH_data.TimeForControl);
            }
        }
        private static void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭
            if (portConfig.IsClosing || IsClose) { return; }

            // 正在读取
            portConfig.IsReading = true;
            int receLength = 0;
            try
            {
                receLength = controlport.BytesToRead;
                byte[] TempReceive = new byte[receLength];
                controlport.Read(TempReceive, 0, receLength);

                foreach (byte ibyte in TempReceive) { portConfig.Receive.Add(ibyte); }
            }
            catch
            {
                portConfig.IsReading = false; portConfig.Receive = new List<byte>(); return;
            }
            portConfig.IsReading = false;

            // 满足长度要求
            if (portConfig.Receive.Count < 60) { return; }
            
            // 寻找 0xf1
            int indexBG = -1;
            for (int i = 0; i < receLength; i++)
            {
                if (portConfig.Receive[i] != 0xf1) { continue; }
                indexBG = i; break;
            }

            // 校验帧头
            if (indexBG == -1) { portConfig.Receive = new List<byte>(); return; }
            if (portConfig.Receive.Count < indexBG + portConfig.ReceiveLength_0x86) { portConfig.Receive = new List<byte>(); return; }
            if (portConfig.Receive[indexBG + 1] != 0x86) { portConfig.Receive = new List<byte>(); return; }

            // 校验帧尾
            uint sumReceived = 0;
            for (int i = 0; i < portConfig.ReceiveLength_0x86 - 2; i++) { sumReceived += portConfig.Receive[indexBG + i]; }
            sumReceived = (sumReceived >> 16) + (sumReceived & 0x0000ffff);

            byte checkH = (byte)(sumReceived >> 8);
            byte checkL = (byte)(sumReceived & 0x00ff);

            if (portConfig.Receive[indexBG + portConfig.ReceiveLength_0x86 - 2] != checkH) { portConfig.Receive = new List<byte>(); return; }
            if (portConfig.Receive[indexBG + portConfig.ReceiveLength_0x86 - 1] != checkL) { portConfig.Receive = new List<byte>(); return; }

            // 填充数据
            TH_data.IsSetting = true;
            while (TH_data.IsGetting) ;

            TH_data.Head_L_Y = (portConfig.Receive[indexBG + 2]) << 8 | portConfig.Receive[indexBG + 3];
            TH_data.Head_L_X = (portConfig.Receive[indexBG + 4]) << 8 | portConfig.Receive[indexBG + 5];
            TH_data.Head_R_X = (portConfig.Receive[indexBG + 6]) << 8 | portConfig.Receive[indexBG + 7];
            TH_data.Head_R_Y = (portConfig.Receive[indexBG + 8]) << 8 | portConfig.Receive[indexBG + 9];
            TH_data.Tail_R_Y = (portConfig.Receive[indexBG + 10]) << 8 | portConfig.Receive[indexBG + 11];
            TH_data.Tail_R_X = (portConfig.Receive[indexBG + 12]) << 8 | portConfig.Receive[indexBG + 13];
            TH_data.Tail_L_X = (portConfig.Receive[indexBG + 14]) << 8 | portConfig.Receive[indexBG + 15];
            TH_data.Tail_L_Y = (portConfig.Receive[indexBG + 16]) << 8 | portConfig.Receive[indexBG + 17];

            //TH_data.Head_L_Y = portConfig.Receive[indexBG + 3];
            //TH_data.Head_L_X = portConfig.Receive[indexBG + 5];
            //TH_data.Head_R_X =  portConfig.Receive[indexBG + 7];
            //TH_data.Head_R_Y = portConfig.Receive[indexBG + 9];
            //TH_data.Tail_R_Y =  portConfig.Receive[indexBG + 11];
            //TH_data.Tail_R_X =  portConfig.Receive[indexBG + 13];
            //TH_data.Tail_L_X = portConfig.Receive[indexBG + 15];
            //TH_data.Tail_L_Y =  portConfig.Receive[indexBG + 17];

            TH_data.IsSetting = false;
            portConfig.Receiving_0x86 = false;
            portConfig.Receive = new List<byte>();
        }

        public static void Initial_TH_SendCommand()
        {
            portConfig.Sent0x70 = false;
            portConfig.Sent0x84 = false;
            portConfig.Sent0x86 = false;
            portConfig.ReceiveLength_0x86 = 20;
            portConfig.ReceiveLength_0x84 = 12;
            portConfig.ReceiveLength_0x70 = 7;
            
            portConfig.IsReading = false;
            portConfig.IsClosing = false;

            portConfig.IsSettingCommand = true;
            portConfig.IsGettingCommand = false;

            portConfig.Receive = new List<byte>();
            
            TH_data.IsSetting = false;
            TH_data.IsGetting = false;

            TH_data.TH_cmd_abort = false;
            TH_data.TH_hanging = false;
            TH_data.TimeForControl = 100;

            AGV_MoveControl_0x70(0, 0, 0);
        }
        
        private static void Fill_CheckBytes(ref byte[] command)
        {
            uint sumCommand = 0;
            for (int i = 0; i < command.Length - 2; i++) { sumCommand += command[i]; }

            sumCommand = (sumCommand >> 16) + (sumCommand & 0x0000ffff);

            command[command.Length - 2] = (byte)(sumCommand >> 8);
            command[command.Length - 1] = (byte)(sumCommand & 0x000000ff);
        }
    }
}
