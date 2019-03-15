using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// HTTP服务事件数据
    /// </summary>
    public class HttpServerEventArgs : EventArgs
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        public HttpServerContext Context
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="context"></param>
        public HttpServerEventArgs(HttpServerContext context)
        {
            this.Context = context;
        }
    }
}