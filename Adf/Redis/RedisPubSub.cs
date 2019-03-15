using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 发布与订阅
    /// </summary>
    public class RedisPubSub
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisPubSub(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 设置一个二进制值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value">值对空格与双引号数据敏感，若值包含空格或引号，请使用base64进行编码后传输，订阅时进行解码</param>
        /// <returns>received client count</returns>
        public int Publish(string channel, string value)
        {
            if (string.IsNullOrEmpty(channel))
                throw new ArgumentException("channel");

            using (var redisWriter = new RedisWriter(this.client, 3, "PUBLISH"))
            {
                redisWriter.WriteArgument(channel);
                redisWriter.WriteArgument(value);
                this.connection.SendCommand(redisWriter);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 订阅指定KEY的消息，使用此方法请确保此实例仅会被当次使用
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="resultCallback"></param>
        public void Subscribe(Action<RedisSubscribeResult> resultCallback, params string[] channels)
        {
            if (channels.Length == 0)
                throw new ArgumentException("channels");

            using (var redisWriter = new RedisWriter(this.client, channels.Length + 1, "SUBSCRIBE"))
            {
                foreach (var key in channels)
                    redisWriter.WriteArgument(key);

                this.connection.SendCommand(redisWriter);
            }

            this.client.PoolAbandon = true; //禁用当前池实例，防止被重用
            while (true)
            {
                var result = this.connection.ExpectSubscribeResult();
                resultCallback(result);
            }
        }

        /// <summary>
        /// 查看订阅与发布系统状态,返回活跃频道组成的列表。
        /// </summary>
        /// <returns></returns>
        public string[] PUBSUB()
        {
            using (var w = new RedisWriter(this.client,2,"PUBSUB"))
            {
                w.WriteArgument("CHANNELS");
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectToStringArray(this.connection.ExpectMultiBulkReply());
        }
    }
}
