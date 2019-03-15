//
// redis-sharp.cs: ECMA CLI Binding to the Redis key-value storage system
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010 Novell, Inc.
//
// Licensed under the same terms of reddis: new BSD license.
//
//#define DEBUG

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Redis 客户端
    /// </summary>
    public class RedisClient : IDisposable, ICache, IPoolInstance
    {
        /// <summary>
        /// 连接
        /// </summary>
        protected RedisConnection Connection
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置二进制序列化器,默认 JsonBinarySerializable, 可通过在appsetting中配置 RedisClientBinarySerializable 来指定其它实例
        /// </summary>
        public IBinarySerializable BinarySerializable { get; set; }

        /// <summary>
        /// 是否从池中废弃此实例
        /// </summary>
        public bool PoolAbandon
        {
            get;
            set;
        }

        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding Encoding
        {
            get;
            private set;
        }

        RedisSets sets;
        /// <summary>
        /// 获取集合
        /// </summary>
        public RedisSets Sets
        {
            get
            {
                return this.sets ?? (this.sets = new RedisSets(this, this.Connection));
            }
        }

        RedisSortedSets sortedSets;
        /// <summary>
        /// 获取有序集合
        /// </summary>
        public RedisSortedSets SortedSets
        {
            get
            {
                return this.sortedSets ?? (this.sortedSets = new RedisSortedSets(this, this.Connection));
            }
        }
        
        RedisLists lists;
        /// <summary>
        /// 获取列表
        /// </summary>
        public RedisLists Lists
        {
            get
            {
                return this.lists ?? (this.lists = new RedisLists(this, this.Connection));
            }
        }

        RedisHashes hashs;
        /// <summary>
        /// 获取哈希表
        /// </summary>
        public RedisHashes Hashs
        {
            get
            {
                return this.hashs ?? (this.hashs = new RedisHashes(this, this.Connection));
            }
        }

        RedisHyperLogLog hyperLogLog;
        ///<summary>
        ///HyperLogLog
        ///</summary>
        public RedisHyperLogLog HyperLogLog
        {
            get {
                return this.hyperLogLog ?? (this.hyperLogLog = new RedisHyperLogLog(this, this.Connection));
            }
        }

        RedisPubSub pubSub;
        ///<summary>
        ///Pub/Sub 发布与订阅
        ///</summary>
        public RedisPubSub PubSub
        {
            get
            {
                return this.pubSub ?? (this.pubSub = new RedisPubSub(this, this.Connection));
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisClient(string host, int port)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            Host = host;
            Port = port;
            SendTimeout = -1;

            this.Encoding = Encoding.UTF8;
            //
            var redisCacheBinarySerializable = Adf.ConfigHelper.GetSetting("RedisClientBinarySerializable");
            if (string.IsNullOrEmpty(redisCacheBinarySerializable))
                this.BinarySerializable = new JsonBinarySerializable(Encoding.UTF8);
            else
                this.BinarySerializable = (IBinarySerializable)Activator.CreateInstance(System.Type.GetType(redisCacheBinarySerializable));
            //
            this.Connection = new RedisConnection(this);
        }

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="host"></param>
        public RedisClient(string host)
            : this(host, 6379)
        {
        }

        /// <summary>
        /// 初始化默认实例
        /// </summary>
        public RedisClient()
            : this("localhost", 6379)
        {
        }

        public string Host { get; private set; }
        public int Port { get; private set; }
        //public int RetryTimeout { get; set; }
        //public int RetryCount { get; set; }
        public int SendTimeout { get; set; }
        public string Password { get; set; }

        int db;
        /// <summary>
        /// 获取当前DB 
        /// </summary>
        public int DB
        {
            get
            {
                return db;
            }
        }

        /// <summary>
        /// 设置当前要操作的DB
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        public bool Select(int dbIndex)
        {
            //SET key value
            using (var redisWriter = new RedisWriter(this, 2, "SELECT"))
            {
                redisWriter.WriteArgument(dbIndex.ToString());
                this.Connection.SendCommand(redisWriter);
            }

            var result = this.Connection.ExpectSuccess();
            if (result)
                this.db = dbIndex;
            return result;
        }
        
        /// <summary>
        /// 设置一个字符串值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");
            
            //SET key value
            using (var redisWriter = new RedisWriter(this, 3, "SET"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }
        /// <summary>
        /// 设置一个二进制值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set(string key, byte[] value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            using (var redisWriter = new RedisWriter(this, 3, "SET"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }


        /// <summary>
        /// 设置一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            using (var redisWriter = new RedisWriter(this, 3, "SET"))
            {
                redisWriter.WriteArgument(key);

                if (value is string)
                    redisWriter.WriteArgument((string)value);
                else if (value is byte[])
                    redisWriter.WriteArgument((byte[])value);
                else
                    redisWriter.WriteArgument(this.BinarySerializable.Serialize(value));

                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }


        /// <summary>
        /// 设置一个二进制值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>返回文本新长度</returns>
        public int Append(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            using (var redisWriter = new RedisWriter(this, 3, "APPEND"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 设置一个值，当键已存在时则忽略
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetNX(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");
            
            using (var redisWriter = new RedisWriter(this, 3, "SETNX"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 设置一个值，当键已存在时则忽略
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetNX(string key, byte[] value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            using (var redisWriter = new RedisWriter(this, 3, "SETNX"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            //Set key to hold string value if key does not exist. In that case, it is equal to SET. When key already holds a value, no operation is performed. SETNX is short for "SET if N ot e X ists".
            //Return value
            //Integer reply, specifically:
            //1 if the key was set
            //0 if the key was not set
            var r = this.Connection.ExpectInt();
            if (r == 1)
                return true;
            if (r == 0)
                return false;

            throw new InvalidOperationException(string.Format("Unknown Result '{0}'", r));
        }
        
        /// <summary>
        /// 设置一个值并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public bool SetEX(string key, string value, int expireSeconds)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            //SETEX mykey 10 "value"
            using (var redisWriter = new RedisWriter(this, 4, "SETEX"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(expireSeconds.ToString());
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 设置一个值并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public bool SetEX(string key, byte[] value, int expireSeconds)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            using (var redisWriter = new RedisWriter(this, 4, "SETEX"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(expireSeconds.ToString());
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 设置一组字符串(MSET)
        /// </summary>
        /// <param name="dict"></param>
        public bool Set(IDictionary<string, string> dict)
        {
            var d = new Dictionary<string, byte[]>(dict.Count);
            var e = dict.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value == null)
                    d.Add(e.Current.Key, new byte[0]);
                else
                    d.Add(e.Current.Key, this.Encoding.GetBytes(e.Current.Value));
            }
            return this.Set(d);
        }

        /// <summary>
        /// 设置一组数据(MSET)
        /// </summary>
        /// <param name="dict"></param>
        public bool Set(IDictionary<string, byte[]> dict)
        {
            if (dict == null)
                throw new ArgumentNullException("dict");
            
            //MSET key1 value1 key2 value2
            using (var w = new RedisWriter(this, dict.Count * 2 + 1, "MSET"))
            {
                foreach (var item in dict)
                {

                    w.WriteArgument(item.Key);
                    w.WriteArgument(item.Value);
                }
                this.Connection.SendCommand(w);
            }
            return this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 获取节字数组
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] GetBytes(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            using (var redisWriter = new RedisWriter(this, 2, "GET"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectBulkReply();
        }

        /// <summary>
        /// 获取字符串结果
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            using (var redisWriter = new RedisWriter(this, 2, "GET"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectToString();
        }

        //public byte[][] Sort(RedisSortOptions options)
        //{
        //    return SendDataCommandExpectMultiBulkReply(null, options.ToCommand() + "\r\n");
        //}

        /// <summary>
        /// 获取并设置一个新值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetSet(string key, byte[] value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");
            
            using (var redisWriter = new RedisWriter(this, 3, "GETSET"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectBulkReply();
        }

        /// <summary>
        /// 获取并设置一个新值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetSet(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length > 1073741824)
                throw new ArgumentException("value exceeds 1G", "value");

            using (var redisWriter = new RedisWriter(this, 3, "GETSET"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(value);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectToString();
        }
        
        /// <summary>
        /// 确认一个键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "EXISTS"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 删除一个键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "DEL"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 删除一批键
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>返回删除的个数</returns>
        public int Remove(params string[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException("args");
            
            using (var redisWriter = new RedisWriter(this, keys.Length + 1, "DEL"))
            {
                foreach (var key in keys)
                {
                    redisWriter.WriteArgument(key);
                }
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 自加一
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Increment(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "INCR"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 自加一个数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Increment(string key, int count)
        {
            if (key == null)
                throw new ArgumentNullException("key");


            using (var redisWriter = new RedisWriter(this, 3, "INCRBY"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(count.ToString());
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 自减一
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Decrement(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "DECR"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 自减指定数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Decrement(string key, int count)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 3, "DECRBY"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(count.ToString());
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt();
        }
        
        /// <summary>
        /// 获取一个键值长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Length(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "STRLEN"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 获取键类型
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisKeyType TypeOf(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "TYPE"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }

            switch (this.Connection.ExpectString())
            {
                case "none":
                    return RedisKeyType.None;
                case "string":
                    return RedisKeyType.String;
                case "set":
                    return RedisKeyType.Set;
                case "list":
                    return RedisKeyType.List;
            }

            throw new RedisResponseException("Invalid value");
        }

        /// <summary>
        /// 随机获取一个键
        /// </summary>
        /// <returns></returns>
        public string RandomKey()
        {
            using (var redisWriter = new RedisWriter(this, 1, "RANDOMKEY"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectToString();
        }

        /// <summary>
        /// 重命名一个键
        /// </summary>
        /// <param name="oldKeyname"></param>
        /// <param name="newKeyname"></param>
        /// <returns></returns>
        public bool Rename(string oldKeyname, string newKeyname)
        {
            if (oldKeyname == null)
                throw new ArgumentNullException("oldKeyname");
            if (newKeyname == null)
                throw new ArgumentNullException("newKeyname");
            
            using (var redisWriter = new RedisWriter(this, 3, "RENAME"))
            {
                redisWriter.WriteArgument(oldKeyname);
                redisWriter.WriteArgument(newKeyname);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 设置键过期间隔
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public bool Expire(string key, int seconds)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            using (var redisWriter = new RedisWriter(this, 3, "EXPIRE"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(seconds.ToString());
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 设置键绝对过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public bool ExpireAt(string key, int timestamp)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            using (var redisWriter = new RedisWriter(this, 3, "EXPIREAT"))
            {
                redisWriter.WriteArgument(key);
                redisWriter.WriteArgument(timestamp.ToString());
                this.Connection.SendCommand(redisWriter);
            }

            return this.Connection.ExpectInt() == 1;
        }

        //public int TimeToLive(string key)
        //{
        //    if (key == null)
        //        throw new ArgumentNullException("key");
        //    return SendExpectInt("TTL {0}\r\n", key);
        //}
        /// <summary>
        /// 获取一个键的TTL值
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// The command returns -2 if the key does not exist.
        /// The command returns -1 if the key exists but has no associated expire.
        /// </returns>
        public int TTL(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            using (var redisWriter = new RedisWriter(this, 2, "TTL"))
            {
                redisWriter.WriteArgument(key);
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 获取数据库大小
        /// </summary>
        /// <returns></returns>
        public int GetDbSize()
        {
            using (var redisWriter = new RedisWriter(this, 1, "DBSIZE"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectInt();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public void Save()
        {
            using (var redisWriter = new RedisWriter(this, 1, "SAVE"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            this.Connection.ExpectSuccess();
        }
        /// <summary>
        /// 背景保存
        /// </summary>
        public void BackgroundSave()
        {
            using (var redisWriter = new RedisWriter(this, 1, "BGSAVE"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            this.Connection.ExpectSuccess();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public string Shutdown()
        {
            using (var redisWriter = new RedisWriter(this, 1, "SHUTDOWN"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectString();
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void FlushAll()
        {
            using (var redisWriter = new RedisWriter(this, 1, "FLUSHALL"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            this.Connection.ExpectString();
        }

        /// <summary>
        /// 清空当前DB
        /// </summary>
        public void FlushDb()
        {
            using (var redisWriter = new RedisWriter(this, 1, "FLUSHDB"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            this.Connection.ExpectString();
        }

        /// <summary>
        /// 获取最后保存时间
        /// </summary>
        public DateTime LastSave()
        {
            using (var redisWriter = new RedisWriter(this, 1, "LASTSAVE"))
            {
                this.Connection.SendCommand(redisWriter);
            }
            var t = this.Connection.ExpectInt();
            return UnixTimestampHelper.ToDateTime(t);

            //const long UnixEpoch = 621355968000000000L;
            //    return new DateTime(UnixEpoch) + TimeSpan.FromSeconds(t);
        }

        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetInfo()
        {
            using (var redisWriter = new RedisWriter(this, 1, "INFO"))
            {
                this.Connection.SendCommand(redisWriter);
            }

            byte[] r = this.Connection.ExpectBulkReply();

            var dict = new Dictionary<string, string>(10);
            var v = this.Encoding.GetString(r);
            foreach (var line in v.Split('\n'))
            {
                int p = line.IndexOf(':');
                if (p == -1)
                    continue;
                dict.Add(line.Substring(0, p), line.Substring(p + 1));
            }
            return dict;
        }

        /// <summary>
        /// 获取所有键列表
        /// </summary>
        public string[] GetKeys()
        {
            using (var redisWriter = new RedisWriter(this, 2, "KEYS"))
            {
                redisWriter.WriteArgument("*");
                this.Connection.SendCommand(redisWriter);
            }

            var keys = this.Connection.ExpectToStringArray();
            return keys;

            //var r = this.Connection.ExpectBulkReply();
            //if (r.Length == 0)
            //    return new string[0];

            //return this.Encoding.GetString(r).Split(' ');
        }

        /// <summary>
        /// 获取匹配的键列表
        /// </summary>
        /// <param name="pattern">匹配规则</param>
        /// <returns></returns>
        public string[] GetKeys(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("key");
            
            using (var redisWriter = new RedisWriter(this, 2, "KEYS"))
            {
                redisWriter.WriteArgument(pattern);
                this.Connection.SendCommand(redisWriter);
            }

            var keys = this.Connection.ExpectToStringArray();
            return keys;

            //var keys = this.Connection.ExpectBulkReply() ;
            //if (keys.Length == 0)
            //    return new string[0];

            //return this.Encoding.GetString(keys).Split(' ');
        }
        /// <summary>
        /// 获取键的字符串值列表
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string[] GetKeys(params string[] keys)
        {
            var buf = this.GetByteKeys(keys);
            if (buf.Length == 0)
            {
                return new string[0];
            }

            var r = new string[buf.Length];
            for(int i=0,l=r.Length;i<l;i++)
            {
                r[i] = this.Encoding.GetString(buf[i]);
            }
            return r;
        }

        /// <summary>
        /// 获取键键值列表
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public byte[][] GetByteKeys(params string[] keys)
        {
            if (keys.Length == 0)
                throw new ArgumentException("keys");

            using (var redisWriter = new RedisWriter(this, keys.Length + 1, "MGET"))
            {
                foreach (var key in keys)
                {
                    redisWriter.WriteArgument(key);
                }
                this.Connection.SendCommand(redisWriter);
            }
            return this.Connection.ExpectMultiBulkReply();
        }


        /// <summary>
       /// 资源释放
       /// </summary>
        public void Dispose()
        {
            this.Connection.Dispose();
        }

        #region icache

        /// <summary>
        /// 设置一个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires">过期时间（以秒为时间单位）</param>
        public void Set(string key, object value, int expires)
        {
            if (value == null || value is string)
                this.SetEX(key, (string)value,expires);
            else if (value is byte[])
                this.SetEX(key, (byte[])value,expires);
            else
                this.SetEX(key, this.BinarySerializable.Serialize(value),expires);
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var b = this.GetBytes(key);
            return (T)this.BinarySerializable.Deserialize(typeof(T), b);
        }

        /// <summary>
        /// 设置一个对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key, Type type)
        {
            var b = this.GetBytes(key);
            return this.BinarySerializable.Deserialize(type, b);
        }

        /// <summary>
        /// 删除一个缓存值
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            this.Remove(key);
        }

        #endregion
    }
    
}