using System;
using System.Text;
using System.Collections.Generic;
using System.Data;

namespace Adf.Db
{
    /// <summary>
    /// 将指定的对象转换为SQL语句
    /// </summary>
    public class SqlBuilder
    {
        /// <summary>
        /// 工厂对象
        /// </summary>
        protected DbFactory Factory
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新对象
        /// </summary>
        /// <param name="factory">当前操作对象</param>
        protected internal SqlBuilder(DbFactory factory)
        {
            this.Factory = factory;
        }

        /// <summary>
        /// get object where
        /// </summary>
        /// <param name="where">指定的条件生成对象</param>
        /// <param name="parameters"></param>
        public virtual String GetWhere(DbEntity where, out IDbDataParameter[] parameters)
        {
            var i = 0;
            parameters = new IDbDataParameter[where.GetInitializePropertyCount()];
            string selectRelation = where.GetWhereRelation() == WhereRelation.AND ? " AND " : " OR ";
            string[] selectRelations = new string[parameters.Length];
            var etor = where.GetEnumerator();
            //
            while (etor.MoveNext())
            {
                selectRelations[i] = string.Format("{0}={1}{0}", etor.Current.Key, Factory.ParameterChar);
                parameters[i]=Factory.CreateParameter(etor.Current.Key, etor.Current.Value);
                i++;
            }
            //
            if (i > 0)
                return string.Concat(" WHERE ", string.Join(selectRelation,selectRelations));

            return string.Empty;
        }

        /// <summary>
        /// get delete sql
        /// </summary>
        /// <param name="where">实体</param>
        /// <param name="parameters"></param>
        public virtual String GetDelete(DbEntity where, out IDbDataParameter[] parameters)
        {
            return string.Format("DELETE FROM {0}{1}", where.GetTableName(), this.GetWhere(where, out parameters));
        }

        /// <summary>
        /// get insert sql
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parameters"></param>
        public virtual String GetInsert(DbEntity entity, out IDbDataParameter[] parameters)
        {
            parameters = null;
            var parameterList = new List<IDbDataParameter>(entity.GetInitializePropertyCount());

            var filed = new StringBuilder();
            var values = new StringBuilder();
            //object property
            Dictionary<string, object>.Enumerator etor = entity.GetEnumerator();
            //Type type;
            while (etor.MoveNext())
            {
                filed.Append("," + etor.Current.Key);
                values.AppendFormat(",{0}{1}", Factory.ParameterChar, etor.Current.Key);
                parameterList.Add(Factory.CreateParameter(etor.Current.Key, etor.Current.Value));
            }

            parameters = parameterList.ToArray();

            //remove comma
            if (filed.Length > 0)
            {
                filed = filed.Remove(0, 1);
                values = values.Remove(0, 1);
            }
            else
            {
                return null;
            }

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})", entity.GetTableName(), filed.ToString(), values.ToString());
        }

        /// <summary>
        /// get update sql
        /// </summary>
        /// <param name="where">更新条件</param>
        /// <param name="update"></param>
        /// <param name="parameters"></param>
        public virtual String GetUpdate(DbEntity update, DbEntity where, out IDbDataParameter[] parameters)
        {
            parameters = null;
            var parameterList = new List<IDbDataParameter>(update.GetInitializePropertyCount() + where.GetInitializePropertyCount());

            StringBuilder tmp = new StringBuilder();
            //object property
            Dictionary<string, object>.Enumerator etor = update.GetEnumerator();
            //Type type;
            while (etor.MoveNext())
            {
                tmp.AppendFormat(",{0}={1}{0}", etor.Current.Key, Factory.ParameterChar);
                parameterList.Add(Factory.CreateParameter(etor.Current.Key, etor.Current.Value));
            }
            string result = null;
            if (tmp.Length > 0)
            {
                IDbDataParameter[] whereParameters = null;
                result = string.Format("UPDATE {0} SET {1}{2}", update.GetTableName(), tmp.Remove(0, 1).ToString(), this.GetWhere(where, out whereParameters));
                if (whereParameters != null)
                    parameterList.AddRange(whereParameters);
            }
            parameters = parameterList.ToArray();
            return result;
        }

        /// <summary>
        /// get select sql
        /// </summary>
        /// <param name="where"></param>
        /// <param name="fields">default is "*" </param>
        /// <param name="parameters"></param>
        public virtual String GetSelect(DbEntity where,string fields, out IDbDataParameter[] parameters)
        {
            var size = where.GetQuerySize();
            if (string.IsNullOrEmpty(fields))
                fields = "*";
            var build = new StringBuilder("SELECT ");
            //set rows size
            if (size > 0)
            {
                build.Append("TOP ");
                build.Append(size);
                build.Append(" ");
            }
            //set select filed
            build.Append(fields);
            build.Append(" ");

            //set select table
            build.Append("FROM ");
            build.Append(where.GetTableName());
            build.Append(" ");

            //set select where
            build.Append(this.GetWhere(where, out parameters));
            
            return build.ToString();
        }
        
        /// <summary>
        /// 生成分页用Sql语句
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
        public virtual string PageSql(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct= false)
        {
            string dist = (distinct) ? " DISTINCT" : string.Empty;

            if (pageindex < 1)
                pageindex = 1;

            var build = new StringBuilder();

            //第一页
            if (pageindex == 1)
            {
                build.AppendFormat("SELECT{0} TOP {1} {2} FROM {3}", dist, pagesize, fields, tablename);

                if (!string.IsNullOrEmpty(condition))
                    build.Append(" WHERE ").Append(condition);

                if (!string.IsNullOrEmpty(groupby))
                    build.Append(" GROUP BY ").Append(groupby);

                if (!string.IsNullOrEmpty(orderby))
                    build.Append(" ORDER BY ").Append(orderby);

                return build.ToString();
            }

            if (string.IsNullOrEmpty(orderby))
                throw new ArgumentNullException("orderby");

            var total = pageindex * pagesize;

            //分页
            build.AppendFormat("WITH _PAGERTABLE AS (SELECT{0} TOP {1} {2},ROW_NUMBER() OVER (ORDER BY {3}) AS _rownum FROM {4}", dist, total, fields, orderby, tablename);
            
            if (!string.IsNullOrEmpty(condition)) 
                build.Append(" WHERE ").Append(condition);

            if (!string.IsNullOrEmpty(groupby))
                build.Append(" GROUP BY  ").Append(groupby);

            build.AppendLine(")");

            build.AppendFormat("SELECT * FROM _PAGERTABLE WHERE _rownum>{0} ", total - pagesize);

            build.Append("ORDER BY _rownum ASC");

            return build.ToString();
        }
        
        /// <summary>
        /// 生成分页用计Count语句
        /// </summary>
        /// <param name="tablename">要进行查询的数据表,可为多个</param>
        /// <param name="condition">要进行查询的查询串,Where的后缀,如果未有,请设置为Empty</param>
        /// <returns>返回生成后的Sql语句</returns>
        public virtual string CountSql(string tablename, string condition)
        {
            var build = new StringBuilder();
            build.Append("SELECT COUNT(*) FROM ");
            build.Append(tablename);

            if (!string.IsNullOrEmpty(condition))
            {
                build.Append(" WHERE ");
                build.Append(condition);
            }

            return build.ToString();
        }
    }
}

