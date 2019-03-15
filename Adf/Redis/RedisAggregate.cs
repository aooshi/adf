using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 集合类型
    /// </summary>
    public enum RedisAggregate
    {
        /// <summary>
        /// 和
        /// </summary>
        SUM
        ,
            /// <summary>
            /// 最小值 
            /// </summary>
        MIN ,
            /// <summary>
            /// 最大值
            /// </summary>
            MAX
    }
}
