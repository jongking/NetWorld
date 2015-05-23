using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetWorld
{
    public abstract class NetWorker
    {
        public abstract int ProcessMessage(string message, EndPoint remote);
        public abstract void SendTo(string message, EndPoint remote);
        public abstract EndPoint ReceiveFrom(ref string message);
        public abstract void Init();

        protected void Main()
        {
            BeforeInit();
            //初始化
            Init();

            AfterInit();

            //接收和处理发送过来的信息
            RecAndProMessage();
        }

        protected virtual void AfterInit()
        {
            
        }

        protected virtual void BeforeInit()
        {
            
        }

        private void RecAndProMessage()
        {
            string message = "";
            while (true)
            {
                //接收信息
                var remote = ReceiveFrom(ref message);

                //处理信息
                if (ProcessMessage(message, remote) == -1) return;
            }
        }
    }

    public class MyUdpNetWorker
    {
        private static readonly Dictionary<int, SocketWrap> Sockets = new Dictionary<int, SocketWrap>();

        //接收消息的处理函数
        private int ServerProcessor(string method, string param, EndPoint remote)
        {
            switch (method)
            {
                case "show":
                    Console.WriteLine(param);
                    break;
            }
            return 0;
        }

        private int ClientProcessor(string method, string param, EndPoint remote)
        {
            switch (method)
            {
                case "quit":
                    return -1;
            }
            return 0;
        }

        public SocketWrap CreateSocketWrap(int port)
        {
            if (Sockets.ContainsKey(port))
            {
                return Sockets[port];
            }
            var socketwrap = new SocketWrap(port, port);
            Sockets.Add(port, socketwrap);
            return socketwrap;
        }

        public void CreateReceWorker(int port)
        {
            //得到本机IP，设置UDP端口号
            var socketwrap = CreateSocketWrap(port);

            CreateReceWorker(socketwrap);
        }

        public void CreateReceWorker(SocketWrap client)
        {
            Thread t = new Thread(ReceMsgWorker);
            t.Start(client);
        }

        private void ReceMsgWorker(object client)
        {
            var socket = (SocketWrap)client;
            try
            {
                string message = "";
                for (; ; )
                {
                    //接收信息
                    var remote = socket.ReceiveFrom(ref message);

                    //处理信息
                    if (ProcessMessage(message, remote) == -1) return;
                    
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            finally
            {
                socket.Socket.Close();
            }
        }

        public int ProcessMessage(string message, EndPoint remote)
        {
            var arr = message.Split(':');
            if (arr.Count() != 2) return 0;

            if (ServerProcessor(arr[0], arr[1], remote) == -1) return -1;
            if (ClientProcessor(arr[0], arr[1], remote) == -1) return -1;
            switch (arr[0])
            {
                case "quit":
                    return -1;
            }
            return 0;
        }

        public void CreateSendWorker(int fromport, string ip, int port, string sendmessage, int timeTicker = 1000)
        {
            var from = CreateSocketWrap(fromport);

            var swp = new SendWorkerParams(from, ip, port, sendmessage, timeTicker);
            Thread t = new Thread(SendWorker);
            t.Priority = ThreadPriority.Lowest;
            t.Start(swp);
        }

        public void CreateSendWorker(SocketWrap from, string ip, int port, string sendmessage, int timeTicker = 1000)
        {
            var swp = new SendWorkerParams(from, ip, port, sendmessage, timeTicker);
            Thread t = new Thread(SendWorker);
            t.Priority = ThreadPriority.Lowest;
            t.Start(swp);
        }

        public class SendWorkerParams
        {
            public SocketWrap From;
            public EndPoint To;
            public int TimeTicker;
            public string Sendmessage;

            public SendWorkerParams(SocketWrap from, string ip, int port, string sendmessage, int timeTicker = 1000)
            {
                From = from;
                To = new IPEndPoint(IPAddress.Parse(ip), port);
                TimeTicker = timeTicker;
                Sendmessage = sendmessage;
            }
        }

        //不停发送一个信息
        public void SendWorker(object swpobj)
        {
            var swp = (SendWorkerParams)swpobj;
            var timeTicker = swp.TimeTicker;
            var endPoint = swp.To;
            var sendmessage = swp.Sendmessage;
            var socketWrap = swp.From;

            try
            {
                for (; ; )
                {
                    socketWrap.SendTo(sendmessage, endPoint);
                    Thread.Sleep(timeTicker);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            finally
            {
                socketWrap.Socket.Close();
            }
        }

        public class SocketWrap
        {
            public int Index;
            public Socket Socket;

            public SocketWrap(int port, int index)
            {
                var ipep = new IPEndPoint(IPAddress.Any, port);
                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sock.Bind(ipep);

                Socket = sock;
                Index = index;
            }

            public SocketWrap(Socket socket, int index)
            {
                Socket = socket;
                this.Index = index;
            }

            public EndPoint ReceiveFrom(ref string message)
            {
                byte[] cacheBuffer = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = sender;
                var recv = Socket.ReceiveFrom(cacheBuffer, ref remote);
                message = DefaultEncoding.GetString(cacheBuffer, 0, recv);
                return remote;
            }

            public void SendTo(string message, EndPoint remote)
            {
                byte[] cacheBuffer = DefaultEncoding.GetBytes(message);
                //发送信息
                Socket.SendTo(cacheBuffer, cacheBuffer.Length, SocketFlags.None, remote);
            }

            private static Encoding DefaultEncoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }

    public class Log
    {
        public static void Write(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var nw = new MyUdpNetWorker();

//            nw.CreateReceWorker(10000);

//            nw.CreateSendWorker(10000, "121.40.208.105", 10000, "show:hello");

            Thread.Sleep(100000000);
        }
    }
}
