using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 日志相关异常
    /// </summary>
    public class LogException : Exception
    {
        /// <summary>
        /// 获取或设置引发异常的日志管理器
        /// </summary>
        public LogManager Manager
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置引发异常的写书器
        /// </summary>
        public LogWriter Writer
        {
            get;
            set;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="message"></param>
        public LogException(string message)
            : base(message)
        {

        }
    }
}