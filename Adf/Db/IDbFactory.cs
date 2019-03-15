using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;

namespace Adf.Db
{
    /// <summary>
    /// 数据工厂接口
    /// </summary>
    // /// <include file='../docs/DB.Factory.xml' path='docs/*'/>
    public interface IDbFactory : IDisposable
    {
        /// <summary>
        /// 获取当前工厂名称描述
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 最后一个Command SQL
        /// </summary>
        string LastSql { get; }

        /// <summary>
        /// 返回可用的<see cref="SqlBuilder"/>对象
        /// </summary>
        SqlBuilder SqlBuilder { get; }

        ///// <summary>
        ///// 关闭数据库连接时
        ///// </summary>
        //event EventHandler Closed;
        
        /// <summary>
        /// 根据数据类型返回一个可以直接应用于SQL语句的安全字符串
        /// </summary>
        /// <param name="value">要添加的值</param>
        string ToSafeString(object value);


        /// <summary>
        /// 根据数据类型返回一个可以直接应用于SQL语句的安全字符串
        /// </summary>
        /// <param name="value">要添加的值</param>
        /// <param name="type">类型</param>
        string ToSafeString(object value, Type type);


        /// <summary>
        /// SQL注入过滤
        /// </summary>
        /// <param name="input">要过滤的数据</param>
        string InjectReplace(string input);


        /// <summary>
        /// 获取Command数，间接反应了执行了多少次数据操作
        /// </summary>
        int CommandCount { get; }
        
        /// <summary>
        /// 获取当前数据库连接串
        /// </summary>
        IDbConnection Connection { get; }

        
        /// <summary>
        /// 根据指定数据返回Command对象,所有的数据操作均应执行此方法来获取Command对象
        /// </summary>
        /// <param name="sqlString">执行串</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parames">参数列表</param>
        IDbCommand CreateCommand(string sqlString, bool storedProcedure, params IDbDataParameter[] parames);
                
        #region 事务

        /// <summary>
        /// 获取当前正在执行的事务
        /// </summary>
        IDbTransaction Transaction { get; }
        
        /// <summary>
        /// 开始一个事务,可通过属性 <see cref="Transaction"/> 获取已开启的事务
        /// </summary>
        void TransactionBegin();

        /// <summary>
        /// 回滚当前事务
        /// </summary>
        void TransactionRollback();

        /// <summary>
        /// 提交当前事务
        /// </summary>
        void TransactionCommit();

        #endregion

        #region execute


        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="sqlString">要执行的操作语句</param>
        int Execute(string sqlString);

        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="sqlString">要执行的操作语句</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parameters">参数列表</param>
        int Execute(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);

        /// <summary>
        /// 执行一个语句或过程
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="storedProcedure">存储过程名</param>
        int Execute(DbEntity entity,string storedProcedure);


        /// <summary>
        /// 执行数据对象插入请求
        /// </summary>
        /// <param name="entity">数据对象</param>
        int Insert(DbEntity entity);


        /// <summary>
        /// 执行数据对象删除处理
        /// </summary>
        /// <param name="where">对象</param>
        int Delete(DbEntity where);


        /// <summary>
        /// 执行数据对象修改
        /// </summary>
        /// <param name="update">数据对象</param>
        /// <param name="where">更新条件</param>
        int Update(DbEntity update, DbEntity where);


        #endregion

        #region get
        /// <summary>
        /// 获取上一执行语句所产生的自增值(此功能不一定支持所有数据库服务器)，如果有误，返回为结果类型初始值
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        T GetIdentity<T>();

        /// <summary>
        /// 返回最后一次产生的键值
        /// </summary>
        /// <returns></returns>
        object GetIdentity();

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="entity">数据对象</param>
        DataSet GetDataSet(DbEntity entity);

        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="entity">数据对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        DataSet GetDataSet(DbEntity entity, string storedProcedure);


        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        DataSet GetDataSet(string sqlString);
        
        /// <summary>
        /// 返回一个数据集
        /// </summary>
        /// <param name="sqlString">要执行的语句或过程</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        DataSet GetDataSet(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="entity">数据对象</param>
        DataTable GetDataTable(DbEntity entity);

        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="entity">数据对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        DataTable GetDataTable(DbEntity entity, string storedProcedure);


        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        DataTable GetDataTable(string sqlString);
        
        /// <summary>
        /// 返回一个数据表
        /// </summary>
        /// <param name="sqlString">要执行的语句或过程</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        DataTable GetDataTable(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        Object GetScalar(string sqlString);
        

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="sqlString">要执行的语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        Object GetScalar(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);

        
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="Object">对象</param>
        Object GetScalar(DbEntity Object);

        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <param name="Object">对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        Object GetScalar(DbEntity Object, string storedProcedure);


        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">要执行的语句</param>
        T GetScalar<T>(string sqlString);


        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">要执行的语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        T GetScalar<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">对象</param>
        T GetScalar<T>(DbEntity entity);
        /// <summary>
        /// 获取第一行第一列的数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        T GetScalar<T>(DbEntity entity, string storedProcedure);

        /// <summary>
        /// 根据表与条件返回一个COUNT值
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="condition">条件</param>
        int GetCount(string tablename, string condition);


        /// <summary>
        /// 获取指定对象的记录数
        /// </summary>
        /// <param name="entity">对象</param>
        int GetCount(DbEntity entity);


        /// <summary>
        /// 获取指定对象的记录数
        /// </summary>
        /// <param name="entity">对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        int GetCount(DbEntity entity, string storedProcedure);


        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        List<T> GetList<T>(string sqlString) where T : IDbEntity;        
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        List<T> GetList<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        List<T> GetList<T>(DbEntity entity) where T : IDbEntity;
        /// <summary>
        /// 返回一个数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        List<T> GetList<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;


        /// <summary>
        /// 返回一个多数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        List<T>[] GetLists<T>(string sqlString) where T : IDbEntity;
        /// <summary>
        /// 返回一个多数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        /// <param name="storedProcedure">是否为存储过程</param>
        /// <param name="parameters">参数列表</param>
        List<T>[] GetLists<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;
        /// <summary>
        /// 返回一个多数据对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询条件对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        List<T>[] GetLists<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;

        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="sqlString">查询语句</param>
        T GetRow<T>(string sqlString) where T : IDbEntity;
        
        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="sqlString">查询语句或过程</param>
        /// <param name="storedProcedure">是否以存储过程执行</param>
        /// <param name="parameters">参数列表</param>
        T GetRow<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;

        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询对象</param>
        /// <param name="storedProcedure">存储过程名</param>
        T GetRow<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;
        /// <summary>
        /// 返回一个数据对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="entity">查询对象</param>
        T GetRow<T>(DbEntity entity) where T : IDbEntity;

        #endregion


        #region Parames

        /// <summary>
        /// 创建一个参数
        /// </summary>
        /// <param name="Name">参数</param>
        /// <param name="Value">参数值</param>
        /// <param name="length">长度 >0 有效</param>
        IDbDataParameter CreateParameter(string Name, object Value, int length = 0);
        /// <summary>
        /// 创建一个参数
        /// </summary>
        /// <param name="Name">参数</param>
        /// <param name="Value">参数值</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="length">长度 >0 有效</param>
        IDbDataParameter CreateParameter(string Name, object Value, DbType dbType, int length = 0);
        /// <summary>
        /// 参数创建时回调
        /// </summary>
        ParameterCallback CreateParameterCallback { get; set; }

        #endregion

        #region 分页

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
        List<T> PageSql<T>(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity;

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
        List<T> PageSql<T>(IDbDataParameter[] parameters, int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity;

        #endregion
    }
}
