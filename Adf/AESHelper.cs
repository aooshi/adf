using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 编码助手
    /// 使用此助手建议配置： AESHelper:Key  与 AESHelper:IV ,此两配置均为32位长度字符串
    /// </summary>
    public static class AESHelper
    {
        /// <summary>
        /// 默认字符编码, 配置：AESHelper:Encoding，默认为 ASCII
        /// </summary>
        public static readonly Encoding Encoding = EncodingHelper.GetConfigEncoding("AESHelper:Encoding", Encoding.ASCII);
        /// <summary>
        /// AES 通用加密密钥, 配置：AESHelper:Key
        /// </summary>
        public static readonly string Key = ConfigHelper.GetSetting("AESHelper:Key", "!a@b#c$d%e^0&1*2_-+={}[]|';:9?5.");
        /// <summary>
        /// AES 加密向量, 配置：AESHelper:IV
        /// </summary>
        public static readonly string IV = ConfigHelper.GetSetting("AESHelper:IV", "!a@b#c$d%e^9&8*7_-+={}[3]|'4;:?.");
        /// <summary>
        /// AES 加密块大小, 配置：AESHelper:Size，默认为 256
        /// </summary>
        public static readonly int SIZE = ConfigHelper.GetSettingAsInt("AESHelper:Size", 256);
        //
        private static readonly byte[] AES_KEY_BYTES = Encoding.GetBytes(Key);
        private static readonly byte[] AES_IV_BYTES = Encoding.GetBytes(IV);
        
        /// <summary>
        /// 使用默认值或配置进行AES 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Encrypt(string input)
        {
            return Encrypt(input, AESHelper.Key , Encoding);
        }

        /// <summary>
        /// AES 加密 (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string input,string key)
        {
            return Encrypt(input, key, Encoding);
        }


        /// <summary>
        /// AES 加密 (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Encrypt(string input, string key,Encoding encoding)
        {
            return Encrypt(input, encoding.GetBytes(key), AES_IV_BYTES,encoding);
        }

        /// <summary>
        /// AES 加密 (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string input, byte[] key, byte[] iv)
        {
            return Encrypt(input, key, iv, Encoding);
        }
        
        /// <summary>
        /// AES 加密  (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Encrypt(string input, byte[] key, byte[] iv, Encoding encoding)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.BlockSize = SIZE;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.IV = iv;
                rm.Key = key;


                using (ICryptoTransform ct = rm.CreateEncryptor())
                {
                    byte[] bytes = encoding.GetBytes(input);
                    var encode = ct.TransformFinalBlock(bytes, 0, bytes.Length);
                    return Convert.ToBase64String(encode);
                }
            }
        }

        /// <summary>
        /// AES 加密  (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] Encrypt(int size,byte[] input, byte[] key, byte[] iv)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.BlockSize = size;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.IV = iv;
                rm.Key = key;

                byte[] value;
                using (ICryptoTransform ct = rm.CreateEncryptor())
                {
                    value = ct.TransformFinalBlock(input, 0, input.Length);
                }
                return value;
            }
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string input, string key)
        {
            return Decrypt(input, key, Encoding);
        }

        /// <summary>
        /// 使用默认值或配置进行AES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decrypt(string input)
        {
            return Decrypt(input, AESHelper.Key, Encoding);
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Decrypt(string input, string key, Encoding encoding)
        {
            return Decrypt(input, encoding.GetBytes(key), AES_IV_BYTES, encoding);
        }
        

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string input, byte[] key, byte[] iv)
        {
            return Decrypt(input, key, iv, Encoding);
        }

        /// <summary>
        /// AES 解密 (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Decrypt(string input, byte[] key, byte[] iv, Encoding encoding)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.BlockSize = SIZE;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.IV = iv;
                rm.Key = key;

                using (ICryptoTransform ct = rm.CreateDecryptor(key, iv))
                {
                    byte[] bytes = Convert.FromBase64String(input);
                    var decode = ct.TransformFinalBlock(bytes, 0, bytes.Length);
                    return encoding.GetString(decode);
                }
            }
        }

        /// <summary>
        /// AES 解密 (CBC - PCKS7)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] Decrypt(int size,byte[] input, byte[] key, byte[] iv)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.BlockSize = size;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.IV = iv;
                rm.Key = key;

                byte[] value;
                using (ICryptoTransform ct = rm.CreateDecryptor(key, iv))
                {
                    value = ct.TransformFinalBlock(input, 0, input.Length);
                }
                return value;
            }
        }

        /// <summary>
        /// AES 加密，输出成URL友好格式
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncryptUrl(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Encrypt(input,AES_KEY_BYTES,AES_IV_BYTES);
        }
        
        /// <summary>
        /// AES 加密，输出成URL友好格式
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptUrl(string input,string key)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Encrypt(input, Encoding.GetBytes(key), AES_IV_BYTES);
        }

        /// <summary>
        /// AES 解密（注意，只能对用EncryptUrl方法进行加密的字符串使用）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DecryptUrl(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Decrypt(input,AES_KEY_BYTES,AES_IV_BYTES);
        }

        /// <summary>
        /// AES 解密（注意，只能对用EncryptUrl方法进行加密的字符串使用）
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptUrl(string input, string key)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Decrypt(input, Encoding.GetBytes(key), AES_IV_BYTES);
        }
    }
}
