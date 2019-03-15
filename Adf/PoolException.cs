using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Pool Item Info
    /// </summary>
    public class PoolException : Exception
    {
        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="message"></param>
        public PoolException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// pool member abandon exception
    /// </summary>
    public class PoolAbandonException : PoolException
    {
        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="message"></param>
        public PoolAbandonException(string message)
            : base(message)
        {
        }
    }
}
