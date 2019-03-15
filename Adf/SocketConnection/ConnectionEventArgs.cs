using System;

namespace Adf.SocketConnection
{
    public class ConnectionEventArgs : EventArgs
    {
        SocketConnection connection;
        /// <summary>
        /// get connection
        /// </summary>
        public SocketConnection Connection
        {
            get { return this.connection; }
        }

        public ConnectionEventArgs(SocketConnection connection)
        {
            this.connection = connection;
        }
    }
}
