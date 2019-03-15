using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Configuration;

namespace Adf
{
    /// <summary>
    /// Memcache池
    /// </summary>
    public class MemcachePool : ICache
    {
        //MemcacheCache,MemcacheCachePoolSize
        const string CONFIG_NAME = "MemcacheCache";

        /// <summary>
        /// 获取池管理对象
        /// </summary>
        public Pool<Memcache> Pool
        {
            get;
            private set;
        }

        /// <summary>
        /// 配置节点名
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取每成员的最大实例数
        /// </summary>
        public virtual int MemberPoolSize
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 根据默认配置初始化新实例
        /// </summary>
        public MemcachePool()
            : this(CONFIG_NAME)
        {
        }

        /// <summary>
        /// 根据指定配置初始化新实例
        /// </summary>
        /// <param name="configName"></param>
        public MemcachePool(string configName)
        {
            this.Name = configName;
            //
            var config = (IpGroupSection)System.Configuration.ConfigurationManager.GetSection(configName);
            if (config == null)
                throw new ConfigException("No find configuration section " + configName);
            var ipcount = config.IpList.Count;
            var poolMembers = new MemcachePoolMember[ipcount];
            for (int i = 0; i < ipcount; i++)
                poolMembers[i] = new MemcachePoolMember(config.IpList[i].Ip, config.IpList[i].Port);
            //
            this.MemberPoolSize = ConfigHelper.GetSettingAsInt(string.Concat(configName,"PoolSize"), 800);

            //
            this.Pool = new Pool<Memcache>(this.MemberPoolSize, poolMembers, config.Hash);
        }
        
        /// <summary>
        /// 根据指定成员初始化新实例
        /// </summary>
        /// <param name="mpms"></param>
        public MemcachePool(MemcachePoolMember[] mpms)
            : this(mpms, 800, 1)
        {
        }

        /// <summary>
        /// 根据指定成员初始化新实例
        /// </summary>
        /// <param name="mpms"></param>
        /// <param name="memberPoolSize"></param>
        /// <param name="hash">zero disable hash, non-zero enable</param>
        public MemcachePool(MemcachePoolMember[] mpms, int memberPoolSize, int hash)
        {
            this.Name = this.GetType().Name;
            //
            if (mpms == null)
                throw new ArgumentNullException("mpms");
            //
            this.MemberPoolSize = memberPoolSize;

            //
            this.Pool = new Pool<Memcache>(this.MemberPoolSize, mpms, hash);
        }

        #region ICache members

        /// <summary>
        /// 设置一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        void ICache.Set(string key, object value, int expires)
        {
            this.Pool.Call(pi =>
            {
                pi.Set(key, value, expires);
            }, key,null);
        }

        /// <summary>
        /// 获取一个字符值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            string result = null;
            this.Pool.Call(pi =>
            {
                result = pi.Get<string>(key);

            }, key,null);
            return result;
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            T result = default(T);
            this.Pool.Call(pi =>
            {
                result = pi.Get<T>(key);
            }, key,null);
            return result;
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key, Type type)
        {
            object result = null;
            this.Pool.Call(pi =>
            {
                result = pi.Get(key, type);
            }, key, null);
            return result;
        }

        /// <summary>
        /// 移除一个键
        /// </summary>
        /// <param name="key"></param>
        void ICache.Delete(string key)
        {
            this.Pool.Call(pi =>
            {
                pi.Delete(key);
            }, key,null);
        }

        #endregion



        /// <summary>
        /// Deletes an object from cache given cache key.
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <returns><c>true</c>, if the data was deleted successfully</returns>
        public bool Delete(string key)
        {
            return this.Delete(key, 0);
        }

        /// <summary>
        /// Deletes an object from cache given cache key, a delete time, and an optional hashcode.
        /// 
        /// The item is immediately made non retrievable.<br/>
        /// Keep in mind: 
        /// <see cref="Add(string,object)">add(string, object)</see> and <see cref="Replace(string,object)">replace(string, object)</see>
        ///	will fail when used with the same key will fail, until the server reaches the
        ///	specified time. However, <see cref="Set(string,object)">set(string, object)</see> will succeed
        /// and the new value will not be deleted.
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <param name="timeout">seconds or unix-stamp</param>
        /// <returns><c>true</c>, if the data was deleted successfully</returns>
        public bool Delete(string key, long timeout)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Delete(key, timeout);
            }, key, null);

            return result;
        }

        /// <summary>
        /// set expires for a key
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <param name="expires">seconds or unix-stamp</param>
        /// <returns><c>true</c>, if the data was touch successfully</returns>
        public bool Touch(string key, long expires)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Touch(key, expires);
            }, key, null);

            return result;
        }

        /// <summary>
        /// Stores data on the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Set(string key, object value)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Set(key, value);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Stores data on the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Set(string key, object value, long expires)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Set(key, value, expires);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Adds data to the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Add(string key, object value)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Add(key, value);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Adds data to the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Add(string key, object value, long expires)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Add(key, value, expires);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Updates data on the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Replace(string key, object value)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Replace(key, value);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Updates data on the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Replace(string key, object value, long expires)
        {
            var result = false;

            this.Pool.Call(pi =>
            {
                result = pi.Replace(key, value, expires);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Increment the value at the specified key by 1, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Increment(string key)
        {
            long result = 0;

            this.Pool.Call(pi =>
            {
                result = pi.Increment(key);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Increment the value at the specified key by passed in val. 
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <param name="inc">how much to increment by</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Increment(string key, long inc)
        {
            long result = 0;

            this.Pool.Call(pi =>
            {
                result = pi.Increment(key, inc);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Decrement the value at the specified key by 1, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Decrement(string key)
        {
            long result = 0;

            this.Pool.Call(pi =>
            {
                result = pi.Decrement(key);

            }, key, null);

            return result;
        }

        /// <summary>
        /// Decrement the value at the specified key by passed in value, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <param name="inc">how much to increment by</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Decrement(string key, long inc)
        {
            long result = 0;

            this.Pool.Call(pi =>
            {
                result = pi.Decrement(key, inc);

            }, key, null);

            return result;
        }
    }


    /// <summary>
    /// 以 MemcacheCache 为配置的默认单例实例
    /// </summary>
    public sealed class MemcacheDefault
    {
        /// <summary>
        /// 一个 MemcacheCache 实例
        /// </summary>
        public static readonly MemcachePool Instance = new MemcachePool("MemcacheCache");
    }
}
