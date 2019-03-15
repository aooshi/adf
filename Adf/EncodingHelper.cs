using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Adf
{
    /// <summary>
    /// 字符编码助手
    /// </summary>
    public static class EncodingHelper
    {
        /// <summary>
        /// GB2312 Encoding
        /// </summary>
        public static readonly Encoding GB2312Encoding = Encoding.GetEncoding("GB2312");

        /// <summary>
        /// 获取配置中的字符编码
        /// </summary>
        /// <param name="appSettingKey"></param>
        /// <param name="defaultEncoding"></param>
        /// <returns></returns>
        public static Encoding GetConfigEncoding(string appSettingKey,Encoding defaultEncoding)
        {
            //string config = ConfigurationManager.AppSettings[appSettingKey];
            string config = Adf.ConfigHelper.GetSetting(appSettingKey);
            if (!string.IsNullOrEmpty(config))
            {
                switch (config.ToUpper())
                {
                    case "UTF-8":
                    case "UTF8":
                        return Encoding.UTF8;
                    case "UTF-7":
                    case "UTF7":
                        return Encoding.UTF7;
                    case "UTF-32":
                    case "UTF32":
                        return Encoding.UTF32;
                    case "ASCII":
                        return Encoding.ASCII;
                    case "UNICODE":
                        return Encoding.Unicode;
                    default:
                        return Encoding.GetEncoding(config);
                }
            }
            return defaultEncoding;
        }
    }
}
