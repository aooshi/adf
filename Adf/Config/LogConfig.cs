using System;

namespace Adf.Config
{
    /// <summary>
    /// 日志配置
    /// </summary>
    public class LogConfig : ConfigValue
    {
        /// <summary>
        /// 获取配置实例
        /// </summary>
        public static readonly LogConfig Instance = new LogConfig();

        private LogConfig()
            : base("Log.config")
        {
            base.AddWatcher();
        }
    }
}