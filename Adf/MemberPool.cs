using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// 成员对象创建委托
    /// </summary>
    /// <returns></returns>
    public delegate T MemberPoolCreater<T>();

    /// <summary>
    /// 实例池管理器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MemberPool<T> : IDisposable
    {
        bool disposed = false;

        int maxMemberCount = 0;
        int availableCount = 0;

        Stack<T> stack;
        object lockObject = new object();

        /// <summary>
        /// 成员创建方法
        /// </summary>
        public MemberPoolCreater<T> Creater;

        /// <summary>
        /// 获取允许的最大成员数
        /// </summary>
        public int MaxMemberCount
        {
            get { return maxMemberCount; }
        }

        /// <summary>
        /// 获取可用成员数
        /// </summary>
        public int AvailableCount
        {
            get { return availableCount; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="maxMemberCount">允许创建的最大成员数</param>
        public MemberPool(int maxMemberCount)
        {
            this.maxMemberCount = maxMemberCount;
            this.availableCount = maxMemberCount;
            this.stack = new Stack<T>(maxMemberCount);
        }

        
        /// <summary>
        /// 从池中取出一个实例，若无可用成员则无限等待至有可用成员时
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            return this.Get(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// 从池中取出一个实例，若无可用成员则等待至有可用成员时，当超过指定超时时间还无可用成员则引发超时异常
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <exception cref="TimeoutException">get available member timeout</exception>
        /// <returns></returns>
        public T Get(int millisecondsTimeout)
        {
            T v = default(T);
            lock (this.lockObject)
            {
                if (this.availableCount < 1)
                {
                    if (Monitor.Wait(this.lockObject, millisecondsTimeout) == false)
                    {
                        throw new TimeoutException("get available member timeout.");
                    }
                }
                //
                if (this.stack.Count == 0)
                {
                    v = this.Creater();
                }
                else
                {
                    v = this.stack.Pop();
                }
                //
                this.availableCount--;
            }
            return v;
        }

        /// <summary>
        /// 归还一个可用实例入池（不可用实例应调用 discard 方法进行废弃）
        /// </summary>
        /// <param name="member"></param>
        /// <exception cref="MemberPoolException">pool full</exception>
        public void Put(T member)
        {
            lock (this.lockObject)
            {
                if (this.availableCount + 1 > this.maxMemberCount)
                {
                    throw new MemberPoolException("pool full");
                }

                this.availableCount++;
                this.stack.Push(member);
                //
                Monitor.Pulse(this.lockObject);
            }
        }

        /// <summary>
        /// 废弃一个实例，使池重新创建一个新对象(若实例可用应调用 put 进行归还）
        /// </summary>
        /// <exception cref="MemberPoolException">pool full</exception>
        public void Discard()
        {
            //此处不应考虑多个归还的情况， 理论上讲应该是取一个用一个， 不会出现批量。 因此将来也不应该增加废弃多个的实现

            lock (this.lockObject)
            {
                if (this.availableCount + 1 > this.maxMemberCount)
                {
                    //throw new InvalidOperationException("pool full");
                    throw new MemberPoolException("pool full");
                }

                this.availableCount++;
                Monitor.Pulse(this.lockObject);
            }
        }

        /// <summary>
        /// 获取当前全部成员
        /// </summary>
        /// <returns></returns>
        public T[] GetMembers()
        {
            T[] members = null;
            lock (this.lockObject)
            {
                members = this.stack.ToArray();
            }
            return members;
        }

        /// <summary>
        /// 释放实现了IDispose接口且未被取出成员资源
        /// </summary>
        public void Dispose()
        {
            if (this.disposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            this.disposed = true;

            T[] members;

            lock (this.lockObject)
            {
                members = this.stack.ToArray();
                this.stack.Clear();
            }

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] is IDisposable)
                {
                    var m = (IDisposable)members[i];
                    m.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// MemberPool Exception 
    /// </summary>
    public class MemberPoolException : Exception
    {    
        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="message"></param>
        public MemberPoolException(string message) : base(message)
        {
        }
    }
}