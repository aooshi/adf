using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// WebSocket  DataFrame  Header
    /// 数据头
    /// </summary>
    public class WebSocketDataFrameHeader
    {
        private bool _fin;
        private bool _rsv1;
        private bool _rsv2;
        private bool _rsv3;
        private sbyte _opcode;
        private bool _maskcode;
        private sbyte _payloadlength;

        public bool FIN { get { return _fin; } }

        public bool RSV1 { get { return _rsv1; } }

        public bool RSV2 { get { return _rsv2; } }

        public bool RSV3 { get { return _rsv3; } }

        public sbyte OpCode { get { return _opcode; } }

        public bool HasMask { get { return _maskcode; } }

        public sbyte Length { get { return _payloadlength; } }

        /// <summary>
        /// 根据字节组初始头
        /// </summary>
        /// <param name="buffer"></param>
        public WebSocketDataFrameHeader(byte[] buffer)
        {
            //第一个字节
            _fin = (buffer[0] & 0x80) == 0x80;
            _rsv1 = (buffer[0] & 0x40) == 0x40;
            _rsv2 = (buffer[0] & 0x20) == 0x20;
            _rsv3 = (buffer[0] & 0x10) == 0x10;
            _opcode = (sbyte)(buffer[0] & 0x0f);

            //第二个字节
            _maskcode = (buffer[1] & 0x80) == 0x80;
            _payloadlength = (sbyte)(buffer[1] & 0x7f);

        }

        /// <summary>
        /// 初始并封装一个数据头
        /// </summary>
        /// <param name="fin"></param>
        /// <param name="rsv1"></param>
        /// <param name="rsv2"></param>
        /// <param name="rsv3"></param>
        /// <param name="opcode"></param>
        /// <param name="hasmask"></param>
        /// <param name="length"></param>
        public WebSocketDataFrameHeader(bool fin, bool rsv1, bool rsv2, bool rsv3, sbyte opcode, bool hasmask, int length)
        {
            _fin = fin;
            _rsv1 = rsv1;
            _rsv2 = rsv2;
            _rsv3 = rsv3;
            _opcode = opcode;
            //第二个字节
            _maskcode = hasmask;
            _payloadlength = (sbyte)length;
        }

        /// <summary>
        /// 返回当前帧字节
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[2] { 0, 0 };

            if (_fin) buffer[0] ^= 0x80;
            if (_rsv1) buffer[0] ^= 0x40;
            if (_rsv2) buffer[0] ^= 0x20;
            if (_rsv3) buffer[0] ^= 0x10;

            buffer[0] ^= (byte)_opcode;

            if (_maskcode) buffer[1] ^= 0x80;

            buffer[1] ^= (byte)_payloadlength;

            return buffer;
        }
    }
}
