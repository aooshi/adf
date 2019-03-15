using System;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket id generator
    /// </summary>
    public class SocketIdentifier
    {
        /// <summary>
        /// generator
        /// </summary>
        public readonly static SocketIdentifier Generator = new SocketIdentifier();

        long sessionId = 0;

        private SocketIdentifier()
        {
        }

        /// <summary>
        /// new session id
        /// </summary>
        /// <returns></returns>
        public long NewSessionId()
        {
            return System.Threading.Interlocked.Increment(ref this.sessionId);
        }
    }
}
