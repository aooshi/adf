using System;
using System.Text;
using System.Configuration;

namespace Adf.Config
{
    /// <summary>
    /// 字符型配置项
    /// </summary>
    public abstract class ConfigValue : ConfigBase<string>
    {
        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="fileName"></param>
        public ConfigValue(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// 新项
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override string NewItem(System.Xml.XmlNode node)
        {
            return node.Attributes["value"].InnerText;
        }

        ///// <summary>
        ///// 获取指定的项并格式化串
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="valueArgs"></param>
        ///// <returns></returns>
        //[Obsolete("obsolete , please invoke GetFormat")]
        //public string this[string name, params object[] valueArgs]
        //{
        //    get
        //    {
        //        string v = null;
        //        if (base.items.TryGetValue(name, out v))
        //        {
        //            if (valueArgs.Length > 0)
        //                v = string.Format(v, valueArgs);
        //        }
        //        return v;
        //    }
        //}

        /// <summary>
        /// 获取一个字符串型配置并进行格式化操作
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetFormat(string name, params object[] args)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            if (v != null && args.Length > 0)
            {
                v = string.Format(v, args);
            }
            return v;
        }

        /// <summary>
        /// 获取一个字符串型配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetString(string name, string @default = null)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : v;
        }

        /// <summary>
        /// 获取一个整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int16 GetInt16(string name, short @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : Int16.Parse(v);
        }

        /// <summary>
        /// 获取一个整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int32 GetInt32(string name, int @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : int.Parse(v);
        }
        
        /// <summary>
        /// 获取一个长整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public Int64 GetInt64(string name, long @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : long.Parse(v);
        }



        /// <summary>
        /// 获取一个整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt16 GetUInt16(string name, ushort @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : UInt16.Parse(v);
        }

        /// <summary>
        /// 获取一个整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt32 GetUInt32(string name, uint @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : uint.Parse(v);
        }

        /// <summary>
        /// 获取一个长整型值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public UInt64 GetUInt64(string name, ulong @default = 0)
        {
            string v = null;
            base.items.TryGetValue(name, out v);
            return v == null ? @default : ulong.Parse(v);
        }


        /// <summary>
        /// 获取一个布尔值配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public bool GetBoolean(string name, Boolean @default = false)
        {
            string v = null;
            base.items.TryGetValue(name, out v);

            if (v == null)
                return @default;

            if (v == "1")
                return true;

            if (v == "0")
                return false;

            return bool.Parse(v);
        }

        /// <summary>
        /// 获取指定配置项，若存在Setting配置，则由Setting配置优先
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetAndSetting(string name, string defaultValue = "")
        {
            var value = ConfigHelper.GetSetting(name, null);
            if (value == null)
            {
                base.items.TryGetValue(name, out value);
                if (value == null)
                {
                    return defaultValue;
                }
            }
            return value;
        }

        ///// <summary>
        ///// 获取值
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public T GetValue<T>(string name, params object[] args)
        //{
        //    var v = this[name];

        //    if (v == null)
        //        return default(T);

        //    return (T)Convert.ChangeType(v, typeof(T));
        //}

    }
}
