using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Adf
{
    /// <summary>
    /// UUID Base58 编码器
    /// </summary>
    public class UUIDEncoder
    {
        static Regex valid_regex = new Regex(@"^[\w]{22}$", RegexOptions.Compiled);

        static byte[] Hex2Bytes(string text)
        {
            // Not the most efficient code in the world, but
            // it works...
            text = text.Replace("-", "");
            byte[] ret = new byte[text.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }
            return ret;
        }
        
        static string Bytes2Hex(byte[] bytes)
        {
            var build = new StringBuilder();
            foreach(var b in bytes)
            {
                build.Append( b.ToString("x2") );
            }
            build.Insert(8, '-'); 
            build.Insert(13, '-');
            build.Insert(18, '-');
            build.Insert(23, '-');
            return build.ToString();
        }

        /// <summary>
        /// 是否符合编码字符串格式
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsMatch(string input)
        {
            return valid_regex.IsMatch(input);
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <returns>返回22位长度字符串</returns>
        public static String NewID()
        {
            var guid = Guid.NewGuid().ToString() ;
            var bytes = Hex2Bytes(guid);
            var r = UUIDBase58.Encode(bytes);

            if (r.Length < 22) r = r.PadRight(22, '0');
            return r;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>返回22位长度字符串</returns>
        public static String Encode(string guid)
        {
            var bytes = Hex2Bytes(guid);
            var r = UUIDBase58.Encode(bytes);

            if (r.Length < 22) r = r.PadRight(22, '0');
            return r;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>返回22位长度字符串</returns>
        public static String Encode(Guid guid)
        {
            var g = guid.ToString();
            var bytes = Hex2Bytes(g);
            var r = UUIDBase58.Encode(bytes);

            if (r.Length < 22) r = r.PadRight(22, '0');
            return r;
        }

        /// <summary>
        /// 解码器
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decode(String input)
        {
            input = input.TrimEnd('0');

            var bytes = UUIDBase58.Decode(input);
            return Bytes2Hex(bytes);
        }

        /// <summary>
        /// 解码器
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid DecodeToGuid(String input)
        {
            input = input.TrimEnd('0');

            var bytes = UUIDBase58.Decode(input);
            return new Guid(  Bytes2Hex(bytes) );
        }
    }
}