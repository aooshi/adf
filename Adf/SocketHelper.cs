using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace Adf
{
    /// <summary>
    /// Socket
    /// </summary>
    public static class SocketHelper
    {
        ///// <summary>
        ///// Socket CLOSE ERROR CODE
        ///// </summary>
        //const int SOCKET_CLOSE_ERROR_CODE = 10054;

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="readSize"></param>
        /// <param name="bufferSize"></param>
        /// <exception cref="SocketException">errorCode= SocketError.Shutdown</exception>
        public static MemoryStream Receive(Socket socket, int readSize, int bufferSize)
        {
            var m = new MemoryStream(bufferSize);
            Receive(m, socket, readSize, bufferSize);
            return m;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="socket"></param>
        /// <param name="readSize"></param>
        /// <param name="bufferSize"></param>
        /// <exception cref="SocketException">errorCode= SocketError.Shutdown</exception>
        public static void Receive(MemoryStream memoryStream, Socket socket, int readSize, int bufferSize)
        {
            var count = 0;
            var buffer = new byte[bufferSize];
            var readCount = 0;
            var readAllCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;
                if (readCount > bufferSize)
                    readCount = bufferSize;

                count = socket.Receive(buffer, readCount, SocketFlags.None);
                readAllCount += count;
                if (count == 0)
                {
                    //throw new SocketReadZeroException();
                    throw new SocketException((int)SocketError.Shutdown);
                }
                memoryStream.Write(buffer, 0, count);
            }
            memoryStream.Position = 0;
        }

        /// <summary>
        /// 读取指定字节数
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="readSize"></param>
        /// <exception cref="SocketException">errorCode= SocketError.Shutdown</exception>
        public static byte[] Receive(Socket socket, int readSize)
        {
            var count = 0;
            var buffer = new byte[readSize];
            var readCount = 0;
            var readAllCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;

                count = socket.Receive(buffer, readAllCount, readCount, SocketFlags.None);
                readAllCount += count;
                if (count == 0)
                {
                    //throw new SocketReadZeroException();
                    throw new SocketException((int)SocketError.Shutdown);
                }
            }

            return buffer;
        }


        /// <summary>
        /// 读取指定字节数填充至缓冲区指定位置
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="readSize"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <exception cref="SocketException">errorCode= SocketError.Shutdown</exception>
        public static byte[] Receive(Socket socket, int readSize, byte[] buffer, int offset)
        {
            if (buffer.Length - offset < readSize)
                throw new ArgumentOutOfRangeException("readSize", "buffer size is too small");

            var count = 0;
            var readAllCount = 0;
            var readCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;
                count = socket.Receive(buffer, offset + readAllCount, readCount, SocketFlags.None);
                if (count == 0)
                {
                    throw new SocketException((int)SocketError.Shutdown);
                }
                readAllCount += count;
            }

            return buffer;
        }

        ///// <summary>
        ///// Reads from the socket until the sequence '\r\n' is encountered, 
        ///// and returns everything up to but not including that sequence as a UTF8-encoded string
        ///// </summary>
        //public static string ReadLine(Stream stream, Encoding encoding)
        //{
        //    using (MemoryStream buffer = new MemoryStream(32))
        //    {
        //        int b;
        //        bool gotReturn = false;
        //        while ((b = stream.ReadByte()) != -1)
        //        {
        //            if (gotReturn)
        //            {
        //                if (b == 10)
        //                {
        //                    break;
        //                }
        //                buffer.WriteByte(13);
        //                gotReturn = false;
        //            }
        //            if (b == 13)
        //            {
        //                gotReturn = true;
        //            }
        //            else
        //            {
        //                buffer.WriteByte((byte)b);
        //            }
        //        }
        //        return encoding.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
        //    }
        //}

        /// <summary>
        /// reads a line
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="encoding"></param>
        /// <returns>String that was read in</returns>
        public static string ReadLine(Socket socket, Encoding encoding)
        {
            var arr = ReadLine(socket);
            if (arr.Count == 0)
                return string.Empty;
            return encoding.GetString(arr.Array, 0, arr.Count);
        }
                
        /// <summary>
        /// reads a line
        /// </summary>
        /// <returns>String that was read in</returns>
        public static ArraySegment<byte> ReadLine(Socket socket)
        {
            if (socket == null || !socket.Connected)
            {
                throw new IOException("socket closed");
            }

            //  var sb = new StringBuilder();
            //int c;
            //while ((c = bstream.ReadByte()) != -1)
            //{
            //    if (c == '\r')
            //        continue;
            //    if (c == '\n')
            //        break;
            //    sb.Append((char)c);
            //}
            //return sb.ToString();

            byte[] b = new byte[1];
            using (MemoryStream memoryStream = new MemoryStream(64))
            {
                //bool eol = false;
                while (socket.Receive(b, 0, 1, SocketFlags.None) != 0)
                {
                    //if (b[0] == 13)
                    //{
                    //    eol = true;
                    //    continue;
                    //}
                    //else
                    //{
                    //    if (eol)
                    //    {
                    //        if (b[0] == 10)
                    //            break;

                    //        eol = false;
                    //    }
                    //}

                    if (b[0] == 13)
                    {
                        if (socket.Receive(b, 0, 1, SocketFlags.None) == 0)
                            break;

                        if (b[0] == 10)
                            break;

                        memoryStream.WriteByte(13);
                        memoryStream.WriteByte(b[0]);

                        continue;
                    }
                    if (b[0] == 10)
                        break;

                    // cast byte into char array
                    memoryStream.WriteByte(b[0]);
                }

                if (memoryStream.Length == 0)
                {
                    //   throw new IOException("closing dead stream");
                    return new ArraySegment<byte>(new byte[0], 0, 0);
                }

                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }



        /// <summary>
        /// read segment for \0
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>String that was read in</returns>
        public static ArraySegment<byte> ReadSegment(Socket socket)
        {
            using (MemoryStream memoryStream = new MemoryStream(64))
            {
                ReadSegment(memoryStream, socket);

                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        /// <summary>
        /// read segment for \0
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="output"></param>
        /// <returns>String that was read in</returns>
        public static void ReadSegment(Stream output, Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            byte[] b = new byte[1];

            while (true)
            {
                if (socket.Receive(b) == 0)
                    break;

                if (b[0] == '\0')
                    break;

                output.WriteByte(b[0]);
            }
        }

        /// <summary>
        /// 是否为关闭
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsClose(SocketException e)
        {
            // return e.ErrorCode.Equals(SOCKET_CLOSE_ERROR_CODE) && e.SocketErrorCode == SocketError.ConnectionReset;
            return e.SocketErrorCode == SocketError.ConnectionAborted
                || e.SocketErrorCode == SocketError.ConnectionReset
                || e.SocketErrorCode == SocketError.Shutdown;
        }

        /// <summary>
        /// 尝试关闭一个连接
        /// </summary>
        /// <param name="socket"></param>
        public static void TryClose(Socket socket)
        {
            if (socket != null)
            {
                try { socket.Shutdown(SocketShutdown.Both); }
                catch { }

                try { socket.Close(); }
                catch { }
            }
        }
        
        /// <summary>
        /// connect to host
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="timeout"></param>
        /// <param name="port"></param>
        /// <param name="addr"></param>
        /// <exception cref="System.TimeoutException">connect timeout</exception>
        public static void Connect(Socket socket, IPAddress addr, int port, int timeout)
        {
            using (var eventHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset))
            {
                socket.BeginConnect(addr, port, ar2 =>
                {
                    try
                    {
                        socket.EndConnect(ar2);
                        //
                        eventHandle.Set();
                    }
                    catch { }

                }, null);
                //
                if (eventHandle.WaitOne(timeout) == false)
                {
                    SocketHelper.TryClose(socket);

                    throw new TimeoutException("Connect to " + addr.ToString() + ":" + port + " timeout.");
                }
            }
        }

        /// <summary>
        /// connect to host
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="timeout"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <exception cref="System.TimeoutException">connect timeout</exception>
        public static void Connect(Socket socket, string host, int port, int timeout)
        {
            using (var eventHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset))
            {
                socket.BeginConnect(host, port, ar2 =>
                {
                    try
                    {
                        socket.EndConnect(ar2);
                        //
                        eventHandle.Set();
                    }
                    catch { }

                }, null);
                //
                if (eventHandle.WaitOne(timeout) == false)
                {
                    SocketHelper.TryClose(socket);

                    throw new TimeoutException("Connect to " + host + ":" + port + " timeout.");
                }
            }
        }
    }
}