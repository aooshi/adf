using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Adf
{
    /// <summary>
    /// Base64 Helper
    /// </summary>
    public static class Base64Helper
    {
        /// <summary>
        /// 默认的字符编码,默认为ASCII
        /// </summary>
        /// <remarks>可通过配置 Base64Helper:DefaultEncoding 设置 </remarks>
        public static readonly Encoding DefaultEncoding = EncodingHelper.GetConfigEncoding("Base64Helper:DefaultEncoding", Encoding.ASCII);
                        
        /// <summary>
		/// 使用默认的Base64编码可用的URL字符串
		/// </summary>
        /// <param name="inputString"></param>
        public static string EncodeUrl(string inputString)
        {
            return EncodeUrl(inputString, DefaultEncoding);
        }

        /// <summary>
		/// 使用Base64编码可用的URL字符串
		/// </summary>
        /// <param name="encoding"></param>
        /// <param name="inputString"></param>
		public static string EncodeUrl(string inputString, Encoding encoding)
		{
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }
            var base64 = Convert.ToBase64String(encoding.GetBytes(inputString));
            
            // "+" 换成 "-"
            // "/" 换成 "_"
            // 去掉 "="

            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            base64 = base64.Replace("=", string.Empty);

            return base64;
		}
        
		/// <summary>
		/// 使用默认编码将UrlBase64编码串转换为源字符串
		/// </summary>
        /// <param name="urlString"></param>
        public static string DecodeUrl(string urlString)
        {
            return DecodeUrl(urlString, DefaultEncoding);
        }

		/// <summary>
		/// 将UrlBase64编码串转换为源字符串
		/// </summary>
        /// <param name="encoding"></param>
        /// <param name="urlString"></param>
		public static string DecodeUrl(string urlString, Encoding encoding)
		{
            if (string.IsNullOrEmpty(urlString))
            {
                return urlString;
            }
            
            // "-" 换成 "+"
            // "_" 换成 "/"
            urlString = urlString.Replace('-', '+');
            urlString = urlString.Replace('_', '/');
            // 添加"="
            int mod = urlString.Length % 4;
            if (mod != 0)
            {
                urlString += new string('=', 4 - mod);
            }

			return encoding.GetString(Convert.FromBase64String(urlString));
		}
    }
}
