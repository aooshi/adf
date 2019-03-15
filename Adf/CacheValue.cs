using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 实例Cache类型
    /// </summary>
    public abstract class CacheValue<T> where T : class
    {
        private bool isInit = false;
        private T value = null;
        private long expireTimestamp = 0L;
        private DateTime expire;
        
        /// <summary>
        /// 实始化实例
        /// </summary>
        protected CacheValue()
        {
            this.Expire = DateTime.MaxValue; 
        }

        /// <summary>
        /// 获取或设置失效时间
        /// </summary>
        public DateTime Expire
        {
            get
            {
                return expire;
            }
            set
            {
                expire = value;
                this.expireTimestamp = value.Ticks;
            }
        }

        /// <summary>
        /// 获取数据初始时间
        /// </summary>
        public DateTime InitTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取结果是否为Null
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this.Value == null;
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public T Value
        {
            private set
            {
                this.value = value;
            }
            get
            {
                return this.GetValue(true);
            }
        }

        /// <summary>
        /// 获取值并指定是否验证过期
        /// </summary>
        /// <param name="isValidExpire">是否验证过期</param>
        public T GetValue(bool isValidExpire)
        {
            if (!this.isInit)
            {
                this.OnInit();
            }
            else if (isValidExpire && DateTime.Now.Ticks > this.expireTimestamp)
            {
                this.OnInit();
            }
            return this.value;
        }

        /// <summary>
        /// 移除实例值
        /// </summary>
        public virtual void Remove()
        {
            this.Value = null;
            this.isInit = false;
        }

        /// <summary>
        /// 触发初始化
        /// </summary>
        public void OnInit()
        {
            this.Value = this.Init();
            this.isInit = true;
            this.InitTime = DateTime.Now;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        protected abstract T Init();
    }
}
