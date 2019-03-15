using System;

namespace Adf
{
    /// <summary>
    /// 主机助手
    /// </summary>
    public static class HostHelper
    {
        /// <summary>
        /// 将主机与端口分开解析
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <returns></returns>
        public static string[] Parse(string hostAndPort)
        {
            if (hostAndPort == null)
                throw new ArgumentNullException("hostAndPort");

            return hostAndPort.Split(':');
        }

        /// <summary>
        /// 将主机与端口分开解析
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <param name="host"></param>
        /// <param name="port">0-65535</param>
        /// <returns></returns>
        public static void Parse(string hostAndPort, ref string host, ref int port)
        {
            if (hostAndPort == null)
                throw new ArgumentNullException("hostAndPort");

            var pairs = hostAndPort.Split(':');

            if (pairs.Length == 2)
            {
                host = pairs[0].Trim();
                if (int.TryParse(pairs[1], out port) == false)
                {
                    throw new ArgumentException("hostAndPort", "port invalid");
                }

                if (port == 0)
                {
                    throw new ArgumentException("hostAndPort", "port invalid");
                }

                if (port < 0 || port > 65535)
                {
                    throw new ArgumentException("hostAndPort", "port invalid");
                }

                if (host == "")
                {
                    throw new ArgumentException("hostAndPort", "host invalid");
                }

            }
            else
            {
                throw new ArgumentException("hostAndPort");
            }
        }

        /// <summary>
        /// 合并主机端口为单一字符串
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns>host:port</returns>
        public static string Combine(string host, int port)
        {
            return string.Concat(host, ":", port);
        }
    }
}