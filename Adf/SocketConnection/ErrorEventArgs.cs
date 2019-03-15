using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// error event args
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        Exception exception = null;
        /// <summary>
        /// get event exception
        /// </summary>
        public Exception Exception
        {
            get { return this.exception; }
        }

        public ErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }
    }
}
