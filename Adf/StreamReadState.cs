using System;
using System.IO;

namespace Adf
{
    /// <summary>
    /// stream read state
    /// </summary>
    public class StreamReadState
    {
        /// <summary>
        /// get reader bufer
        /// </summary>
        public readonly byte[] Buffer;
        /// <summary>
        /// get buffer size
        /// </summary>
        public readonly int BufferSize;
        /// <summary>
        /// base stream;
        /// </summary>
        public readonly Stream Stream;
        /// <summary>
        /// read completed callback
        /// </summary>
        public readonly Action<StreamReadState> Callback;

        /// <summary>
        /// completed read length
        /// </summary>
        public int ReadLength;
        /// <summary>
        /// wait read size
        /// </summary>
        public int ReadSize = 0;
        ///// <summary>
        ///// index position
        ///// </summary>
        //public int Position = 0;

        /// <summary>
        /// Indicates whether the execution succeeded
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// user state
        /// </summary>
        public object UserState;

        /// <summary>
        /// failure exception, success is null
        /// </summary>
        public Exception Exception;


        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="callback"></param>
        public StreamReadState(Stream stream, byte[] buffer, Action<StreamReadState> callback)
        {
            this.Buffer = buffer;
            this.Stream = stream;
            this.Callback = callback;
            this.BufferSize = buffer.Length;
            this.ReadLength = 0;
            this.ReadSize = this.BufferSize;
        }

        /// <summary>
        /// reset variable, and read length zero, and set read size to buffer size
        /// </summary>
        public void Reset()
        {
            this.ReadLength = 0;
            this.ReadSize = this.BufferSize;
            this.Success = false;
            this.UserState = null;
            this.Exception = null;
        }

        /// <summary>
        /// reset variable, and read length zero.
        /// </summary>
        /// <param name="readSize">set wait read size</param>
        public void Reset(int readSize)
        {
            this.ReadLength = 0;
            this.ReadSize = readSize;
            this.Success = false;
            this.UserState = null;
            this.Exception = null;
        }

        /// <summary>
        /// reset variable
        /// </summary>
        /// <param name="readLength">set completed read length</param>
        /// <param name="readSize">set wait read size</param>
        public void Reset(int readLength, int readSize)
        {
            this.ReadLength = readLength;
            this.ReadSize = readSize;
            this.Success = false;
            this.UserState = null;
            this.Exception = null;
        }
    }
}
