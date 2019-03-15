using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// Sqlite专用语句生成
    /// </summary>
    public class SqliteBuilder : SqlBuilder
    {
        /// <summary>
        /// 初始化新对象
        /// </summary>
        /// <param name="factory">当前操作对象</param>
        protected internal SqliteBuilder(DbFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// get select sql
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="parameters"></param>
        /// <param name="where"></param>
        public override String GetSelect(DbEntity where,string fields, out IDbDataParameter[] parameters)
        {
            var size = where.GetQuerySize();
            if (string.IsNullOrEmpty(fields))
                fields = "*";

            var build = new StringBuilder("SELECT ");
            //set select filed
            build.Append(fields);
            build.Append(" ");

            //set select table
            build.Append("FROM ");
            build.Append(where.GetTableName());
            build.Append(" ");

            //set select where
            build.Append(this.GetWhere(where, out parameters));

            //set rows size
            if (size > 0)
            {
                build.Append(" LIMIT ");
                build.Append(size);
            }

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
        public override string PageSql(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false)
        {
            string dist = (distinct) ? " distinct" : "";

            //分页
            var build = new StringBuilder();
            build.AppendFormat("select{0} {1} from {2}", dist, fields, tablename);

            if (!string.IsNullOrEmpty(condition))
                build.Append(" where ").Append(condition);

            if (!string.IsNullOrEmpty(groupby))
                build.Append(" group by ").Append(groupby);

            if (!string.IsNullOrEmpty(orderby))
                build.Append(" order by ").Append(orderby);

            if (pageindex < 1) 
                pageindex = 1;

            if (pagesize > 1)
                build.AppendFormat(" limit {0},{1}", (pageindex - 1) * pagesize, pagesize);

            else
                build.AppendFormat(" limit {0}", pagesize);

            return build.ToString();
        }
    }
}