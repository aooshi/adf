using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket listener exception
    /// </summary>
    public class SocketListenerException : Exception
    {
        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        public SocketListenerException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public SocketListenerException(string message, Exception inner)
            : base(message,inner)
        {

        }
    }
}
