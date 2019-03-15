using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Adf.Db
{
    /// <summary>
    /// ACCESS 资源产生类
    /// </summary>
    public class AccessSqlBuilder : SqlBuilder
    {
        /// <summary>
        /// 初始化新对象
        /// </summary>
        /// <param name="factory">当前操作对象</param>
        protected internal AccessSqlBuilder(DbFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// 获取查询条件
        /// </summary>
        /// <param name="where">指定的条件生成对象</param>
        /// <param name="parameters"></param>
        public override String GetWhere(DbEntity where, out IDbDataParameter[] parameters)
        {
            var parameterList = new List<IDbDataParameter>(where.GetInitializePropertyCount());
            string selectRelation = where.GetWhereRelation() == WhereRelation.AND ? " AND" : " OR";
            var etor = where.GetEnumerator();

            var build = new StringBuilder();
            //Type type;
            while (etor.MoveNext())
            {
                build.Append(selectRelation);

                if (etor.Current.Value != null && etor.Current.Value is DateTime)
                {
                    build.AppendFormat("{0}=#{1}#", etor.Current.Key, etor.Current.Value);
                }
                else
                {
                    build.AppendFormat("{0}={1}{0}", etor.Current.Key, Factory.ParameterChar);
                    parameterList.Add(Factory.CreateParameter(etor.Current.Key, etor.Current.Value));
                }
            }

            parameters = parameterList.ToArray();

            if (build.Length > 0)
                return string.Concat(" WHERE ", build.Remove(0, selectRelation.Length).ToString());

            return string.Empty;
        }

        /// <summary>
        /// get insert sql
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parameters"></param>
        public override String GetInsert(DbEntity entity, out IDbDataParameter[] parameters)
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
                if (etor.Current.Value != null && etor.Current.Value is DateTime)
                {
                    values.AppendFormat(",#{0}#", etor.Current.Value);
                }
                else
                {
                    values.AppendFormat(",{0}{1}", Factory.ParameterChar, etor.Current.Key);
                    parameterList.Add(Factory.CreateParameter(etor.Current.Key, etor.Current.Value));
                }
            }

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
        public override String GetUpdate(DbEntity update, DbEntity where, out IDbDataParameter[] parameters)
        {
            parameters = null;
            var parameterList = new List<IDbDataParameter>(update.GetInitializePropertyCount() + where.GetInitializePropertyCount());

            StringBuilder build = new StringBuilder();
            //object property
            Dictionary<string, object>.Enumerator etor = update.GetEnumerator();
            //Type type;
            while (etor.MoveNext())
            {
                if (etor.Current.Value != null && etor.Current.Value is DateTime)
                {
                    build.AppendFormat(",{0}=#{1}#", etor.Current.Key, etor.Current.Value);
                }
                else
                {
                    build.AppendFormat(",{0}={1}{0}", etor.Current.Key, Factory.ParameterChar);
                    parameterList.Add(Factory.CreateParameter(etor.Current.Key, etor.Current.Value));
                }
            }

            string result = null;
            if (build.Length > 0)
            {
                IDbDataParameter[] whereParameters = null;
                result = string.Format("UPDATE {0} SET {1}{2}", update.GetTableName(), build.Remove(0, 1).ToString(), this.GetWhere(where, out whereParameters));
                if (whereParameters != null)
                    parameterList.AddRange(whereParameters);
            }
            parameters = parameterList.ToArray();
            return result;
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
        public override string PageSql(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false)
        {
            if (pageindex < 1)
                pageindex = 1;

            string dist = (distinct) ? " DISTINCT" : "";

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
            build.AppendFormat("Select{0} Top {1} {2} From {3} Where {4} Not In (", dist, total, fields, tablename, key);
            //
            build.AppendFormat("Select{0} Top {1} {2} From {3}", dist, total - pagesize, key, tablename);

            if (!string.IsNullOrEmpty(condition))
                build.Append(" WHERE ").Append(condition);

            if (!string.IsNullOrEmpty(groupby))
                build.Append(" GROUP BY ").Append(groupby);

            if (!string.IsNullOrEmpty(orderby))
                build.Append(" ORDER BY ").Append(orderby);

            build.Append(")"); //条件完成

            if (!string.IsNullOrEmpty(condition))
                build.Append(" And ").Append(condition);

            if (!string.IsNullOrEmpty(groupby))
                build.Append(" GROUP BY ").Append(groupby);

            if (!string.IsNullOrEmpty(orderby))
                build.Append(" ORDER BY ").Append(orderby);

            return build.ToString();
        }
    }
}
