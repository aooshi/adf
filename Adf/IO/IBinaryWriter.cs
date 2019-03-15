using System;

namespace Adf.IO
{
    public interface IBinaryWriter : IDisposable
    {
        void WriteBoolean(bool value);

        void WriteInt16(short value);
        
        void WriteUInt16(ushort value);
        
        void WriteInt32(int value);
        
        void WriteUInt32(uint value);
        
        void WriteInt64(long value);
        
        void WriteUInt64(ulong value);
        
        void WriteSingle(float value);
        
        void WriteDouble(double  value);
        
        void WriteDecimal(decimal value);
        
        void WriteString(String value);

        void WriteByte(byte value);
                
        /// <summary>
        /// 写入一组字节内容
        /// </summary>
        /// <param name="value"></param>
        void Write(byte[] value);

        /// <summary>
        /// 从字节组中指定位置处写入指定长度的字节内容
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Write(byte[] buffer, int offset, int length);
    }
}
