using System;
using System.Text;
using System.IO;

namespace Adf.IO
{
    /// <summary>
    /// 字节读取器(Little Endian)
    /// </summary>
    /// <exception cref="IOException">全系列读取方法均有可能引发此异常。</exception>
    public class BinaryReaderLE : IBinaryReader, IDisposable
    {
        byte[] _buffer = new byte[16];

        Stream stream = null;
        /// <summary>
        /// get stream
        /// </summary>
        public Stream Stream
        {
            get { return this.stream; }
        }

        Encoding encoding = null;
        /// <summary>
        /// get encoding
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
        }

        public BinaryReaderLE(Stream input)
        {
            this.stream = input;
            this.encoding = new UTF8Encoding();
        }

        public BinaryReaderLE(Stream input, Encoding encoding)
        {
            this.stream = input;
            this.encoding = encoding;
        }

        public virtual bool ReadBoolean()
        {
            StreamHelper.Receive(this.stream, this._buffer, 1);

            return this._buffer[0] == 1;
        }

        public virtual short ReadInt16()
        {
            StreamHelper.Receive(this.stream, this._buffer, 2);

            return Adf.BaseDataConverter.ToInt16LE(this._buffer, 0);
        }

        public virtual ushort ReadUInt16()
        {
            StreamHelper.Receive(this.stream, this._buffer, 2);

            return Adf.BaseDataConverter.ToUInt16LE(this._buffer, 0);
        }

        public virtual int ReadInt32()
        {
            StreamHelper.Receive(this.stream, this._buffer, 4);

            return Adf.BaseDataConverter.ToInt32LE(this._buffer, 0);
        }

        public virtual uint ReadUInt32()
        {
            StreamHelper.Receive(this.stream, this._buffer, 4);

            return Adf.BaseDataConverter.ToUInt32LE(this._buffer, 0);
        }
        
        public virtual long ReadInt64()
        {
            StreamHelper.Receive(this.stream, this._buffer, 8);

            return Adf.BaseDataConverter.ToInt64LE(this._buffer, 0);
        }
        
        public virtual ulong ReadUInt64()
        {
            StreamHelper.Receive(this.stream, this._buffer, 8);

            return Adf.BaseDataConverter.ToUInt64LE(this._buffer, 0);
        }
        
        public virtual float ReadSingle()
        {
            StreamHelper.Receive(this.stream, this._buffer, 4);

            return Adf.BaseDataConverter.ToSingleLE(this._buffer, 0);
        }
        
        public virtual double ReadDouble()
        {
            StreamHelper.Receive(this.stream, this._buffer, 8);

            return Adf.BaseDataConverter.ToDoubleLE(this._buffer, 0);
        }
        
        public virtual decimal ReadDecimal()
        {
            StreamHelper.Receive(this.stream, this._buffer, 16);

            return Adf.BaseDataConverter.ToDecimalLE(this._buffer, 0);
        }

        public virtual String ReadString(int length)
        {
            byte[] buf = new byte[length];
            StreamHelper.Receive(this.stream, buf, length);

            return this.encoding.GetString(buf, 0, length);
        }

        public virtual String ReadStringEX()
        {
            int len = this.ReadUInt16();
            return this.ReadString(len);
        }

        public virtual byte ReadByte()
        {
            int b = this.stream.ReadByte();
            if (b == -1)
                throw new IOException("Can't read to readSize");

            return (byte)b;
        }

        public virtual byte[] ReadBytes(int length)
        {
            byte[] buf = new byte[length];
            StreamHelper.Receive(this.stream, buf, length);
            return buf;
        }

        public virtual void Read(byte[] buffer)
        {
            StreamHelper.Receive(this.stream, buffer, buffer.Length);
        }

        public virtual void Read(byte[] buffer, int offset, int length)
        {
            StreamHelper.Receive(this.stream, length, buffer, offset);
        }

        /// <summary>
        /// 清理资源（不会清理初始传入的流）
        /// </summary>
        public virtual void Dispose()
        {
            this._buffer = null;
            this.encoding = null;
        }

    }
}
