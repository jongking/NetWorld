﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetWorld
{
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

        public override string ToString()
        {
            return Ip + ":" + Port;
        }
    }

    class Program
    {
        public static List<Ipv4Adress> ServerIpList = new List<Ipv4Adress>();

        static void Main(string[] args)
        {
            //端口配置
            const int mainRecPort = 10000;
            const int tryConnectPort = 9998;
            //数据库配置
            var jdb = new JsonDb(@"C:\log");

            if (jdb.Exists("serverIpList"))
            {
                ServerIpList = jdb.Select<List<Ipv4Adress>>("serverIpList");
            }
            else
            {
                //设置默认的服务器列表
                ServerIpList.Add(new Ipv4Adress() { Ip = "121.40.208.105", Port = mainRecPort });
                jdb.Insert("serverIpList", ServerIpList);
            }

            //设置guid
            var guid = "";
            if (jdb.Exists("clientGuid"))
            {
                guid = jdb.Select<string>("clientGuid");
            }
            else
            {
                guid = Guid.NewGuid().ToString();
                jdb.Insert("clientGuid", guid);
            }

            UdpNetWorker.Debug = false;

            var nw = new UdpNetWorker(new UdpNetWorkerProcessor(jdb));

            nw.CreateReceWorker(mainRecPort);

            //每秒发送一个连接信息给'服务器'
            foreach (var ipv4Adress in ServerIpList)
            {
                //不发信息给自己
                var ips = NetHelper.GetLocalIps();
                if (ips.Contains(ipv4Adress.Ip)) continue;

                //尝试连接到'服务器'
                if (nw.TryConnect(tryConnectPort, ipv4Adress.Ip, ipv4Adress.Port, 1))
                {
                    //连接并且不断发送心跳包到第一个连接上的'服务器'
                    nw.CreateSendWorker(mainRecPort, ipv4Adress.Ip, ipv4Adress.Port, UdpNetWorker.BaseProtocol.HeartBeat + ":" + guid);

                    while (true)
                    {
                        var cmd = Console.ReadLine();
                        var swp = nw.CreateSocketWrap(mainRecPort);
                        swp.SendTo(cmd, ipv4Adress.Ip, ipv4Adress.Port);
                    }
                    break;
                }
            }

            Thread.Sleep(100000000);
        }
    }
}
