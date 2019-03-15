using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 关闭事件
    /// </summary>
    public class WebSocketCloseEventArgs : EventArgs
    {
        /// <summary>
        /// 关闭原因
        /// </summary>
        public WebSocketCloseReason Reason
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="reason"></param>
        public WebSocketCloseEventArgs(WebSocketCloseReason reason)
        {
            this.Reason = reason;
        }
    }

    /// <summary>
    /// WebSocketMessageEvent Args
    /// </summary>
    public class WebSocketMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 获取消息体
        /// </summary>
        public string Message
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取二进制传输时的二进制数据
        /// </summary>
        public byte[] Buffer
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取消息体 Opcode
        /// </summary>
        public WebSocketOpcode Opcode
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始数据
        /// </summary>
        /// <param name="opcode"></param>
        public WebSocketMessageEventArgs(WebSocketOpcode opcode)
        {
            this.Opcode = opcode;
        }
    }


    /// <summary>
    /// 错误事件参数
    /// </summary>
    public class WebSocketErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 获取引发事件的异常
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="exception"></param>
        public WebSocketErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
    
    /// <summary>
    /// 异步发送完成事件数据
    /// </summary>
    public class WebSocketSendEventArgs : EventArgs
    {
        /// <summary>
        /// 获取或设置异常信息
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置一个值表示发送是否成功
        /// </summary>
        public bool IsSuccess
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户请求时状态保持数据
        /// </summary>
        public object UserState
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新实例，置状态为成功
        /// </summary>
        /// <param name="userState"></param>
        public WebSocketSendEventArgs(object userState)
        {
            this.UserState = userState;
            this.Exception = null;
            this.IsSuccess = true;
        }

        /// <summary>
        /// 初始化新实例，置状态为失败
        /// </summary>
        /// <param name="userState"></param>
        /// <param name="exception"></param>
        public WebSocketSendEventArgs(Exception exception, object userState)
        {
            this.UserState = userState;
            this.Exception = exception;
            this.IsSuccess = false;
        }
    }
}