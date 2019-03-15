using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Pool Item New Element Info
    /// </summary>
    public class PoolNewInstanceExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// item
        /// </summary>
        public IPoolMember Item
        {
            get;
             private set;
        }

        /// <summary>
        /// exception
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        internal PoolNewInstanceExceptionEventArgs(IPoolMember item, Exception exception)
        {
            this.Item = item;
            this.Exception = exception;
        }
    }
}
