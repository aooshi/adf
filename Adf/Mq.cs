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
    /// 任务队列管理器
    /// </summary>
    public class Mq : IDisposable
    {
        private MessageQueue[] messageQueue;
        private LogManager logger;
        private bool initLogger = false;
        private bool disposed = false;
        private int queueCount;
        private int queueIndex;
        private static TimeSpan errorReceiveSleep = TimeSpan.FromSeconds(5);
        private int receiveRuningCount = 0;

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get;
         private    set;
        }

        /// <summary>
        /// 是否正在接收
        /// </summary>
        public bool Receiving
        {
            get;
            private set;
        }

        /// <summary>
        /// 异常时重连间隔，单位：秒，默认:60seconds
        /// </summary>
        public int ReconnectInterval
        {
            get;
            private set;
        }

        int availableThread;
        
        /// <summary>
        /// initialize a instance
        /// </summary>
        /// <param name="name"></param>
        public Mq(string name)
            : this(name,new LogManager(name))
        {
            this.initLogger = true;
        }
        
        /// <summary>
        /// initialize a instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="logger"></param>
        public Mq(string name, LogManager logger)
        {
            var configs = ((Config.IpGroupSection)ConfigurationManager.GetSection(name));
            if (configs == null)
            {
                throw new ConfigurationErrorsException("not find config " + name);
            }
            this.Name = name;
            this.Receiving = false;
            this.ReconnectInterval = 60000;
            var servers = configs.IpList;
            this.messageQueue = new MessageQueue[servers.Count];
            for (int i = 0, l = servers.Count; i < l; i++)
            {
                this.messageQueue[i] = new MessageQueue(servers[i].Ip);
                //if (!MessageQueue.Exists(servers[i].Ip))
                //    throw new ApplicationException(string.Format("Not Find Queue. {0}",servers[i].Ip));
            }
            this.queueCount = this.messageQueue.Length;
            this.queueIndex = 0;
            //
            this.logger = logger;
        }
        
        /// <summary>
        /// 可用接收线程数
        /// </summary>
        public int GetAvailableThread()
        {
            return this.availableThread;
        }


        /// <summary>
        /// 发送
        /// </summary>
        public void Send<T>(T message)
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            var m = new Message(message);
            if (this.queueCount == 1)
            {
                this.messageQueue[0].Send(m);
                return;
            }

            var count = this.queueCount;
            lock (this.messageQueue)
            {
                while (true)
                {
                    try
                    {
                        var index = this.queueIndex++;
                        if (this.queueIndex == this.queueCount)
                            this.queueIndex = 0;

                        this.messageQueue[index].Send(m);
                        break;
                    }
                    catch (Exception e)
                    {
                        this.logger.Exception(e);
                        if (count == 1) 
                            throw;
                    }
                    count--;
                }
            }
        }
        
        /// <summary>
        /// 异步接收
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxThreadSize"></param>
        /// <exception cref="MqException">MqException 已调用过 Receive了</exception>
        /// <exception cref="System.Messaging.MessageQueueException"> System.Messaging.MessageQueueException</exception>
        public void Receive<T>(int maxThreadSize, Action<T> action)
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.Receiving)
                throw new MqException("Has Been Received.");
            
            this.Receiving = true;
            this.availableThread = maxThreadSize;
            var results = new IAsyncResult[ this.messageQueue.Length ];
            try
            {
                for (var i = 0; i < this.messageQueue.Length; i++)
                {
                    this.messageQueue[i].ReceiveCompleted += new ReceiveCompletedEventHandler(ReceiveCompleted<T>);
                    results[i] = this.messageQueue[i].BeginReceive(MessageQueue.InfiniteTimeout, new ReceiveState<T>()
                    {
                        Action = action,
                        Count = 0,
                        MaxCount = maxThreadSize,
                        ArrayIndex = i
                    });
                }
            }
            catch
            {
                foreach (var result in results)
                {
                    if (result != null)
                        result.AsyncWaitHandle.Close();
                }

                this.Receiving = false;

                throw;
            }
        }

        /// <summary>
        /// 完成一个接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReceiveCompleted<T>(object sender, ReceiveCompletedEventArgs e)
        {
            var mq = (MessageQueue)sender;
            var state = (ReceiveState<T>)e.AsyncResult.AsyncState;
            Message message = null;
            try
            {
                message = mq.EndReceive(e.AsyncResult);
            }
            catch (MessageQueueException exception)
            {
                //已dispose
                if (this.disposed)
                    return;

                this.logger.Exception(exception);

                //只要出现 队列错误，均视为队列失效，重建队列
                mq = this.ReconnectQueue<T>(mq, state.ArrayIndex, out message);
            }
            catch (Exception exception)
            {
                this.logger.Exception(exception);
                message = null;
            }
            //
            if (message != null)
            {
                lock (state)
                {
                    state.Count++;
                    Interlocked.Decrement(ref this.availableThread);
                    //run
                    new Thread(() => {
                        Interlocked.Increment(ref this.receiveRuningCount);
                        try
                        {
                            if (message != null)
                            {
                                message.Formatter = new XmlMessageFormatter(new[] { typeof(T) });
                                state.Action((T)message.Body);
                            }
                        }
                        catch (Exception exception)
                        {
                            this.logger.Exception(exception);
                        }
                        finally
                        {
                            lock (state)
                            {
                                state.Count--;
                                Interlocked.Increment(ref this.availableThread);
                                Monitor.Pulse(state);
                            }
                            Interlocked.Decrement(ref this.receiveRuningCount);
                        }
                    }).Start();
                    if (state.Count == state.MaxCount)
                    {
                        Monitor.Wait(state);
                    }
                }
            }

            //new receive
            while (this.Receiving)
            {
                try
                {
                    mq.BeginReceive(MessageQueue.InfiniteTimeout, state);
                    break;
                }
                catch (Exception exception)
                {
                    this.logger.Exception(exception);
                    Thread.Sleep(errorReceiveSleep);
                }
            }  
        }

        /// <summary>
        /// 重建一个失败的队列
        /// </summary>
        /// <param name="mq"></param>
        /// <param name="queueIndex">队列所在索引</param>
        /// <param name="message">接收到的消息</param>
        /// <returns></returns>
        private MessageQueue ReconnectQueue<T>(MessageQueue mq, int queueIndex,out Message message)
        {
            this.logger.Warning.WriteTimeLine("Queue Error, Reconnect: {0}",mq.Path);
            //Reconnect
            MessageQueue newmq = null;
            message = null;
            while (this.Receiving)
            {
                try
                {
                    newmq = new MessageQueue(mq.Path);
                    message = newmq.Receive(MessageQueue.InfiniteTimeout);
                    break;
                }
                catch (Exception exception)
                {
                    this.logger.Warning.WriteTimeLine("Reconnect Fail, after {2} seconds to continue: {0},{1}", exception.GetType(), exception.Message, this.ReconnectInterval);
                    //
                    try { newmq.Dispose(); }
                    catch { }
                    //
                    Thread.Sleep(this.ReconnectInterval);
                    //Thread.Sleep(5000);
                    continue;
                }
            }
            this.logger.Warning.WriteTimeLine("Reconnect Success: {0}", mq.Path);
            //replace && destry old
            newmq.ReceiveCompleted += new ReceiveCompletedEventHandler(ReceiveCompleted<T>);
            this.messageQueue[queueIndex] = newmq;
            try { mq.Dispose(); }
            catch { }
            //return
            return newmq;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Receiving = false;
                this.disposed = true;

                //释放队列
                foreach (var m in this.messageQueue)
                {
                    m.Dispose();
                }

                if (this.initLogger)
                {
                    //释放日志
                    this.logger.Dispose();
                }
                else
                {
                    //刷新日志
                    this.logger.Flush();
                }

                //确保所有接收器均已完成
                while (this.receiveRuningCount > 0)
                    Thread.Sleep(10);
            }
        }

        class ReceiveState<T>
        {
            public int ArrayIndex;
            public int Count;
            public int MaxCount;
            public Action<T> Action;
        }

    }
}
