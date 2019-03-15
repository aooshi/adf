using System;

namespace Adf
{
    /// <summary>
    /// 计数器
    /// </summary>
    public class Counter
    {
        long init = 0;

        /// <summary>
        /// get counter init value
        /// </summary>
        public long Init
        {
            get { return this.init; }
        }

        long value = 0;

        /// <summary>
        /// get counter value
        /// </summary>
        public long Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// initialze new instance
        /// </summary>
        public Counter()
        {
            this.init = 0;
            this.value = 0;
        }

        /// <summary>
        /// initialze new instance
        /// </summary>
        /// <param name="init"></param>
        public Counter(long init)
        {
            this.init = init;
            this.value = init;
        }

        /// <summary>
        /// 以原子操作的形式，将值设置为初始值并返回原始值
        /// </summary>
        /// <returns></returns>
        public long Reset()
        {
            long source = System.Threading.Interlocked.Exchange(ref this.value, this.init);
            return source;
        }

        /// <summary>
        /// 以原子操作的形式，将值设置为指定值并返回原始值
        /// </summary>
        /// <param name="value"></param>
        public long Set(long value)
        {
            long source = System.Threading.Interlocked.Exchange(ref this.value, value);
            return source;
        }

        /// <summary>
        /// 值增量
        /// </summary>
        public long Increment()
        {
            long source = System.Threading.Interlocked.Increment(ref this.value);
            return source;
        }

        /// <summary>
        /// 值减量
        /// </summary>
        public long Decrement()
        {
            long source = System.Threading.Interlocked.Decrement(ref this.value);
            return source;
        }

        /// <summary>
        /// 值增量
        /// </summary>
        /// <param name="incrValue"></param>
        public long Increment(long incrValue)
        {
            long source = 0;

            if (incrValue == 0)
            {
                source = System.Threading.Interlocked.Read(ref this.value);
            }
            else
            {
                source = System.Threading.Interlocked.Add(ref this.value, incrValue);
            }

            return source;
        }

        /// <summary>
        /// 值减量
        /// </summary>
        /// <param name="decrValue"></param>
        public long Decrement(long decrValue)
        {
            return this.Increment(decrValue * -1);
        }

    }
}
