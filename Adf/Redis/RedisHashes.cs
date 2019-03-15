using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// Redis 哈希表
    /// </summary>
    public class RedisHashes
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisHashes(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 删除一个项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool HDEL(string key, string field)
        {
            //HDEL myhash field1

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            using (var w = new RedisWriter(this.client, 3, "HDEL"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);

                this.connection.SendCommand(w);
            }

            //Integer reply: the number of fields that were removed from the hash, not including specified but non existing fields.
            return this.connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 判断一个项是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool HEXISTS(string key, string field)
        {
            //HEXISTS myhash field1

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            using (var w = new RedisWriter(this.client, 3, "HEXISTS"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);

                this.connection.SendCommand(w);
            }

            //            Integer reply, specifically:
            //1 if the hash contains field.
            //0 if the hash does not contain field, or key does not exist.
            return this.connection.ExpectInt() == 1;
        }

        /// <summary>
        /// 获取一个项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public string HGET(string key, string field)
        {
            //HGET key field

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            using (var w = new RedisWriter(this.client, 3, "HGET"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);

                this.connection.SendCommand(w);
            }

            //            Integer reply, specifically:
            //1 if the hash contains field.
            //0 if the hash does not contain field, or key does not exist.
            return this.connection.ExpectToString();
        }

        /// <summary>
        /// 获取一个项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, string> HGETALL(string key)
        {
            //HGETALL myhash

            if (key == null)
                throw new ArgumentNullException("key");

            using (var w = new RedisWriter(this.client, 2, "HGETALL"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }

            //Array reply: list of fields and their values stored in the hash, or an empty list when key does not exist.
            var b = this.connection.ExpectMultiBulkReply();

            var dict = new Dictionary<string, string>(b.Length);
            for (int i = 0, l = b.Length; i < l; i += 2)
            {
                dict[this.client.Encoding.GetString(b[i])] = this.client.Encoding.GetString(b[i + 1]);
            }
            return dict;
        }
             
        
        /// <summary>
        /// 对某项自增一数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="increment"></param>
        /// <returns>自增后的数值</returns>
        public int HINCRBY(string key, string field, int increment)
        {
            //HINCRBY key field increment

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            using (var w = new RedisWriter(this.client, 4, "HINCRBY"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);
                w.WriteArgument(increment.ToString());

                this.connection.SendCommand(w);
            }

            //Integer reply: the value at field after the increment operation.
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 对某项自增一浮点数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="increment"></param>
        /// <returns>自增后的数值,服务端版本要求:Available since 2.6.0.</returns>
        public double HINCRBYFLOAT(string key, string field, double increment)
        {
            //HINCRBYFLOAT mykey field 0.1

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            using (var w = new RedisWriter(this.client, 4, "HINCRBYFLOAT"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);
                w.WriteArgument(increment.ToString());

                this.connection.SendCommand(w);
            }

            //Bulk string reply: the value of field after the increment.
            return this.connection._ExpectToDouble();
        }

        /// <summary>
        /// 获取元素的键项列表
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] HKEYS(string key)
        {
            //HKEYS myhash

            if (key == null)
                throw new ArgumentNullException("key");

            using (var w = new RedisWriter(this.client,2, "HKEYS"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }

            //Array reply
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }

        /// <summary>
        /// 获取元素的键项列表
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] HVALS(string key)
        {
            //HVALS key

            if (key == null)
                throw new ArgumentNullException("key");

            using (var w = new RedisWriter(this.client, 2, "HVALS"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }

            //Array reply
            var data = this.connection.ExpectMultiBulkReply();
            return this.connection.ExpectToStringArray(data);
        }

        /// <summary>
        /// 获取哈希表元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int HLEN(string key)
        {
            //HLEN key

            if (key == null)
                throw new ArgumentNullException("key");

            using (var w = new RedisWriter(this.client, 2, "HLEN"))
            {
                w.WriteArgument(key);

                this.connection.SendCommand(w);
            }

            //Integer reply: number of fields in the hash, or 0 when key does not exist.
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 获取元素的键项列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public string[] HMGET(string key, params string[] fields)
        {
            //HMGET key field [field ...]

            if (key == null)
                throw new ArgumentNullException("key");

            if (fields.Length == 0)
                throw new ArgumentException("fields no data");

            using (var w = new RedisWriter(this.client, fields.Length + 2, "HMGET"))
            {
                w.WriteArgument(key);

                foreach (var field in fields)
                    w.WriteArgument(field);

                this.connection.SendCommand(w);
            }

            //Array reply
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }

        /// <summary>
        /// 设置一组元素值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dictionary"></param>
        public bool HMSET(string key, IDictionary<string, string> dictionary)
        {
            //HMSET key field value [field value ...]

            if (key == null)
                throw new ArgumentNullException("key");

            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            using (var w = new RedisWriter(this.client, dictionary.Count * 2 + 2, "HMSET"))
            {
                w.WriteArgument(key);

                var e = dictionary.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Key == null)
                        throw new ArgumentNullException("dictionary", "dictionary key contains null, it's not allowed");
                    if (e.Current.Value == null)
                        throw new ArgumentNullException("dictionary", "dictionary key " + e.Current.Key + " value is null");

                    w.WriteArgument(this.client.Encoding.GetBytes(e.Current.Key));
                    w.WriteArgument(this.client.Encoding.GetBytes(e.Current.Value));
                }

                this.connection.SendCommand(w);
            }

            //Simple string reply
            return this.connection.ExpectSuccess();
        }
        
        /// <summary>
        /// 设置一个元素项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set.
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        public int HSET(string key, string field,string value)
        {
            //HSET key field value

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            if (value == null)
                throw new ArgumentNullException("value");

            using (var w = new RedisWriter(this.client, 4, "HSET"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);
                w.WriteArgument(value);

                this.connection.SendCommand(w);
            }

            //         Integer reply, specifically:
            //1 if field is a new field in the hash and value was set.
            //0 if field already exists in the hash and the value was updated.
            return this.connection.ExpectInt();
        }
        
        /// <summary>
        /// 设置一个元素项,如果元素项不存在则成功，否则设置失败
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set.
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        public int HSETNX(string key, string field, string value)
        {
            //HSETNX key field value

            if (key == null)
                throw new ArgumentNullException("key");

            if (field == null)
                throw new ArgumentNullException("field");

            if (value == null)
                throw new ArgumentNullException("value");
            
            using (var w = new RedisWriter(this.client, 4, "HSETNX"))
            {
                w.WriteArgument(key);
                w.WriteArgument(field);
                w.WriteArgument(value);

                this.connection.SendCommand(w);
            }

            //         Integer reply, specifically:
            //1 if field is a new field in the hash and value was set.
            //0 if field already exists in the hash and the value was updated.
            return this.connection.ExpectInt();
        }
    }
}