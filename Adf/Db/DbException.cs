using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// 异常基础类
    /// </summary>
    public class DbException:Exception
    {
        /// <summary>
        /// 初始化新的异常实体
        /// </summary>
        /// <param name="message">消息</param>
        public DbException(string message) : base(message) { }
    }
}
