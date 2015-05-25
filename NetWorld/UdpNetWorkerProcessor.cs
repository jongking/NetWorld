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

        public UdpNetWorkerProcessor(JsonDb jdb)
        {
            Jdb = jdb;
        }

        public int ServerProcessor(string method, string param, EndPoint remote, UdpNetWorker.SocketWrap socket, UdpNetWorker udpNetWorker)
        {
            switch (method)
            {

            }
            return 0;
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

        public class MyProtocol : UdpNetWorker.BaseProtocol
        {
             
        }
    }
}
