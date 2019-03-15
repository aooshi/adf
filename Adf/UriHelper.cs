using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace Adf
{
    /// <summary>
    /// Uri Helper
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// QueryString to String
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string QueryStringToString(NameValueCollection queryString)
        {
            var build = new StringBuilder();
            for (int i = 0, l = queryString.Count; i < l; i++)
            {
                if (i != 0)
                {
                    build.Append("&");
                }
                build.Append(queryString.GetKey(i));
                build.Append("=");
                build.Append(queryString.Get(i));
            }
            return build.ToString();
        }

        /// <summary>
        /// Parse QueryString In Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        public static NameValueCollection ParseQueryString(string url, Encoding encoding)
        {
            return ParseQueryString(url, encoding, new NameValueCollection(5,StringComparer.OrdinalIgnoreCase));
        }
        /// <summary>
        /// Parse QueryString In Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="queryString"></param>
        public static NameValueCollection ParseQueryString(string url, Encoding encoding, NameValueCollection queryString)
        {
            string path;
            return ParseQueryString(url, encoding, queryString, out path);
        }


        /// <summary>
        /// Parse QueryString In Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="queryString"></param>
        /// <param name="path"></param>
        public static NameValueCollection ParseQueryString(string url, Encoding encoding, NameValueCollection queryString,out string path)
        {
            path = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                var index = url.IndexOf('?');
                string query;
                if (index > -1)
                {
                    query = url.Substring(index + 1);
                    path = url.Substring(0, index);
                }
                else if (url.IndexOf('=') > -1 || url.IndexOf('&') > -1)
                {
                    path = string.Empty;
                    query = url;
                }
                else
                {
                    path = url;
                    query = string.Empty;
                }
                var queryitem = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if (queryitem.Length > 0)
                {
                    foreach (var item in queryitem)
                    {
                        index = item.IndexOf('=');
                        if (index > 0)
                        {
                            var n = UrlDecode(item.Substring(0, index), encoding);
                            var v = UrlDecode(item.Substring(index + 1), encoding);

                            queryString.Add(n, v);
                        }
                    }
                }
                else if (string.IsNullOrEmpty(path))
                {
                    path = url;
                }
            }

            return queryString;
        }

        /// <summary>
        /// UrlEncode
        /// </summary>
        /// <param name="str"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            } 
            byte[] bytes = e.GetBytes(str);
            bytes = UrlEncode(bytes, 0, bytes.Length);
            return Encoding.ASCII.GetString(bytes);
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if ((bytes == null) && (count == 0))
            {
                return false;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((offset < 0) || (offset > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return true;
        }

        /// <summary>
        /// UrlDecode
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string UrlDecode(string value, Encoding encoding)
        {
            if (value == null)
            {
                return null;
            }
            int length = value.Length;
            UrlDecoder decoder = new UrlDecoder(length, encoding);
            for (int i = 0; i < length; i++)
            {
                char ch = value[i];
                if (ch == '+')
                {
                    ch = ' ';
                }
                else if ((ch == '%') && (i < (length - 2)))
                {
                    if ((value[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = HexToInt(value[i + 2]);
                        int num4 = HexToInt(value[i + 3]);
                        int num5 = HexToInt(value[i + 4]);
                        int num6 = HexToInt(value[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_010B;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HexToInt(value[i + 1]);
                    int num8 = HexToInt(value[i + 2]);
                    if ((num7 >= 0) && (num8 >= 0))
                    {
                        byte b = (byte)((num7 << 4) | num8);
                        i += 2;
                        decoder.AddByte(b);
                        continue;
                    }
                }
            Label_010B:
                if ((ch & 0xff80) == 0)
                {
                    decoder.AddByte((byte)ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }

        /// <summary>
        /// IsUrlSafeChar
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsUrlSafeChar(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }
                
        /// <summary>
        /// IntToHex
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }
         
        /// <summary>
        /// HexToInt
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
       public static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }        

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsUrlSafeChar(ch))
                {
                    num2++;
                }
            }
            if ((num == 0) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsUrlSafeChar(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        private class UrlDecoder
        {
            // Fields
            private int _bufferSize;
            private byte[] _byteBuffer;
            private char[] _charBuffer;
            private Encoding _encoding;
            private int _numBytes;
            private int _numChars;

            // Methods
            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                this._bufferSize = bufferSize;
                this._encoding = encoding;
                this._charBuffer = new char[bufferSize];
            }

            internal void AddByte(byte b)
            {
                if (this._byteBuffer == null)
                {
                    this._byteBuffer = new byte[this._bufferSize];
                }
                this._byteBuffer[this._numBytes++] = b;
            }

            internal void AddChar(char ch)
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                this._charBuffer[this._numChars++] = ch;
            }

            private void FlushBytes()
            {
                if (this._numBytes > 0)
                {
                    this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
                    this._numBytes = 0;
                }
            }

            internal string GetString()
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                if (this._numChars > 0)
                {
                    return new string(this._charBuffer, 0, this._numChars);
                }
                return string.Empty;
            }
        }
    }
}
