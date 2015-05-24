using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetWorld
{
    public static class Protocol
    {
        public const string HeartBeat = "0";//心跳包
    }
    public class Log
    {
        public static void Write(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    public class Ipv4Adress
    {
        public string Ip;
        public int Port;
    }

    class Program
    {
        public static List<Ipv4Adress> ServerIpList = new List<Ipv4Adress>();

        static void Main(string[] args)
        {
            JsonDb jdb = new JsonDb(@"C:\log");
            if (jdb.Exists("serverIpList"))
            {
                ServerIpList = jdb.Select<List<Ipv4Adress>>("serverIpList");
            }
            else
            {
                //设置默认的服务器列表
                ServerIpList.Add(new Ipv4Adress() { Ip = "115.28.129.244", Port = 10000 });
                jdb.Insert("serverIpList", ServerIpList);
            }

            UdpNetWorker.Debug = true;

            var nw = new UdpNetWorker(new UdpNetWorkerProcessor(jdb));

            nw.CreateReceWorker(10000);

            //每秒发送一个连接信息给'服务器'
            foreach (var ipv4Adress in ServerIpList)
            {
                //不发信息给自己
                var ips = NetHelper.GetLocalIps();
                if (ips.Contains(ipv4Adress.Ip)) continue;

                //尝试连接到'服务器'
                if (nw.TryConnect(9999, ipv4Adress.Ip, ipv4Adress.Port, 1))
                {
                    //连接并且不断发送心跳包到第一个连接上的'服务器'
                    nw.CreateSendWorker(10000, ipv4Adress.Ip, ipv4Adress.Port, Protocol.HeartBeat);
                    break;
                }
            }

            Thread.Sleep(100000000);
        }
    }
}
