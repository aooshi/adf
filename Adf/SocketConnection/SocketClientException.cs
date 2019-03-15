using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket client exception
    /// </summary>
    public class SocketClientException : Exception
    {
        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        public SocketClientException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public SocketClientException(string message, Exception inner)
            : base(message,inner)
        {

        }
    }
}
