using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;

namespace Adf
{
    /// <summary>
    /// IP 助手
    /// </summary>
    public static class IpHelper
    {
        /// <summary>
        /// 将IP地址转换为数值
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static long IPv4ToNumber(string ipAddress)
        {
            //return 4278190079;
            if (ipAddress.Length > 0)
            {
                string[] ips = ipAddress.Split('.');
                if (ips.Length == 4)
                {
                    return (Convert.ToInt64(ips[0]) << 24) + (Convert.ToInt64(ips[1]) << 16) + (Convert.ToInt64(ips[2]) << 8) + Convert.ToInt64(ips[3]);
                }
            }
            return 0;
        }

        /// <summary>
        /// 将数值IP转换为IP地址
        /// </summary>
        /// <param name="ipCode"></param>
        /// <returns></returns>
        public static string NumberToIPv4(long ipCode)
        {
            var ip = new StringBuilder();
            int subIP = 0;

            long one = ipCode >> 24;
            subIP = Convert.ToInt32(one.ToString("f0")) % 256;
            ip.Append(subIP);
            ip.Append(".");
            long two = ipCode >> 16;
            subIP = Convert.ToInt32(two.ToString("f0")) % 256;
            ip.Append(subIP);
            ip.Append(".");
            long three = ipCode >> 8;
            subIP = Convert.ToInt32(three.ToString("f0")) % 256;
            ip.Append(subIP);
            ip.Append(".");
            long four = ipCode % 256;
            subIP = Convert.ToInt32(four.ToString("f0"));
            ip.Append(subIP);
            ip.Append(".");
            return ip.ToString();

        }

        /// <summary>
        /// 将指定的IPv4，后两位替换为 *
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static string MaskIPv4(string ip)
        {
            return MaskIPv4(ip, 2);
        }

        /// <summary>
        /// 将指定的IP，除指定长度外，按*显示
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="length">显示长度</param>
        public static string MaskIPv4(string ip, int length)
        {
            string[] s = ip.Split('.');

            if (s.Length != 4)
                return "***.***.***.***";

            return string.Format("{0}.{1}.***.***", s[0], s[1]);
        }

        /// <summary>
        /// string end point convert to ep object
        /// </summary>
        /// <param name="ep"></param>
        /// <returns>cast failure return false</returns>
        public static IPEndPoint ParseEndPoint(string ep)
        {
            if (ep == null)
            {
                ep = "";
            }

            IPEndPoint iep = null;

            var items = ep.Split(':');
            if (items.Length == 2)
            {
                IPAddress address = null;
                ushort port = 0;

                if (IPAddress.TryParse(items[0], out address) == false)
                {
                }
                else if (ushort.TryParse(items[1], out port) == false)
                {
                }
                else
                {
                    iep = new IPEndPoint(address, port);
                }
            }

            return iep;
        }

        /// <summary>
        /// parse host:port
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool ParseEndPoint(string ep, ref string host, ref int port)
        {
            if (ep == null)
            {
                return false;
            }

            var items = ep.Split(':');
            if (items.Length != 2)
            {
                return false;
            }

            if (items[0] == "")
            {
                return false;
            }

            if (int.TryParse(items[1], out port) == false || port < 1 || port > 65535)
            {
                return false;
            }

            host = items[0];

            return true;
        }

        /// <summary>
        /// parse port
        /// </summary>
        /// <param name="portString"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool ParseEndPoint(string portString, ref int port)
        {
            if (portString == null)
            {
                return false;
            }

            if (int.TryParse(portString, out port) == false || port < 1 || port > 65535)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// check effective port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckPort(int port)
        {
            return port > 0 && port < 65536;
        }
    }
}