using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 数字助手
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// 获得分数的整数部分和小数部分
        /// </summary>
        /// <param name="rating"></param>
        /// <param name="bigInt"></param>
        /// <param name="smallInt"></param>
        public static void GetRating(float rating, out int bigInt, out int smallInt)
        {
            bigInt = (int)Math.Floor(rating + 0.01);
            smallInt = (int)Math.Floor(Math.Abs((rating - bigInt) * 10 + 0.01));
        }

        /// <summary>
        /// 将16进制数值转换为Byte数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexToBytes(string hexString)
        {
            var chars = hexString.ToCharArray();
            if (chars.Length % 2 != 0)
            {
                throw new ArgumentOutOfRangeException("hexString", "param not hex string");
            }
            //
            byte[] ret = new byte[chars.Length / 2];
            for (int i = 0, l = ret.Length; i < l; i++)
            {
                //ret[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

                ret[i] = Convert.ToByte(chars[i * 2] + "" + chars[i * 2 + 1], 16);
            }
            return ret;
        }

        /// <summary>
        /// 将Byte数组转换为16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToHex(byte[] bytes)
        {
            //var build = new StringBuilder();
            //foreach (var b in bytes)
            //{
            //    build.Append(b.ToString("x2"));
            //}
            //var str1 = build.ToString();
            //return str1;

            var chars = new char[bytes.Length * 2];
            for (int i = 0, l = bytes.Length; i < l; i++)
            {
                chars[i * 2] = HexToChar(bytes[i] >> 4);
                chars[i * 2 + 1] = HexToChar(bytes[i]);
            }
            var str2 = new string(chars);
            return str2;
        }

        ///// <summary>
        ///// 将Byte数组转换为16进制字符串
        ///// </summary>
        ///// <param name="bytes"></param>
        ///// <param name="isUpper"></param>
        ///// <returns></returns>
        //public static string BytesToHex(byte[] bytes, bool isUpper)
        //{
        //    var build = new StringBuilder();
        //    if (isUpper == true)
        //    {
        //        foreach (var b in bytes)
        //        {
        //            build.Append(b.ToString("X2"));
        //        }
        //    }
        //    else
        //    {
        //        foreach (var b in bytes)
        //        {
        //            build.Append(b.ToString("x2"));
        //        }
        //    }
        //    return build.ToString();
        //}

        private static char HexToChar(int a)
        {
            a = a & 0xf;
            return (char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
        }
 
    }
}