using System;

namespace Adf
{
    /// <summary>
    /// 线程安全队列
    /// </summary>
    public class SyncQueue : IDisposable
    {
        System.Collections.Queue queue;
        System.Threading.AutoResetEvent waitHandler;
        bool disposed = false;

        /// <summary>
        /// 当前队列数
        /// </summary>
        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        public SyncQueue()
        {
            this.waitHandler = new System.Threading.AutoResetEvent(false);
            this.queue = new System.Collections.Queue();
        }

        /// <summary>
        /// 从队列取出一个值
        /// </summary>
        /// <returns></returns>
        public object Dequeue()
        {
            while (this.disposed == false)
            {
                lock (this.queue)
                {
                    if (this.queue.Count > 0)
                    {
                        return this.queue.Dequeue();
                    }
                    else
                    {
                        this.queue.TrimToSize();
                    }
                }
                this.waitHandler.WaitOne();
            }
            return null;
        }

        /// <summary>
        /// 压入队列一个值
        /// </summary>
        /// <param name="obj"></param>
        public void Enqueue(object obj)
        {
            lock (this.queue)
            {
                this.queue.Enqueue(obj);
            }
            this.waitHandler.Set();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            lock (this.queue)
            {
                this.disposed = true;
                this.queue.Clear();
            }
            this.waitHandler.Close();
        }
    }
}