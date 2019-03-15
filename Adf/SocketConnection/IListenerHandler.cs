using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// listener handler
    /// </summary>
    public interface IListenerHandler
    {
        /// <summary>
        /// create connection
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        SocketConnection CreateConnection(SocketListener listener);
    }
}
