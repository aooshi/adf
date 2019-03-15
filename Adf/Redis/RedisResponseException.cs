using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Redis 响应异常
    /// </summary>
    public class RedisResponseException : Exception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="code"></param>
        internal RedisResponseException(string code)
            : base("Response error, " + code)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}
