using System;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Adf.Config
{
    /// <summary>
    /// 字符K-V型配置项
    /// </summary>
    public class NameValue : ConfigValue
    {
        static readonly Dictionary<string, NameValue> dictionary = new Dictionary<string, NameValue>();
        static readonly Object lockObject = new object();

        /// <summary>
        /// 通过单例模式获取一个配置实例
        /// </summary>
        /// <param name="fileName">配置文件名称,区分大小写</param>
        /// <returns></returns>
        public static NameValue GetConfiguration(string fileName)
        {
            NameValue nv = null;
            //
            lock (lockObject)
            {
                if (dictionary.TryGetValue(fileName, out nv) == false)
                {
                    nv = new NameValue(fileName);
                    dictionary.Add(fileName, nv);
                }
            }
            //
            return nv;
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="fileName"></param>
        public NameValue(string fileName)
            : base(fileName)
        {
            base.AddWatcher();
        }
    }
}