using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// Object Cache 处理器接口
    /// </summary>
    public interface IObjectCache
    {
        /// <summary>
        /// 单个对象过期时间,默认：300秒
        /// </summary>
        int ObjectExpires
        {
            get;
            set;
        }
        /// <summary>
        /// 盒子储过期时间,默认：300秒
        /// </summary>
        int BoxExpires
        {
            get;
            set;
        }
        /// <summary>
        /// 获取缓存实例
        /// </summary>
        ICache Cache
        {
            get;
        }
        /// <summary>
        /// 联动保持，当为true时确保任一对象失效时盒子也失效, 默认 false
        /// </summary>
        bool KeepGanged
        {
            get;
            set;
        }
        /// <summary>
        /// 生成单项缓存键
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string BuildObjectCacheKey(object value);
        /// <summary>
        /// 存储一个缓存盒
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="boxCacheKey">缓存盒键</param>
        /// <returns></returns>
        void SaveBox(string boxCacheKey, IEnumerable data);
        /// <summary>
        /// 根据缓存盒键获取数据
        /// </summary>
        /// <param name="boxCacheKey">缓存盒键</param>
        /// <returns>null is not exist</returns>
        object GetData(string boxCacheKey);
        /// <summary>
        /// 根据缓存键获取对象数据
        /// </summary>
        /// <param name="objectCacheKeys"></param>
        /// <returns></returns>
        IEnumerable GetObjectList(string[] objectCacheKeys);
        /// <summary>
        /// 根据缓存键获取对象数据
        /// </summary>
        /// <param name="objectCacheKey"></param>
        /// <returns></returns>
        object GetObject(string objectCacheKey);
    }
}
