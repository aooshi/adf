using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 块输出状态
    /// </summary>
    public enum HttpServerChunkStatus
    {
        /// <summary>
        /// No Begin
        /// </summary>
        NoBegin = 0,
        /// <summary>
        /// In The Writing
        /// </summary>
        Writing,
        /// <summary>
        /// ENd
        /// </summary>
        End
    }
}
