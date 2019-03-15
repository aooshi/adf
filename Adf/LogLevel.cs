using System;

namespace Adf
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel : int
    {
        /// <summary>
        /// 未指定
        /// </summary>
        None = 0,
        /// <summary>
        /// 消息
        /// </summary>
        Message = 0x1,
        /// <summary>
        /// 警告
        /// </summary>
        Warning = 0x2,
        /// <summary>
        /// 调试
        /// </summary>
        Debug = 0x4,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 0x8
    }
}
