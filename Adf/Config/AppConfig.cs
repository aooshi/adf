using System;

namespace Adf.Config
{
    /// <summary>
    /// 应用配置项
    /// </summary>
    public class AppConfig : ConfigValue
    {
        /// <summary>
        /// 获取默认应用配置实例
        /// </summary>
        public static readonly AppConfig Instance = new AppConfig();

        /// <summary>
        /// App.Config
        /// </summary>
        private AppConfig()
            : base("App.config")
        {
            base.AddWatcher();
        }


        /// <summary>
        /// AppName.Config
        /// </summary>
        /// <param name="appName"></param>
        private AppConfig(string appName)
            : base(appName + ".config")
        {
            base.AddWatcher();
        }
    }
}