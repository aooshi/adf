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
    public static class MD5Helper
    {
        /// <summary>
        /// MD5默认字符编码，默认为 ASCII
        /// </summary>
        public static readonly Encoding Encoding = EncodingHelper.GetConfigEncoding("MD5Helper:Encoding", Encoding.ASCII);

        /// <summary>
        /// MD5 encodes the passed string
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string MD5(string input)
        {
            return MD5(input, Encoding);
        }

        /// <summary>
        /// MD5 encodes the passed string
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <param name="encoding">encoding</param>
        /// <returns>An encoded string.</returns>
        public static string MD5(string input, Encoding encoding)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Create a new instance of the MD5CryptoServiceProvider object.
            using (var md5Hasher = System.Security.Cryptography.MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hasher.ComputeHash(encoding.GetBytes(input));

                //return encoding.GetString(data);

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var builder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                foreach (var b in data)
                {
                    builder.Append(b.ToString("x2"));
                }

                // Return the hexadecimal string.
                return builder.ToString();
            }
        }

        /// <summary>
        /// MD5 encodes the passed string
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static byte[] MD5(byte[] input)
        {
            if (input == null)
                return input;

            if (input.Length == 0)
                return input;

            // Create a new instance of the MD5CryptoServiceProvider object.
            using (var md5Hasher = System.Security.Cryptography.MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hasher.ComputeHash(input);

                return data;
            }
        }

        /// <summary>
        /// File MD5
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string FileMD5(string filepath)
        {
            byte[] retVal = null;

            using (FileStream file = new FileStream(filepath, FileMode.Open))
            {
                using (System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                {
                    retVal = md5.ComputeHash(file);
                }
                file.Close();
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// stream MD5
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamMD5(Stream stream)
        {
            byte[] retVal = null;

            using (System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                retVal = md5.ComputeHash(stream);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}