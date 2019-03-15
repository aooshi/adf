using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Memcache池成员
    /// </summary>
    public class MemcachePoolMember : IPoolMember
    {
        /// <summary>
        /// 获取主机名
        /// </summary>
        public string Host
        { get; private set; }

        /// <summary>
        /// 获取主机端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 是否活动可用
        /// </summary>
        public bool PoolActive
        {
            get;
            set;
        } 
                
        /// <summary>
        /// 初始一个新实例
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public MemcachePoolMember(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.PoolActive = true;
            this.PoolMemberId = string.Concat(host, ":", port);
        }

        /// <summary>
        /// 创建新实例
        /// </summary>
        /// <returns></returns>
        public IPoolInstance CreatePoolInstance()
        {
            return new Memcache(this.Host, this.Port);
        }

        /// <summary>
        /// 池成员标识
        /// </summary>
        public string PoolMemberId
        {
            get;
            private set;

        }
    }
}
