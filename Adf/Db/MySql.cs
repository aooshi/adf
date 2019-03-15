using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace Adf.Db
{
    /// <summary>
    /// MySql 数据操作类
    /// </summary>
    public class MySql:DbFactory
    {
        ///// <summary>
        ///// 初始化操作类型
        ///// </summary>
        ///// <param name="connectionstring">数据库连接字符串</param>
        //public MySQL(string connectionstring)
        //    : base(MySqlClientFactory.Instance, new MySqlConnection(connectionstring))
        //{
        //}

        /// <summary>
        /// 初始化操作类型
        /// </summary>
        /// <param name="connection">对象连接</param>
        /// <param name="factory">工厂</param>
        public MySql(DbProviderFactory factory, IDbConnection connection)
            : base (factory,connection)
        {
        }

        MySqlBuilder mySqlSqlBuilder = null;

        /// <summary>
        /// 已重载,语句创建
        /// </summary>
        public override SqlBuilder SqlBuilder
        {
            get
            {
                if (mySqlSqlBuilder == null)
                    mySqlSqlBuilder = new MySqlBuilder(this);

                return mySqlSqlBuilder;
            }
        }
        
        /// <summary>
        /// 获取创建参数时,使用的前缀,默认使用@符,如果当前操作引擎不是,请重写该属性
        /// </summary>
        protected internal override string ParameterChar
        {
            get { return "?"; }
        }
        /// <summary>
        /// 获取上一执行语句所产生的自增值(此功能不一定支持所有数据库服务器)，如果有误，返回为结果类型初始值
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        public override T GetIdentity<T>()
        {
            //object o = ((MySqlCommand)base.Connection.CreateCommand()).LastInsertedId;
            //return (T)Convert.ChangeType(o, typeof(T));

            var value = base.GetScalar("SELECT LAST_INSERT_ID()");
            if (value is T)
                return (T)value;
            
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
