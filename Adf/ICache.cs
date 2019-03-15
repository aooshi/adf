using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// ICache
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// set cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        void Set(string key, object value, int expires);

        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns>null is not find</returns>
        object Get(string key,Type type);

        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// delete cache
        /// </summary>
        /// <param name="key"></param>
        void Delete(string key);
    }
}
