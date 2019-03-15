using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Adf
{
    /// <summary>
    /// WebSocket 握手处理
    /// </summary>
    internal class WebSocketHandshake
    {
        /// <summary>
        /// Magic KEY
        /// </summary>
        public const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="secWebSocketKey"></param>
        /// <returns></returns>
        public static String HandshakeSecurityHash09(String secWebSocketKey)
        {
            //const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            String secWebSocketAccept = String.Empty;
            // 1. Combine the request Sec-WebSocket-Key with magic key.
            String ret = string.Concat(secWebSocketKey, MagicKEY);
            // 2. Compute the SHA1 hash
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
                // 3. Base64 encode the hash
                secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            }
            return secWebSocketAccept;
        }
    }
}
