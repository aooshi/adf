using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// parser exception
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        public ParserException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// init new instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ParserException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
