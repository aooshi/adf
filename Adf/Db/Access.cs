using System;
using System.Data;
using System.Data.OleDb;

namespace Adf.Db
{
    /// <summary>
    /// Access数据库操作对象
    /// </summary>
    // /// <include file='../docs/DB.Factory.xml' path='docs/*'/>
    public class Access : DbFactory, IDisposable, IDbFactory
    {
        /// <summary>
        /// 根据数据连接字符串创建一个新的数据实列
        /// </summary>
        /// <param name="connectionstring">数据连接字符串</param>
        public Access(string connectionstring)
            : base(OleDbFactory.Instance, new OleDbConnection(connectionstring))
        {
        }
        /// <summary>
        /// 创建新的实例
        /// </summary>
        /// <param name="connection">数据库连接</param>
        public Access(OleDbConnection connection)
            : base(OleDbFactory.Instance, connection)
        {
        }
        
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public new OleDbConnection Connection
        {
            get { return (OleDbConnection)base.Connection; }
        }

        AccessSqlBuilder accessSqlBuilder = null;

        /// <summary>
        /// 已重载
        /// </summary>
        /// <returns></returns>
        public override SqlBuilder SqlBuilder
        {
            get
            {
                if (accessSqlBuilder == null)
                    accessSqlBuilder = new AccessSqlBuilder(this);

                return accessSqlBuilder;
            }
        }

        /// <summary>
        /// 根据类型返回可直接用于SQL语句的安全字符串
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="type">类型</param>
        public override string ToSafeString(object value, Type type)
        {
            if (value == null)
                return SAFESTRING_DEFAULT;

            if (type.Equals(TypeHelper.DATETIME))
                return string.Concat("#", Convert.ToString(value), "#");

            else if (type.Equals(TypeHelper.BOOLEAN))
                return value.ToString();

            return base.ToSafeString(value, type);
        }
    }
}
