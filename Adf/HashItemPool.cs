using System;
using System.Collections.Generic;

namespace Adf
{
    /// <summary>
    /// hash item pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashItemPool<T>
    {
        object lockObject = new object();
        Queue<T>[] queues = null;
        int bucketSize = 0;

        /// <summary>
        /// get bucket size
        /// </summary>
        public int BucketSize
        {
            get { return this.bucketSize; }
        }

        ICreater<T> _creater;
        /// <summary>
        /// get or set creater
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value is null</exception>
        public ICreater<T> Creater
        {
            get { return this._creater; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._creater = value;
            }
        }

        bool isSynchronized = false;
        /// <summary>
        /// get or set synchronize state
        /// </summary>
        public bool IsSynchronized
        {
            get { return this.isSynchronized; }
            set { this.isSynchronized = value; }
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="bucketSize"></param>
        public HashItemPool(int bucketSize)
        {
            if (bucketSize < 1)
                throw new ArgumentOutOfRangeException("bucketSize", "value must than zero.");

            this.bucketSize = bucketSize;

            this.queues = new Queue<T>[bucketSize];

            for (int i = 0; i < bucketSize; i++)
            {
                this.queues[i] = new Queue<T>(8);
            }
        }

        /// <summary>
        /// get a item
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">invalid invoke, no set Creater property.</exception>
        public T Get(int hashCode)
        {
            var bucket = (hashCode & 0x7FFFFFFF) % this.bucketSize;
            var queue = this.queues[bucket];

            if (this.isSynchronized)
            {
                lock (this.lockObject)
                {
                    if (queue.Count == 0)
                    {
                        this.CreateItem(queue);
                    }

                    return queue.Dequeue();
                }
            }


            if (queue.Count == 0)
            {
                this.CreateItem(queue);
            }

            return queue.Dequeue();
        }

        /// <summary>
        /// put a item
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="item"></param>
        public void Put(int hashCode, T item)
        {
            var bucket = (hashCode & 0x7FFFFFFF) % this.bucketSize;
            var queue = this.queues[bucket];

            if (this.isSynchronized)
            {
                lock (this.lockObject)
                {
                    queue.Enqueue(item);
                }
            }
            else
            {
                queue.Enqueue(item);
            }
        }

        private void CreateItem(Queue<T> queue)
        {
            var creater = this._creater;
            if (creater == null)
                throw new InvalidOperationException("invalid invoke, no set Creater property.");

            var item = creater.Create();

            queue.Enqueue(item);
        }

        /// <summary>
        /// for each all item
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            if (this.isSynchronized)
            {
                lock (this.lockObject)
                {
                    for (int i = 0; i < this.bucketSize; i++)
                    {
                        var queue = this.queues[i];
                        var count = queue.Count;

                        for (int j = 0; j < count; j++)
                        {
                            var item = queue.Dequeue();
                            action(item);
                            queue.Enqueue(item);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.bucketSize; i++)
                {
                    var queue = this.queues[i];
                    var count = queue.Count;

                    for (int j = 0; j < count; j++)
                    {
                        var item = queue.Dequeue();
                        action(item);
                        queue.Enqueue(item);
                    }
                }
            }
        }

        /// <summary>
        /// loop all item and remove item
        /// </summary>
        /// <param name="action"></param>
        public void LoopRemove(Action<T> action)
        {
            if (this.isSynchronized)
            {
                lock (this.lockObject)
                {
                    for (int i = 0; i < this.bucketSize; i++)
                    {
                        var queue = this.queues[i];
                        var count = queue.Count;

                        for (int j = 0; j < count; j++)
                        {
                            var item = queue.Dequeue();
                            action(item);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.bucketSize; i++)
                {
                    var queue = this.queues[i];
                    var count = queue.Count;

                    for (int j = 0; j < count; j++)
                    {
                        var item = queue.Dequeue();
                        action(item);
                    }
                }
            }
        }

    }
}