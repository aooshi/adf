using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Pool Element Interface
    /// 池实例
    /// </summary>
    public interface IPoolInstance : IDisposable
    {
        /// <summary>
        /// 获取或设置是否废弃此实例
        /// </summary>
        bool PoolAbandon
        {
            get;
            set;
        }
    }
}
