using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace Adf
{
    /// <summary>
    /// 日志事件
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// log event args
        /// </summary>
        /// <param name="content"></param>
        public LogEventArgs(string content)
        {
            this.content = content;
        }

        /// <summary>
        /// log event args
        /// </summary>
        /// <param name="content"></param>
        /// <param name="level"></param>
        public LogEventArgs(string content, LogLevel level)
        {
            this.content = content;
            this.logLevel = level;
        }

        LogLevel logLevel = LogLevel.None;
        /// <summary>
        /// 获取日志级别
        /// </summary>
        public LogLevel Level
        {
            get { return this.logLevel; }
        }



        string content = null;

        /// <summary>
        /// 获取内容
        /// </summary>
        public string Content
        {
            get { return this.content; }
        }
    }
}
