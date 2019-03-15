using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Adf.Db
{
    /// <summary>
    /// MSSQL数据库基础操作
    /// </summary>
    // /// <include file='../docs/DB.Factory.xml' path='docs/*'/>
    public class SqlServer : DbFactory,IDisposable,IDbFactory
    {
        /// <summary>
        /// 根据数据连接字符串创建一个新的数据实列
        /// </summary>
        /// <param name="connectionstring">数据连接字符串</param>
        public SqlServer(string connectionstring) : base(SqlClientFactory.Instance, new SqlConnection(connectionstring))
        {
        }
        /// <summary>
        /// 创建新的实例
        /// </summary>
        /// <param name="connection">数据库连接</param>
        public SqlServer(SqlConnection connection): base(SqlClientFactory.Instance, connection)
        {
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public new SqlConnection Connection
        {
            get { return (SqlConnection)base.Connection; }
        }
    }
}
