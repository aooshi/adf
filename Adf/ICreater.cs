using System;

namespace Adf
{
    /// <summary>
    /// object creater
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICreater<T>
    {
        /// <summary>
        /// create new object
        /// </summary>
        /// <returns></returns>
        T Create();
    }
}
