using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// connection handler interface
    /// </summary>
    public interface IConnectionHandler
    {
        /// <summary>
        /// parse message 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="firstByte"></param>
        /// <returns></returns>
        object Parse(SocketConnection connection, byte firstByte);
    }
}
