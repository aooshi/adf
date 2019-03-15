using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Adf
{
    /// <summary>
    /// pool new instance action
    /// </summary>
    /// <param name="pool"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    public delegate void PoolNewInstance<T>(Pool<T> pool, T instance) where T : IPoolInstance;

    /// <summary>
    /// Pool Manager
    /// </summary>
    public class Pool<T> : IDisposable where T : IPoolInstance
    {
        object resumeObject = new object();
        bool resumeState = false;
        Thread resumtThread;
        //
        object lockObject = new object();
        bool isDisposed = false;
        //
        int memberIndex;
        int memberSize;
        MemberInfo[] memberInfos;
        //
        int activeInstanceCount;
        int runingInstanceCount;
        //
        ConsistentHashing<MemberInfo> hashing;
        Dictionary<string, int> memberIndexs;

        /// <summary>
        /// new element exception
        /// </summary>
        public event EventHandler<PoolNewInstanceExceptionEventArgs> NewInstanceException = null;

        /// <summary>
        /// new instance
        /// </summary>
        public event PoolNewInstance<T> NewInstance = null;

        CFunc<Exception, bool> retryInspector = null;
        /// <summary>
        /// 设置用于判断对于异常是否进行重试操作的函数，NULL 时不检查将对所有异常进行重试,  默认为 NULL， 此属性受 Retry 属性限制, 返回true表示应该启用重试，返回false表示禁止重试机制
        /// </summary>
        public CFunc<Exception, bool> RetryInspector
        {
            get { return this.retryInspector; }
            set { this.retryInspector = value; }
        }

        int memberInstanceSize;
        /// <summary>
        /// pool size to member
        /// </summary>
        public int MemberInstanceSize
        {
            get { return this.memberInstanceSize; }
        }

        /// <summary>
        /// Instance Re check Seconds, Default 60, zero ignore
        /// 实例异常恢复检测间隔
        /// </summary>
        public int ResumeCheckInterval
        {
            get;
            set;
        }

        int timeout;
        /// <summary>
        /// full wait timeout,default zero, not timeout
        /// </summary>
        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        IPoolMember[] members;
        /// <summary>
        /// Elements
        /// </summary>
        public IPoolMember[] Members
        {
            get { return this.members; }
        }

        /// <summary>
        /// Active Instance Count
        /// </summary>
        public int ActiveCount
        {
            get
            {
                return this.activeInstanceCount;
            }
        }

        /// <summary>
        /// Runing Instance Count
        /// </summary>
        public int RuningCount
        {
            get
            {
                return this.runingInstanceCount;
            }
        }

        bool supportHash;

        /// <summary>
        /// is support hash
        /// </summary>
        public bool SupportHash { get { return this.supportHash; } }


        int retry = 3;
        /// <summary>
        /// get or set retry number for call method, zero to disable. default 3.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">value must than or equal zero. for set</exception>
        public int Retry
        {
            get { return this.retry; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "value must than or equal zero.");

                this.retry = value;
            }
        }

        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="memberInstanceSize"></param>
        /// <param name="members"></param>
        public Pool(int memberInstanceSize, IPoolMember[] members)
            : this(memberInstanceSize, members, 1)
        {
        }

        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="memberInstanceSize"></param>
        /// <param name="members"></param>
        /// <param name="hash">zero disable hash, non-zero enable</param>
        public Pool(int memberInstanceSize, IPoolMember[] members, int hash)
        {
            this.memberInstanceSize = memberInstanceSize;
            this.members = members;
            this.memberIndexs = new Dictionary<string, int>(members.Length);
            //
            this.memberIndex = 0;
            this.memberSize = members.Length;
            this.memberInfos = new MemberInfo[this.memberSize];
            //
            this.activeInstanceCount = memberInstanceSize * this.memberSize;
            //
            this.ResumeCheckInterval = 60;
            //
            for (int i = 0; i < members.Length; i++)
            {
                this.memberIndexs.Add(members[i].PoolMemberId, i);
                //
                this.memberInfos[i] = new MemberInfo()
                {
                    IsError = false,
                    Index = i,
                    Member = members[i],
                    InstanceIndex = 0,
                    Instances = new InstanceInfo[memberInstanceSize]
                };
            }
            //
            this.supportHash = hash > 0 && this.memberSize > 1;
            if (this.supportHash)
            {
                this.hashing = new ConsistentHashing<MemberInfo>(this.memberInfos);
            }
        }

        /// <summary>
        /// get a active element,need use using
        /// </summary>
        /// <param name="hashkey"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        private InstanceInfo Get(string hashkey, string memberId)
        {
            lock (this.lockObject)
            {
                //loop
                while (true)
                {
                    //full
                    if (this.activeInstanceCount == 0)
                    {
                        if (this.timeout > 0)
                        {
                            if (!Monitor.Wait(this.lockObject, this.timeout))
                                throw new TimeoutException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use.");
                        }
                        else
                        {
                            Monitor.Wait(this.lockObject);
                        }
                    }

                    var loop = 0;
                    MemberInfo member;
                    if (this.supportHash && hashkey != null)
                        member = this.GetActiveHashMember(hashkey);
                    else if (memberId != null)
                        member = this.GetMember(memberId);
                    else
                        member = this.GetActiveMember();

                    // Console.WriteLine(member.Index);
                    while (!member.IsError && loop++ < this.memberInstanceSize)
                    {
                        var index = member.InstanceIndex++;
                        if (member.InstanceIndex == this.memberInstanceSize)
                        {
                            member.InstanceIndex = 0;
                        }
                        var instanceInfo = member.Instances[index];
                        if (instanceInfo == null)
                        {
                            instanceInfo = this.CreateInstance(member, index);
                            if (instanceInfo == null)
                            {
                                // Console.WriteLine("create null jump");
                                continue;
                            }
                            member.Instances[index] = instanceInfo;
                        }
                        else if (instanceInfo.Disabled || instanceInfo.Instance.PoolAbandon)
                        {
                            try
                            {
                                instanceInfo.Instance.Dispose();
                            }
                            catch { }

                            member.Instances[index] = null;
                            //Console.WriteLine("disabled jump");
                            continue;
                        }
                        else if (!instanceInfo.IsActive)
                        {
                            //Console.WriteLine("no active jump");
                            continue;
                        }

                        this.activeInstanceCount--;
                        instanceInfo.IsActive = false;
                        return instanceInfo;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定成员
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        private MemberInfo GetMember(string memberId)
        {
            int index = 0;
            if (!this.memberIndexs.TryGetValue(memberId, out index))
            {
                throw new PoolException(string.Concat("No Find Member ", memberId));
            }
            //
            var member = this.memberInfos[index];
            if (member.IsError)
                throw new PoolException(string.Concat("Member ", memberId, " Not Available"));

            return member;
        }

        /// <summary>
        /// 获取可用成员
        /// </summary>
        /// <returns></returns>
        private MemberInfo GetActiveMember()
        {
            if (this.memberSize == 1)
            {
                return this.memberInfos[0];
            }
            //
            for (int i = 0; i < this.memberSize; i++)
            {
                var index = this.memberIndex++;
                if (this.memberIndex == this.memberSize)
                {
                    this.memberIndex = 0;
                }

                var member = this.memberInfos[index];
                if (!member.IsError)
                {
                    return member;
                }
            }

            throw new PoolException("No Active Member");
        }

        /// <summary>
        /// 获取可用HASH成员
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private MemberInfo GetActiveHashMember(string key)
        {
            if (this.memberSize == 1)
            {
                return this.memberInfos[0];
            }

            if (this.hashing == null)
                throw new PoolException("Not Enable Hash Get");

            var member = this.hashing.GetPrimary(key);

            //若当前hash成员有异常，则去除去异常成员后重构hash-table
            if (member.IsError)
            {
                var list = new List<MemberInfo>(this.memberInfos.Length);
                for (int i = 0, l = this.memberInfos.Length; i < l; i++)
                {
                    if (this.memberInfos[i].IsError == false)
                    {
                        list.Add(this.memberInfos[i]);
                    }
                }
                if (list.Count == 0)
                {
                    throw new PoolException("No Active Member");
                }
                this.hashing = new ConsistentHashing<MemberInfo>(list.ToArray());
                //嵌套获取
                member = this.GetActiveHashMember(key);
            }

            //if (member.IsError)
            //{
            //    for (int i = 0; i < this.memberSize; i++)
            //    {
            //        var index = member.Index + 1;
            //        if (index == this.memberSize)
            //        {
            //            index = 0;
            //        }

            //        member = this.memberInfos[index];
            //        if (!member.IsError)
            //        {
            //            return member;
            //        }
            //    }

            //    throw new PoolException("No Active Member");
            //}

            return member;
        }

        /// <summary>
        /// 创建成员实例
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="instanceIndex"></param>
        /// <returns></returns>
        private InstanceInfo CreateInstance(MemberInfo memberInfo, int instanceIndex)
        {
            InstanceInfo instanceInfo = null;

            try
            {
                instanceInfo = new InstanceInfo()
                {
                    IsActive = true,
                    MemberInfo = memberInfo,
                    Instance = (T)memberInfo.Member.CreatePoolInstance(),
                    Index = instanceIndex
                };
            }
            catch (Exception exception)
            {
                instanceInfo = null;

                //resume
                if (this.memberSize > 1)
                {
                    //仅一个时，不设置异常
                    memberInfo.IsError = true;

                    if (this.ResumeCheckInterval > 0)
                    {
                        this.ResumeStart();
                    }
                }
                //
                this.DisabledMember(memberInfo);
                //event
                this.OnNewInstanceException(memberInfo.Member, exception);
                //
                if (this.memberSize == 1)
                    throw;
            }

            //event
            if (instanceInfo != null && this.NewInstance != null)
            {
                this.NewInstance(this, instanceInfo.Instance);
            }

            return instanceInfo;
        }

        private void ResumeStart()
        {
            lock (this.resumeObject)
            {
                if (this.resumeState == false)
                {
                    var thread = new Thread(this.ResumeHandler);
                    this.resumtThread = thread;
                    this.resumeState = true;
                    //
                    thread.IsBackground = true;
                    thread.Start();
                    //
                }
            }
        }

        /// <summary>
        /// resume handler
        /// </summary>
        private void ResumeHandler()
        {
            while (this.isDisposed == false && this.resumeState == true)
            {
                Thread.Sleep(this.ResumeCheckInterval * 1000);
                //
                var resumeCount = 0;
                var errorCount = 0;
                for (int i = 0, l = this.memberInfos.Length; i < l; i++)
                {
                    var memberInfo = this.memberInfos[i];
                    if (memberInfo.IsError)
                    {
                        errorCount++;

                        try
                        {
                            using (var instance = memberInfo.Member.CreatePoolInstance())
                            {
                                memberInfo.IsError = false;
                                resumeCount++;
                            }
                            continue;
                        }
                        catch (Exception exception2)
                        {
                            //event
                            this.OnNewInstanceException(memberInfo.Member, exception2);
                        }
                    }
                }
                //当具有成员恢复,并启用HASH时
                if (resumeCount > 0 && this.supportHash)
                {
                    var list = new List<MemberInfo>(this.memberInfos.Length);
                    for (int i = 0, l = this.memberInfos.Length; i < l; i++)
                    {
                        if (this.memberInfos[i].IsError == false)
                        {
                            list.Add(this.memberInfos[i]);
                        }
                    }
                    if (list.Count > 0)
                    {
                        lock (this.lockObject)
                        {
                            this.hashing = new ConsistentHashing<MemberInfo>(list.ToArray());
                        }
                    }
                }
                //无异常成员,退出异常检查线程
                if (errorCount == 0)
                {
                    lock (this.resumeObject)
                    {
                        this.resumeState = false;
                    }
                }
            }
        }

        /// <summary>
        /// 禁用所有成员实例
        /// </summary>
        /// <param name="memberInfo"></param>
        private void DisabledMember(MemberInfo memberInfo)
        {
            foreach (var instance in memberInfo.Instances)
            {
                if (instance != null)
                    instance.Disabled = true;
            }
        }

        /// <summary>
        /// trigger NewInstanceException event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="exception"></param>
        protected void OnNewInstanceException(IPoolMember item, Exception exception)
        {
            if (this.NewInstanceException != null)
            {
                this.NewInstanceException(this, new PoolNewInstanceExceptionEventArgs(item, exception));
            }
        }

        /// <summary>
        /// give instance
        /// </summary>
        /// <param name="instanceInfo"></param>
        private void Giveback(InstanceInfo instanceInfo)
        {
            lock (this.lockObject)
            {
                this.activeInstanceCount++;
                instanceInfo.IsActive = true;

                //若已完成索引小于当前最大索引，恢复索引，以便使池子实例数控制至最小
                if (instanceInfo.Index < instanceInfo.MemberInfo.InstanceIndex)
                {
                    instanceInfo.MemberInfo.InstanceIndex = instanceInfo.Index;
                }

                Monitor.Pulse(this.lockObject);
            }
        }

        /// <summary>
        /// call
        /// </summary>
        /// <param name="action"></param>
        public void Call(Action<T> action)
        {
            this.Call(action, null, null);
        }

        /// <summary>
        /// call
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hashkey">null is no hash</param>
        public void Call(Action<T> action, string hashkey)
        {
            this.Call(action, hashkey, null);
        }

        /// <summary>
        /// call
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hashkey">null is no hash</param>
        /// <param name="memberId">null is no memberid</param>
        public void Call(Action<T> action, string hashkey, string memberId)
        {
            var error = false;
            var instanceInfo = this.Get(hashkey, memberId);
            System.Threading.Interlocked.Increment(ref this.runingInstanceCount);
            try
            {
                action(instanceInfo.Instance);
            }
            catch (Exception exception)
            {
                if (this.CallExceptionIsAbandon(exception))
                {
                    instanceInfo.Disabled = true;
                    instanceInfo.Instance.PoolAbandon = true;
                }

                if (exception is PoolException)
                {
                    //内部异常直接抛出
                    throw;
                }

                if (this.retry == 0)
                {
                    throw;
                }

                var inspector = this.retryInspector;
                if (inspector != null && inspector(exception) == false)
                {
                    throw;
                }

                error = true;
            }
            finally
            {
                this.Giveback(instanceInfo);
                System.Threading.Interlocked.Decrement(ref this.runingInstanceCount);
            }

            //hash error and retry
            if (error == true)
            {
                this.TryCall(action, hashkey, memberId);
            }
        }

        /// <summary>
        /// call
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hashkey">null is no hash</param>
        /// <param name="memberId">null is no memberid</param>
        private void TryCall(Action<T> action, string hashkey, string memberId)
        {
            int retryCounter = 0;
            while (true)
            {
                var instanceInfo = this.Get(hashkey, memberId);
                System.Threading.Interlocked.Increment(ref this.runingInstanceCount);
                try
                {
                    action(instanceInfo.Instance);
                    break;
                }
                catch (Exception exception)
                {
                    if (this.CallExceptionIsAbandon(exception))
                    {
                        instanceInfo.Disabled = true;
                        instanceInfo.Instance.PoolAbandon = true;
                    }
                    
                    if (exception is PoolException)
                    {
                        //内部异常直接抛出
                        throw;
                    }

                    retryCounter++;

                    if (this.retry == retryCounter)
                        throw;

                    //wait 1 seconds
                    Thread.Sleep(1000);
                }
                finally
                {
                    this.Giveback(instanceInfo);
                    System.Threading.Interlocked.Decrement(ref this.runingInstanceCount);
                }
            }
        }

        /// <summary>
        /// call member
        /// </summary>
        /// <param name="action"></param>
        /// <param name="memberId">null is no memberid</param>
        public void CallMember(Action<T> action, string memberId)
        {
            this.Call(action, null, memberId);
        }

        /// <summary>
        /// Call异常时指示该异常是否应禁止当前实例,默认处理 PoolAbandonException / SocketException 异常，若有其它异常处理请自行重载判断
        /// </summary>
        /// <param name="exception"></param>
        protected virtual bool CallExceptionIsAbandon(Exception exception)
        {
            if (exception is PoolAbandonException)
            {
                return true;
            }

            if (exception is SocketException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            lock (this.lockObject)
            {
                this.isDisposed = true;
                this.NewInstanceException = null;

                if (this.memberInfos != null)
                {
                    foreach (var memberInfo in this.memberInfos)
                        foreach (var instance in memberInfo.Instances)
                        {
                            if (instance != null)
                                instance.Instance.Dispose();
                        }
                    this.memberInfos = null;
                }
            }
        }

        private class MemberInfo : IConsistentHashingNode
        {
            /// <summary>
            /// 成员索引
            /// </summary>
            public int Index;
            /// <summary>
            /// 成员
            /// </summary>
            public IPoolMember Member;
            //
            /// <summary>
            /// 实例索引
            /// </summary>
            public int InstanceIndex;
            /// <summary>
            /// 实例数组
            /// </summary>
            public InstanceInfo[] Instances;
            /// <summary>
            /// 是否发生错误
            /// </summary>
            public bool IsError;

            /// <summary>
            /// 获取哈希标识
            /// </summary>
            /// <returns></returns>
            public string GetHashingIdentity()
            {
                return this.Member.PoolMemberId;
            }
        }

        private class InstanceInfo
        {
            /// <summary>
            /// 是否可用
            /// </summary>
            public bool IsActive;

            /// <summary>
            /// Pool Member
            /// </summary>
            public MemberInfo MemberInfo;

            /// <summary>
            /// 实例
            /// </summary>
            public T Instance;

            /// <summary>
            /// 当前实例索引
            /// </summary>
            public int Index;

            /// <summary>
            /// 是否禁用
            /// </summary>
            public bool Disabled;
        }
    }
}