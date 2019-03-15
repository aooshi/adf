using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Contains the values of the opcode that indicates the type of a WebSocket frame.
    /// </summary>
    /// <remarks>
    /// The values of the opcode are defined in
    /// <see href="http://tools.ietf.org/html/rfc6455#section-5.2">Section 5.2</see> of RFC 6455.
    /// </remarks>
    public enum WebSocketOpcode : sbyte
    {
        /// <summary>
        /// Equivalent to numeric value 0.
        /// Indicates a continuation frame.
        /// </summary>
        Cont = 0x0,
        /// <summary>
        /// Equivalent to numeric value 1.
        /// Indicates a text frame.
        /// </summary>
        Text = 0x1,
        /// <summary>
        /// Equivalent to numeric value 2.
        /// Indicates a binary frame.
        /// </summary>
        Binary = 0x2,
        /// <summary>
        /// Equivalent to numeric value 8.
        /// Indicates a connection close frame.
        /// </summary>
        Close = 0x8,
        /// <summary>
        /// Equivalent to numeric value 9.
        /// Indicates a ping frame.
        /// </summary>
        Ping = 0x9,
        /// <summary>
        /// Equivalent to numeric value 10.
        /// Indicates a pong frame.
        /// </summary>
        Pong = 0xa
    }

//Opcode: 4 bits
//定义了“负载数据”的解释。如果收到一个未知的操作码，接收端点必须_失败WebSocket连接_。定义了以下值。

//%x0 代表一个继续帧
//%x1 代表一个文本帧
//%x2 代表一个二进制帧
//%x3-7 保留用于未来的非控制帧
//%x8 代表连接关闭
//%x9 代表ping
//%xA 代表pong
//%xB-F 保留用于未来的控制帧
}
