using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// 队列任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class QueueTasks<T> : IDisposable
    {
        bool disposed = false;

        Thread thread = null;
        EventWaitHandle waitEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle endEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle comEventHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        Semaphore semaphore;

        Queue<T> queue = new Queue<T>(32);
        object runningLockObject = new object();

        Action<T> action = null;

        /// <summary>
        /// get wait action count
        /// </summary>
        public int Count
        {
            get { return this.queue.Count; }
        }

        /// <summary>
        /// get or set max task thread count
        /// </summary>
        [Obsolete("obsolete, please use MaxThreadCount", true)]
        public int MaxTaskThreadCount
        {
            get { return this.maxThreadCount; }
            set { this.maxThreadCount = value; }
        }

        int maxThreadCount = 0;
        /// <summary>
        /// get max thread count
        /// </summary>
        public int MaxThreadCount
        {
            get { return this.maxThreadCount; }
        }

        int runningCount = 0;
        /// <summary>
        /// get current running thread count
        /// </summary>
        public int RunningThreadCount
        {
            get { return this.runningCount; }
        }

        /// <summary>
        /// get completed wait handle
        /// </summary>
        [Obsolete("obsolete, please use WaitCompleted method.", true)]
        public WaitHandle CompletedWaitHandle
        {
            get { return this.comEventHandle; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxThreadCount">max run thread size,proposal less than or equal 64</param>
        /// <exception cref="ArgumentNullException">action is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxTaskThreadCount Less than zero</exception>
        public QueueTasks(Action<T> action, int maxThreadCount)
        {
            if (action == null)
            {
                throw new ArgumentNullException("taskAction");
            }

            if (maxThreadCount < 0)
            {
                throw new ArgumentOutOfRangeException("max thread count must be greater than zero");
            }

            this.action = action;
            this.maxThreadCount = maxThreadCount;
            this.semaphore = new Semaphore(maxThreadCount, maxThreadCount);

            //
            this.thread = new Thread(this.Processor);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        /// <summary>
        /// 添加新任务
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (this.disposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            lock (this.queue)
            {
                this.queue.Enqueue(item);
                this.waitEventHandle.Set();
            }
        }

        private void Processor()
        {
            while (true)
            {
                this.waitEventHandle.WaitOne();
                //have item notify
                this.comEventHandle.Reset();

                T item = default(T);

                lock (this.queue)
                {
                    if (this.queue.Count == 0)
                    {
                        if (this.disposed == true)
                        {
                            //exit
                            this.comEventHandle.Set();
                            break;
                        }

                        //wait next
                        this.waitEventHandle.Reset();
                        continue;
                    }

                    //get current
                    item = this.queue.Dequeue();
                }

                //
                this.semaphore.WaitOne();
                System.Threading.ThreadPool.QueueUserWorkItem(this.Run, item);
            }
            
            //wait run completed
            this.comEventHandle.WaitOne();

            //
            this.endEventHandle.Set();
        }

        private void Run(object state)
        {
            var item = (T)state;
            var semaphoreCount = 0;
            System.Threading.Interlocked.Increment(ref this.runningCount);
            try
            {
                this.action(item);
            }
            finally
            {
                semaphoreCount = this.semaphore.Release() + 1;
                System.Threading.Interlocked.Decrement(ref this.runningCount);
            }

            if (semaphoreCount == this.maxThreadCount)
            {
                //notify completed
                this.comEventHandle.Set();
            }
        }

        /// <summary>
        /// 清空未执行完成的任务
        /// </summary>
        public void Clear()
        {
            lock (this.queue)
            {
                this.queue.Clear();
            }
        }

        /// <summary>
        /// 等待所有任务完成
        /// </summary>
        public void WaitCompleted()
        {
            this.WaitCompleted(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// 等待所有任务完成
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间</param>
        /// <returns>是否已完成</returns>
        public bool WaitCompleted(int millisecondsTimeout)
        {
            var result = this.comEventHandle.WaitOne(millisecondsTimeout);
            return result;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            if (this.disposed == false)
            {
                this.disposed = true;
                //
                this.waitEventHandle.Set();
                this.endEventHandle.WaitOne();
                //
                this.waitEventHandle.Close();
                this.endEventHandle.Close();
                this.comEventHandle.Close();
                //
                this.semaphore.Close();
            }
        }
    }
}