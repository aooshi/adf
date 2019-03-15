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
    public sealed class QueueTask<T> : IDisposable
    {
        bool disposed = false;

        Thread thread = null;
        EventWaitHandle waitEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle endEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle comEventHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        Queue<T> queue = new Queue<T>(32);

        Action<T> action = null;
                
        /// <summary>
        /// get wait action count
        /// </summary>
        public int Count
        {
            get { return this.queue.Count; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException">action is null</exception>
        public QueueTask(Action<T> action)
        {
            this.action = action;

            if (action == null)
            {
                throw new ArgumentNullException("taskAction");
            }

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
                            break;
                        }

                        //no item notify
                        this.comEventHandle.Set();

                        //wait next
                        this.waitEventHandle.Reset();
                        continue;
                    }

                    //get current
                    item = this.queue.Dequeue();
                }

                this.action(item);
            }

            this.comEventHandle.Set();
            this.endEventHandle.Set();
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
            }
        }
    }
}