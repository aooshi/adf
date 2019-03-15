using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Adf.Db
{
    /// <summary>
    /// ACCESS ��Դ������
    /// </summary>
    public class AccessSqlBuilder : SqlBuilder
    {
        /// <summary>
        /// ��ʼ���¶���
        /// </summary>
        /// <param name="factory">��ǰ��������</param>
        protected internal AccessSqlBuilder(DbFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// ��ȡ��ѯ����
        /// </summary>
        /// <param name="where">ָ�����������ɶ���</param>
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
        /// <param name="where">��������</param>
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
            if (pageindex < 1)
                pageindex = 1;

            string dist = (distinct) ? " DISTINCT" : "";

            var build = new StringBuilder();

            //��һҳ
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

            //��ҳ
            build.AppendFormat("Select{0} Top {1} {2} From {3} Where {4} Not In (", dist, total, fields, tablename, key);
            //
            build.AppendFormat("Select{0} Top {1} {2} From {3}", dist, total - pagesize, key, tablename);

            if (!string.IsNullOrEmpty(condition))
                build.Append(" WHERE ").Append(condition);

            if (!string.IsNullOrEmpty(groupby))
                build.Append(" GROUP BY ").Append(groupby);

            if (!string.IsNullOrEmpty(orderby))
                build.Append(" ORDER BY ").Append(orderby);

            build.Append(")"); //�������

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
