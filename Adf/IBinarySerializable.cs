using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 可序列化标记接口
    /// </summary>
    public interface IBinarySerializable
    { 
        /// <summary>
        /// 序例化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        byte[] Serialize(object value);
        
        /// <summary>
        /// 返序例化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        object Deserialize(Type type,byte[] data);
    }
}
