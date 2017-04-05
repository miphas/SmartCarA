using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO;
using System.IO.Ports;

namespace Smart_Car {
    public class ConPort : Port {

        private bool lockControl = true;
        private ConCommand conCommand;
        private PosCommand posCommand;
        private DisCommand disCommand;
        private SonicCommand sonicCommand;
        private Commander commander = new Commander();

        public bool LockControl {
            get {
                return lockControl;
            }
            set {
                lockControl = value;
            }
        }
        /// <summary>
        /// default constructor and destructor
        /// </summary>
        public ConPort() {
        }
        ~ConPort() { }
        private static ConPort conport;
        public static ConPort getInstance(string portName, string portBaudrate)
        {
            if (conport == null)
            {
                conport = new ConPort();
                conport.OpenPort(portName, portBaudrate);
            }
            return conport;
        }
        /// <summary>
        /// override openPort method for ConPort 
        /// </summary>
        /// <param name="portName">portName of contol port</param>
        /// <param name="portBaudrate">portBaudrate of control port</param>
        /// <returns>true if open successfully</returns>
        public override bool OpenPort(string portName, string portBaudrate) {
            bool IsOpen = base.OpenPort(portName, portBaudrate);
            if (IsOpen) {
                TH_SendCommand.controlport = base.port;

                lockControl = false;
                conCommand = new ConCommand(port);
                posCommand = new PosCommand(port);
                disCommand = new DisCommand(port);
                sonicCommand = new SonicCommand(port);
            }
            return IsOpen;
        }

        /// <summary>
        /// override closePort method for ConPort
        /// </summary>
        /// <returns>if close successfully</returns>
        public override bool ClosePort() {
            bool IsClose = base.ClosePort();
            if (IsClose) {
                lockControl = true;
            }
            return IsClose;
        }



        /// <summary>
        /// send direct control command to auto car by using go, shift and rotate speed
        /// </summary>
        /// <param name="goingSpeed">speed for moving ahead</param>
        /// <param name="shiftSpeed">speed for shifting</param>
        /// <param name="rotateSpeed">speed for rotating</param>
        public void controlDirectAGV(int goingSpeed, int shiftSpeed, int rotateSpeed) {

            //conCommand.GoingSpeed = goingSpeed;
            //conCommand.ShiftSpeed = shiftSpeed;
            //conCommand.RotateSpeed = rotateSpeed;

            //20160531
            conCommand.setWithOldParam(goingSpeed, shiftSpeed, rotateSpeed);

            if (!lockControl) {
                commander.Command = conCommand;
                commander.SendCommand();
            }
        }

        public void UrgControl(int goingSpeed, double angleOffset, double disOffset) {
            int rotateSpeed = -(int)(1.25 * angleOffset);
            int shiftSpeed = -(int)(0.02 * disOffset);

            controlDirectAGV(goingSpeed, shiftSpeed, rotateSpeed);
        }

        public void getSonicDistance() {
            if (!lockControl) {
                commander.Command = sonicCommand;
                commander.SendCommand();
            }
        }

        public int HeadLL {
            get {
                return sonicCommand.HeadLL;
            }
        }
        public int HeadLH {
            get {
                return sonicCommand.HeadLH;
            }
        }
        public int HeadRH {
            get {
                return sonicCommand.HeadRH;
            }
        }
        public int HeadRR {
            get {
                return sonicCommand.HeadRR;
            }
        }
        public int BackRR {
            get {
                return sonicCommand.BackRR;
            }
        }
        public int BackRB {
            get {
                return sonicCommand.BackRB;
            }
        }
        public int BackLB {
            get {
                return sonicCommand.BackLB;
            }
        }
        public int BackLL {
            get {
                return sonicCommand.BackLL;
            }
        }


        public int PosX {
            get {
                return posCommand.PosX;
            }
            set {
                posCommand.PosX = value;
            }
        }
        public int PosY {
            get {
                return posCommand.PosY;
            }
            set {
                posCommand.PosY = value;
            }
        }
        public void GetPosition() {
            if (!lockControl) {
                commander.Command = posCommand;
                commander.SendCommand();
            }
        }


        public int LHeadSpeed {
            get {
                return disCommand.LHeadSpeed;
            }
            set {
                disCommand.LHeadSpeed = value;
            }
        }
        public int RHeadSpeed {
            get {
                return disCommand.RHeadSpeed;
            }
            set {
                disCommand.RHeadSpeed = value;
            }
        }
        public int LBackSpeed {
            get {
                return disCommand.LBackSpeed;
            }
            set {
                disCommand.LBackSpeed = value;
            }
        }
        public int RBackSpeed {
            get {
                return disCommand.RBackSpeed;
            }
            set {
                disCommand.RBackSpeed = value;
            }
        }
        public void GetWheelSpeed() {
            if (!lockControl) {
                commander.Command = disCommand;
                commander.SendCommand();
            }
        }

        public String getInfo() {
            return posCommand.ReceiveToString();
        }
        public String getConInfo(int len) {
            return conCommand.getReceiveInfo(len);
        }

        // bottom of class ConPort
    }
    // end of class ConPort 



    // 命令类，各种命令的父类
    public abstract class Command {
        protected SerialPort port;
        private byte[] command;
        private byte[] receive;
        private byte[] check;

        public Command(SerialPort port, int commandLength, int receiveLength) {
            this.port = port;
            command = new byte[commandLength];
            receive = new byte[receiveLength];
            check = new byte[2];
        }
        public abstract void Init();
        public abstract void CreateCommand();
        public abstract bool ExecuteCommand();
        protected void SetCommand(int pos, byte value) {
            command[pos] = value;
        }
        protected void SetCommand(byte[] value) {
            for (int i = 0; i < value.Length; ++i) {
                command[i] = value[i];
            }
        }
        protected byte GetReceive(int pos) {
            return receive[pos];
        }
        protected void CreateCheckNum(byte[] comm, int start, int end) {
            uint checkNum = 0;
            for (int i = start; i < end; ++i) {
                checkNum += (uint)comm[i];
            }
            check[0] = (byte)(checkNum / 256);
            check[1] = (byte)(checkNum % 256);
        }
        protected void CheckNumFill(int pos) {
            CreateCheckNum(command, 0, pos);
            command[pos++] = check[0];
            command[pos++] = check[1];
        }
        protected bool CheckReceive(int pos) {
            CreateCheckNum(receive, 0, pos);
            return (receive[0] == command[0]) && (receive[1] == command[1])
                && (receive[pos] == check[0]) && (receive[pos + 1] == check[1]);
        }
        protected int GetNumber(int start, int end) {
            int result = 0;
            for (int i = start; i < end; ++i) {
                result = result * 256 + (int)(receive[i]);
            }
            //result = BitConverter.ToInt32(receive, start);
            return result;
        }
        protected int GetLongNum(int start, int end) {
            long result = 0;
            //if (end)
            return (int)result;
        }
        protected void SendCommand() {
            if (port != null && port.IsOpen) {
                port.Write(command, 0, command.Length);
            }
        }
        protected void GetReceive() {
            if (port != null && port.IsOpen) {
                //port.Read(receive, 0, receive.Length);
            }
        }
        protected String getReceiveString() {
            return BitConverter.ToString(receive);
        }
    }

    // 控制行进的命令
    public class ConCommand : Command {
        private int goingSpeed;
        private int shiftSpeed;
        private int rotateSpeed;

        private const int limGoSpeedL = 0, limGoSpeedU = 800;
        private const int limShSpeedL = 0, limShSpeedU = 359;
        private const int limRoSpeedL = -128, limRoSpeedU = 127;

        public int GoingSpeed {
            get {
                return goingSpeed;
            }
            set {
                goingSpeed = limitValue(value, limGoSpeedL, limGoSpeedU);
                //goingSpeed = value;
            }
        }
        public int ShiftSpeed {
            get {
                return shiftSpeed;
            }
            set {
                shiftSpeed = limitValue(value, limShSpeedL, limShSpeedU);
                //shiftSpeed = value;
            }
        }
        public int RotateSpeed {
            get {
                return rotateSpeed;
            }
            set {
                rotateSpeed = limitValue(value, limRoSpeedL, limRoSpeedU);
                //rotateSpeed = value;
            }
        }
        public ConCommand(SerialPort port)
            : base(port, 11, 7) {
            this.Init();
        }

        public override void Init() {
            byte[] controlCommand = { 0xF1 , 0x70 , 0x00 , 0x00 ,
                                      0x00 , 0x00 , 0x00 , 0x00 ,
                                      0x00 , 0x00 , 0x00};
            SetCommand(controlCommand);
        }
        public override void CreateCommand() {

            // 5-21 add
            //GetAngleInfo();

            uint speed = (uint)GoingSpeed;
            SetCommand(2, (byte)(speed / 256));
            SetCommand(3, (byte)(speed % 256));

            uint angle = (uint)ShiftSpeed;
            SetCommand(4, (byte)(angle / 256));
            SetCommand(5, (byte)(angle % 256));

            uint rotate = (RotateSpeed < 0) ? (uint)(128 - RotateSpeed) : (uint)(RotateSpeed);
            SetCommand(6, (byte)rotate);

            CheckNumFill(9);
        }
        public override bool ExecuteCommand() {
            SendCommand();
            GetReceive();
            if (!CheckReceive(5)) {
                return false;
            }
            return true;
        }

        public void setWithOldParam(int ahead, int shift, int rotate) {
            double param = 180 / Math.PI;
            shift = -shift;
            if (ahead == 0 && shift >= 0) {
                ahead = shift;
                shift = 0;
            } else if (ahead == 0 && shift < 0) {
                ahead = -shift;
                shift = 180;
            } else if (ahead > 0 && shift == 0) {
                shift = 90;
            } else if (ahead < 0 && shift == 0) {
                ahead = -ahead;
                shift = 270;
            } else if (ahead > 0 && shift > 0) {
                shift = (int)(param * Math.Atan(ahead / (double)shift));
            } else if (ahead > 0 && shift < 0) {
                shift = 180 - (int)(param * Math.Atan(ahead / -(double)shift));
            } else if (ahead < 0 && shift > 0) {
                shift = 360 - (int)(param * Math.Atan(ahead / -(double)shift));
                ahead = -ahead;
            } else if (ahead < 0 && shift < 0) {
                shift = 180 + (int)(param * Math.Atan(ahead / (double)shift));
                ahead = -ahead;
            }

            GoingSpeed = ahead;
            ShiftSpeed = shift;
            RotateSpeed = rotate;
        }
        public String getReceiveInfo(int len) {
            return base.getReceiveString();
        }
        private int limitValue(int value, int minVal, int maxVal) {
            if (value < minVal) {
                value = minVal;
            } else if (value > maxVal) {
                value = maxVal;
            }
            return value;
        }

        private void GetAngleInfo() {
            if (GoingSpeed == 0 && ShiftSpeed == 0) {
                return;
            }
            if (GoingSpeed == 0 && ShiftSpeed > 0) {
                GoingSpeed = ShiftSpeed;
                ShiftSpeed = 0;
                return;
            }
            if (GoingSpeed > 0 && ShiftSpeed > 0) {
                int speed = (int)Math.Sqrt(Math.Pow(GoingSpeed, 2) + Math.Pow(ShiftSpeed, 2));
                ShiftSpeed = (int)Math.Atan(GoingSpeed / ShiftSpeed);
                GoingSpeed = speed;
                return;
            }
            if (GoingSpeed > 0 && ShiftSpeed == 0) {
                ShiftSpeed = 90;
                return;
            }
            if (GoingSpeed > 0 && ShiftSpeed < 0) {
                int speed = (int)Math.Sqrt(Math.Pow(GoingSpeed, 2) + Math.Pow(ShiftSpeed, 2));
                ShiftSpeed = 90 + (int)Math.Atan(-GoingSpeed / ShiftSpeed);
                GoingSpeed = speed;
                return;
            }
            if (GoingSpeed == 0 && ShiftSpeed < 0) {
                GoingSpeed = -ShiftSpeed;
                ShiftSpeed = 180;
                return;
            }
            if (GoingSpeed < 0 && ShiftSpeed < 0) {
                int speed = (int)Math.Sqrt(Math.Pow(GoingSpeed, 2) + Math.Pow(ShiftSpeed, 2));
                ShiftSpeed = 180 + (int)Math.Atan(GoingSpeed / ShiftSpeed);
                GoingSpeed = speed;
                return;
            }
            if (GoingSpeed < 0 && ShiftSpeed == 0) {
                GoingSpeed = -GoingSpeed;
                ShiftSpeed = 270;
                return;
            }
            if (GoingSpeed < 0 && ShiftSpeed > 0) {
                int speed = (int)Math.Sqrt(Math.Pow(GoingSpeed, 2) + Math.Pow(ShiftSpeed, 2));
                ShiftSpeed = 270 + (int)Math.Atan(-GoingSpeed / ShiftSpeed);
                GoingSpeed = speed;
            }
        }



    }
    // 获得位置信息的命令
    public class PosCommand : Command {
        public int PosX {
            get;
            set;
        }
        public int PosY {
            get;
            set;
        }
        public PosCommand(SerialPort port)
            : base(port, 4, 8) {
            this.Init();
        }
        public override void Init() {
            SetCommand(0, 0xF1);
            SetCommand(1, 0x84);
            CheckNumFill(2);
        }
        public override void CreateCommand() {
        }
        public override bool ExecuteCommand() {
            SendCommand();
            /*
            for (int i = 0; i < 12; ++i) {
                port.ReadByte();
            }
            */
            GetReceive();
            /*
            if ( !CheckReceive(10)) {
                return false;
            }
             **/
            PosY = GetNumber(2, 6);
            //PosX = GetNumber(6, 10);
            return true;
        }
        public String ReceiveToString() {
            byte[] test = new byte[2];
            port.Read(test, 0, 2);
            return BitConverter.ToString(test);
        }
    }

    // 获得历程信息的命令
    public class DisCommand : Command {
        public int LHeadSpeed {
            get;
            set;
        }
        public int RHeadSpeed {
            get;
            set;
        }
        public int LBackSpeed {
            get;
            set;
        }
        public int RBackSpeed {
            get;
            set;
        }
        public DisCommand(SerialPort port)
            : base(port, 4, 12) {
            this.Init();
        }

        public override void Init() {
            SetCommand(0, 0xF1);
            SetCommand(1, 0x85);
            CheckNumFill(2);
        }

        public override void CreateCommand() {
        }

        public override bool ExecuteCommand() {
            SendCommand();
            GetReceive();
            if (!CheckReceive(10)) {
                return false;
            }
            LHeadSpeed = GetNumber(2, 4);
            RHeadSpeed = GetNumber(4, 6);
            LBackSpeed = GetNumber(6, 8);
            RBackSpeed = GetNumber(8, 10);
            return true;
        }
    }

    public class SonicCommand : Command {
        public int HeadLL {
            get;
            set;
        }
        public int HeadLH {
            get;
            set;
        }
        public int HeadRR {
            get;
            set;
        }
        public int HeadRH {
            get;
            set;
        }
        public int BackLL {
            get;
            set;
        }
        public int BackLB {
            get;
            set;
        }
        public int BackRR {
            get;
            set;
        }
        public int BackRB {
            get;
            set;
        }

        public SonicCommand(SerialPort port)
            : base(port, 4, 20) {
            this.Init();
        }

        public override void Init() {
            SetCommand(0, 0xF1);
            SetCommand(1, 0x86);
            CheckNumFill(2);
        }

        public override void CreateCommand() {
        }

        public override bool ExecuteCommand() {
            SendCommand();
            GetReceive();
            if (!CheckReceive(18)) {
                return false;
            }
            HeadLL = GetNumber(3,4);
            HeadLH = GetNumber(5, 6);
            HeadRH = GetNumber(7, 8);
            HeadRR = GetNumber(9, 10);
            BackRR = GetNumber(11, 12);
            BackRB = GetNumber(13, 14);
            BackLB = GetNumber(15, 16);
            BackLL = GetNumber(17, 18);
            return true;
        }

        public String getReceiveInfo() {
            return base.getReceiveString();
        }

    }

    // 发送命令的命令者
    public class Commander {
        Command command;
        public Command Command {
            set {
                command = value;
            }
        }

        public void SendCommand() {
            command.CreateCommand();
            command.ExecuteCommand();
        }
    }
}
