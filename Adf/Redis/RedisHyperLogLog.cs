using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;


namespace Adf
{
    /// <summary>
    /// HyperLogLog
    /// Redis 在 2.8.9 版本添加了该结构。
    /// </summary>
    public class RedisHyperLogLog
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisHyperLogLog(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 将所有元素参数添加到 HyperLogLog 数据结构中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>"OK" 添加成功</returns>
        public int PFADD(string key, params string[] value)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (value == null || value.Length == 0)
                throw new ArgumentNullException("value");
            using (var w = new RedisWriter(this.client, value.Length + 2, "PFADD"))
            {
                w.WriteArgument(key);
                foreach (var member in value)
                    w.WriteArgument(member);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 返回给定 HyperLogLog 的基数估算值。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int PFCOUNT(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            using (var w = new RedisWriter(this.client, 2, "PFCOUNT"))
            {
                w.WriteArgument(key);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 将多个 HyperLogLog 合并为一个 HyperLogLog ，合并后的 HyperLogLog 的基数估算值.
        /// </summary>
        /// <param name="destkey"></param>
        /// <param name="sourcekeys"></param>
        /// <returns></returns>
        public bool PFMERGE(string destkey, params string[] sourcekeys)
        {
            if (string.IsNullOrEmpty(destkey))
                throw new ArgumentNullException("destkey");
            if (sourcekeys.Length == 0)
                throw new ArgumentNullException("sourcekeys");
            using (var w = new RedisWriter(this.client, sourcekeys.Length + 2, "PFMERGE"))
            {
                w.WriteArgument(destkey);
                foreach (var member in sourcekeys)
                    w.WriteArgument(member);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectSuccess();
        }

        /// <summary>
        /// 将俩个 HyperLogLog 合并为一个 HyperLogLog ，合并后的 HyperLogLog 的基数估算值.
        /// </summary>
        /// <param name="destkey"></param>
        /// <param name="sourcekey1"></param>
        /// <param name="sourcekey2"></param>
        /// <returns></returns>
        public bool PFMERGE(string destkey, string sourcekey1, string sourcekey2)
        {
            //if(string.IsNullOrEmpty(destkey))
            //    throw new ArgumentNullException("destkey");
            if (string.IsNullOrEmpty(sourcekey1))
                throw new ArgumentNullException("sourcekey1");
            if (string.IsNullOrEmpty(sourcekey2))
                throw new ArgumentNullException("sourcekey2");
            var param = new string[] { sourcekey1, sourcekey2 };
            return PFMERGE(destkey, param);
        }
    }
}
