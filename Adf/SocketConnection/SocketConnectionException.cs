using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket connection exception
    /// </summary>
    public class SocketConnectionException : Exception
    {
        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        public SocketConnectionException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public SocketConnectionException(string message, Exception inner)
            : base(message,inner)
        {

        }
    }
}
