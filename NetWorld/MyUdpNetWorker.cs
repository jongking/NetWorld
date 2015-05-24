using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetWorld
{
    public class UdpNetWorker
    {
        private readonly IUdpNetWorkerProcessor _iWorkerProcessor;
        private readonly Dictionary<int, SocketWrap> _sockets = new Dictionary<int, SocketWrap>();

        public static bool Debug = false;

        public UdpNetWorker(IUdpNetWorkerProcessor iWorkerProcessor)
        {
            _iWorkerProcessor = iWorkerProcessor;
        }

        public SocketWrap CreateSocketWrap(int port)
        {
            if (_sockets.ContainsKey(port))
            {
                return _sockets[port];
            }
            var socketwrap = new SocketWrap(port, port);
            _sockets.Add(port, socketwrap);
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
            var t = new Thread(ReceMsgWorker);
            t.Start(client);
        }

        private void ReceMsgWorker(object client)
        {
            var socket = (SocketWrap) client;
            try
            {
                var message = "";
                for (;;)
                {
                    //接收信息
                    var remote = socket.ReceiveFrom(ref message);

                    //处理信息
                    if (ProcessMessage(message, remote, socket) == -1) return;
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

        public int ProcessMessage(string message, EndPoint remote, SocketWrap socket)
        {
            var arr = message.Split(':');
            var method = arr[0];
            var param = "";
            if (arr.Count() > 1)
            {
                param = arr[1];
            }

            if (_iWorkerProcessor.ServerProcessor(method, param, remote, socket, this) == -1) return -1;
            if (_iWorkerProcessor.ClientProcessor(method, param, remote, socket, this) == -1) return -1;
            switch (method)
            {
                case "TryConnect":
                    Thread.Sleep(100);
                    socket.SendTo("TryConnectOK", remote);
                    break;
                case "quit":
                    return -1;
            }
            return 0;
        }

        public void CreateSendWorker(int fromport, string ip, int port, string sendmessage, int timeTicker = 1000)
        {
            var from = CreateSocketWrap(fromport);

            var swp = new SendWorkerParams(from, ip, port, sendmessage, timeTicker);
            var t = new Thread(SendWorker);
            t.Priority = ThreadPriority.Lowest;
            t.Start(swp);
        }

        public void CreateSendWorker(SocketWrap from, string ip, int port, string sendmessage, int timeTicker = 1000)
        {
            var swp = new SendWorkerParams(from, ip, port, sendmessage, timeTicker);
            var t = new Thread(SendWorker);
            t.Priority = ThreadPriority.Lowest;
            t.Start(swp);
        }

        //不停发送一个信息
        public void SendWorker(object swpobj)
        {
            var swp = (SendWorkerParams) swpobj;
            var timeTicker = swp.TimeTicker;
            var endPoint = swp.To;
            var sendmessage = swp.Sendmessage;
            var socketWrap = swp.From;
            var runtime = swp.RunTime;

            var count = 0;
            try
            {
                while (true)
                {
                    if (_iWorkerProcessor.BeforeSendProcessor(swp, count, this) == -1) break;
                    socketWrap.SendTo(sendmessage, endPoint);
                    Thread.Sleep(timeTicker);
                    if (_iWorkerProcessor.AfterSendProcessor(swp, count, this) == -1) break;
                    if (runtime > 0)
                    {
                        count++;
                        if (count >= runtime)
                        {
                            break;
                        }
                    }
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

        public bool TryConnect(int fromport, string ip, int port, int timeout = 3000)
        {
            string sendmessage = string.Format("TryConnect:{0}", fromport.ToString());

            var from = CreateSocketWrap(fromport);

            var message = "";

            var swp = new SendWorkerParams(from, ip, port, sendmessage, 0);

            from.SendTo(sendmessage, swp.To);

            from.Socket.ReceiveTimeout = timeout;
            try
            {
                IPEndPoint recEndpoint = (IPEndPoint) @from.ReceiveFrom(ref message);
                if (recEndpoint.Address.ToString() == ip && message == "TryConnectOK")
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private static void DebugW(string msg)
        {
            if (Debug) Console.WriteLine(msg);
        }
        public class SendWorkerParams
        {
            public SocketWrap From;
            public int RunTime;
            public string Sendmessage;
            public int TimeTicker;
            public EndPoint To;

            public SendWorkerParams(SocketWrap from, string ip, int port, string sendmessage, int runtime = 0,
                int timeTicker = 1000)
            {
                From = from;
                To = new IPEndPoint(IPAddress.Parse(ip), port);
                TimeTicker = timeTicker;
                Sendmessage = sendmessage;
                RunTime = runtime;
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
                Index = index;
            }

            private static Encoding DefaultEncoding
            {
                get { return Encoding.UTF8; }
            }

            public EndPoint ReceiveFrom(ref string message)
            {
                var cacheBuffer = new byte[1024];
                var sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = sender;
                var recv = Socket.ReceiveFrom(cacheBuffer, ref remote);
                message = DefaultEncoding.GetString(cacheBuffer, 0, recv);

                DebugW(string.Format("{2}  ->  ReceiveFrom({0},{1})", message, remote, this.Socket.LocalEndPoint));
                return remote;
            }

            public void SendTo(string message, EndPoint remote)
            {
                DebugW(string.Format("{2}  ->  SendTo({0},{1})", message, remote, this.Socket.LocalEndPoint));

                var cacheBuffer = DefaultEncoding.GetBytes(message);
                //发送信息
                Socket.SendTo(cacheBuffer, cacheBuffer.Length, SocketFlags.None, remote);
            }
        }

        public interface IUdpNetWorkerProcessor
        {
            int ServerProcessor(string method, string param, EndPoint remote, SocketWrap socket, UdpNetWorker udpNetWorker);
            int ClientProcessor(string method, string param, EndPoint remote, SocketWrap socket, UdpNetWorker udpNetWorker);
            int AfterSendProcessor(SendWorkerParams swp, int count, UdpNetWorker udpNetWorker);
            int BeforeSendProcessor(SendWorkerParams swp, int count, UdpNetWorker udpNetWorker);
        }

    }
}