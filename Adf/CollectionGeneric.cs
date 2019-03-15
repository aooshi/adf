using System;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 通用泛型集合基类
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public abstract class CollectionGeneric<T> : CollectionBase
    {
        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        /// <param name="index">索引</param>
        public T this[int index]
        {
            get
            {
                return (T)base.InnerList[index];
            }
        }

        /// <summary>
        /// 添加一个新元素
        /// </summary>
        /// <param name="item">元素</param>
        public virtual void Add(T item)
        {
            base.InnerList.Add(item);
        }

        /// <summary>
        /// 添加一组新元素
        /// </summary>
        /// <param name="arritem">元素组</param>
        public virtual void AddRange(T[] arritem)
        {
            base.InnerList.AddRange(arritem);
        }

        /// <summary>
        /// 移除指定的元素
        /// </summary>
        /// <param name="item">元素</param>
        public virtual void Remove(T item)
        {
            base.InnerList.Remove(item);
        }

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index">移除指定索引处的元素</param>
        public virtual void Remove(int index)
        {
            base.InnerList.RemoveAt(index);
        }

    }
}
