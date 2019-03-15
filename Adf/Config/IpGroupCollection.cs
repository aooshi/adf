using System;
using System.Configuration;

namespace Adf.Config
{
    /// <summary>
    /// IP组配置集合
    /// </summary>
    [ConfigurationCollection(typeof(IpGroupElement))]
    public class IpGroupCollection  : ConfigurationElementCollection
    {
        /// <summary>
        /// 创建新元素
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new IpGroupElement();
        }

        /// <summary>
        /// 获取键
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            var ele =  (IpGroupElement)element;
            return string.Concat(ele.Ip, ":", ele.Port);
            //return ((IpGroupElement)element).Name;
        }

        /// <summary>
        /// 获取指定索引的元素
        /// </summary>
        /// <param name="index">索引</param>
        public IpGroupElement this[int index]
        {
            get
            {
                return (IpGroupElement)base.BaseGet(index);
            }
        }
    }
}
