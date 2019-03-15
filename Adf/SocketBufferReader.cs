using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Adf
{
    /// <summary>
    /// Socket Buffer Reader
    /// </summary>
    public class SocketBufferReader : SocketReader
    {
        int bufferSize = 0;
        int bufferCount = 0;
        int bufferPosition = 0;
        byte[] buffer;
        
        Socket socket;
        int maxLength;

        int readAllLength = 0;
        /// <summary>
        /// get all read length
        /// </summary>
        public override int ReadAllLength
        {
            get { return this.readAllLength; }
        }
                
        /// <summary>
        /// new instance
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="encoding"></param>
        /// <param name="maxLength"></param>
        public SocketBufferReader(Socket socket, Encoding encoding, int maxLength)
            : base(socket,encoding,maxLength)
        {
            this.bufferSize = socket.ReceiveBufferSize;
            this.buffer = new byte[ this.bufferSize ];
            this.socket = socket;
            this.maxLength = maxLength;
        }

        private void ReceiveBuffer()
        {
            //read from socket
            this.bufferCount = this.socket.Receive(this.buffer, 0, this.bufferSize, SocketFlags.None);
            if (this.bufferCount == 0)
            {
                //throw new IOException("remote host has been closed");
                throw new SocketException((int)SocketError.Shutdown);
            }

            this.readAllLength += this.bufferCount;
            this.bufferPosition = 0;
            //
            if (this.readAllLength > this.maxLength)
            {
                throw new InvalidOperationException("exceed the permitted maximum length");
            }
        }

        /// <summary>
        /// read a byte from Socket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="SocketException">remote host has been closed</exception>
        /// <exception cref="InvalidOperationException">exceed the permitted maximum length</exception>
        public override byte Read()
        {
            if (this.bufferPosition == this.bufferCount)
            {
                this.ReceiveBuffer();
            }
            return this.buffer[this.bufferPosition++];
        }
    }
}