using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// Object Object
    /// 对象缓存器, 将一组对象拆分为: 盒(box)与单个对象(object)，进行缓存
    /// 此方法不适合一组对象过大的情况
    /// </summary>
    public abstract class ObjectCache<T> : IObjectCache
    {
        /// <summary>
        /// 支持同时获取缓存的线程数，默认： 5
        /// </summary>
        public virtual int ThreadCount
        {
            get;
            set;
        }
        /// <summary>
        /// 进行多线程处理时的数据值阀值,默认：3
        /// </summary>
        protected virtual int MultiThreadThreshold
        {
            get;
            set;
        }
        /// <summary>
        /// 单个对象过期时间,默认：300秒
        /// </summary>
        public virtual int ObjectExpires
        {
            get;
            set;
        }
        /// <summary>
        /// 盒子储过期时间,默认：300秒
        /// </summary>
        public virtual int BoxExpires
        {
            get;
            set;
        }
        /// <summary>
        /// 获取缓存实例
        /// </summary>
        public abstract ICache Cache
        {
            get;
        }
        /// <summary>
        /// 联动保持，当为true时确保任一对象失效时盒子也失效, 默认 false
        /// 启用此值在会导致缓存对象写入频率增加，应谨慎设置
        /// </summary>
        public virtual bool KeepGanged
        {
            get;
            set;
        }

        /// <summary>
        /// 初始新实例
        /// </summary>
        public ObjectCache()
        {
            this.ThreadCount = 5;
            this.MultiThreadThreshold = 3;
            this.BoxExpires = 300;
            this.ObjectExpires = 300;
            this.KeepGanged = false;
        }

        /// <summary>
        /// 根据缓存键从数据源获取单项数据
        /// </summary>
        /// <param name="objectCacheKey"></param>
        /// <returns></returns>
        protected abstract T GetObjectSource(string objectCacheKey);
        /// <summary>
        /// 根据缓存键从数据源获取多项数据
        /// </summary>
        /// <param name="objectCacheKeys"></param>
        /// <returns></returns>
        protected abstract IList<T> GetObjectListSource(string[] objectCacheKeys);
        /// <summary>
        /// 生成单项缓存键
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string BuildObjectCacheKey(T value);
        /// <summary>
        /// 根据数据存储该数据缓存盒
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="boxCacheKey">缓存盒键</param>
        /// <returns></returns>
        public virtual void SaveBox(string boxCacheKey, IEnumerable data)
        {
            if (data == null)
            {
                //null
                //this.Cache.Set(boxCacheKey, new string[0], this.BoxExpires);
            }
            else
            {
                var objectCacheKeylist = new List<string>(10);
                var ie = data.GetEnumerator();
                while (ie.MoveNext())
                {
                    var objectCacheKey = this.BuildObjectCacheKey(ie.Current);
                    objectCacheKeylist.Add(objectCacheKey);

                    //在保持状态下，盒子保存，则对象也应保存，否则将会出现不能保持的情况
                    if (this.KeepGanged)
                        this.Cache.Set(objectCacheKey, ie.Current, this.ObjectExpires);

                }
                this.Cache.Set(boxCacheKey, objectCacheKeylist.ToArray(), this.BoxExpires);
            }
        }
        /// <summary>
        /// 执行一个缓存处理
        /// </summary>
        /// <param name="boxCacheKey">缓存盒键</param>
        /// <returns>null is not exist</returns>
        public virtual T[] GetData(string boxCacheKey)
        {
            string[] objectCacheKeys = this.Cache.Get<string[]>(boxCacheKey);
            if (objectCacheKeys == null)
            {
                ////create data
                //objectCacheKeys = boxCreator();
                //if (objectCacheKeys != null)
                //    this.Cache.Set(boxCacheKey, objectCacheKeys, this.BoxExpires);

                return null;
            }
            return this.GetObjectList(objectCacheKeys);
        }
        /// <summary>
        /// 根据缓存值获取数据
        /// </summary>
        /// <param name="objectCacheKeys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">data no match parameters objectCacheKeys</exception>
        public T[] GetObjectList(string[] objectCacheKeys)
        {
            if (objectCacheKeys == null)
                return null;

            var length = objectCacheKeys.Length;
            T[] result = new T[length];
            if (length == 0)
            {
            }
            else if (length == 1)
            {
                result[0] = this.GetObject(objectCacheKeys[0]);
            }
            else
            {
                Dictionary<string, int> nocaches = new Dictionary<string, int>(length);
                var multiThread = length >= this.MultiThreadThreshold;
                //more thread
                if (multiThread)
                {
                    //缓存获取数据
                    ThreadTasks.ProcessTask(length, this.ThreadCount, null, (object state, int index) =>
                    {
                        var objectCacheKey = objectCacheKeys[index];
                        var obj = this.Cache.Get(objectCacheKey, typeof(T));
                        if (obj != null)
                        {
                            result[index] = (T)obj;
                        }
                        else
                        {
                            nocaches.Add(objectCacheKey, index);
                        }
                    });
                    //子元素失效，不能保持则直接输出
                    if (this.KeepGanged && nocaches.Count > 0)
                        return null;
                }
                //single thread
                else
                {
                    //缓存获取数据
                    for (int index = 0; index < length; index++)
                    {
                        var objectCacheKey = objectCacheKeys[index];
                        var obj = this.Cache.Get(objectCacheKey, typeof(T));
                        if (obj != null)
                        {
                            result[index] = (T)obj;
                        }
                        else
                        {
                            if (this.KeepGanged)
                                return null;//子元素失效，不能保持则直接输出
                            nocaches.Add(objectCacheKey, index);
                        }
                    }
                }
                //从数据源获取 未在缓存中匹配到的数据
                if (nocaches.Count > 0)
                {
                    string[] keys = new string[nocaches.Keys.Count];
                    nocaches.Keys.CopyTo(keys, 0);
                    //
                    var sourceData = this.GetObjectListSource(keys);
                    var index = 0;
                    multiThread = multiThread && sourceData.Count >= this.MultiThreadThreshold;
                    //set value
                    foreach (var item in sourceData)
                    {
                        var objectCacheKey = this.BuildObjectCacheKey(item);
                        if (!nocaches.TryGetValue(objectCacheKey, out index))
                            throw new ArgumentOutOfRangeException(objectCacheKey + " no match parameters objectCacheKeys");
                                                
                        //set value
                        result[index] = item;
                        //set cache
                        if (!multiThread)
                            this.Cache.Set(objectCacheKey, item, this.ObjectExpires);
                    }
                    //ste cache to multiThread
                    if (multiThread)
                    {
                        //缓存获取数据
                        ThreadTasks.ProcessTask(sourceData.Count, this.ThreadCount, null, (object state, int index2) =>
                        {
                            var objectCacheKey = this.BuildObjectCacheKey(sourceData[index2]);
                            this.Cache.Set(objectCacheKey, sourceData[index2], this.ObjectExpires);
                        });
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 根据缓存键获取单项数据
        /// </summary>
        /// <param name="objectCacheKey"></param>
        /// <returns></returns>
        public T GetObject(string objectCacheKey)
        {
            var obj = this.Cache.Get(objectCacheKey, typeof(T));
            if (obj == null)
            {
                var data = this.GetObjectSource(objectCacheKey);
                this.Cache.Set(objectCacheKey, data, this.ObjectExpires);
                return data;
            }
            return (T)obj;
        }
        /// <summary>
        /// 生成单项缓存键
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string BuildObjectCacheKey(object value)
        {
            return this.BuildObjectCacheKey((T)value);
        }


        object IObjectCache.GetObject(string objectCacheKey)
        {
            return this.GetObject(objectCacheKey);
        }
        IEnumerable IObjectCache.GetObjectList(string[] objectCacheKeys)
        {
            return this.GetObjectList(objectCacheKeys);
        }
        object IObjectCache.GetData(string boxCacheKey)
        {
            return this.GetData(boxCacheKey);
        }
    }
}
