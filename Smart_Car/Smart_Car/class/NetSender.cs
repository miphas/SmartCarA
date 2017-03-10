using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Smart_Car {
    class PositionServer {
        static Socket socket;
        static Socket clientSocket;
        private static String ipStr = "172.22.173.149"; //172.22.173.149  192.168.0.2
        private static int port = 8885;   //端口号
        private static byte[] result = new byte[1024];

        public bool connectClient() {
            //服务器IP地址
            IPAddress ip = IPAddress.Parse(ipStr);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                //绑定IP地址：端口
                socket.Bind(new IPEndPoint(ip, port));
                socket.Listen(10);    //设定最多10个排队连接请求
            }
            catch (Exception ex) {
                return false;
            }
            Console.WriteLine("NetWork Build Successed");
            return true;
        }

        /// <summary>
        /// 监听客户端连接
        /// </summary>
        public bool sendPosition(int x, int y) {
            //Socket clientSocket = socket.Accept();
            try {
                //通过clientSocket接收数据
                int receiveNumber = clientSocket.Receive(result);
                String msg = Encoding.ASCII.GetString(result, 0, receiveNumber);
                Console.WriteLine(msg);
                if (msg == "position") {
                    clientSocket.Send(Encoding.ASCII.GetBytes(x.ToString() + " " + y.ToString()));
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
                //clientSocket.Shutdown(SocketShutdown.Both);
                //clientSocket.Close();
                //break;
            }
            return true;
        }

        public void sendMsg(String msg) {
            clientSocket = socket.Accept();
            clientSocket.Send(Encoding.ASCII.GetBytes(msg));
        }

    }

    class NetSender {
        private DrPort dp;
        private PositionServer ps;

        public bool init(DrPort drport) {
            this.dp = drport;
            return true;
        }
        public void startSender() {
            ps = new PositionServer();
            ps.connectClient();
            ps.sendMsg("Start");
            Thread myThread = new Thread(sendMsgAlways);
            myThread.Start();
        }

        private void sendMsgAlways() {
            while (true) {
                bool flag = true;
                while (flag) {
                    Point p = dp.getPoint();
                    flag = ps.sendPosition((int)(p.x * 1000), (int)(p.y * 1000));
                }
                if (!flag) {
                    ps.sendMsg("ReStart");
                }
            }
        }
    }
}
