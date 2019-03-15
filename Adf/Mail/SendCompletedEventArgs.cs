//using System;

//namespace Adf.Mail
//{
//    /// <summary>
//    /// 异步发送完成事件数据
//    /// </summary>
//    public class SendCompletedEventArgs : EventArgs
//    {
//        Exception exception = null;
//        /// <summary>
//        /// 失败时的异常信息
//        /// </summary>
//        public Exception Exception { get { return this.exception; } }


//        bool success = false;
//        /// <summary>
//        /// 指示是否成功
//        /// </summary>
//        public bool Success { get { return this.success; } }


//        object userToken = null;
//        /// <summary>
//        /// 获取发送时传递的用户数据
//        /// </summary>
//        public object UserToken { get { return this.userToken; } }

//        /// <summary>
//        /// 初始化新实例
//        /// </summary>
//        /// <param name="exception"></param>
//        /// <param name="userToken"></param>
//        public SendCompletedEventArgs(Exception exception, object userToken)
//        {
//            this.exception = exception;
//            this.userToken = userToken;
//            this.success = exception == null;
//        }
//    }
//}