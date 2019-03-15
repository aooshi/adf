using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 基础数字压缩编码
    /// </summary>
    public class NumberBaseEncode
    {
        private const string DEFAULT_CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 字符集
        /// </summary>
        public string Chars
        {
            get;
            private set;
        }

        /// <summary>
        /// 字符集长度
        /// </summary>
        public int Length
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 使用默认字符集初始新实例
        /// </summary>
        public NumberBaseEncode()
        {
            this.Chars = DEFAULT_CHARS;
            this.Length = this.Chars.Length;
        }

        /// <summary>
        /// 初始实例并指定压缩字符集
        /// </summary>
        /// <param name="chars"></param>
        public NumberBaseEncode(string chars)
        {
            this.Chars = chars;
            this.Length = this.Chars.Length;
        }

        /// <summary>
        /// 将数字进行字符编码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Encode(long value)
        {
            StringBuilder builder = new StringBuilder();
            while (value > 0)
            {
                int remainder = (int)(value % this.Length);  // 余数
                builder.Insert(0, this.Chars[remainder]);
                value /= this.Length;
            }
            return builder.ToString();
        }

        /// <summary>
        /// 将已编码的字符还原为数字
        /// </summary>
        /// <param name="encodeValue"></param>
        /// <returns></returns>
        public long Decode(string encodeValue)
        {
            long result = 0;
            for (int i = 0, l = encodeValue.Length; i < l; i++)
            {
                result += this.Chars.IndexOf(encodeValue[i]) * (long)Math.Pow(this.Length, l - i - 1);
            }
            return result;
        }
    }
}
