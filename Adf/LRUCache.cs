using System;
using System.Collections.Generic;

namespace Adf
{
    /// <summary>
    /// LRU缓存处理项（无线程安全，若需要则使用时处理）lru cache handler
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LRUCache<TKey, TValue>
    {
        private int capacity;
        private readonly Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> dictionary;
        private readonly LinkedList<LRUCacheItem<TKey, TValue>> list;

        /// <summary>
        /// 获取或设置缓存最大项数，最小值 10
        /// <para>Get or set the maximum number of entries in the cache. min value 10</para>
        /// </summary>
        public int Capacity
        {
            get { return this.capacity + 1; }
            set
            {
                if (capacity < 10)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be greater than ten.");
                }
                this.capacity = value - 1;
            }
        }

        /// <summary>
        /// 获取当前缓存中的项（若有具备TTL的项时该值为参考值，可能包含部份已过期项）
        /// <para>Get all cache item count.</para>
        /// </summary>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// 初始化新的实例，initialize lru cache
        /// </summary>
        /// <param name="capacity"></param>
        public LRUCache(int capacity)
        {
            if (capacity < 10)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity must be greater than ten.");
            }

            this.capacity = capacity - 1;
            this.dictionary = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>(capacity);
            this.list = new LinkedList<LRUCacheItem<TKey, TValue>>();
        }

        /// <summary>
        /// 获取一个缓存项，get a item
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    TValue value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return value;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    TValue value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return value;
                }
            }
            return default(TValue);
        }
        
        /// <summary>
        /// 获取一个缓存项，get a item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return true;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// 判断一个缓存项是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(TKey key)
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    return true;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 添加一个缓存项，若键已存在则添加失败，add a new item,  if exists to failure
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl">缓存时间，零值永不过期,单位：秒 <para>ttl, zero is no expired, unit: seconds.</para></param>
        /// <returns>success: true, ,key exists: false </returns>
        public bool Add(TKey key, TValue value, int ttl)
        {
            int endTick = 0;

            //capacity - 1 differ

            if (this.dictionary.Count > this.capacity)
            //if (cachedObjects.Count >= capacity)
            {
                this.RemoveLastUsed();
            }

            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    return false;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    return false;
                }
                else //update
                {
                    if (ttl > 0)
                    {
                        endTick = Environment.TickCount + ttl;
                    }

                    this.list.Remove(node);
                    this.list.AddLast(node);

                    node.Value.ItemValue = value;
                    node.Value.EndTick = endTick;

                    return true;
                }
            }

            if (ttl > 0)
            {
                endTick = Environment.TickCount + ttl;
            }

            LRUCacheItem<TKey, TValue> cacheItem = new LRUCacheItem<TKey, TValue>(key, value, endTick);
            node = new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);
            this.list.AddLast(node);
            this.dictionary.Add(key, node);

            return true;
        }

        /// <summary>
        /// 设置一个缓存项，若键已存在则将覆盖，add a new item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl">缓存时间，零值永不过期,单位：秒 <para>ttl, zero is no expired, unit: seconds.</para></param>
        public void Set(TKey key, TValue value, int ttl)
        {
            int endTick = 0;
            if (ttl > 0)
            {
                endTick = Environment.TickCount + ttl;
            }

            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                this.list.Remove(node);
                this.list.AddLast(node);

                node.Value.ItemValue = value;
                node.Value.EndTick = endTick;
            }
            else
            {
                //capacity - 1 differ
                if (this.dictionary.Count > this.capacity)
                //if (cachedObjects.Count >= capacity)
                {
                    this.RemoveLastUsed();
                }

                LRUCacheItem<TKey, TValue> cacheItem = new LRUCacheItem<TKey, TValue>(key, value, endTick);
                node = new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);
                this.list.AddLast(node);
                this.dictionary.Add(key, node);
            }
        }

        /// <summary>
        /// 移去一个缓存项 remove a item
        /// </summary>
        /// <param name="key"></param>
        /// <returns>找到并成功移除返回true, 否则返回 false, success: true, not exists: false</returns>
        public bool Delete(TKey key)
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                this.list.Remove(node);
                this.dictionary.Remove(key);

                return true;
            }

            return false;
        }

        private TValue RemoveLastUsed()
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node = list.First;

            this.list.RemoveFirst();
            this.dictionary.Remove(node.Value.ItemKey);

            return node.Value.ItemValue;
        }

        /// <summary>
        /// 清空所有项存项 clear all items
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();
            this.list.Clear();
        }
    }

    class LRUCacheItem<K, V>
    {
        public K ItemKey;
        public V ItemValue;
        public int EndTick;

        public LRUCacheItem(K key, V value, int endTick)
        {
            ItemKey = key;
            ItemValue = value;
            this.EndTick = endTick;
        }
    }

    /// <summary>
    /// LRU缓存处理项（无线程安全，若需要则使用时处理）lru cache handler
    /// </summary>
    public class LRUCache : ICache
    {
        private int capacity;
        private readonly Dictionary<string, LinkedListNode<LRUCacheItemObject>> dictionary;
        private readonly LinkedList<LRUCacheItemObject> list;

        /// <summary>
        /// 获取或设置缓存最大项数，值不能小于10
        /// <para>Get or set the maximum number of entries in the cache. min value 10</para>
        /// </summary>
        public int Capacity
        {
            get { return this.capacity + 1; }
            set
            {
                if (capacity < 10)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be greater than ten.");
                }
                this.capacity = value - 1;
            }
        }

        /// <summary>
        /// 获取当前缓存中的项（若有具备TTL的项时该值为参考值，可能包含部份已过期项）
        /// <para>Get all cache item count.</para>
        /// </summary>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// 初始化新的实例，initialize lru cache
        /// </summary>
        /// <param name="capacity"></param>
        public LRUCache(int capacity)
        {
            if (capacity < 10)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity must be greater than ten.");
            }

            this.capacity = capacity - 1;
            this.dictionary = new Dictionary<string, LinkedListNode<LRUCacheItemObject>>(capacity);
            this.list = new LinkedList<LRUCacheItemObject>();
        }

        /// <summary>
        /// 获取一个缓存项，get a item
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    object value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return value;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    object value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取一个缓存项，get a item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(string key, out object value)
        {
            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return true;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    value = node.Value.ItemValue;
                    this.list.Remove(node);
                    this.list.AddLast(node);
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// 判断一个缓存项是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    return true;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 添加一个缓存项，若键已存在则添加失败，add a new item,  if exists to failure
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl">缓存时间，零值永不过期,单位：秒 <para>ttl, zero is no expired, unit: seconds.</para></param>
        /// <returns>success: true, ,key exists: false </returns>
        public bool Add(string key, object value, int ttl)
        {
            int endTick = 0;

            //capacity - 1 differ

            if (this.dictionary.Count > this.capacity)
            //if (cachedObjects.Count >= capacity)
            {
                this.RemoveLastUsed();
            }

            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                if (node.Value.EndTick == 0)
                {
                    return false;
                }
                else if (Environment.TickCount - node.Value.EndTick < 0)
                {
                    return false;
                }
                else //update
                {
                    if (ttl > 0)
                    {
                        endTick = Environment.TickCount + ttl;
                    }

                    this.list.Remove(node);
                    this.list.AddLast(node);

                    node.Value.ItemValue = value;
                    node.Value.EndTick = endTick;

                    return true;
                }
            }

            if (ttl > 0)
            {
                endTick = Environment.TickCount + ttl;
            }

            LRUCacheItemObject cacheItem = new LRUCacheItemObject(key, value, endTick);
            node = new LinkedListNode<LRUCacheItemObject>(cacheItem);
            this.list.AddLast(node);
            this.dictionary.Add(key, node);

            return true;
        }

        /// <summary>
        /// 设置一个缓存项，若键已存在则将覆盖，add a new item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl">缓存时间，零值永不过期,单位：秒 <para>ttl, zero is no expired, unit: seconds.</para></param>
        public void Set(string key, object value, int ttl)
        {
            int endTick = 0;
            if (ttl > 0)
            {
                endTick = Environment.TickCount + ttl;
            }

            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                this.list.Remove(node);
                this.list.AddLast(node);

                node.Value.ItemValue = value;
                node.Value.EndTick = endTick;
            }
            else
            {
                //capacity - 1 differ
                if (this.dictionary.Count > this.capacity)
                //if (cachedObjects.Count >= capacity)
                {
                    this.RemoveLastUsed();
                }

                LRUCacheItemObject cacheItem = new LRUCacheItemObject(key, value, endTick);
                node = new LinkedListNode<LRUCacheItemObject>(cacheItem);
                this.list.AddLast(node);
                this.dictionary.Add(key, node);
            }
        }

        /// <summary>
        /// 移去一个缓存项 remove a item
        /// </summary>
        /// <param name="key"></param>
        /// <returns>找到并成功移除返回true, 否则返回 false, success: true, not exists: false</returns>
        public bool Delete(string key)
        {
            LinkedListNode<LRUCacheItemObject> node;
            if (this.dictionary.TryGetValue(key, out node))
            {
                this.list.Remove(node);
                this.dictionary.Remove(key);

                return true;
            }

            return false;
        }

        private void RemoveLastUsed()
        {
            LinkedListNode<LRUCacheItemObject> node = list.First;

            this.list.RemoveFirst();
            this.dictionary.Remove(node.Value.ItemKey);

            //return node.Value.ItemValue;
        }

        /// <summary>
        /// 清空所有项存项 clear all items
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();
            this.list.Clear();
        }

        string ICache.Get(string key)
        {
            return (string)this.Get(key);
        }

        /// <summary>
        /// 获取指定类型的缓存对象(此对象不支持该方法)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object ICache.Get(string key, Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 获取指定类型的缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var v = this.Get(key);
            if (v == null)
                return default(T);

            return (T)v;
        }

        void ICache.Delete(string key)
        {
            this.Delete(key);
        }
    }


    class LRUCacheItemObject
    {
        public string ItemKey;
        public object ItemValue;
        public int EndTick;

        public LRUCacheItemObject(string key, object value, int endTick)
        {
            ItemKey = key;
            ItemValue = value;
            this.EndTick = endTick;
        }
    }
}