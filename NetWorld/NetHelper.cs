using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetWorld
{
    public static class NetHelper
    {
        public static IPHostEntry GetLocalIpAdress()
        {
            string strHostName = Dns.GetHostName();  //得到本机的主机名
            return Dns.GetHostByName(strHostName); //取得本机IP
        }

        public static List<string> GetLocalIps()
        {
            var list = GetLocalIpAdress();
            return list.AddressList.Select(ipAddress => ipAddress.ToString()).ToList();
        }
    }
}
