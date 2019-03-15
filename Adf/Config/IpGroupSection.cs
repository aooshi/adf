using System;
using System.Configuration;

namespace Adf.Config
{
    /// <summary>
    /// IP组配置节解释器
    /// </summary>
    public class IpGroupSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _iplist = new ConfigurationProperty(null, typeof(IpGroupCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        ///// <summary>
        ///// 本地Ip
        ///// </summary>
        //[ConfigurationProperty("ip", DefaultValue = "")]
        //public string Ip
        //{
        //    get { return (string)base["ip"]; }
        //    set { base["ip"] = value; }
        //}

        ///// <summary>
        ///// 本地端口
        ///// </summary>
        //[ConfigurationProperty("port", DefaultValue = 0)]
        //public int Port
        //{
        //    get { return (int)base["port"]; }
        //    set { base["port"] = value; }
        //}

        /// <summary>
        /// 本地级别
        /// </summary>
        [ConfigurationProperty("level", DefaultValue = 0)]
        public int Level
        {
            get { return (int)base["level"]; }
            set { base["level"] = value; }
        }

        /// <summary>
        /// 哈希获取时的虚拟节点数
        /// </summary>
        [ConfigurationProperty("hash", DefaultValue = 0)]
        public int Hash
        {
            get { return (int)base["hash"]; }
            set { base["hash"] = value; }
        }

        /// <summary>
        /// 本地描述
        /// </summary>
        [ConfigurationProperty("description", DefaultValue = "")]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }

        /// <summary>
        /// 检测间隔(秒)
        /// </summary>
        [ConfigurationProperty("check", DefaultValue = 0)]
        public int Check
        {
            get { return (int)base["check"]; }
            set { base["check"] = value; }
        }

        /// <summary>
        /// Ip列表
        /// </summary>
        [ConfigurationProperty(null, IsDefaultCollection = true)]
        public IpGroupCollection IpList
        {
            get 
            {
                return (IpGroupCollection)base[_iplist]; 
            }
        }
    }
}
