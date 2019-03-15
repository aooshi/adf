using System;
using System.Collections.Generic;

using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// 正则配置
    /// </summary>
    public class RegularConfig : ConfigValue
    {
        /// <summary>
        /// 获取配置实例
        /// </summary>
        public static readonly RegularConfig Instance = new RegularConfig();
        
        /// <summary>
        /// 初始化新实例
        /// </summary>
        private RegularConfig()
            : base("Regular.config")
        {
            base.AddWatcher();
        }
    }
}
