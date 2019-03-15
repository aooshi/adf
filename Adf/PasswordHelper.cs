using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 编码助手
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// IV CHARS
        /// </summary>
        public const string IV_CHARS = "01234abcdefghij56789klmnopqrPQRSTUVWXYZstuvwxyzABCDEFGHIJKLMNO!@#$%^&*()_+-=[]{};:.,><?/";
        /// <summary>
        /// IV CHARS LENGTH
        /// </summary>
        public const int IV_CHARS_LENGTH = 88;

        /// <summary>
        /// MD5 encodes the passed string
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string MD5Strong(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            //一次加密
            var md5String = MD5Helper.MD5(input, MD5Helper.Encoding);

            //切除加密防止还原
            var start = 10;
            var len = 20;
            //
            var inputLen = input.Length;
            if (inputLen > start)
                start = inputLen % start;
            else
                start = start % inputLen;
            //
            return MD5Helper.MD5(md5String.Substring(start, len));
        }


        /// <summary>
        /// MD5 encodes the passed string
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <param name="salt"></param>
        /// <returns>An encoded string.</returns>
        public static string MD5Slat(string input, string salt)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            //
            return MD5Helper.MD5(string.Concat(input, salt), MD5Helper.Encoding);
        }


        /// <summary>
        /// AES Password, Charset UTF8
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AES(string input, string key)
        {
            return AES(input, key, Encoding.UTF8);
        }

        /// <summary>
        /// AES Password
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string AES(string input, string key, Encoding encoding)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            //
            var op = encoding.GetBytes(input)[0];
            var ok = (int)key[0];
            var ivnum = op + ok;
            var ivbuild = new StringBuilder();
            for (var i = 1; i <= 32; i++)
            {
                ivnum = ivnum % IV_CHARS_LENGTH;
                ivnum = (int)IV_CHARS[ivnum] * (ivnum + i);
                ivnum = ivnum % IV_CHARS_LENGTH;
                ivbuild.Append(IV_CHARS[ivnum]);
            }

            //
            return AESHelper.Encrypt(input, encoding.GetBytes(key), encoding.GetBytes(ivbuild.ToString()), encoding);
        }
    }
}