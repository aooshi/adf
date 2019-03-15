using System;
using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// 配置管理器员
    /// </summary>
    public class GlobalConfig : ConfigValue
    {
        /// <summary>
        /// 获取配置实例
        /// </summary>
        public static readonly GlobalConfig Instance = new GlobalConfig();
        
        /// <summary>
        /// Global Config
        /// </summary>
        private GlobalConfig()
            : base("Global.config")
        {
            base.AddWatcher();
        }
        

//#if DEBUG
//        private void Example()
//        {
//            //<item name="config" value="config value" />
//            var configValue = GlobalConfig.Instance["config"];

//            //<item name="configformat" value="format index1={0}, index2={1}" />
//            var config2 = GlobalConfig.Instance["config2"];
//            if (config2 == null)
//            {
//                //未找到
//            }

//            //var configFormatValue = ConfigHelper.GlobalConfig["configformat", "1", "2"];
//        }
//#endif
    }
}
