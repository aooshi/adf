using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static readonly T Instance = new T();
    }
}
