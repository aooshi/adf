using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Redis 列表
    /// </summary>
    public class RedisLists
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisLists(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 获取列表中指定位置元素
        /// 当"start=0,end=-1"获取整个集合中所有元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string[] LRange(string key, int start, int end)
        {
            using (var w = new RedisWriter(this.client, 4, "LRANGE"))
            {
                w.WriteArgument(key);
                w.WriteArgument(start.ToString());
                w.WriteArgument(end.ToString());

                this.connection.SendCommand(w);
            }
            var data = this.connection.ExpectMultiBulkReply();
            return this.connection.ExpectToStringArray( data );
        }

        /// <summary>
        /// 移除元素后的一个列表范围
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="count">
        /// 
        /// count &lt; 0: Remove elements equal to value moving from head to tail.
        /// count &gt; 0: Remove elements equal to value moving from tail to head.
        /// count = 0: Remove all elements equal to value.
        /// 
        /// </param>
        /// <returns>Integer reply: the number of removed elements.</returns>
        public int LRem(string key, string value, int count)
        {
            using (var w = new RedisWriter(this.client, 4, "LREM"))
            {
                w.WriteArgument(key);
                w.WriteArgument(count.ToString());
                w.WriteArgument(value);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 设置指定索引元素值
        /// 将索引为index的元素赋值为value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns>Integer reply: the number of removed elements.</returns>
        public bool LSet(string key, int index, string value)
        {
            var v = value == null ? new byte[0] : this.client.Encoding.GetBytes(value);
            using (var w = new RedisWriter(this.client, 4, "LSET"))
            {
                w.WriteArgument(key);
                w.WriteArgument(index.ToString());
                w.WriteArgument(value);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectSuccess();
        }
                

        /// <summary>
        /// 在指定位置添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="before">元素前后,true before,false after</param>
        /// <param name="pivot">位置元素</param>
        /// <returns>添加后元素数</returns>
        public int LInsert(string key, string value, string pivot, bool before)
        {
            //Integer reply: the length of the list after the insert operation, or -1 when the value pivot was not found.
            //LINSERT mylist BEFORE "World" "There"
            //LINSERT key BEFORE|AFTER pivot value
            using (var w = new RedisWriter(this.client, 5, "LINSERT"))
            {
                w.WriteArgument(key);
                w.WriteArgument(before ? "BEFORE" : "AFTER");
                w.WriteArgument(pivot);
                w.WriteArgument(value);

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();
        }
        
        /// <summary>
        /// 在列表左侧添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>添加后元素数</returns>
        public int LPush(string key, string value)
        {
            //Integer reply: the length of the list after the push operations.
            using (var w = new RedisWriter(this.client, 3, "LPUSH"))
            {
                w.WriteArgument(key);
                w.WriteArgument(value);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        
       /// <summary>
       /// 在列表右侧添加元素
       /// </summary>
       /// <param name="key"></param>
       /// <param name="value"></param>
       /// <returns>添加后元素数</returns>
        public int RPush(string key, string value)
        {
            //Integer reply: the length of the list after the push operation.

            using (var w = new RedisWriter(this.client, 3, "RPUSH"))
            {
                w.WriteArgument(key);
                w.WriteArgument(value);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 列表中元素的个数
        /// 当键不存在时LLEN会返回0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int LLen(string key)
        {
            //Integer reply: the length of the list at key.
            using (var w = new RedisWriter(this.client, 2, "LLEN"))
            {
                w.WriteArgument(key);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }
        
        /// <summary>
        /// 获取指定索引元素
        /// 如果index是负数则表示从右边开始计算的索引，最右边元素的索引是-1
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string LIndex(string key, int index)
        {
            //Bulk string reply: the requested element, or nil when index is out of range.
            using (var w = new RedisWriter(this.client, 3, "LINDEX"))
            {
                w.WriteArgument(key);
                w.WriteArgument(index.ToString());
                this.connection.SendCommand(w);
            }
            var data = this.connection.ExpectBulkReply();
            return this.client.Encoding.GetString(data);
        }
        
        /// <summary>
        /// 取出列表第一元素
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string LPop(string key)
        {
            //Bulk string reply: the value of the first element, or nil when key does not exist.

            using (var w = new RedisWriter(this.client, 2, "LPOP"))
            {
                w.WriteArgument(key);
                this.connection.SendCommand(w);
            }
            var data =  this.connection.ExpectBulkReply();
            return this.client.Encoding.GetString(data);
        }
        
        /// <summary>
        /// 取出列表最后一个元素
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string RPop(string key)
        {
            //Bulk string reply: the value of the first element, or nil when key does not exist.

            using (var w = new RedisWriter(this.client, 2, "RPOP"))
            {
                w.WriteArgument(key);
                this.connection.SendCommand(w);
            }
            return this.client.Encoding.GetString( this.connection.ExpectBulkReply() );
        }

        /// <summary>
        /// 将元素从一个列表转到另一个列表
        /// 原子性地返回并移除存储在 source 的列表的最后一个元素（列表尾部元素）， 并把该元素放入存储在 destination 的列表的第一个元素位置（列表头部)
        /// </summary>
        /// <param name="destinationKey"></param>
        /// <param name="sourceKey"></param>
        public string RPOPLPUSH(string sourceKey, string destinationKey)
        {
            using (var w = new RedisWriter(this.client, 3, "RPOPLPUSH"))
            {
                w.WriteArgument(sourceKey);
                w.WriteArgument(destinationKey);
                this.connection.SendCommand(w);
            }
            var data = this.connection.ExpectBulkReply();
            return this.client.Encoding.GetString(data);
        }
    }

}