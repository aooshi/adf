using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Adf
{
    /// <summary>
    /// 二进制序例化处理器
    /// </summary>
    public class BinarySerializable : IBinarySerializable
    {
        /// <summary>
        /// 默认的实例
        /// </summary>
        public readonly static BinarySerializable Instance = new BinarySerializable();

        /// <summary>
        /// 将对象序例化为二进制
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize(object value)
        {
            if (value == null)
                return null;

            IFormatter formatter = new BinaryFormatter();
            using (var m = new MemoryStream(128))
            {
                formatter.Serialize(m, value);
                return m.ToArray();
            }
        }

        /// <summary>
        /// 将数组反序例化为对象
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(Type type,byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (data == null || data.Length == 0)
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);

                return null;
            }

            IFormatter formatter = new BinaryFormatter();
            using (var m = new MemoryStream(data.Length))
            {
                m.Write(data, 0, data.Length);
                m.Position = 0;

                var v = formatter.Deserialize(m);
                if (v != null && !v.GetType().Equals(type))
                {
                    return null;
                }
                return v;
            }
        }
    }
}
