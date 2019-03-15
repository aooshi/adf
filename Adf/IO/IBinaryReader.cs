using System;

namespace Adf.IO
{
    public interface IBinaryReader : IDisposable
    {
        bool ReadBoolean();

        short ReadInt16();

        ushort ReadUInt16();

        int ReadInt32();

        uint ReadUInt32();

        long ReadInt64();

        ulong ReadUInt64();

        float ReadSingle();

        double ReadDouble();

        decimal ReadDecimal();

        String ReadString(int length);

        /// <summary>
        /// 读取一个字节
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// 读取一个字节组
        /// </summary>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        byte[] ReadBytes(int length);

        /// <summary>
        /// 读取内容至缓冲区，数量为缓冲区长度
        /// </summary>
        /// <param name="buffer"></param>
        void Read(byte[] buffer);

        /// <summary>
        /// 读取内容至缓冲区，指定读取长度
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Read(byte[] buffer, int offset, int length);
    }
}
