using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Adf.Config;

namespace Adf
{
    /// <summary>
    /// Setting Helper
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 配置文件根目录
        /// </summary>
        public static readonly string PATH_ROOT = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Config\\");
        /// <summary>
        /// 应用根目录
        /// </summary>
        public static readonly string PATH_APP_ROOT = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        static string appName = null;
        /// <summary>
        /// 获取应用名称,配置名：AppName
        /// </summary>
        [Obsolete("obsolete property")]
        public static string AppName
        {
            get
            {
                if (appName == null)
                {
                    var name = ConfigurationManager.AppSettings["AppName"];
                    if (string.IsNullOrEmpty(name))
                    {
                        var assembly = System.Reflection.Assembly.GetEntryAssembly();
                        if (assembly != null)
                        {
                            var an = assembly.GetName();
                            if (an != null)
                                name = an.Name;
                        }
                    }

                    appName = name ?? string.Empty;
                }
                return appName;
            }
        }

        
        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSetting(string key, string defaultValue = "")
        {
            string result = ConfigurationManager.AppSettings[key];
            return null == result ? defaultValue : result;
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetSettingAsInt(string key, int defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : int.Parse(setting);
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static uint GetSettingAsUInt(string key, uint defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : uint.Parse(setting);
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Int64 GetSettingAsInt64(string key, Int64 defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : Int64.Parse(setting);
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static UInt64 GetSettingAsUInt64(string key, UInt64 defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : UInt64.Parse(setting);
        }


        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Int16 GetSettingAsInt16(string key, Int16 defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : Int16.Parse(setting);
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static UInt16 GetSettingAsUInt16(string key, UInt16 defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : UInt16.Parse(setting);
        }
        
        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static byte GetSettingAsByte(string key, byte defaultValue = 0)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : byte.Parse(setting);
        }

        /// <summary>
        /// 获取一项配置，若未配置则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetSettingAsBoolean(string key, bool defaultValue = false)
        {
            var setting = ConfigurationManager.AppSettings[key];
            if (setting == null)
                return defaultValue;
            if (setting == "1")
                return true;
            if (setting == "0")
                return false;
            return bool.Parse(setting);
        }
        /// <summary>
        /// 获取一个配置节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetSection<T>(string name)
        {
            return (T)ConfigurationManager.GetSection(name);
        }
    }
}