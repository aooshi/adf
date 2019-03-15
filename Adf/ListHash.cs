using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 哈希数组查找
    /// </summary>
    public class ListHash
    {
        Dictionary<object, int> hashtable;

        /// <summary>
        /// 列表对象
        /// </summary>
        public IList List
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化一个新实例
        /// </summary>
        /// <param name="array"></param>
        /// <param name="converter"></param>
        public ListHash(IList array, Converter<object, object> converter)
        {
            this.List = array;
            this.hashtable = new Dictionary<object,int>(10);
            if (array != null)
            {
                object key;
                int index = 0;
                foreach (var item in array)
                {
                    key = converter(item);
                    if (this.hashtable.ContainsKey(key))
                    {
                        throw new InvalidCastException(string.Concat("convert error, Duplicate key ", key));
                    }
                    this.hashtable.Add(key, index++);
                }
            }
        }

        /// <summary>
        /// 判断一个键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(object key)
        {
            return this.hashtable.ContainsKey(key);
        }

        /// <summary>
        /// 根据键查找一个数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                int index;
                if (this.hashtable.TryGetValue(key, out index))
                {
                    return this.List[index];
                }
                return null;
            }
        }

    }

    /// <summary>
    /// 哈希数组查找
    /// </summary>
    public class ListHash<T,TKey>
    {
        Dictionary<TKey, int> hashtable;

        /// <summary>
        /// 列表对象
        /// </summary>
        public IList<T> List
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化一个新实例
        /// </summary>
        /// <param name="array"></param>
        /// <param name="converter"></param>
        public ListHash(IList<T> array, Converter<T, TKey> converter)
        {
            this.List = array;
            this.hashtable = new Dictionary<TKey, int>(10); 
            if (array != null)
            {
                TKey key;
                int index = 0;
                foreach (var item in array)
                {
                    key = converter(item);
                    if (this.hashtable.ContainsKey(key))
                    {
                        throw new InvalidCastException(string.Concat("convert error, Duplicate key " , key));
                    }
                    this.hashtable.Add(key, index++);
                }
            }
        }

        /// <summary>
        /// 判断一个键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(TKey key)
        {
            return this.hashtable.ContainsKey(key);
        }

        /// <summary>
        /// 根据键查找一个数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T this[TKey key]
        {
            get
            {
                int index;
                if (this.hashtable.TryGetValue(key, out index))
                {
                    return this.List[index];
                }
                return default(T);
            }
        }

    }
}
