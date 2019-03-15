using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// default listener handler
    /// </summary>
    public class ListenerHandler : IListenerHandler
    {
        /// <summary>
        /// get default handler
        /// </summary>
        public readonly static ListenerHandler Default = new ListenerHandler();

        /// <summary>
        /// create connection
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public SocketConnection CreateConnection(SocketListener listener)
        {
            return new SocketConnection();
        }
    }
}
