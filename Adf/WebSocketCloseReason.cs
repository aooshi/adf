using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// WebSocket 关闭原因
    /// </summary>
    public enum WebSocketCloseReason : byte
    {
        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout = 1,
        /// <summary>
        /// 通信错误
        /// </summary>
        IOError = 2,
        /// <summary>
        /// 主动关闭
        /// </summary>
        Close = 3,
        /// <summary>
        /// 通信断开
        /// </summary>
        Disconnected = 4,
        /// <summary>
        /// 常规性错误
        /// </summary>
        Error = 5,
    }
}
