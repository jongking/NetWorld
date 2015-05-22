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

    public class MyNetWorker : NetWorker
    {
        private int _port;
        private byte[] _cacheBuffer;
        private Socket _sock;
        private readonly Thread _thread;

        private int ServerProcessor(string message, EndPoint remote)
        {
            switch (message)
            {
                case "quit":
                    return -1;
            }
            return 0;
        }

        private int ClientProcessor(string message, EndPoint remote)
        {
            switch (message)
            {
                case "quit":
                    return -1;
            }
            return 0;
        }

        public override int ProcessMessage(string message, EndPoint remote)
        {
            if (ServerProcessor(message, remote) == -1) return -1;
            if (ClientProcessor(message, remote) == -1) return -1;
            switch (message)
            {
                case "quit":
                    return -1;
            }
            return 0;
        }

        public MyNetWorker(int port = 10000)
        {
            _port = port;
            _thread = new Thread(Main) { IsBackground = true };
        }

        public void Run()
        {
            if (IsRuning()) return;
            _thread.Start();
        }

        public bool IsRuning()
        {
            return _thread.IsAlive;
        }

        public override EndPoint ReceiveFrom(ref string message)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = sender;
            var recv = _sock.ReceiveFrom(_cacheBuffer, ref remote);
            message = DefaultEncoding.GetString(_cacheBuffer, 0, recv);
            return remote;
        }

        public override void Init()
        {
            InitSocket();
        }

        private void InitSocket()
        {
            //得到本机IP，设置TCP端口号         
            var ipep = new IPEndPoint(IPAddress.Any, _port);
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //绑定网络地址
            _sock.Bind(ipep);
        }

        public override void SendTo(string message, EndPoint remote)
        {
            _cacheBuffer = DefaultEncoding.GetBytes(message);
            //发送信息
            _sock.SendTo(_cacheBuffer, _cacheBuffer.Length, SocketFlags.None, remote);
        }

        private static Encoding DefaultEncoding
        {
            get { return Encoding.UTF8; }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
