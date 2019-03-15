using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// Sqliteר���������
    /// </summary>
    public class SqliteBuilder : SqlBuilder
    {
        /// <summary>
        /// ��ʼ���¶���
        /// </summary>
        /// <param name="factory">��ǰ��������</param>
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
        /// ���ɷ�ҳ��Sql���
        /// </summary>
        /// <param name="fields">�ֶ�ֵ,ǰ�󲻴��ո�,λ�� Select �� From ֮����ֶβ���ʾ</param>
        /// <param name="tablename">Ҫ���в�ѯ�����ݱ�,��Ϊ���</param>
        /// <param name="condition">Ҫ���в�ѯ�Ĳ�ѯ��,Where�ĺ�׺,���δ��,������Ϊ null</param>
        /// <param name="orderby">���򷽷�,���δ��������Ϊnull</param>
        /// <param name="groupby">����,����������Ϊnull</param>
        /// <param name="pageindex">ҳ��</param>
        /// <param name="pagesize">ҳ��С</param>
        /// <param name="key">��</param>
        /// <param name="distinct"></param>
        /// <returns>�������ɺ��Sql���</returns>
        public override string PageSql(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false)
        {
            string dist = (distinct) ? " distinct" : "";

            //��ҳ
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