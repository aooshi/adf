using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace Adf.Db
{
    /// <summary>
    /// Sqlite 数据操作类
    /// </summary>
    public class Sqlite:DbFactory
    {
        /// <summary>
        /// 初始化操作类型
        /// </summary>
        /// <param name="connection">对象连接</param>
        /// <param name="factory">工厂</param>
        public Sqlite(DbProviderFactory factory, IDbConnection connection)
            : base (factory,connection)
        {
        }

        SqliteBuilder mySqlSqlBuilder = null;

        /// <summary>
        /// 已重载,语句创建
        /// </summary>
        public override SqlBuilder SqlBuilder
        {
            get
            {
                if (mySqlSqlBuilder == null)
                    mySqlSqlBuilder = new SqliteBuilder(this);

                return mySqlSqlBuilder;
            }
        }
     
        /// <summary>
        /// 获取上一执行语句所产生的自增值(此功能不一定支持所有数据库服务器)，如果有误，返回为结果类型初始值
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        public override T GetIdentity<T>()
        {
            var value = base.GetScalar("select last_insert_rowid()");
            if (value is T)
                return (T)value;
            
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
