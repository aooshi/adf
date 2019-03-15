using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 订阅事件参数
    /// </summary>
    public class RedisSubscribeResult
    {
        /// <summary>
        /// Type 为订阅类型操作时,返回当前订阅此频道的数量，如： subscribe/unsubscribe
        /// </summary>
        public int SubscribeCount { get; internal set; }
        /// <summary>
        /// 频道名
        /// </summary>
        public string Channel { get; private set; }
        /// <summary>
        /// 结果类型
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Type 为 message 时取得消息内容
        /// </summary>
        public string Message { get; internal set; }
        
        internal RedisSubscribeResult(string type,string channel)
        {
            this.Type = type;
            this.Channel = channel;
        }
    }
}
