using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace Adf
{
    /// <summary>
    /// Json Helper
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Serialize(object value)
        {
            return ProcuriosJson.JsonEncode(value, false);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string Serialize(object value,bool hex)
        {
            return ProcuriosJson.JsonEncode(value, hex);
        }

        /// <summary>
        /// 将JSON字符串解码为一个基础类型对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object DeserializeBase(string json)
        {
            return ProcuriosJson2.JsonDecode(json);
        }

        /// <summary>
        /// 将JSON字符串解码
        /// </summary>
        /// <param name="json"></param>
        /// <returns>An ArrayList, a Dictionary&gt;string,object&lt;, a double, a string, null, true, or false</returns>
        public static object Deserialize(string json)
        {
            return ProcuriosJson.JsonDecode(json); 
        }

        /// <summary>
        /// 返序列化为一个对象
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, string json)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return ObjectConverter.ConvertValue(Deserialize(json), type, ConvertValueCustom);
        }

        /// <summary>
        /// 返序列化为一个对象
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return (T)ObjectConverter.ConvertValue(Deserialize(json), typeof(T), ConvertValueCustom);
        }

        /// <summary>
        /// 转换值时的自处理回调
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        static bool ConvertValueCustom(object value, Type objectType, out object result)
        { 
            //value is string to json
            if (value != null && value is string)
            {
                if (objectType.Equals(TypeHelper.DATETIME))
                {
                    result = DateTime.Parse((string)value);
                    return true;
                }

                if (objectType.Equals(TypeHelper.GUID))
                {
                    result = new Guid((string)value);
                    return true;
                }
            }

            result = null;
            return false;
        }
        

        /// <summary>
        /// 编码一个值,仅支持基础数据类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string EncodeValue(object value,bool hex=true)
        {
            if (value is string)
            {
                return EncodeValue((string)value,hex);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                return "true";
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                return ("false");
            }
            else if (value is ValueType)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
            else if (value == null)
            {
                return "";// ("null");
            }
            return Serialize(value,hex);
        }

        /// <summary>
        /// 编码字符串
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string EncodeValue(string inputString,bool hex=true)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("\"");

            char[] charArray = inputString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else if (!hex)
                {
                    builder.Append(c);
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
            return builder.ToString();
        }

    }
}