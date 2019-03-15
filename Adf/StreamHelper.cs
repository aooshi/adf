using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace Adf
{
    /// <summary>
    /// Stream
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readSize"></param>
        /// <param name="bufferSize"></param>
        /// <exception cref="SocketException">errorCode= SocketError.Shutdown</exception>
        public static MemoryStream Receive(Stream stream, int readSize, int bufferSize)
        {
            var m = new MemoryStream(bufferSize);
            Receive(m, stream, readSize, bufferSize);
            return m;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="receiveStream"></param>
        /// <param name="readSize"></param>
        /// <param name="bufferSize"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static void Receive(MemoryStream memoryStream, Stream receiveStream, int readSize, int bufferSize)
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

                count = receiveStream.Read(buffer, 0, readCount);
                readAllCount += count;
                if (count == 0)
                {
                    throw new IOException("Can't read to readSize");
                }
                memoryStream.Write(buffer, 0, count);
            }
            memoryStream.Position = 0;

            //if (readAllCount != readCount)
            //{
            //    throw new IOException("Can't read to readSize");
            //}
        }

        /// <summary>
        /// 读取指定字节数
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readSize"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static byte[] Receive(Stream stream, int readSize)
        {
            var count = 0;
            var buffer = new byte[readSize];
            var readCount = 0;
            var readAllCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;

                count = stream.Read(buffer, readAllCount, readCount);
                readAllCount += count;
                if (count == 0)
                {
                    throw new IOException("Can't read to readSize");
                }
            }

            return buffer;
        }

        /// <summary>
        /// 读取指定字节数至指定缓冲区
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readSize"></param>
        /// <param name="buffer"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static int Receive(Stream stream, byte[] buffer, int readSize)
        {
            var count = 0;
            var readCount = 0;
            var readAllCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;

                count = stream.Read(buffer, readAllCount, readCount);
                readAllCount += count;
                if (count == 0)
                {
                    throw new IOException("Can't read to readSize");
                    //return readAllCount;
                }
            }

            return readAllCount;
        }

        ///// <summary>
        ///// 从头读取全部数据至缓冲区，不受流索引位置影响，不改变当前索引
        ///// </summary>
        ///// <param name="stream">支持索引位置调节的流</param>
        //public static byte[] ReceiveAll(Stream stream)
        //{
        //    int readSize = (int)stream.Position;
        //    int readOffset = 0;
        //    int bufferOffset = 0;
        //    byte[] buffer = new byte[readSize];
        //    //
        //    Adf.StreamHelper.ReceivePostition(stream, readOffset, readSize, buffer, bufferOffset);
        //    //
        //    return buffer;
        //}

        ///// <summary>
        ///// 从头读取全部数据至缓冲区，不受流索引位置影响，不改变当前索引
        ///// </summary>
        ///// <param name="stream">支持索引位置调节的流</param>
        ///// <param name="buffer"></param>
        ///// <param name="bufferOffset"></param>
        //public static void ReceiveAll(Stream stream, byte[] buffer, int bufferOffset)
        //{
        //    int readSize = (int)stream.Position;
        //    int readOffset = 0;
        //    //
        //    Adf.StreamHelper.ReceivePostition(stream, readOffset, readSize, buffer, bufferOffset);
        //}

        /// <summary>
        /// 从流的指定位置处读取，不受流索引位置影响，不改变当前索引
        /// </summary>
        /// <param name="stream">支持索引位置调节的流</param>
        /// <param name="streamOffset"></param>
        /// <param name="readSize"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static void ReceivePostition(Stream stream, int streamOffset, int readSize, byte[] buffer, int bufferOffset)
        {
            long position = stream.Position;
            //diff
            if (position != streamOffset)
            {
                stream.Position = streamOffset;
            }
            //read
            Adf.StreamHelper.Receive(stream, readSize, buffer, bufferOffset);
            //reset
            stream.Position = position;
        }

        /// <summary>
        ///  读取指定字节数填充至缓冲区指定位置
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readSize"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static byte[] Receive(Stream stream, int readSize, byte[] buffer, int offset)
        {
            if (buffer.Length - offset < readSize)
                throw new ArgumentOutOfRangeException("readSize", "buffer size is too small");

            var count = 0;
            var readCount = 0;
            var readAllCount = 0;

            while (readAllCount != readSize)
            {
                readCount = readSize - readAllCount;

                count = stream.Read(buffer, offset + readAllCount, readCount);
                if (count == 0)
                {
                    //throw new SocketReadZeroException();
                    //throw new SocketException((int)SocketError.Shutdown);
                    //throw new IOException("io has been closed");
                    throw new IOException("Can't read to readSize");
                    //break;
                }
                readAllCount += count;
            }

            //if (readAllCount != readCount)
            //{
            //    throw new IOException("Can't read to readSize");
            //}

            return buffer;
        }

        /// <summary>
        /// reads a line
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns>String that was read in</returns>
        public static string ReadLine(Stream stream, Encoding encoding)
        {
            var arr = ReadLine(stream);
            if (arr.Count == 0)
                return string.Empty;
            return encoding.GetString(arr.Array, 0, arr.Count);
        }

        /// <summary>
        /// reads a line
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>String that was read in</returns>
        public static ArraySegment<byte> ReadLine(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
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
                while (stream.Read(b, 0, 1) != 0)
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
                        if (stream.Read(b, 0, 1) == 0)
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
                    //throw new IOException("closing dead stream");
                    return new ArraySegment<byte>(new byte[0], 0, 0);
                }

                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }


        /// <summary>
        /// read segment for \0
        /// </summary>
        /// <param name="input"></param>
        /// <returns>String that was read in</returns>
        public static ArraySegment<byte> ReadSegment(Stream input)
        {
            using (MemoryStream memoryStream = new MemoryStream(64))
            {
                ReadSegment(memoryStream, input);

                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        /// <summary>
        /// read segment for \0
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns>String that was read in</returns>
        public static void ReadSegment(Stream output, Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            int c = 0;

            while ((c = input.ReadByte()) != -1)
            {
                if (c == '\0')
                    break;

                output.WriteByte((byte)c);
            }
        }

        /// <summary>
        /// reads a line
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferPosition"></param>
        /// <returns>String that was read in</returns>
        public static void ReadLine(Stream stream, byte[] buffer, ref int bufferPosition)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            //
            byte[] b = new byte[1];
            while (stream.Read(b, 0, 1) != 0)
            {
                if (b[0] == 13)
                {
                    if (stream.Read(b, 0, 1) == 0)
                        break;

                    if (b[0] == 10)
                        break;

                    buffer[bufferPosition] = 13;
                    buffer[bufferPosition + 1] = b[0];
                    bufferPosition += 2;

                    //memoryStream.WriteByte(13);
                    //memoryStream.WriteByte(b[0]);

                    continue;
                }

                if (b[0] == 10)
                    break;

                buffer[bufferPosition] = b[0];
                bufferPosition += 1;

                // cast byte into char array
                //memoryStream.WriteByte(b[0]);
            }
        }

        /// <summary>
        /// 尝试关闭一个流
        /// </summary>
        /// <param name="stream"></param>
        public static void TryClose(Stream stream)
        {
            if (stream != null)
            {
                try { stream.Close(); }
                catch { }
                try { stream.Dispose(); }
                catch { }
            }
        }

        /// <summary>
        /// 异步读取指定字节数填充至缓冲区指定位置
        /// </summary>
        /// <param name="srs"></param>
        /// <exception cref="ArgumentOutOfRangeException">read length or read size error.read size not allow zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public static void Receive(StreamReadState srs)
        {
            if (srs.ReadLength >= srs.ReadSize)
            {
                throw new ArgumentOutOfRangeException("srs", "read length or read size error.");
            }

            if (srs.ReadSize == 0)
            {
                throw new ArgumentOutOfRangeException("srs", "read size not allow zero.");
            }

            if (srs.ReadSize > srs.BufferSize)
            {
                throw new ArgumentOutOfRangeException("srs", "read size not allow than buffer size.");
            }

            srs.Stream.BeginRead(srs.Buffer
                , srs.ReadLength
                , srs.ReadSize - srs.ReadLength
                , ReadCallback, srs);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            var srs = ar.AsyncState as StreamReadState;
            if (srs == null)
                return;
            //
            int read = 0;
            //
            try
            {
                read = srs.Stream.EndRead(ar);
            }
            catch (Exception exception)
            {
                srs.Success = false;
                srs.Exception = exception;
                srs.Callback(srs);
                return;
            }

            if (read == 0)
            {
                srs.Success = false;
                srs.Exception = new IOException("Stream is closed.");
                srs.Callback(srs);
                return;
            }

            srs.ReadLength += read;
            if (srs.ReadLength == srs.ReadSize)
            {
                srs.Success = true;
                srs.Exception = null;
                srs.Callback(srs);
                return;
            }

            try
            {
                srs.Stream.BeginRead(srs.Buffer
                    , srs.ReadLength
                    , srs.ReadSize - srs.ReadLength
                    , ReadCallback, srs);
            }
            catch (Exception exception)
            {
                srs.Success = false;
                srs.Exception = exception;
                srs.Callback(srs);
            }
        }
    }
}

