using System;
using System.Collections.Generic;
using System.Text;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace Adf.Db
{
    /// <summary>
    /// MySql ���ݲ�����
    /// </summary>
    public class MySql:DbFactory
    {
        ///// <summary>
        ///// ��ʼ����������
        ///// </summary>
        ///// <param name="connectionstring">���ݿ������ַ���</param>
        //public MySQL(string connectionstring)
        //    : base(MySqlClientFactory.Instance, new MySqlConnection(connectionstring))
        //{
        //}

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        /// <param name="connection">��������</param>
        /// <param name="factory">����</param>
        public MySql(DbProviderFactory factory, IDbConnection connection)
            : base (factory,connection)
        {
        }

        MySqlBuilder mySqlSqlBuilder = null;

        /// <summary>
        /// ������,��䴴��
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
        /// ��ȡ��������ʱ,ʹ�õ�ǰ׺,Ĭ��ʹ��@��,�����ǰ�������治��,����д������
        /// </summary>
        protected internal override string ParameterChar
        {
            get { return "?"; }
        }
        /// <summary>
        /// ��ȡ��һִ�����������������ֵ(�˹��ܲ�һ��֧���������ݿ������)��������󣬷���Ϊ������ͳ�ʼֵ
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
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
