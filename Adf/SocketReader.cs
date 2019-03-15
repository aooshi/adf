using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Adf
{
    /// <summary>
    /// Socket 读取器
    /// </summary>
    public class SocketReader
    {
        int maxLength;

        /// <summary>
        /// get max read length
        /// </summary>
        public int MaxLength
        {
            get { return this.maxLength; }
        }

        Socket socket;

        /// <summary>
        /// get current socket
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
        }

        Encoding encoding;

        /// <summary>
        /// get encoding
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
        }

        int readAllLength = 0;
        /// <summary>
        /// get all read length
        /// </summary>
        public virtual int ReadAllLength
        {
            get { return this.readAllLength; }
        }

        //
        int readBufferCount = 0;
        byte[] readBuffer = new byte[1];
        //
        int lineBufferPosition = 0;
        int lineBufferCount = 0;
        byte[] lineBuffer = new byte[512];

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="encoding"></param>
        /// <param name="maxLength"></param>
        public SocketReader(Socket socket, Encoding encoding, int maxLength)
        {
            this.encoding = encoding;
            this.socket = socket;
            this.maxLength = maxLength;
        }

        /// <summary>
        /// read a byte from Socket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="SocketException">remote host has been closed</exception>
        /// <exception cref="InvalidOperationException">exceed the permitted maximum length</exception>
        public virtual byte Read()
        {
            if (this.readAllLength > this.maxLength)
            {
                throw new InvalidOperationException("exceed the permitted maximum length");
            }

            //read from socket
            this.readBufferCount = this.socket.Receive(this.readBuffer, 0, 1, SocketFlags.None);
            if (this.readBufferCount == 0)
            {
                throw new SocketException((int)SocketError.Shutdown);
            }

            this.readAllLength++;
            return this.readBuffer[0];
        }

        /// <summary>
        /// read a bytes for a line
        /// </summary>
        /// <exception cref="SocketException">remote host has been closed</exception>
        /// <exception cref="InvalidOperationException">request entity exceed the permitted maximum length</exception>
        /// <returns></returns>
        public byte[] ReadByteLine()
        {
            this.lineBufferPosition = 0;
            //
            byte b = 0;
            while (true)
            {
                b = this.Read();
                if (b == 13)
                {
                    b = this.Read();
                    if (b == 10)
                    {
                        if (this.lineBufferPosition == 0)
                            return new byte[0];

                        var buffer3 = new byte[this.lineBufferPosition];
                        Array.Copy(this.lineBuffer, 0, buffer3, 0, this.lineBufferPosition);
                        return buffer3;
                    }

                    this.SetLineBuffer(13);
                    this.SetLineBuffer(b);
                    continue;
                }
                else if (b == 10)
                {
                    if (this.lineBufferPosition == 0)
                        return new byte[0];

                    var buffer3 = new byte[this.lineBufferPosition];
                    Array.Copy(this.lineBuffer, 0, buffer3, 0, this.lineBufferPosition);
                    return buffer3;
                }

                this.SetLineBuffer(b);
            }
        }

        /// <summary>
        /// read a string for a line
        /// </summary>
        /// <exception cref="SocketException">remote host has been closed</exception>
        /// <exception cref="InvalidOperationException">request entity exceed the permitted maximum length</exception>
        /// <returns></returns>
        public string ReadStringLine()
        {
            this.lineBufferPosition = 0;
            //
            byte b = 0;
            while (true)
            {
                b = this.Read();
                if (b == 13)
                {
                    b = this.Read();
                    if (b == 10)
                    {
                        if (this.lineBufferPosition == 0)
                            return "";

                        return this.encoding.GetString(this.lineBuffer, 0, this.lineBufferPosition);
                    }

                    this.SetLineBuffer(13);
                    this.SetLineBuffer(b);
                    continue;
                }
                else if (b == 10)
                {
                    if (this.lineBufferPosition == 0)
                        return "";

                    return this.encoding.GetString(this.lineBuffer, 0, this.lineBufferPosition);
                }

                this.SetLineBuffer(b);
            }
        }

        private void SetLineBuffer(byte value)
        {
            if (this.lineBufferPosition != 0 && this.lineBufferPosition == this.lineBufferCount)
            {
                this.lineBufferCount *= 2;
                var buffer = new byte[this.lineBufferCount];
                Array.Copy(this.lineBuffer, 0, buffer, 0, this.lineBuffer.Length);
                this.lineBuffer = buffer;
            }

            this.lineBuffer[this.lineBufferPosition++] = value;
        }
    }
}