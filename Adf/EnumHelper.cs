using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Adf
{
    /// <summary>
    /// 枚举助手
    /// </summary>
    public static class EnumHelper
    {
        static readonly Dictionary<string, string> cache = new Dictionary<string, string>();
        static readonly Dictionary<string, Dictionary<Enum, string>> cacheDictionary = new Dictionary<string, Dictionary<Enum, string>>();

        /// <summary>
        /// 获取枚举的描述(Description属性)
        /// </summary>
        /// <param name="value"></param>
        /// <example>
        /// <code>
        ///      System.Console.WriteLine(Utility.EnumHelper.GetDescritpion(EnumA.Value));
        ///      System.Console.WriteLine(Utility.EnumHelper.GetDescritpion(EnumB.Value));
        /// </code>
        /// </example>
        /// <returns></returns>
        public static string GetDescription(Enum value)
        {
            var type = value.GetType();
            var name = value.ToString();
            var typeName = type.AssemblyQualifiedName;
            //  
            var cacheKey = string.Concat(typeName, "@", name);
            var result = string.Empty;
            //
            if (!cache.TryGetValue(cacheKey, out result))
            {
                //var field = type.GetField(name);
                //if (field != null)
                //{
                //    var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                //    if (attributes != null && attributes.Length > 0)
                //    {
                //        result = ((DescriptionAttribute)attributes[0]).Description;
                //        cache[cacheKey] = result;
                //    }
                //}

                var descriptions = GetDescriptions(type);
                if (descriptions != null)
                {
                    descriptions.TryGetValue(value,out result);
                }
                cache[cacheKey] = result;
            }

            return result;
        }

        /// <summary>
        /// 获取枚举的描述列表
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<Enum, string> GetDescriptions(Type enumType)
        {
            var cacheKey = enumType.AssemblyQualifiedName;
            Dictionary<Enum, string> result = null;
            //
            if (!cacheDictionary.TryGetValue(cacheKey, out result))
            {
                var fields = enumType.GetFields();
                var fieldDescriptions = new Dictionary<string, string>(fields.Length);
                foreach (var field in fields)
                {
                    var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        fieldDescriptions[field.Name] = ((DescriptionAttribute)attributes[0]).Description;
                    }
                }

                result = new Dictionary<Enum, string>(fieldDescriptions.Count);
                foreach (Enum item in Enum.GetValues(enumType))
                {
                    result[item] = fieldDescriptions[item.ToString()];
                }
                cacheDictionary[cacheKey] = result;
            }
            return result;
        }
    }
}
