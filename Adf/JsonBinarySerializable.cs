using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Json 二进制序列化实例
    /// </summary>
    public class JsonBinarySerializable : IBinarySerializable
    {
        /// <summary>
        /// 获取以UTF-8编码的默认实例对象
        /// </summary>
        public static readonly JsonBinarySerializable DefaultInstance = new JsonBinarySerializable(Encoding.UTF8);


        Encoding encoding;

        /// <summary>
        /// 获取编码字符集
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="encoding"></param>
        public JsonBinarySerializable(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// 将指定对象转换为JSON字节码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize(object value)
        {
            if (value == null)
                return null;

            return this.encoding.GetBytes(JsonHelper.Serialize(value));
        }

        /// <summary>
        /// 返序列化JSON字节码为对象
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (data == null || data.Length == 0)
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);

                return null;
            }

            return JsonHelper.Deserialize(type, this.encoding.GetString(data));
        }
    }
}
