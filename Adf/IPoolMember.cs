using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Pool Member Info
    /// 池成员
    /// </summary>
    public interface IPoolMember
    {
        /// <summary>
        /// is active ,成员是否可用
        /// </summary>
        bool PoolActive
        {
            get;
            set;
        }

        /// <summary>
        /// Pool Member Identity
        /// </summary>
        string PoolMemberId
        {
            get;
        }
        
        /// <summary>
        /// Create
        /// </summary>
        /// <returns></returns>
        IPoolInstance CreatePoolInstance();
    }
}
