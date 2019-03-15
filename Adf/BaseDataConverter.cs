using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Adf
{
    /// <summary>
    /// 基础数据类型与字节数组相互转换工具, 此转换工具中数值转换序列以Big Endian方式进行
    /// </summary>
    public static class BaseDataConverter
    {
        /// <summary>
        /// Int64 to Bytes , Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Int64 value)
        {
            byte[] bytes = new byte[8];
            ToBytes(value, bytes, 0);
            return bytes;
        }
        /// <summary>
        /// Int64 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(Int64 value)
        {
            byte[] bytes = new byte[8];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int64 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Int64 value, byte[] bytes, int offset)
        {
            bytes[0 + offset] = (byte)(value >> 0x38);//56
            bytes[1 + offset] = (byte)(value >> 0x30);//48
            bytes[2 + offset] = (byte)(value >> 0x28);//40
            bytes[3 + offset] = (byte)(value >> 0x20);//32
            bytes[4 + offset] = (byte)(value >> 0x18);//24
            bytes[5 + offset] = (byte)(value >> 0x10);//16
            bytes[6 + offset] = (byte)(value >> 8);
            bytes[7 + offset] = (byte)(value);
        }

        /// <summary>
        /// Int64 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(Int64 value, byte[] bytes, int offset)
        {
            bytes[0 + offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
            bytes[2 + offset] = (byte)(value >> 0x10);//16
            bytes[3 + offset] = (byte)(value >> 0x18);//24
            bytes[4 + offset] = (byte)(value >> 0x20);//32
            bytes[5 + offset] = (byte)(value >> 0x28);//40
            bytes[6 + offset] = (byte)(value >> 0x30);//48
            bytes[7 + offset] = (byte)(value >> 0x38);//56
        }

        /// <summary>
        /// To Int64 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Int64 ToInt64(byte[] array)
        {
            return ToInt64(array, 0);
        }

        /// <summary>
        /// To Int64
        /// </summary>
        /// <param name="array"></param>
        public static Int64 ToInt64LE(byte[] array)
        {
            return ToInt64LE(array, 0);
        }

        /// <summary>
        /// To Int64 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int64 ToInt64(byte[] array, int offset)
        {
            return (Int64)((Int64)array[offset] << 0x38
                    | (Int64)array[1 + offset] << 0x30
                    | (Int64)array[2 + offset] << 0x28
                    | (Int64)array[3 + offset] << 0x20
                    | (Int64)array[4 + offset] << 0x18
                    | (Int64)array[5 + offset] << 0x10
                    | (Int64)array[6 + offset] << 8
                    | array[7 + offset]);
        }
        
        /// <summary>
        /// To Int64
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int64 ToInt64LE(byte[] array, int offset)
        {
            return (Int64)(
                    array[offset]
                    | (Int64)array[1 + offset] << 8
                    | (Int64)array[2 + offset] << 0x10
                    | (Int64)array[3 + offset] << 0x18
                    | (Int64)array[4 + offset] << 0x20
                    | (Int64)array[5 + offset] << 0x28
                    | (Int64)array[6 + offset] << 0x30
                    | (Int64)array[7 + offset] << 0x38
                    );
        }


        /// <summary>
        /// Int64 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(UInt64 value)
        {
            byte[] bytes = new byte[8];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int64 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(UInt64 value)
        {
            byte[] bytes = new byte[8];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int64 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(UInt64 value, byte[] bytes, int offset)
        {
            bytes[0 + offset] = (byte)(value >> 0x38);//56
            bytes[1 + offset] = (byte)(value >> 0x30);//48
            bytes[2 + offset] = (byte)(value >> 0x28);//40
            bytes[3 + offset] = (byte)(value >> 0x20);//32
            bytes[4 + offset] = (byte)(value >> 0x18);//24
            bytes[5 + offset] = (byte)(value >> 0x10);//16
            bytes[6 + offset] = (byte)(value >> 8);
            bytes[7 + offset] = (byte)(value);
        }

        /// <summary>
        /// Int64 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(UInt64 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
            bytes[2 + offset] = (byte)(value >> 0x10);//16
            bytes[3 + offset] = (byte)(value >> 0x18);//24
            bytes[4 + offset] = (byte)(value >> 0x20);//32
            bytes[5 + offset] = (byte)(value >> 0x28);//40
            bytes[6 + offset] = (byte)(value >> 0x30);//48
            bytes[7 + offset] = (byte)(value >> 0x38);//56
        }

        /// <summary>
        /// To UInt64 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static UInt64 ToUInt64(byte[] array)
        {
            return ToUInt64(array, 0);
        }

        /// <summary>
        /// To UInt64
        /// </summary>
        /// <param name="array"></param>
        public static UInt64 ToUInt64LE(byte[] array)
        {
            return ToUInt64LE(array, 0);
        }

        /// <summary>
        /// To UInt64 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt64 ToUInt64(byte[] array, int offset)
        {
            return (UInt64)((UInt64)array[0 + offset] << 0x38
                    | (UInt64)array[1 + offset] << 0x30
                    | (UInt64)array[2 + offset] << 0x28
                    | (UInt64)array[3 + offset] << 0x20
                    | (UInt64)array[4 + offset] << 0x18
                    | (UInt64)array[5 + offset] << 0x10
                    | (UInt64)array[6 + offset] << 8
                    | array[7 + offset]);
        }

        /// <summary>
        /// To UInt64
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt64 ToUInt64LE(byte[] array, int offset)
        {
            return (UInt64)(
                     array[offset]
                    | (UInt64)array[1 + offset] << 8
                    | (UInt64)array[2 + offset] << 0x10
                    | (UInt64)array[3 + offset] << 0x18
                    | (UInt64)array[4 + offset] << 0x20
                    | (UInt64)array[5 + offset] << 0x28
                    | (UInt64)array[6 + offset] << 0x30
                    | (UInt64)array[7 + offset] << 0x38
                    );
        }

        /// <summary>
        /// Int32 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Int32 value)
        {
            byte[] bytes = new byte[4];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int32 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(Int32 value)
        {
            byte[] bytes = new byte[4];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int32 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Int32 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value >> 0x18);
            bytes[1 + offset] = (byte)(value >> 0x10);
            bytes[2 + offset] = (byte)(value >> 8);
            bytes[3 + offset] = (byte)(value);
        }

        /// <summary>
        /// Int32 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(Int32 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
            bytes[2 + offset] = (byte)(value >> 0x10);
            bytes[3 + offset] = (byte)(value >> 0x18);
        }

        /// <summary>
        /// To Int32 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Int32 ToInt32(byte[] array)
        {
            return ToInt32(array, 0);
        }

        /// <summary>
        /// To Int32
        /// </summary>
        /// <param name="array"></param>
        public static Int32 ToInt32LE(byte[] array)
        {
            return ToInt32LE(array, 0);
        }

        /// <summary>
        /// To Int32 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int32 ToInt32(byte[] array, int offset)
        {
            return (Int32)((Int32)array[0 + offset] << 0x18
                    | (Int32)array[1 + offset] << 0x10
                    | (Int32)array[2 + offset] << 8
                    | array[3 + offset]);
        }

        /// <summary>
        /// To Int32
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int32 ToInt32LE(byte[] array, int offset)
        {
            return (Int32)(
                     array[offset]
                    | (Int32)array[1 + offset] << 8
                    | (Int32)array[2 + offset] << 0x10
                | (Int32)array[3 + offset] << 0x18
            );
        }

        /// <summary>
        /// UInt32 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(UInt32 value)
        {
            byte[] bytes = new byte[4];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// UInt32 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(UInt32 value)
        {
            byte[] bytes = new byte[4];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// UInt32 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(UInt32 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value >> 0x18);
            bytes[1 + offset] = (byte)(value >> 0x10);
            bytes[2 + offset] = (byte)(value >> 8);
            bytes[3 + offset] = (byte)(value);
        }

        /// <summary>
        /// UInt32 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(UInt32 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
            bytes[2 + offset] = (byte)(value >> 0x10);
            bytes[3 + offset] = (byte)(value >> 0x18);
        }

        /// <summary>
        /// To UInt32 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static UInt32 ToUInt32(byte[] array)
        {
            return ToUInt32(array, 0);
        }

        /// <summary>
        /// To UInt32
        /// </summary>
        /// <param name="array"></param>
        public static UInt32 ToUInt32LE(byte[] array)
        {
            return ToUInt32LE(array, 0);
        }

        /// <summary>
        /// To UInt32 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt32 ToUInt32(byte[] array, int offset)
        {
            return (UInt32)((UInt32)array[0 + offset] << 0x18
                    | (UInt32)array[1 + offset] << 0x10
                    | (UInt32)array[2 + offset] << 8
                    | array[3 + offset]);
        }

        /// <summary>
        /// To UInt32
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt32 ToUInt32LE(byte[] array, int offset)
        {
            return (UInt32)(
                     array[offset]
                    | (UInt32)array[1 + offset] << 8
                    | (UInt32)array[2 + offset] << 0x10
                | (UInt32)array[3 + offset] << 0x18
            );
        }

        /// <summary>
        /// Int16 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Int16 value)
        {
            byte[] bytes = new byte[2];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int16 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(Int16 value)
        {
            byte[] bytes = new byte[2];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Int16 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Int16 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value >> 8);
            bytes[1 + offset] = (byte)(value);
        }

        /// <summary>
        /// Int16 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(Int16 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
        }

        /// <summary>
        /// To Int16 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Int16 ToInt16(byte[] array)
        {
            return ToInt16(array, 0);
        }

        /// <summary>
        /// To Int16
        /// </summary>
        /// <param name="array"></param>
        public static Int16 ToInt16LE(byte[] array)
        {
            return ToInt16LE(array, 0);
        }

        /// <summary>
        /// To Int16 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int16 ToInt16(byte[] array, int offset)
        {
            return (Int16)((Int16)array[offset] << 8 | array[1 + offset]);
        }
        
        /// <summary>
        /// To Int16
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Int16 ToInt16LE(byte[] array, int offset)
        {
            return (Int16)(array[offset] | (Int16)array[1 + offset] << 8);
        }

        /// <summary>
        /// UInt16 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(UInt16 value)
        {
            byte[] bytes = new byte[2];
            ToBytes(value);
            return bytes;
        }

        /// <summary>
        /// UInt16 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(UInt16 value)
        {
            byte[] bytes = new byte[2];
            ToBytesLE(value);
            return bytes;
        }

        /// <summary>
        /// UInt16 to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(UInt16 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value >> 8);
            bytes[1 + offset] = (byte)(value);
        }

        /// <summary>
        /// UInt16 to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(UInt16 value, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(value);
            bytes[1 + offset] = (byte)(value >> 8);
        }

        /// <summary>
        /// To UInt16 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static UInt16 ToUInt16(byte[] array)
        {
            return ToUInt16(array, 0);
        }

        /// <summary>
        /// To UInt16
        /// </summary>
        /// <param name="array"></param>
        public static UInt16 ToUInt16LE(byte[] array)
        {
            return ToUInt16LE(array, 0);
        }

        /// <summary>
        /// To UInt16 ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt16 ToUInt16(byte[] array, int offset)
        {
            return (UInt16)((UInt16)array[offset] << 8 | array[1 + offset]);
        }

        /// <summary>
        /// To UInt16
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static UInt16 ToUInt16LE(byte[] array, int offset)
        {
            return (UInt16)(array[offset] | (UInt16)array[1 + offset] << 8);
        }

        /// <summary>
        /// Char to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Char value)
        {
            return ToBytes((Int16)value);
        }
        
        /// <summary>
        /// Char to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Char value, byte[] bytes, int offset)
        {
            ToBytes((Int16)value, bytes, 0);
        }

        /// <summary>
        /// To Char ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Char ToChar(byte[] array)
        {
            return ToChar(array, 0);
        }

        /// <summary>
        /// To Char
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Char ToChar(byte[] array, int offset)
        {
            return (Char)ToInt16(array, offset);
        }

        /// <summary>
        /// Double to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Double value)
        {
            byte[] bytes = new byte[8];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Double to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(Double value)
        {
            byte[] bytes = new byte[8];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Double to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Double value, byte[] bytes, int offset)
        {
            byte[] bytes1 = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                bytes[offset] = bytes1[7];
                bytes[1 + offset] = bytes1[6];
                bytes[2 + offset] = bytes1[5];
                bytes[3 + offset] = bytes1[4];
                bytes[4 + offset] = bytes1[3];
                bytes[5 + offset] = bytes1[2];
                bytes[6 + offset] = bytes1[1];
                bytes[7 + offset] = bytes1[0];
            }
            else
            {
                bytes[offset] = bytes1[0];
                bytes[1 + offset] = bytes1[1];
                bytes[2 + offset] = bytes1[2];
                bytes[3 + offset] = bytes1[3];
                bytes[4 + offset] = bytes1[4];
                bytes[5 + offset] = bytes1[5];
                bytes[6 + offset] = bytes1[6];
                bytes[7 + offset] = bytes1[7];
            }
        }

        /// <summary>
        /// Double to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(Double value, byte[] bytes, int offset)
        {
            byte[] bytes1 = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                bytes[offset] = bytes1[0];
                bytes[1 + offset] = bytes1[1];
                bytes[2 + offset] = bytes1[2];
                bytes[3 + offset] = bytes1[3];
                bytes[4 + offset] = bytes1[4];
                bytes[5 + offset] = bytes1[5];
                bytes[6 + offset] = bytes1[6];
                bytes[7 + offset] = bytes1[7];
            }
            else
            {
                bytes[offset] = bytes1[7];
                bytes[1 + offset] = bytes1[6];
                bytes[2 + offset] = bytes1[5];
                bytes[3 + offset] = bytes1[4];
                bytes[4 + offset] = bytes1[3];
                bytes[5 + offset] = bytes1[2];
                bytes[6 + offset] = bytes1[1];
                bytes[7 + offset] = bytes1[0];
            }
        }

        /// <summary>
        /// To Double ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Double ToDouble(byte[] array)
        {
            return ToDouble(array, 0);
        }

        /// <summary>
        /// To Double
        /// </summary>
        /// <param name="array"></param>
        public static Double ToDoubleLE(byte[] array)
        {
            return ToDoubleLE(array, 0);
        }

        /// <summary>
        /// To Double ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Double ToDouble(byte[] array, int offset)
        {
            //long v = ToInt64(array, offset);
            //return System.BitConverter.Int64BitsToDouble(v);

            if (System.BitConverter.IsLittleEndian)
            {
                var bytes2 = new byte[8];
                bytes2[0] = array[7 + offset];
                bytes2[1] = array[6 + offset];
                bytes2[2] = array[5 + offset];
                bytes2[3] = array[4 + offset];
                bytes2[4] = array[3 + offset];
                bytes2[5] = array[2 + offset];
                bytes2[6] = array[1 + offset];
                bytes2[7] = array[0 + offset];
                return System.BitConverter.ToDouble(bytes2, 0);
            }

            return System.BitConverter.ToDouble(array, offset);
        }

        /// <summary>
        /// To Double
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Double ToDoubleLE(byte[] array, int offset)
        {
            //long v = ToInt64LE(array, offset);
            //return System.BitConverter.Int64BitsToDouble(v);


            if (System.BitConverter.IsLittleEndian)
            {
                return System.BitConverter.ToDouble(array, offset);
            }

            var bytes2 = new byte[8];
            bytes2[0] = array[7 + offset];
            bytes2[1] = array[6 + offset];
            bytes2[2] = array[5 + offset];
            bytes2[3] = array[4 + offset];
            bytes2[4] = array[3 + offset];
            bytes2[5] = array[2 + offset];
            bytes2[6] = array[1 + offset];
            bytes2[7] = array[0 + offset];

            return System.BitConverter.ToDouble(bytes2, 0);
        }

        /// <summary>
        /// Float to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Single value)
        {
            byte[] bytes = new byte[4];
            ToBytes(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Float to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytesLE(Single value)
        {
            byte[] bytes = new byte[4];
            ToBytesLE(value, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Float to Bytes ,Big Endian
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytes(Single value, byte[] bytes, int offset)
        {
            byte[] bytes1 = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                bytes[offset] = bytes1[3];
                bytes[1 + offset] = bytes1[2];
                bytes[2 + offset] = bytes1[1];
                bytes[3 + offset] = bytes1[0];
            }
            else
            {
                bytes[offset] = bytes1[0];
                bytes[1 + offset] = bytes1[1];
                bytes[2 + offset] = bytes1[2];
                bytes[3 + offset] = bytes1[3];
            }
        }

        /// <summary>
        /// Float to Bytes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void ToBytesLE(Single value, byte[] bytes, int offset)
        {
            byte[] bytes1 = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                bytes[offset] = bytes1[0];
                bytes[1 + offset] = bytes1[1];
                bytes[2 + offset] = bytes1[2];
                bytes[3 + offset] = bytes1[3];
            }
            else
            {
                bytes[offset] = bytes1[3];
                bytes[1 + offset] = bytes1[2];
                bytes[2 + offset] = bytes1[1];
                bytes[3 + offset] = bytes1[0];
            }
        }

        /// <summary>
        /// To Single ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        public static Single ToSingle(byte[] array)
        {
            return ToSingle(array, 0);
        }

        /// <summary>
        /// To Single
        /// </summary>
        /// <param name="array"></param>
        public static Single ToSingleLE(byte[] array)
        {
            return ToSingleLE(array, 0);
        }

        /// <summary>
        /// To Single ,Big Endian
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Single ToSingle(byte[] array, int offset)
        {
            if (System.BitConverter.IsLittleEndian)
            {
                var bytes2 = new byte[4];
                bytes2[0] = array[3 + offset];
                bytes2[1] = array[2 + offset];
                bytes2[2] = array[1 + offset];
                bytes2[3] = array[0 + offset];
                return System.BitConverter.ToSingle(bytes2, 0);
            }

            return System.BitConverter.ToSingle(array, offset);
        }
        
        /// <summary>
        /// To Single
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public static Single ToSingleLE(byte[] array, int offset)
        {
            if (System.BitConverter.IsLittleEndian)
            {
                return System.BitConverter.ToSingle(array, offset);
            }

            var bytes2 = new byte[4];
            bytes2[0] = array[3 + offset];
            bytes2[1] = array[2 + offset];
            bytes2[2] = array[1 + offset];
            bytes2[3] = array[0 + offset];

            return System.BitConverter.ToSingle(bytes2, 0);
        }

        /// <summary>
        /// 将<c>Decimal</c>对象转换为字节数组 ,Big Endian
        /// </summary>
        /// <param name="value">要转换的对象。</param>
        /// <returns>表示<c>Decimal</c>对象的字节数组。</returns>
        public static byte[] ToBytes(Decimal value)
        {
            byte[] bytes = new byte[16];
            ToBytes(value);
            return bytes;
        }

        /// <summary>
        /// 将<c>Decimal</c>对象转换为字节数组
        /// </summary>
        /// <param name="value">要转换的对象。</param>
        /// <returns>表示<c>Decimal</c>对象的字节数组。</returns>
        public static byte[] ToBytesLE(Decimal value)
        {
            byte[] bytes = new byte[16];
            ToBytesLE(value);
            return bytes;
        }

        /// <summary>
        /// 将<c>Decimal</c>对象转换为字节数组 ,Big Endian
        /// </summary>
        /// <param name="value">要转换的对象。</param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns>表示<c>Decimal</c>对象的字节数组。</returns>
        public static void ToBytes(Decimal value, byte[] bytes, int offset)
        {
            Int32[] bits = Decimal.GetBits(value);
            if (System.BitConverter.IsLittleEndian)
            {
                //byte[] bytes = new byte[bits.Length * 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bytes[3 + i * 4 + offset] = (byte)(bits[i] >> 0x18);
                    bytes[2 + i * 4 + offset] = (byte)(bits[i] >> 0x10);
                    bytes[1 + i * 4 + offset] = (byte)(bits[i] >> 8);
                    bytes[0 + i * 4 + offset] = (byte)(bits[i]);

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));
                    //}
                }
            }
            else
            {
                //byte[] bytes = new byte[bits.Length * 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bytes[0 + i * 4 + offset] = (byte)(bits[i] >> 0x18);
                    bytes[1 + i * 4 + offset] = (byte)(bits[i] >> 0x10);
                    bytes[2 + i * 4 + offset] = (byte)(bits[i] >> 8);
                    bytes[3 + i * 4 + offset] = (byte)(bits[i]);

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));
                    //}
                }
            }
        }
        
        /// <summary>
        /// 将<c>Decimal</c>对象转换为字节数组。
        /// </summary>
        /// <param name="value">要转换的对象。</param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns>表示<c>Decimal</c>对象的字节数组。</returns>
        public static void ToBytesLE(Decimal value, byte[] bytes, int offset)
        {
            Int32[] bits = Decimal.GetBits(value);
            if (System.BitConverter.IsLittleEndian)
            {
                //byte[] bytes = new byte[bits.Length * 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bytes[0 + i * 4 + offset] = (byte)(bits[i] >> 0x18);
                    bytes[1 + i * 4 + offset] = (byte)(bits[i] >> 0x10);
                    bytes[2 + i * 4 + offset] = (byte)(bits[i] >> 8);
                    bytes[3 + i * 4 + offset] = (byte)(bits[i]);

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));
                    //}
                }
            }
            else
            {
                //byte[] bytes = new byte[bits.Length * 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bytes[3 + i * 4 + offset] = (byte)(bits[i] >> 0x18);
                    bytes[2 + i * 4 + offset] = (byte)(bits[i] >> 0x10);
                    bytes[1 + i * 4 + offset] = (byte)(bits[i] >> 8);
                    bytes[0 + i * 4 + offset] = (byte)(bits[i]);

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));
                    //}
                }
            }
        }

        /// <summary>
        /// 将字节数组转换为<c>Decimal</c>对象 ,Big Endian
        /// </summary>
        /// <param name="array">要转换的字节数组。</param>
        /// <returns>所转换的<c>Decimal</c>对象。</returns>
        public static Decimal ToDecimal(byte[] array)
        {
            return ToDecimal(array, 0);
        }

        /// <summary>
        /// 将字节数组转换为<c>Decimal</c>对象。
        /// </summary>
        /// <param name="array">要转换的字节数组。</param>
        /// <returns>所转换的<c>Decimal</c>对象。</returns>
        public static Decimal ToDecimalLE(byte[] array)
        {
            return ToDecimalLE(array, 0);
        }

        /// <summary>
        /// 将字节数组转换为<c>Decimal</c>对象 ,Big Endian
        /// </summary>
        /// <param name="array">要转换的字节数组。</param>
        /// <param name="offset"></param>
        /// <returns>所转换的<c>Decimal</c>对象。</returns>
        public static Decimal ToDecimal(byte[] array, int offset)
        {
            Int32[] bits = new Int32[4];
            if (System.BitConverter.IsLittleEndian)
            {
                //Int32[] bits = new Int32[array.Length / 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bits[i] = (Int32)array[i * 4 + 3 + offset] << 0x18
                        | (Int32)array[i * 4 + 2 + offset] << 0x10
                        | (Int32)array[i * 4 + 1 + offset] << 8
                        | array[i * 4 + 0 + offset];

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bits[i] |= array[i * 4 + j] << j * 8;
                    //}
                }
            }
            else
            {
                //Int32[] bits = new Int32[array.Length / 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bits[i] = (Int32)array[i * 4 + 0 + offset] << 0x18
                        | (Int32)array[i * 4 + 1 + offset] << 0x10
                        | (Int32)array[i * 4 + 2 + offset] << 8
                        | array[i * 4 + 3 + offset];

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bits[i] |= array[i * 4 + j] << j * 8;
                    //}
                }
            }
            return new Decimal(bits);
        }

        /// <summary>
        /// 将字节数组转换为<c>Decimal</c>对象。
        /// </summary>
        /// <param name="array">要转换的字节数组。</param>
        /// <param name="offset"></param>
        /// <returns>所转换的<c>Decimal</c>对象。</returns>
        public static Decimal ToDecimalLE(byte[] array, int offset)
        {
            Int32[] bits = new Int32[4];
            if (System.BitConverter.IsLittleEndian)
            {
                //Int32[] bits = new Int32[array.Length / 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bits[i] = (Int32)array[i * 4 + 0 + offset] << 0x18
                        | (Int32)array[i * 4 + 1 + offset] << 0x10
                        | (Int32)array[i * 4 + 2 + offset] << 8
                        | array[i * 4 + 3 + offset];

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bits[i] |= array[i * 4 + j] << j * 8;
                    //}
                }
            }
            else
            {
                //Int32[] bits = new Int32[array.Length / 4];
                //for (Int32 i = 0; i < bits.Length; i++)
                for (Int32 i = 0; i < 4; i++)
                {
                    bits[i] = (Int32)array[i * 4 + 3 + offset] << 0x18
                        | (Int32)array[i * 4 + 2 + offset] << 0x10
                        | (Int32)array[i * 4 + 1 + offset] << 8
                        | array[i * 4 + 0 + offset];

                    //for (Int32 j = 0; j < 4; j++)
                    //{
                    //    bits[i] |= array[i * 4 + j] << j * 8;
                    //}
                }
            }
            return new Decimal(bits);
        }

        /// <summary>
        /// lower 31 bits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Bits31(int value)
        {
            //lower 31 bits
            //取正值

            return value & 0x7FFFFFFF;
        }
    }
}