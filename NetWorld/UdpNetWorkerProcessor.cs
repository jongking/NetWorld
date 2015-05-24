using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace NetWorld
{
    /// <summary>
    /// UdpNetWorker中处理函数的实现
    /// </summary>
    public class UdpNetWorkerProcessor : UdpNetWorker.IUdpNetWorkerProcessor
    {
        public JsonDb Jdb;
        public readonly Dictionary<string, NetClient> Clientsockets = new Dictionary<string, NetClient>();

        public UdpNetWorkerProcessor(JsonDb jdb)
        {
            Jdb = jdb;
        }

        public int ServerProcessor(string method, string param, EndPoint remote, UdpNetWorker.SocketWrap socket, UdpNetWorker udpNetWorker)
        {
            switch (method)
            {
                case Protocol.HeartBeat:
                    OnHeartBeat(remote, socket);
                    break;
            }
            return 0;
        }

        private void OnHeartBeat(EndPoint remote, UdpNetWorker.SocketWrap socket)
        {
            if (Clientsockets.ContainsKey(remote.ToString()))
            {
                Clientsockets[remote.ToString()].UpdataTime();
            }
            else
            {
                var nc = new NetClient(remote.ToString(), socket);
                Clientsockets.Add(remote.ToString(), nc);
            }
            Console.WriteLine(Clientsockets.Count);
        }

        public int ClientProcessor(string method, string param, EndPoint remote, UdpNetWorker.SocketWrap socket, UdpNetWorker udpNetWorker)
        {
            switch (method)
            {
            }
            return 0;
        }

        public int AfterSendProcessor(UdpNetWorker.SendWorkerParams swp, int count, UdpNetWorker udpNetWorker)
        {
            return 0;
        }

        public int BeforeSendProcessor(UdpNetWorker.SendWorkerParams swp, int count, UdpNetWorker udpNetWorker)
        {
            return 0;
        }

        //代表连接进来的客户
        public class NetClient
        {
            public string IpAddress;//用来做key
            public UdpNetWorker.SocketWrap Socket;
            private DateTime _dateTime = DateTime.Now;

            public NetClient(string ipAddress, UdpNetWorker.SocketWrap socket)
            {
                Socket = socket;
                this.IpAddress = ipAddress;
            }

            public void UpdataTime()
            {
                _dateTime = DateTime.Now;
            }

            public bool IsTimeOut()
            {
                return _dateTime.AddSeconds(5) <= DateTime.Now;//超出5秒就是超时了
            }
        }
    }
}
