using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 记录异常事件数据
    /// </summary>
    public class LogExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="exception"></param>
        public LogExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}