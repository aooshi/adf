using System;
using System.Configuration;

namespace Adf.Config
{
    /// <summary>
    /// IP组客户端配置元素
    /// </summary>
    public class IpGroupElement : ConfigurationElement
    {
        ///// <summary>
        ///// name
        ///// </summary>
        //[ConfigurationProperty("name", IsRequired = true)]
        //public string Name
        //{
        //    get { return (string)base["name"]; }
        //    set { base["name"] = value; }
        //}
        /// <summary>
        /// Ip Address
        /// </summary>
        [ConfigurationProperty("ip", IsRequired = true)]
        public string Ip
        {
            get { return (string)base["ip"]; }
            set { base["ip"] = value; }
        }

        /// <summary>
        /// 端口
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value; }
        }

        /// <summary>
        /// 级别
        /// </summary>
        [ConfigurationProperty("level", DefaultValue = 0 )]
        public int Level
        {
            get { return (int)base["level"]; }
            set { base["level"] = value; }
        }

        /// <summary>
        /// 描述
        /// </summary>
        [ConfigurationProperty("description", DefaultValue = "")]
        public string Description
        {
            get { return (string)base["level"]; }
            set { base["level"] = value; }
        }
    }
}
