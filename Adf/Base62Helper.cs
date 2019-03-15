using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Base62 助手
    /// </summary>
    public static class Base62Helper
    {
        private const string CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        //private const int LENGTH = 62;


        private static readonly NumberBaseEncode base62encode = new NumberBaseEncode(CHARS);
                
        /// <summary>
        /// 将数字进行62字符编码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Encode(long value)
        {
            return base62encode.Encode(value);
        }
        
        /// <summary>
        /// 将已编码的62字符还原为数字
        /// </summary>
        /// <param name="encodeString"></param>
        /// <returns></returns>
        public static long Decode(string encodeString)
        {
            return base62encode.Decode(encodeString);
        }
    }
}
