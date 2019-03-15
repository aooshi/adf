using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// convert helper, the convert is safe
    /// </summary>
    public static class ConvertHelper
    {
        ///// <summary>
        ///// 示例对象
        ///// </summary>
        //private class exi
        //{
        //    public int id = 0;
        //    public string body = string.Empty;
        //}

        ///// <summary>
        ///// 示例
        ///// </summary>
        //private void ex()
        //{
        //    //数组
        //    var idArray = new int[] { 1, 2 };
        //    var idstring = ConvertHelper.ArrayToString(idArray, ",");

        //    //对象
        //    var list = new List<exi>();
        //    list.Add(new exi());
        //    list.Add(new exi());
        //    var liststring = ConvertHelper.ArrayToString<exi>(list, ",", i => { return i.id.ToString(); });
        //}

        /// <summary>
        /// string to array
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<string> StringToArray(string ids, char separator = ',')
        {
            if (string.IsNullOrEmpty(ids))
                return new List<string>();

            var items = ids.Split(separator);
            var list = new List<string>(items.Length);
            list.AddRange(items);
            return list;
        }

        /// <summary>
        /// string to array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <param name="convert"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<T> StringToArray<T>(string ids, Converter<string, T> convert, char separator = ',')
        {
            if (string.IsNullOrEmpty(ids))
                return new List<T>();

            var items = ids.Split(separator);
            var list = new List<T>(items.Length);

            foreach (var item in items)
                list.Add(convert.Invoke(item));

            return list;
        }

        /// <summary>
        /// 可列表对象转换为以指定字符分隔的字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ArrayToString<T>(IEnumerable<T> inputs, string separator)
        {
            return ArrayToString<T>(inputs, separator, p => { return p.ToString(); });
        }

        /// <summary>
        /// 列表对象以指定方式转换为按指定分隔符分隔的字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="separator"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static string ArrayToString<T>(IEnumerable inputs, string separator, Converter<T, string> converter)
        {
            if (inputs == null)
                return string.Empty;

            var build = new StringBuilder();
            var first = true;
            foreach (T input in inputs)
            {
                if (first)
                    first = false;
                else
                    build.Append(separator);

                build.Append(converter.Invoke(input));
            }

            return build.ToString();
        }

        /// <summary>
        /// 列表对象以指定方式转换为按指定分隔符分隔的字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="separator"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static string ArrayToString<T>(IEnumerable<T> inputs, string separator, Converter<T, string> converter)
        {
            if (inputs == null)
                return string.Empty;

            var build = new StringBuilder();
            var first = true;
            foreach (T input in inputs)
            {
                if (first)
                    first = false;
                else
                    build.Append(separator);

                build.Append(converter.Invoke(input));
            }

            return build.ToString();
        }

        /// <summary>
        /// 将一组对象数组转换为以对象某种规则排列的字曲， 要求列表中生成键值的规则不重复
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="array"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TItem> ArrayToDictionary<TKey, TItem>(IEnumerable<TItem> array, Converter<TItem, TKey> converter)
        {
            var dictionary = new Dictionary<TKey, TItem>();
            if (array == null)
                return dictionary;

            TKey key = default(TKey);
            foreach (var item in array)
            {
                key = converter(item);
                dictionary.Add(key, item);
            }
            return dictionary;
        }

        /// <summary>
        /// 数组转换为逗号单条限4000长度分隔字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string[] ArrayCompart<T>(IEnumerable<T> inputs)
        {
            const int len = 4000;
            return ArrayCompartToString<T>(inputs, ',', len);
        }

        /// <summary>
        /// 数组转换为逗号限定长度分隔字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="itemMaxLength"></param>
        /// <returns></returns>
        public static string[] ArrayCompartToString<T>(IEnumerable<T> inputs, int itemMaxLength)
        {
            return ArrayCompartToString<T>(inputs, ',', itemMaxLength);
        }
        /// <summary>
        /// 数组转换为限定长度分隔字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="compart"></param>
        /// <param name="itemMaxLength"></param>
        /// <returns></returns>
        public static string[] ArrayCompartToString<T>(IEnumerable<T> inputs, char compart, int itemMaxLength)
        {
            var ids = ArrayToString<T>(inputs, compart.ToString());
            return StringHelper.Compart(ids, itemMaxLength, compart);
        }

        /// <summary>
        /// 将参数列表转换为单项院4000字符串数组
        /// </summary>
        /// <param name="list">列表</param>
        /// <returns></returns>
        public static string[] ListCompartToString<TFirst, TSecond>(List<KeyValuePair<TFirst, TSecond>> list)
        {
            const int len = 4000;

            return ListCompartToString<TFirst, TSecond>(list, len);
        }

        /// <summary>
        /// 将参数列表转换为字符串数组
        /// </summary>
        /// <param name="list">列表</param>
        /// <param name="itemMaxLength">列表串单项最大长度</param>
        /// <returns></returns>
        public static string[] ListCompartToString<TFirst, TSecond>(List<KeyValuePair<TFirst, TSecond>> list, int itemMaxLength)
        {
            var builder = new StringBuilder();
            foreach (var item in list)
            {
                builder.AppendFormat("{0}|{1},", item.Key, item.Value);
            }
            var ids = builder.ToString().TrimEnd(',');
            return StringHelper.Compart(ids, itemMaxLength, ',');
        }
        
        /// <summary>
        /// 将类型装换为int类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        [Obsolete("obsolete, please invoke ToInt32")]
        public static int ToInt(object obj, int defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            int val;
            return int.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为int类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static int ToInt32(object obj, int defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            int val;
            return int.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }
        
        /// <summary>
        /// 将类型装换为int类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static UInt32 ToUInt32(object obj, UInt32 defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            uint val;
            return uint.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为int16类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static short ToInt16(object obj, short defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            Int16 val;
            return Int16.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为uint16类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static UInt16 ToUInt16(object obj, UInt16 defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            UInt16 val;
            return UInt16.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }


        /// <summary>
        /// 将类型装换为uint64类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static UInt64 ToUInt64(object obj, UInt64 defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            UInt64 val;
            return UInt64.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }


        /// <summary>
        /// 将类型装换为int64类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static long ToInt64(object obj, long defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            long val;
            return long.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 安全将类型装换为字符串
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>int类型</returns>
        public static string ToString(object obj, string defaultValue = "")
        {
            return null == obj ? defaultValue : obj.ToString();
        }

        /// <summary>
        /// 将类型装换为Guid类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值,若无请输入 <see cref="Guid.Empty"/></param>
        /// <returns>int类型</returns>
        public static Guid ToGuid(object obj, Guid defaultValue)
        {
            if (null == obj)
            {
                return defaultValue;
            }

#if NET2
            try
            {
                return new Guid(obj.ToString());
            }
            catch
            {
                return defaultValue;
            }
#elif NET35
            try
            {
                return new Guid(obj.ToString());
            }
            catch
            {
                return defaultValue;
            }
#else
            Guid val;
            return Guid.TryParse(obj.ToString(), out val) ? val : defaultValue;
#endif
        }

        /// <summary>
        /// 将类型装换为double类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>double类型</returns>
        public static double ToDouble(object obj, double defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }
            double val;
            return double.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为bool类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>bool类型</returns>
        public static bool ToBool(object obj, bool defaultValue = false)
        {
            if (null == obj)
            {
                return defaultValue;
            }
            bool val;
            return Boolean.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为decimal类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>decimal类型</returns>
        public static decimal ToDecimal(object obj, decimal defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            decimal val;
            return decimal.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为float类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>float类型</returns>
        public static float ToFloat(object obj, float defaultValue = 0)
        {
            if (null == obj)
            {
                return defaultValue;
            }
            float val;
            return float.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型装换为DateTime类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值,若无默认值可设置为 <see cref="DateTime.MinValue"/></param>
        /// <returns>DateTime类型</returns>
        public static DateTime ToDateTime(object obj, DateTime defaultValue)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            DateTime val;
            return DateTime.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }

        /// <summary>
        /// 将类型转换为byte类型
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>byte类型</returns>
        public static byte ToByte(object obj, byte defaultValue=0)
        {
            if (null == obj)
            {
                return defaultValue;
            }

            byte val;
            return byte.TryParse(obj.ToString(), out val) ? val : defaultValue;
        }
        
        /// <summary>
        /// 转换为枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumName"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ToEnum<T>(string enumName, T defaultValue, bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(enumName))
                return defaultValue;
#if NET2
            try
            {
                return (T)Enum.Parse(typeof(T), enumName,ignoreCase);
            }
            catch
            {
                return defaultValue;
            }
            
#elif NET35
            try
            {
                return (T)Enum.Parse(typeof(T), enumName,ignoreCase);
            }
            catch
            {
                return defaultValue;
            }
#else
            T result ;
            return Enum.TryParse<T>(enumName,ignoreCase, out result) ? result : defaultValue;
#endif
        }

        /// <summary>
        /// 将字符数组转换成16进制字符串 bin2hex
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BinToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 将16进制字符串转换成字符数组 hex2bin
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexToBin(string hexString)
        {
            if (hexString == null || hexString.Length < 2)
            {
                throw new ArgumentException("hexString");
            }

            int l = hexString.Length / 2;
            byte[] result = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                result[i] = Convert.ToByte(hexString.Substring(2 * i, 2), 16);
                //bin[i] = (byte)int.Parse(hexstr.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            return result;
        }
    }
}
