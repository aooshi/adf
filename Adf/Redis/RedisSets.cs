using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Redis 集合
    /// </summary>
    public class RedisSets
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisSets(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool SAdd(string key, string member)
        {
            if (member == null)
                throw new ArgumentNullException("member");

            using (var w = new RedisWriter(this.client, 3, "SADD"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            //Integer reply: the number of elements that were added to the set, not including all the elements already present into the set.
            return this.connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 返回集合数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int SCARD(string key)
        {
            //Integer reply: the cardinality (number of elements) of the set, or 0 if key does not exist
            using (var w = new RedisWriter(this.client, 2, "SCARD"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 返回2个集合差集
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public string[] SDIFF(string key1, string key2)
        {
            var param = new[] { key1, key2 };
            return SDIFF(param);
        }

        /// <summary>
        /// 返回多个集合差集
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string[] SDIFF(params string[] keys)
        {
            if (keys.Length == 0)
                throw new ArgumentNullException("keys");

            using (var w = new RedisWriter(this.client, keys.Length + 1, "SDIFF"))
            {
                foreach (var key in keys)
                {
                    w.WriteArgument(key);
                }

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }

        /// <summary>
        /// 该命令类似于 SDIFF, 不同之处在于该命令不返回结果集，而是将结果存放在destination集合中.
        /// 如果destination 已经存在, 则将其覆盖重写.
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="keys"></param>
        /// <returns>结果集元素的个数</returns>
        public int SDIFFSTORE(string destKey, params string[] keys)
        {
            return this.StoreSetCommands("SDIFFSTORE", destKey, keys);
        }

        /// <summary>
        /// 返回多个集合交集
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string[] SINTER(params string[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            using (var w = new RedisWriter(this.client, keys.Length + 1, "SINTER"))
            {
                foreach (var key in keys)
                    w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }

        /// <summary>
        /// 这个命令与SINTER命令类似, 但是它并不是直接返回结果集,而是将结果保存在 destination集合中.
        ///如果destination 集合存在, 则会被重写.
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="keys"></param>
        /// <returns>结果集中成员的个数.</returns>
        public int SINTERSTORE(string destKey, params string[] keys)
        {
            return StoreSetCommands("SINTERSTORE", destKey, keys);
        }

        /// <summary>
        /// 判断是否存在元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool SISMEMBER(string key, string member)
        {
            //            Integer reply, specifically:
            //1 if the element is a member of the set.
            //0 if the element is not a member of the set, or if key does not exist.
            using (var w = new RedisWriter(this.client, 3, "SISMEMBER"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 获取集合内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] SMEMBERS(string key)
        {
            using (var w = new RedisWriter(this.client, 2, "SMEMBERS"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }

        /// <summary>
        /// 移动元素至指定集合
        /// </summary>
        /// <param name="srcKey"></param>
        /// <param name="destKey"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool SMOVE(string srcKey, string destKey, string member)
        {
            //            Integer reply, specifically:
            //1 if the element is moved.
            //0 if the element is not a member of source and no operation was performed.

            using (var w = new RedisWriter(this.client, 4, "SMOVE"))
            {
                w.WriteArgument(srcKey);
                w.WriteArgument(destKey);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt() == 1;
        }


        /// <summary>
        /// 随机返回指定数量元素
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string SRANDMEMBER(string key)
        {
            using (var w = new RedisWriter(this.client, 2, "SRANDMEMBER"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToString();
        }



        /// <summary>
        /// 随机返回指定数量元素,Redis 2.6
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string[] SRANDMEMBER(string key, int count)
        {
            using (var w = new RedisWriter(this.client, 3, "SRANDMEMBER"))
            {
                w.WriteArgument(key);
                w.WriteArgument(count.ToString());

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray();
        }


        /// <summary>
        /// 移除并返回一个集合中的随机元素
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string SPOP(string key)
        {
            using (var w = new RedisWriter(this.client, 2, "SPOP"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToString();
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool REM(string key, string member)
        {
            //Integer reply: the number of members that were removed from the set, not including non existing members.

            using (var w = new RedisWriter(this.client, 3, "SREM"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 合并返回指定的集合
        /// 返回给定的多个集合的并集中的所有成员.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public string[] SUNION(string key1,string key2)
        {
            var param = new[] { key1, key2 };
            return SUNION(param);
        }

        /// <summary>
        /// 合并返回指定的集合
        /// 返回给定的多个集合的并集中的所有成员.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string[] SUNION(params string[] keys)
        {
            using (var w = new RedisWriter(this.client, keys.Length + 1, "SUNION"))
            {
                foreach (var key in keys)
                    w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray();
        }


        /// <summary>
        /// 合并元素至目标键
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="keys"></param>
        public int SUNIONSTORE(string destKey, params string[] keys)
        {
            return StoreSetCommands("SUNIONSTORE", destKey, keys);
        }

        int StoreSetCommands(string cmd, string destKey, params string[] keys)
        {
            if (String.IsNullOrEmpty(cmd))
                throw new ArgumentNullException("cmd");

            if (String.IsNullOrEmpty(destKey))
                throw new ArgumentNullException("destKey");

            if (keys == null)
                throw new ArgumentNullException("keys");

            using (var w = new RedisWriter(this.client, keys.Length + 2, cmd))
            {
                w.WriteArgument(destKey);
                foreach (var key in keys)
                    w.WriteArgument(key);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }
    }

}