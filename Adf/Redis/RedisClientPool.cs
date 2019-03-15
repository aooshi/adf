using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Configuration;

namespace Adf
{
    /// <summary>
    /// Redis Cache 管理器
    /// </summary>
    public class RedisClientPool : ICache
    {
        const string CONFIG_NAME = "RedisCache";

        /// <summary>
        /// 获取池管理对象
        /// </summary>
        public Pool<RedisClient> Pool
        {
            get;
            protected set;
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
        /// 初始化新实例
        /// </summary>
        public RedisClientPool()
            : this(CONFIG_NAME)
        {
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="configName"></param>
        public RedisClientPool(string configName)
        {
            this.Name = configName;
            this.MemberPoolSize = ConfigHelper.GetSettingAsInt(string.Concat(configName, "PoolSize"), 800);
            this.Init(configName);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="configName"></param>
        protected virtual void Init(string configName)
        {
            var config = (IpGroupSection)System.Configuration.ConfigurationManager.GetSection(configName);
            if (config == null)
                throw new ConfigException("No Find ConfigSection" + configName);
            var ipcount = config.IpList.Count;
            var poolMembers = new RedisClientPoolMember[ipcount];
            for (int i = 0; i < ipcount; i++)
                poolMembers[i] = new RedisClientPoolMember(config.IpList[i].Ip, config.IpList[i].Port);
            //
            this.Pool = new Pool<RedisClient>(this.MemberPoolSize, poolMembers, config.Hash);
        }

        /// <summary>
        /// 设置一个字符值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        public void Set(string key, string value, int expires)
        {
            this.Pool.Call(pi =>
            {
                pi.Set(key, value);
                if (expires > 0)
                    pi.Expire(key, expires);
            }, key,null);
        }

        /// <summary>
        /// 设置一个字符值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        public void Set(string key, object value, int expires)
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
                result = pi.Get(key);
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
        public void Delete(string key)
        {
            this.Pool.Call(pi =>
            {
                pi.Remove(key);
            }, key,null);
        }
    }

    /// <summary>
    /// 以 RedisCache 为配置的默认单例实例
    /// </summary>
    public sealed class RedisClientDefault
    {
        /// <summary>
        /// 一个 RedisCache 实例
        /// </summary>
        public static readonly RedisClientPool Instance = new RedisClientPool("RedisCache");
    }
}