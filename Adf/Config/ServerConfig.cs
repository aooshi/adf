using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// Server Config Item
    /// </summary>
    public class ServerConfig : ConfigBase<ServerConfigItem>
    {
        /// <summary>
        /// 获取配置实例
        /// </summary>
        public static readonly ServerConfig Instance = new ServerConfig();


        static readonly Dictionary<string, ServerConfig> dictionary = new Dictionary<string, ServerConfig>();
        static readonly Object lockObject = new object();

        /// <summary>
        /// 通过单例模式获取一个配置实例
        /// </summary>
        /// <param name="fileName">配置文件名称,区分大小写</param>
        /// <returns></returns>
        public static ServerConfig GetConfiguration(string fileName)
        {
            //若等于默认实例
            if ("Server.config".Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                return ServerConfig.Instance;
            }

            ServerConfig sc = null;
            //
            lock (lockObject)
            {
                if (dictionary.TryGetValue(fileName, out sc) == false)
                {
                    sc = new ServerConfig(fileName);
                    dictionary.Add(fileName, sc);
                }
            }
            //
            return sc;
        }


        /// <summary>
        /// initialize new server.config instance
        /// </summary>
        private ServerConfig()
            : base("Server.config")
        {
            base.AddWatcher();
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="fileName"></param>
        public ServerConfig(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// new config
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override ServerConfigItem NewItem(System.Xml.XmlNode node)
        {
            return new ServerConfigItem()
            {
                Description = XmlHelper.GetAttribute(node, "description", string.Empty),
                Ip = XmlHelper.GetAttribute(node, "ip", string.Empty),
                Level = ConvertHelper.ToInt32(XmlHelper.GetAttribute(node, "level", string.Empty), 0),
                Port = ConvertHelper.ToInt32(XmlHelper.GetAttribute(node, "port", string.Empty), 0)
            };
        }

        /// <summary>
        /// get hosts
        /// </summary>
        /// <returns>[ "host1:port1", "host2,port2", ... ]</returns>
        public string[] GetHosts()
        {
            var items = this.GetItems();
            var hosts = new string[items.Length];
            for (int i = 0, l = items.Length; i < l; i++)
            {
                hosts[i] = items[i].Ip + ":" + items[i].Port;
            }
            return hosts;
        }
    }
}