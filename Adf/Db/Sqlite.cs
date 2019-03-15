using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace Adf.Db
{
    /// <summary>
    /// Sqlite ���ݲ�����
    /// </summary>
    public class Sqlite:DbFactory
    {
        /// <summary>
        /// ��ʼ����������
        /// </summary>
        /// <param name="connection">��������</param>
        /// <param name="factory">����</param>
        public Sqlite(DbProviderFactory factory, IDbConnection connection)
            : base (factory,connection)
        {
        }

        SqliteBuilder mySqlSqlBuilder = null;

        /// <summary>
        /// ������,��䴴��
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
        /// ��ȡ��һִ�����������������ֵ(�˹��ܲ�һ��֧���������ݿ������)��������󣬷���Ϊ������ͳ�ʼֵ
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        public override T GetIdentity<T>()
        {
            var value = base.GetScalar("select last_insert_rowid()");
            if (value is T)
                return (T)value;
            
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
