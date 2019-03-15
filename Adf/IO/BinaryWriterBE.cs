using System;
using System.Text;
using System.IO;

namespace Adf.IO
{
    /// <summary>
    /// 字节写书器(Big Endian)
    /// </summary>
    public class BinaryWriterBE : IBinaryWriter, IDisposable
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
        
        public BinaryWriterBE(Stream output)
        {
            this.stream = output;
            this.encoding = new UTF8Encoding();
        }

        public BinaryWriterBE(Stream output, Encoding encoding)
        {
            this.stream = output;
            this.encoding = encoding;
        }

        /// <summary>
        /// 清理资源（不会清理初始传入的流）
        /// </summary>
        public virtual void Dispose()
        {
            this._buffer = null;
            this.encoding = null;
        }

        public virtual void WriteBoolean(bool value)
        {
            if (value)
                this.stream.WriteByte(1);
            else
                this.stream.WriteByte(0);
        }

        public virtual void WriteInt16(short value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 2);
        }

        public virtual void WriteUInt16(ushort value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 2);
        }

        public virtual void WriteInt32(int value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 4);
        }

        public virtual void WriteUInt32(uint value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 4);
        }

        public virtual void WriteInt64(long value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 8);
        }

        public virtual void WriteUInt64(ulong value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 8);
        }

        public virtual void WriteSingle(float value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 4);
        }

        public virtual void WriteDouble(double value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 8);
        }

        public virtual void WriteDecimal(decimal value)
        {
            Adf.BaseDataConverter.ToBytes(value, _buffer, 0);

            this.stream.Write(_buffer, 0, 16);
        }

        public virtual void WriteStringEX(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            byte[] buf = this.encoding.GetBytes(value);
            //
            this.WriteUInt16((UInt16)buf.Length);
            this.stream.Write(buf, 0, buf.Length);
        }

        public virtual void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            byte[] buf = this.encoding.GetBytes(value);

            this.stream.Write(buf, 0, buf.Length);
        }

        public virtual void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        public virtual void Write(byte[] value)
        {
            this.stream.Write(value, 0, value.Length);
        }

        public virtual void Write(byte[] buffer, int offset, int length)
        {
            this.stream.Write(buffer, offset, length);
        }
    }
}
