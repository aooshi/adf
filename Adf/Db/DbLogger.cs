using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// 日志书写器
    /// </summary>
    public static class DbLogger
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly LogManager Instance = new LogManager("db");
    }
}