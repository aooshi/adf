using System;
using System.IO;

namespace Adf.SocketConnection
{
    /// <summary>
    /// default connection handler
    /// </summary>
    public class ConnectionHandler : IConnectionHandler
    {
        /// <summary>
        /// default handler
        /// </summary>
        public readonly static ConnectionHandler Default = new ConnectionHandler();

        /// <summary>
        /// parse message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="firstByte"></param>
        /// <returns></returns>
        public object Parse(SocketConnection connection, byte firstByte)
        {
            Stream inputStream = connection.Stream;

            using (var outputStream = new MemoryStream(128))
            {
                outputStream.WriteByte(firstByte);
                //
                Adf.StreamHelper.ReadSegment(outputStream, inputStream);
                //
                return System.Text.Encoding.UTF8.GetString(outputStream.GetBuffer(), 0, (int)outputStream.Position);
            }
        }
    }
}
