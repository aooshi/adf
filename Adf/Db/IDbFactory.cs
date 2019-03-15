using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;

namespace Adf.Db
{
    /// <summary>
    /// ���ݹ����ӿ�
    /// </summary>
    // /// <include file='../docs/DB.Factory.xml' path='docs/*'/>
    public interface IDbFactory : IDisposable
    {
        /// <summary>
        /// ��ȡ��ǰ������������
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ���һ��Command SQL
        /// </summary>
        string LastSql { get; }

        /// <summary>
        /// ���ؿ��õ�<see cref="SqlBuilder"/>����
        /// </summary>
        SqlBuilder SqlBuilder { get; }

        ///// <summary>
        ///// �ر����ݿ�����ʱ
        ///// </summary>
        //event EventHandler Closed;
        
        /// <summary>
        /// �����������ͷ���һ������ֱ��Ӧ����SQL���İ�ȫ�ַ���
        /// </summary>
        /// <param name="value">Ҫ��ӵ�ֵ</param>
        string ToSafeString(object value);


        /// <summary>
        /// �����������ͷ���һ������ֱ��Ӧ����SQL���İ�ȫ�ַ���
        /// </summary>
        /// <param name="value">Ҫ��ӵ�ֵ</param>
        /// <param name="type">����</param>
        string ToSafeString(object value, Type type);


        /// <summary>
        /// SQLע�����
        /// </summary>
        /// <param name="input">Ҫ���˵�����</param>
        string InjectReplace(string input);


        /// <summary>
        /// ��ȡCommand������ӷ�Ӧ��ִ���˶��ٴ����ݲ���
        /// </summary>
        int CommandCount { get; }
        
        /// <summary>
        /// ��ȡ��ǰ���ݿ����Ӵ�
        /// </summary>
        IDbConnection Connection { get; }

        
        /// <summary>
        /// ����ָ�����ݷ���Command����,���е����ݲ�����Ӧִ�д˷�������ȡCommand����
        /// </summary>
        /// <param name="sqlString">ִ�д�</param>
        /// <param name="storedProcedure">�Ƿ��Դ洢����ִ��</param>
        /// <param name="parames">�����б�</param>
        IDbCommand CreateCommand(string sqlString, bool storedProcedure, params IDbDataParameter[] parames);
                
        #region ����

        /// <summary>
        /// ��ȡ��ǰ����ִ�е�����
        /// </summary>
        IDbTransaction Transaction { get; }
        
        /// <summary>
        /// ��ʼһ������,��ͨ������ <see cref="Transaction"/> ��ȡ�ѿ���������
        /// </summary>
        void TransactionBegin();

        /// <summary>
        /// �ع���ǰ����
        /// </summary>
        void TransactionRollback();

        /// <summary>
        /// �ύ��ǰ����
        /// </summary>
        void TransactionCommit();

        #endregion

        #region execute


        /// <summary>
        /// ִ��һ���������
        /// </summary>
        /// <param name="sqlString">Ҫִ�еĲ������</param>
        int Execute(string sqlString);

        /// <summary>
        /// ִ��һ���������
        /// </summary>
        /// <param name="sqlString">Ҫִ�еĲ������</param>
        /// <param name="storedProcedure">�Ƿ��Դ洢����ִ��</param>
        /// <param name="parameters">�����б�</param>
        int Execute(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);

        /// <summary>
        /// ִ��һ���������
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <param name="storedProcedure">�洢������</param>
        int Execute(DbEntity entity,string storedProcedure);


        /// <summary>
        /// ִ�����ݶ����������
        /// </summary>
        /// <param name="entity">���ݶ���</param>
        int Insert(DbEntity entity);


        /// <summary>
        /// ִ�����ݶ���ɾ������
        /// </summary>
        /// <param name="where">����</param>
        int Delete(DbEntity where);


        /// <summary>
        /// ִ�����ݶ����޸�
        /// </summary>
        /// <param name="update">���ݶ���</param>
        /// <param name="where">��������</param>
        int Update(DbEntity update, DbEntity where);


        #endregion

        #region get
        /// <summary>
        /// ��ȡ��һִ�����������������ֵ(�˹��ܲ�һ��֧���������ݿ������)��������󣬷���Ϊ������ͳ�ʼֵ
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        T GetIdentity<T>();

        /// <summary>
        /// �������һ�β����ļ�ֵ
        /// </summary>
        /// <returns></returns>
        object GetIdentity();

        /// <summary>
        /// ����һ�����ݼ�
        /// </summary>
        /// <param name="entity">���ݶ���</param>
        DataSet GetDataSet(DbEntity entity);

        /// <summary>
        /// ����һ�����ݼ�
        /// </summary>
        /// <param name="entity">���ݶ���</param>
        /// <param name="storedProcedure">�洢������</param>
        DataSet GetDataSet(DbEntity entity, string storedProcedure);


        /// <summary>
        /// ����һ�����ݼ�
        /// </summary>
        /// <param name="sqlString">Ҫִ�е����</param>
        DataSet GetDataSet(string sqlString);
        
        /// <summary>
        /// ����һ�����ݼ�
        /// </summary>
        /// <param name="sqlString">Ҫִ�е��������</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        DataSet GetDataSet(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// ����һ�����ݱ�
        /// </summary>
        /// <param name="entity">���ݶ���</param>
        DataTable GetDataTable(DbEntity entity);

        /// <summary>
        /// ����һ�����ݱ�
        /// </summary>
        /// <param name="entity">���ݶ���</param>
        /// <param name="storedProcedure">�洢������</param>
        DataTable GetDataTable(DbEntity entity, string storedProcedure);


        /// <summary>
        /// ����һ�����ݱ�
        /// </summary>
        /// <param name="sqlString">Ҫִ�е����</param>
        DataTable GetDataTable(string sqlString);
        
        /// <summary>
        /// ����һ�����ݱ�
        /// </summary>
        /// <param name="sqlString">Ҫִ�е��������</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        DataTable GetDataTable(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <param name="sqlString">Ҫִ�е����</param>
        Object GetScalar(string sqlString);
        

        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <param name="sqlString">Ҫִ�е����</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        Object GetScalar(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);

        
        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <param name="Object">����</param>
        Object GetScalar(DbEntity Object);

        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <param name="Object">����</param>
        /// <param name="storedProcedure">�洢������</param>
        Object GetScalar(DbEntity Object, string storedProcedure);


        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">Ҫִ�е����</param>
        T GetScalar<T>(string sqlString);


        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">Ҫִ�е����</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        T GetScalar<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters);


        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">����</param>
        T GetScalar<T>(DbEntity entity);
        /// <summary>
        /// ��ȡ��һ�е�һ�е�����
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">����</param>
        /// <param name="storedProcedure">�洢������</param>
        T GetScalar<T>(DbEntity entity, string storedProcedure);

        /// <summary>
        /// ���ݱ�����������һ��COUNTֵ
        /// </summary>
        /// <param name="tablename">����</param>
        /// <param name="condition">����</param>
        int GetCount(string tablename, string condition);


        /// <summary>
        /// ��ȡָ������ļ�¼��
        /// </summary>
        /// <param name="entity">����</param>
        int GetCount(DbEntity entity);


        /// <summary>
        /// ��ȡָ������ļ�¼��
        /// </summary>
        /// <param name="entity">����</param>
        /// <param name="storedProcedure">�洢������</param>
        int GetCount(DbEntity entity, string storedProcedure);


        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">��ѯ���</param>
        List<T> GetList<T>(string sqlString) where T : IDbEntity;        
        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">��ѯ���</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        List<T> GetList<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;
        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">��ѯ��������</param>
        List<T> GetList<T>(DbEntity entity) where T : IDbEntity;
        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">��ѯ��������</param>
        /// <param name="storedProcedure">�洢������</param>
        List<T> GetList<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;


        /// <summary>
        /// ����һ�������ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">��ѯ���</param>
        List<T>[] GetLists<T>(string sqlString) where T : IDbEntity;
        /// <summary>
        /// ����һ�������ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="sqlString">��ѯ���</param>
        /// <param name="storedProcedure">�Ƿ�Ϊ�洢����</param>
        /// <param name="parameters">�����б�</param>
        List<T>[] GetLists<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;
        /// <summary>
        /// ����һ�������ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">��ѯ��������</param>
        /// <param name="storedProcedure">�洢������</param>
        List<T>[] GetLists<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;

        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">���ݶ�������</typeparam>
        /// <param name="sqlString">��ѯ���</param>
        T GetRow<T>(string sqlString) where T : IDbEntity;
        
        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">���ݶ�������</typeparam>
        /// <param name="sqlString">��ѯ�������</param>
        /// <param name="storedProcedure">�Ƿ��Դ洢����ִ��</param>
        /// <param name="parameters">�����б�</param>
        T GetRow<T>(string sqlString, bool storedProcedure, params IDbDataParameter[] parameters) where T : IDbEntity;

        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">��ѯ����</param>
        /// <param name="storedProcedure">�洢������</param>
        T GetRow<T>(DbEntity entity, string storedProcedure) where T : IDbEntity;
        /// <summary>
        /// ����һ�����ݶ���
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="entity">��ѯ����</param>
        T GetRow<T>(DbEntity entity) where T : IDbEntity;

        #endregion


        #region Parames

        /// <summary>
        /// ����һ������
        /// </summary>
        /// <param name="Name">����</param>
        /// <param name="Value">����ֵ</param>
        /// <param name="length">���� >0 ��Ч</param>
        IDbDataParameter CreateParameter(string Name, object Value, int length = 0);
        /// <summary>
        /// ����һ������
        /// </summary>
        /// <param name="Name">����</param>
        /// <param name="Value">����ֵ</param>
        /// <param name="dbType">��������</param>
        /// <param name="length">���� >0 ��Ч</param>
        IDbDataParameter CreateParameter(string Name, object Value, DbType dbType, int length = 0);
        /// <summary>
        /// ��������ʱ�ص�
        /// </summary>
        ParameterCallback CreateParameterCallback { get; set; }

        #endregion

        #region ��ҳ

        /// <summary>
        /// ����һ����ҳ�б����
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
        List<T> PageSql<T>(int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity;

        /// <summary>
        /// ����һ����ҳ�б����
        /// </summary>
        /// <param name="parameters">����</param>
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
        List<T> PageSql<T>(IDbDataParameter[] parameters, int pageindex, int pagesize, string fields, string tablename, string condition, string orderby, string key = "", string groupby = "", bool distinct = false) where T : IDbEntity;

        #endregion
    }
}
