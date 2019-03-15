using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Xml;

namespace Adf
{
    /// <summary>
    /// 队列服务客户端池
    /// </summary>
    [Obsolete("class obsolete, please QueueServerBase")]
    public class QueueServerPool
    {
        /// <summary>
        /// 获取池管理对象
        /// </summary>
        public Pool<QueueServerClient> Pool
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取配置节点名, 默认： QueueServer
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取每成员的最大实例数,默认:800,配置节： ConfigName + MemberPoolSize
        /// </summary>
        public virtual int MemberPoolSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取连接队列名
        /// </summary>
        public string Topic
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取连接消息处理超时时间，单位：秒， 默认 30s, 配置节： ConfigName + CommitTimeout
        /// </summary>
        public ushort CommitTimeout
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取请求超时时间，单位：秒， 默认 5s, 配置节： ,配置节： ConfigName + Timeout
        /// </summary>
        public ushort Timeout
        {
            get;
            private set;
        }

        /// <summary>
        /// 指定配置文件以初始化新实例
        /// </summary>
        /// <param name="configName"></param>
        public QueueServerPool(string configName)
        {
            if (string.IsNullOrEmpty(configName) == true)
                throw new ArgumentNullException("configName");
            //
            string topic = "";
            ushort commitTimeout = 30;
            ushort timeout = 5;
            //
            PoolMember[] poolMembers = null;
            int memberPoolSize = 800;
            int hash = 0;
            //
            var configuration = ServerConfig.GetConfiguration(configName);
            if (configuration.FileExist == true)
            {
                topic = configuration.GetAttr("topic");
                commitTimeout = configuration.GetAttrAsUInt16("commitTimeout", commitTimeout);
                timeout = configuration.GetAttrAsUInt16("timeout", timeout);
                memberPoolSize = configuration.GetAttrAsInt32("memberPoolSize", memberPoolSize);
                hash = configuration.GetAttrAsInt32("hash", 0);
                //
                if (string.IsNullOrEmpty(topic) == true)
                    throw new ConfigException("no config topic from "+ configName +".config");
                //
                var members = configuration.GetItems();
                var ipcount = members.Length;
                poolMembers = new PoolMember[ipcount];
                for (int i = 0; i < ipcount; i++)
                    poolMembers[i] = new PoolMember(members[i].Ip, members[i].Port, topic, timeout, commitTimeout);
            }
            else
            {
                topic = Adf.ConfigHelper.GetSetting(configName + "Topic");
                commitTimeout = Adf.ConfigHelper.GetSettingAsUInt16(configName + "CommitTimeout", commitTimeout);
                timeout = Adf.ConfigHelper.GetSettingAsUInt16(configName + "Timeout", timeout);
                memberPoolSize = Adf.ConfigHelper.GetSettingAsInt(configName + "MemberPoolSize", memberPoolSize);
                //
                if (string.IsNullOrEmpty(topic) == true)
                    throw new ConfigException("no config topic from application config");
                //
                var config = (IpGroupSection)System.Configuration.ConfigurationManager.GetSection(configName);
                if (config == null)
                    throw new Adf.ConfigException("No find configSection" + configName + " or " + configName + ".config");

                var ipcount = config.IpList.Count;
                poolMembers = new PoolMember[ipcount];
                for (int i = 0; i < ipcount; i++)
                    poolMembers[i] = new PoolMember(config.IpList[i].Ip, config.IpList[i].Port, topic, timeout, commitTimeout);

                hash = config.Hash;
            }
            //
            this.Name = configName;
            this.Topic = topic;
            this.Timeout = timeout;
            this.CommitTimeout = commitTimeout;
            this.MemberPoolSize = memberPoolSize;
            //
            this.Pool = new Pool<QueueServerClient>(memberPoolSize, poolMembers, hash);
        }

        class PoolMember : Adf.IPoolMember
        {
            public bool PoolActive
            {
                get;
                set;
            }

            public string PoolMemberId
            {
                get;
                private set;
            }

            private string host;
            private int port;
            private string topic;
            private ushort timeout;
            private ushort commitTimeout;

            public PoolMember(string host, int port, string topic, ushort timeout, ushort commitTimeout)
            {
                this.host = host;
                this.port = port;
                this.topic = topic;
                this.timeout = timeout;
                this.commitTimeout = commitTimeout;

                this.PoolMemberId = host + ":" + port;
            }

            public Adf.IPoolInstance CreatePoolInstance()
            {
                var instance = new QueueServerClient(this.host, this.port, this.topic, this.commitTimeout);
                instance.Timeout = this.timeout;
                return instance;
            }
        }
    }

    /// <summary>
    /// 以 QueueServer 为配置的默认单例实例
    /// </summary>
    [Obsolete("class obsolete, please WebSocketClient")]
    public sealed class QueueServerDefault
    {
        /// <summary>
        /// 一个 QueueServer 实例
        /// </summary>
        public static readonly QueueServerPool Instance = new QueueServerPool("QueueServer");
    }
}