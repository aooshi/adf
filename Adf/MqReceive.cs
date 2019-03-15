using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Messaging;
using System.IO;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// 任务队列接收器
    /// </summary>
    public abstract class MqReceive<T> : IDisposable
    {
        /// <summary>
        /// MQ
        /// </summary>
        protected Mq Mq
        {
            get;
            private set;
        }

        /// <summary>
        /// 管理器
        /// </summary>
        public virtual LogManager Logger
        {
            get;
            protected set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxThreadSize"></param>
        public MqReceive(string name, int maxThreadSize)
            : this(name,maxThreadSize,new LogManager(name))
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxThreadSize"></param>
        /// <param name="logManager"></param>
        public MqReceive(string name,int maxThreadSize, LogManager logManager)
        {
            this.Name = name;
            this.Logger = logManager;
            this.Mq = new Mq(name,logManager);  
            this.Mq.Receive<T>(maxThreadSize, this.New);
        }

        /// <summary>
        /// 新项
        /// </summary>
        /// <param name="item"></param>
        protected abstract void New(T item);

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.Mq.Dispose();
            this.Logger.Dispose();
        }

    }
}
