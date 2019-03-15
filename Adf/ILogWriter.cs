using System;

namespace Adf
{
    /// <summary>
    /// 日志书写器
    /// </summary>
    public interface ILogWriter : IDisposable
    {
        /// <summary>
        /// 书写器关闭完成
        /// </summary>
        event EventHandler Disposed;
        /// <summary>
        /// 日志写入事件
        /// </summary>
        event EventHandler<LogEventArgs> Writing;
        /// <summary>
        /// 获取日志写书器级别
        /// </summary>
        LogLevel Level { get; }
        /// <summary>
        /// 获取当前日志名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 获取或设置是否启用该书写器
        /// </summary>
        bool Enable { get; set; }
        /// <summary>
        /// 写入日志内容并格式化内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args">格式化数据</param>
        void Write(string content, params object[] args);
        /// <summary>
        /// 写入日志内容
        /// </summary>
        /// <param name="content"></param>
        void Write(string content);
        /// <summary>
        /// 写入以时间起始的行日志内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        void WriteTimeLine(string content, params object[] args);
        /// <summary>
        /// 写入以时间起始的日志内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        void WriteTime(string content, params object[] args);
        /// <summary>
        /// 写入一行日志内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        void WriteLine(string content, params object[] args);
        /// <summary>
        /// 写入一个空行
        /// </summary>
        void WriteLine();
    }
}
