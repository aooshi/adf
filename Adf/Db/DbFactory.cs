using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
//using Aooshi.Configuration;
using System.Configuration;

namespace Adf.Db
{
    /// <summary>
    /// 数据操作基类
    /// </summary>
    // /// <include file='../docs/DB.Factory.xml' path='docs/*'/>
    public abstract class DbFactory : IDisposable, IDbFactory
    {
        /// <summary>
        /// 默认的LIST返回集合大小
        /// </summary>
        public const int CALLBACK_LIST_CAPACITY = 10;
        /// <summary>
        /// 是否开启日志，配置名：Adf:Db:LogEnable
        /// </summary>
        public static bool LOG_ENABLE = ConfigHelper.GetSettingAsBoolean("Adf:Db:LogEnable");

        /// <summary>
        /// 调用一个对象实例，并在执行完成后并闭连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public static void Call<T>(Action<T> callback) where T : IDbFactory, new()
        {
            using (T db = new T())
            {
                if (db.Connection.State != ConnectionState.Open)
                {
                    db.Connection.Open();
                }
                callback(db);
            }
        }

        string name = null;
        /// <summary>
        /// 获取当前操作对象的名称描述
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// 日志撰写器
        /// 若启用日志,默认的日志管理实例受FlushInterval影响，若配置该值则应在程序执行结束后进行相应的Flush操作，Factory不会进行默认缓冲刷新
        /// </summary>
        public LogWriter Logger
        {
            get;
            private set;
        }

        #region new member
        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="connection">对象连接</param>
        /// <param name="factory">工厂</param>
        public DbFactory(DbProviderFactory factory, IDbConnection connection)
        {
            this.name = this.GetType().Name;
            this.DbProviderFactory = factory;
            this.Connection = connection;
            this.Transaction = null;
            this.CreateParameterCallback = null;
            //this.Connection.Open();
            this.LoggerInitialize();
        }

        /// <summary>
        /// 创建日志书写实例
        /// </summary>
        /// <returns></returns>
        protected virtual LogWriter CreateLogWriter()
        {
            return DbLogger.Instance.GetWriter(this.Name);
        }

        /// <summary>
        /// 日志书写器初始化
        /// </summary>
        private void LoggerInitialize()
        {
            if (LOG_ENABLE)
            {
                var logger = this.CreateLogWriter();
                if (logger == null)
                {
                    throw new InvalidOperationException("Logger enable but create log writer failure.");
                }
                this.Logger = logger;
            }
        }

        SqlBuilder sqlBuilder = null;
        /// <summary>
        /// 返回可用的<see cref="SqlBuilder"/>对象
        /// </summary>
        /// <returns></returns>
        public virtual SqlBuilder SqlBuilder
        {
            get
            {
                if (sqlBuilder == null)
                    sqlBuilder = new SqlBuilder(this);

                return sqlBuilder;
            }
        }

        /// <summary>
        /// SafeString 默认值
        /// </summary>
        protected const string SAFESTRING_DEFAULT = "''";

        /// <summary>
        /// 根据数据类型返回一个可以直接应用于SQL语句的安全字符串
        /// </summary>
        /// <param name="value">要添加的值</param>
        public virtual string ToSafeString(object value)
        {
            if (value == null)
                return SAFESTRING_DEFAULT;

            return ToSafeString(value, value.GetType());
        }

        /// <summary>
        /// 根据数据类型返回一个可以直接应用于SQL语句的安全字符串
        /// </summary>
        /// <param name="value">要添加的值</param>
        /// <param name="type">类型</param>
        public virtual string ToSafeString(object value, Type type)
        {
            if (value == null)
                return SAFESTRING_DEFAULT;

            if (type.Equals(TypeHelper.BOOLEAN))
                return true.Equals(value) ? "1" : "0";

            var valueString = value.ToString();

            if (type.Equals(TypeHelper.INT16)) return valueString;
            else if (type.Equals(TypeHelper.INT32)) return valueString;
            else if (type.Equals(TypeHelper.INT64)) return valueString;
            else if (type.Equals(TypeHelper.SBYTE)) return valueString;
            else if (type.Equals(TypeHelper.BYTE)) return valueString;
            else if (type.Equals(TypeHelper.SINGLE)) return valueString;
            else if (type.Equals(TypeHelper.UINT16)) return valueString;
            else if (type.Equals(TypeHelper.UINT32)) return valueString;
            else if (type.Equals(TypeHelper.UINT64)) return valueString;
            else if (type.Equals(TypeHelper.DOUBLE)) return valueString;

            return string.Concat("'", this.InjectReplace(valueString), "'");
        }

        /// <summary>
        /// SQL注入过滤
        /// </summary>
        /// <param name="input">要过滤的数据</param>
        public virtual string InjectReplace(string input)
        {
            return DbHelper.Replace(input);
        }

        /// <summary>
        /// 获取Command数，间接反应了执行了多少次数据操作
        /// </summary>
        public int CommandCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取当前数据数据处理方法集
        /// </summary>
        protected DbProviderFactory DbProviderFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取最后一个查询SQL
        /// </summary>
        public virtual string LastSql
        {
            get;
            private set;
        }

        /// <summary>
        /// 根据指定数据返回Command对象,所有的数据操作均应执行此方法来获取Command对象
        /// </summary>
        /// <param name="sqlString">执行串</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parames">参数列表</param>
        public virtual IDbCommand CreateCommand(string sqlString, bool storedProcedure, params IDbDataParameter[] parames)
        {
            //local variable
            IDbCommand command = this.Connection.CreateCommand();
            command.CommandText = sqlString;

            this.LastSql = sqlString;

            if (storedProcedure)
                command.CommandType = CommandType.StoredProcedure;

            if (null != parames && parames.Length > 0)
                foreach (IDbDataParameter parame in parames)
                    command.Parameters.Add(parame);

            command.Connection = this.Connection;

            if (this.Transaction != null)
                command.Transaction = this.Transaction;

            if (LOG_ENABLE)
            {
                var logContent = this.LogBuilder(command);
                this.Logger.Write(logContent);
            }

            this.CommandCount++;

            return command;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 获取当前正在执行的事务
        /// </summary>
        public virtual IDbTransaction Transaction
        {
            get;
            private set;
        }

        /// <summary>
        /// 事务开始,可通过属性 <see cref="Transaction"/> 获取已开启的事务
        /// </summary>
        public virtual void TransactionBegin()
        {
            if (this.Transaction != null)
            {
                throw new DbException("has been a transaction started.");
            }
            this.Transaction = this.Connection.BeginTransaction();
        }

        /// <summary>
        /// 提交当前事务
        /// </summary>
        public virtual void TransactionCommit()
        {
            if (this.Transaction == null)
            {
                throw new DbException("not invoke TransactionBegin().");
            }
            try
            {
                this.Transaction.Commit();
            }
            finally
            {
                this.Transaction = null;
            }
        }

        /// <summary>
        /// 回滚当前事务
        /// </summary>
        public virtual void TransactionRollback()
        {
            if (this.Transaction == null)
            {
                throw new DbException("not invoke TransactionBegin().");
            }
            try
            {
                this.Transaction.Rollback();
            }
            finally
            {
                this.Transaction = null;
            }
        }

        #endregion

        #region IFactory 成员

        #region connection

        /// <summary>
        /// 获取当前数据库连接串
        /// </summary>
        public virtual IDbConnection Connection { get; protected set; }

        #endregion

        #region execute

        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="sqlString">要执行的操作语句</param>
        public virtual int Execute(string sqlString)
        {
            return Execute(sqlString, false, null);
        }
        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="sqlString">要执行的操作语句</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parames">参数列表</param>
        public virtual int Execute(string sqlString, bool storedProcedure, params IDbDataParameter[] parames)
        {
            using (IDbCommand cmd = this.CreateCommand(sqlString, storedProcedure, parames))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="storedProcedure">存储过程名</param>
        /// <returns>受影响行数</returns>
        public virtual int Execute(DbEntity entity, string storedProcedure)
        {
            return this.Execute(storedProcedure, true, this.CreateParameter(entity));
        }

        /// <summary>
        /// 执行数据对象插入请求
        /// </summary>
        /// <param name="entity">数据对象</param>
        public virtual int Insert(DbEntity entity)
        {
            IDbDataParameter[] parameters = null;
            return Execute(this.SqlBuilder.GetInsert(entity, out parameters), false, parameters);
        }

        /// <summary>
        /// 执行数据对象删除处理
        /// </summary>
        /// <param name="where">对象</param>
        public virtual int Delete(DbEntity where)
        {
            IDbDataParameter[] parameters = null;
            return Execute(this.SqlBuilder.GetDelete(where, out parameters), false, parameters);
        }

        /// <summary>
        /// 执行数据对象修改
        /// </summary>
        /// <param name="update">数据对象</param>
        /// <param name="where">条件对像</param>
        public virtual int Update(DbEntity update, DbEntity where)
        {
            IDbDataParameter[] parameters = null;
            return Execute(this.SqlBuilder.GetUpdate(update, where, out parameters), false, parameters);
        }

        #endregion

        #region get
        /// <summary>
        /// 获取上一执行语句所产生的自增值(此功能不一定支持所有数据库服务器)，如果有误，返回为结果类型初始值
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        public virtual T GetIdentity<T>()
        {
            var value = this.GetIdentity();

            if (value is T)
                return (T)value;

            return (T)Convert.ChangeType(value, typeof(T));
        }
        /// <summary>
        /// 获取上一执行语句所产生的自增值(此功能不一定支持所有数据库服务器)，如果有误，返回为结果类型初始值
        /// </summary>
        public virtual object GetIdentity()
        {
            using (IDbCommand cmd = CreateCommand("SELECT @@IDENTITY", false))
            {
                object value = cmd.ExecuteScalar();

                if (DBNull.Value.Equals(value))
                    return null;

                return value;
            }
        }

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="entity">数据对象</param>
        public virtual DataSet GetDataSet(DbEntity entity)
        {
            IDbDataParameter[] parameters = null;
            return GetDataSet(this.SqlBuilder.GetSelect(entity, "*", out parameters), false, parameters);
        }

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="entity">数据对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public virtual DataSet GetDataSet(DbEntity entity, string storedProcedure)
        {
            return this.GetDataSet(storedProcedure, true, this.CreateParameter(entity));
        }

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        public virtual DataSet GetDataSet(string sqlString)
        {
            return GetDataSet(sqlString, false);
        }

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="sqlString">要执行的语句或过程</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parames">参数列表</param>
        public virtual DataSet GetDataSet(string sqlString, bool storedProcedure, params IDbDataParameter[] parames)
        {
            DataSet dataset = new DataSet();
            IDbDataAdapter adapter = this.DbProviderFactory.CreateDataAdapter();
            using (adapter.SelectCommand = CreateCommand(sqlString, storedProcedure, parames))
            {
                adapter.Fill(dataset);
                return dataset;
            }
        }


        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="entity">数据对象</param>
        public virtual DataTable GetDataTable(DbEntity entity)
        {
            IDbDataParameter[] parameters = null;
            return GetDataTable(this.SqlBuilder.GetSelect(entity, "*", out parameters), false, parameters);
        }
        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="entity">数据对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public virtual DataTable GetDataTable(DbEntity entity, string storedProcedure)
        {
            return this.GetDataTable(storedProcedure, true, this.CreateParameter(entity));
        }
        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        public virtual DataTable GetDataTable(string sqlString)
        {
            return GetDataTable(sqlString, false);
        }
        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="sqlString">要执行的语句或过程</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        public virtual DataTable GetDataTable(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters)
        {
            return DbHelper.IDataReaderToDataTable(CreateCommand(sqlString, storedProcedure, parameters).ExecuteReader());
        }

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        public virtual Object GetScalar(string sqlString)
        {
            return GetScalar(sqlString, false);
        }
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parames">参数列表</param>
        public virtual object GetScalar(string sqlString, bool storedProcedure, params IDbDataParameter[] parames)
        {
            using (IDbCommand cmd = CreateCommand(sqlString, storedProcedure, parames))
            {
                object value = cmd.ExecuteScalar();
                return DBNull.Value.Equals(value) ? null : value;
            }
        }

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="entity">对象</param>
        public virtual Object GetScalar(DbEntity entity)
        {
            IDbDataParameter[] parameters = null;
            return GetScalar(this.SqlBuilder.GetSelect(entity, "*", out parameters), false, parameters);
        }
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="entity">对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public virtual Object GetScalar(DbEntity entity, string storedProcedure)
        {
            return this.GetScalar(storedProcedure, true, this.CreateParameter(entity));

        }

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">要执行的语句</param>
        public virtual T GetScalar<T>(string sqlString)
        {
            return GetScalar<T>(sqlString, false);
        }
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">要执行的语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parames">参数列表</param>
        public virtual T GetScalar<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parames)
        {
            return (T)this.GetScalar(sqlString, storedProcedure, parames);
        }
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">对象</param>
        public virtual T GetScalar<T>(DbEntity entity)
        {
            return (T)this.GetScalar(entity);
        }

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public virtual T GetScalar<T>(DbEntity entity, string storedProcedure)
        {
            return (T)this.GetScalar(storedProcedure, true, this.CreateParameter(entity));
        }

        /// <summary>
        /// 获取指定对象的记录数
        /// </summary>
        /// <param name="entity">对象</param>
        public virtual int GetCount(DbEntity entity)
        {
            IDbDataParameter[] parameters = null;
            entity.SetQuerySize(1);
            return Convert.ToInt32(GetScalar(this.SqlBuilder.GetSelect(entity, "COUNT(*)", out parameters), false, parameters));
        }
        /// <summary>
        /// 获取指定对象的记录数
        /// </summary>
        /// <param name="entity">对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public virtual int GetCount(DbEntity entity, string storedProcedure)
        {
            return Convert.ToInt32(this.GetScalar(storedProcedure, true, this.CreateParameter(entity)));
        }

        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        public virtual List<T> GetList<T>(string sqlString) where T : IDbEntity
        {
            return GetList<T>(sqlString, false);
        }

        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parames">参数列表</param>
        public virtual List<T> GetList<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parames) where T : IDbEntity
        {
            List<T> list = new List<T>(CALLBACK_LIST_CAPACITY);
            using (IDbCommand cmd = CreateCommand(sqlString, storedProcedure, parames))
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    var dbReader = new DbReader(reader);
                    while (reader.Read())
                    {
                        var obj = Activator.CreateInstance<T>();
                        obj.Initialize(dbReader);
                        list.Add(obj);
                    }
                }
            }

            return list;
        }
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public List<T> GetList<T>(DbEntity entity, string storedProcedure) where T : IDbEntity
        {
            return this.GetList<T>(storedProcedure, true, this.CreateParameter(entity));
        }
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        public List<T> GetList<T>(DbEntity entity) where T : IDbEntity
        {
            IDbDataParameter[] parameters = null;
            return this.GetList<T>(this.SqlBuilder.GetSelect(entity, "*", out parameters), false, parameters);
        }
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        public virtual List<T>[] GetLists<T>(string sqlString) where T : IDbEntity
        {
            return GetLists<T>(sqlString, false);
        }

        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parames">参数列表</param>
        public virtual List<T>[] GetLists<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parames) where T : IDbEntity
        {
            var lists = new List<List<T>>();
            using (IDbCommand cmd = CreateCommand(sqlString, storedProcedure, parames))
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    do
                    {
                        var list = new List<T>(CALLBACK_LIST_CAPACITY);
                        var dbReader = new DbReader(reader);
                        while (reader.Read())
                        {
                            var obj = Activator.CreateInstance<T>();
                            obj.Initialize(dbReader);
                            list.Add(obj);
                        }
                        lists.Add(list);
                    }
                    while (reader.NextResult());
                }
            }

            return lists.ToArray();
        }
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public List<T>[] GetLists<T>(DbEntity entity, string storedProcedure) where T : IDbEntity
        {
            return this.GetLists<T>(storedProcedure, true, this.CreateParameter(entity));
        }

        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="SqlString">查询语句</param>
        public T GetRow<T>(string SqlString) where T : IDbEntity
        {
            return GetRow<T>(SqlString, false);
        }
        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="sqlString">查询语句或过程</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parames">参数列表</param>
        public T GetRow<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parames) where T : IDbEntity
        {
            using (IDbCommand cmd = CreateCommand(sqlString, storedProcedure, parames))
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    var dbReader = new DbReader(reader);
                    while (reader.Read())
                    {
                        var obj = Activator.CreateInstance<T>();
                        obj.Initialize(dbReader);
                        return obj;
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询对象</param>
        /// <param name="storedProcedure">要执行的存储过程名</param>
        public T GetRow<T>(DbEntity entity, string storedProcedure) where T : IDbEntity
        {
            return this.GetRow<T>(storedProcedure, true, this.CreateParameter(entity));
        }

        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询对象</param>
        public T GetRow<T>(DbEntity entity) where T : IDbEntity
        {
            IDbDataParameter[] parameters = null;
            return this.GetRow<T>(this.SqlBuilder.GetSelect(entity, "*", out parameters), false, parameters);
        }

        #endregion

        #region Parames
        /// <summary>
        /// 获取创建参数时,使用的前缀,默认使用@符,如果当前操作引擎不是,请重写该属性
        /// </summary>
        protected internal virtual string ParameterChar
        {
            get { return "@"; }
        }

        /// <summary>
        /// 创建一个参数
        /// </summary>
        /// <param name="name">参数</param>
        /// <param name="value">参数值</param>
        /// <param name="length">长度 >0 有效</param>
        public virtual IDbDataParameter CreateParameter(string name, object value, int length = 0)
        {
            var param = this.DbProviderFactory.CreateParameter();
            param.ParameterName = name;
            param.Value = value;

            if (length > 0)
                param.Size = length;

            if (this.CreateParameterCallback != null)
                this.CreateParameterCallback(param);

            return param;
        }


        /// <summary>
        /// 创建一个参数
        /// </summary>
        /// <param name="name">参数</param>
        /// <param name="value">参数值</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="length">长度 >0 有效</param>
        public virtual IDbDataParameter CreateParameter(string name, object value, DbType dbType, int length = 0)
        {
            var param = this.DbProviderFactory.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            param.DbType = dbType;

            if (length > 0)
                param.Size = length;

            if (this.CreateParameterCallback != null)
                this.CreateParameterCallback(param);

            return param;
        }

        /// <summary>
        /// 根据对象创建参数列表,使用<see cref="ParameterChar"/>所指定的参数前缀
        /// </summary>
        /// <param name="entity">对象</param>
        protected virtual IDbDataParameter[] CreateParameter(DbEntity entity)
        {
            int i = 0;
            int size = entity.GetInitializePropertyCount();
            Dictionary<string, object>.Enumerator etor = entity.GetEnumerator();
            IDbDataParameter[] parameters = new IDbDataParameter[size];
            while (etor.MoveNext())
            {
                parameters[i] = this.CreateParameter(string.Concat(ParameterChar, etor.Current.Key), etor.Current.Value);
                i++;
            }
            return parameters;

            //Dictionary<string, object>.Enumerator etor = entity.GetEnumerator();
            //List<IDbDataParameter> list = new List<IDbDataParameter>();

            //if (entity.IsSetFieldLength())
            //{
            //    while (etor.MoveNext())
            //        list.Add(this.CreateParameter(string.Concat(ParameterChar, etor.Current.Key), etor.Current.Value,entity.GetFieldLength(etor.Current.Key)));
            //}
            //else
            //{
            //while (etor.MoveNext())
            //    list.Add(this.CreateParameter(string.Concat(ParameterChar, etor.Current.Key), etor.Current.Value));
            //}

            //return list;
        }

        /// <summary>
        /// 参数创建时回调
        /// </summary>
        public ParameterCallback CreateParameterCallback { get; set; }

        #endregion

        #endregion

        #region log

        /// <summary>
        /// 一个操作命令的日志构建
        /// </summary>
        /// <param name="command"></param>
        protected virtual string LogBuilder(IDbCommand command)
        {
            StringBuilder build = new StringBuilder();
            build.AppendLine("************************************");
            build.AppendFormat("Time:{0:HH:mm:ss}", DateTime.Now).AppendLine();
            if (command.CommandType == CommandType.StoredProcedure)
                build.AppendLine("Type: StoredProcedure");
            build.Append("SQL:").AppendLine(command.CommandText);
            if (command.Parameters != null && command.Parameters.Count > 0)
            {
                build.AppendLine("Parameters:");
                foreach (IDbDataParameter parame in command.Parameters)
                {
                    build.AppendFormat("    {0}={1}({2})"
                        , parame.ParameterName
                        , DBNull.Value.Equals(parame.Value) ? string.Empty : parame.Value
                        , parame.Direction);
                }
            }
            build.AppendLine();
            return build.ToString();
        }

        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放所占用的资源
        /// </summary>
        public virtual void Dispose()
        {
            //this.Connection.Close();

            var connection = this.Connection;
            if (connection != null)
                connection.Dispose();
        }

        #endregion

        #region 分页

        /// <summary>
        /// 根据表与条件返回一个COUNT值
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="condition">条件</param>
        public virtual int GetCount(string tablename, string condition)
        {
            return Convert.ToInt32(this.GetScalar(this.SqlBuilder.CountSql(tablename, condition)));
        }

        /// <summary>
        /// 返回一个分页列表对象
        /// </summary>
        /// <param name="fields">字段值,前后不带空格,位于 Select 与 From 之间的字段部表示</param>
        /// <param name="tablename">要进行查询的数据表,可为多个</param>
        /// <param name="condition">要进行查询的查询串,Where的后缀,如果未有,请设置为 null</param>
        /// <param name="orderby">排序方法,如果未有排序则为null</param>
        /// <param name="groupby">分组,分组请设置为null</param>
        /// <param name="pageindex">页序</param>
        /// <param name="pagesize">页大小</param>
        /// <param name="key">键</param>
        /// <param name="distinct"></param>
        /// <returns>返回生成后的Sql语句</returns>
        public virtual List<T> PageSql<T>(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity
        {
            return GetList<T>(this.SqlBuilder.PageSql(pageindex, pagesize, fields, tablename, condition, orderby, groupby));
        }


        /// <summary>
        /// 返回一个分页列表对象
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="fields">字段值,前后不带空格,位于 Select 与 From 之间的字段部表示</param>
        /// <param name="tablename">要进行查询的数据表,可为多个</param>
        /// <param name="condition">要进行查询的查询串,Where的后缀,如果未有,请设置为 null</param>
        /// <param name="orderby">排序方法,如果未有排序则为null</param>
        /// <param name="groupby">分组,分组请设置为null</param>
        /// <param name="pageindex">页序</param>
        /// <param name="pagesize">页大小</param>
        /// <param name="key">键</param>
        /// <param name="distinct"></param>
        /// <returns>返回生成后的Sql语句</returns>
        public virtual List<T> PageSql<T>(IDbDataParameter[] parameters, int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity
        {
            return GetList<T>(this.SqlBuilder.PageSql(pageindex, pagesize, fields, tablename, condition, orderby, groupby), false, parameters);
        }

        #endregion

    }
}