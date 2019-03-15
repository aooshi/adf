using System;
using System.Threading;
using System.Collections.Generic;

namespace Adf
{
    /// <summary>
    /// wait table
    /// </summary>
    public class WaitTable : IDisposable
    {
        Dictionary<long, WaitTableHandle> dictionary = new Dictionary<long, WaitTableHandle>();
        object lockObject = new object();
        long _identity = 0;

        /// <summary>
        /// initialize new instance
        /// </summary>
        public WaitTable()
        {

        }

        /// <summary>
        /// create a new identity
        /// </summary>
        /// <returns></returns>
        public long NewIdentity()
        {
            long identity = System.Threading.Interlocked.Increment(ref this._identity);
            return identity;
        }

        /// <summary>
        /// craete a handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal void Remove(WaitTableHandle handle)
        {
            lock (this.lockObject)
            {
                this.dictionary.Remove(handle.Identity);
            }
        }

        /// <summary>
        /// join a handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal void Add(WaitTableHandle handle)
        {
            lock (this.lockObject)
            {
                this.dictionary.Add(handle.Identity, handle);
            }
        }

        /// <summary>
        /// craete a wait table handle
        /// </summary>
        /// <returns></returns>
        public WaitTableHandle Create()
        {
            return new WaitTableHandle(this);
        }

        ///// <summary>
        ///// set a handle
        ///// </summary>
        ///// <param name="identity"></param>
        ///// <returns></returns>
        //public bool Set(long identity)
        //{
        //    var handle = this.GetHandle(identity);
        //    if (handle == null)
        //        return false;

        //    handle.Set();
        //    return true;
        //}

        ///// <summary>
        ///// set a handle
        ///// </summary>
        ///// <param name="identity"></param>
        ///// <param name="userState"></param>
        ///// <returns></returns>
        //public bool Set(long identity, object userState)
        //{
        //    var handle = this.GetHandle(identity);
        //    if (handle == null)
        //        return false;

        //    handle.UserState = userState;
        //    handle.Set();
        //    return true;
        //}

        ///// <summary>
        ///// set a handle
        ///// </summary>
        ///// <param name="identity"></param>
        ///// <param name="userIdentity"></param>
        ///// <returns></returns>
        //public bool Set(long identity, int userIdentity)
        //{
        //    var handle = this.GetHandle(identity);
        //    if (handle == null)
        //        return false;

        //    handle.UserIdentity = userIdentity;
        //    handle.Set();
        //    return true;
        //}

        ///// <summary>
        ///// reset a handle
        ///// </summary>
        ///// <param name="identity"></param>
        ///// <returns></returns>
        //public bool Reset(long identity)
        //{
        //    var handle = this.GetHandle(identity);
        //    if (handle == null)
        //        return false;

        //    handle.Reset();
        //    return true;
        //}

        ///// <summary>
        ///// set a handle user state
        ///// </summary>
        ///// <param name="identity"></param>
        ///// <param name="userState"></param>
        ///// <returns></returns>
        //public bool SetUserState(long identity,object userState)
        //{
        //    var handle = this.GetHandle(identity);
        //    if (handle == null)
        //        return false;

        //    handle.UserState = userState;
        //    return true;
        //}
        
        /// <summary>
        /// get handle
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public WaitTableHandle GetHandle(long identity)
        {
            WaitTableHandle handle = null;
            lock (this.lockObject)
            {
                this.dictionary.TryGetValue(identity, out handle);
            }
            return handle;
        }

        /// <summary>
        /// get all handles
        /// </summary>
        /// <returns></returns>
        public WaitTableHandle[] GetHandles()
        {
            WaitTableHandle[] handles = null;
            lock (this.lockObject)
            {
                handles = new WaitTableHandle[this.dictionary.Count];
                this.dictionary.Values.CopyTo(handles, 0);
            }
            return handles;
        }
        
        /// <summary>
        /// clean resource
        /// </summary>
        public void Dispose()
        {
            var handles = this.GetHandles();
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].Close();
            }
        }
    }

    /// <summary>
    /// wait table handle
    /// </summary>
    public class WaitTableHandle : EventWaitHandle
    {
        /// <summary>
        /// user state
        /// </summary>
        public object UserState
        {
            get;
            set;
        }

        /// <summary>
        /// user identity state
        /// </summary>
        public int UserIdentity
        {
            get;
            set;
        }

        long identity;
        /// <summary>
        /// identity
        /// </summary>
        public long Identity
        {
            get { return this.identity; }
        }

        WaitTable table;
        /// <summary>
        /// table
        /// </summary>
        public WaitTable Table
        {
            get { return this.table; }
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="table"></param>
        public WaitTableHandle(WaitTable table)
            : base(false, EventResetMode.ManualReset)
        {
            this.table = table;
            this.identity = table.NewIdentity();
            //
            table.Add(this);
        }

        /// <summary>
        /// 在派生类中被重写时，释放由当前 System.Threading.WaitHandle 持有的所有资源。
        /// </summary>
        /// <param name="explicitDisposing"></param>
        protected override void Dispose(bool explicitDisposing)
        {
            if (explicitDisposing == true)
            {
                this.table.Remove(this);
            }
            //
            base.Dispose(explicitDisposing);
        }
    }
}
