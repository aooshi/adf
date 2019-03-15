using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections;

namespace Adf.Db
{
    /// <summary>
    /// �������
    /// </summary>
    [Serializable]
#if NET4
    [DataContract]
#endif
    public abstract class DbEntity : IDbEntity
    {
        Dictionary<string, object> propertys;
        int querySize;
        WhereRelation whereRelation;
        string _name = null;

        
        /// <summary>
        /// ��ʼ��һʵ��
        /// </summary>
        protected DbEntity(string name)
            : this()
        {
            this.SetTableName(name);
        }

        /// <summary>
        /// ��ʼ��һʵ��
        /// </summary>
        protected DbEntity()
        {
            propertys = new Dictionary<string, object>(10,StringComparer.OrdinalIgnoreCase);
            this.SetWhereRelation(WhereRelation.AND);
            this.SetQuerySize(0);
        }
        
        /// <summary>
        /// ���ò�ѯӰ������
        /// </summary>
        /// <param name="size">0����</param>
        public DbEntity SetQuerySize(int size)
        {
            this.querySize = size;
            return this;
        }

        /// <summary>
        /// ��ȡ�����õĲ�ѯӰ������
        /// </summary>
        /// <returns></returns>
        public int GetQuerySize()
        {
            return this.querySize;
        }        
        
        /// <summary>
        /// ���ò�ѯʱ��������ϵ��Ĭ��ΪAND��ϵ
        /// </summary>
        /// <param name="whereRelation"></param>
        public DbEntity SetWhereRelation(WhereRelation whereRelation)
        {
            this.whereRelation = whereRelation;
            return this;
        }

        /// <summary>
        /// ��ȡ�����õĲ�ѯ����
        /// </summary>
        /// <returns></returns>
        public WhereRelation GetWhereRelation()
        {
            return this.whereRelation;
        }

        #region Property Auto method

        /// <summary>
        /// ��ӻ�����ָ��������
        /// </summary>
        /// <param name="Name">��������</param>
        /// <param name="Value">����ֵ</param>
        public virtual void Set(string Name, object Value)
        {
            propertys[Name] = Value;
        }

        /// <summary>
        /// �Ƴ�ָ��������������
        /// </summary>
        /// <param name="Name">��������</param>
        public virtual void Remove(string Name)
        {
            propertys.Remove(Name);
        }

        /// <summary>
        /// ����һ��ֵ,��ʾ�Ƿ��ѳ�ʼ�����ù�ָ�����Ƶ�����ֵ
        /// </summary>
        /// <param name="Name">��������</param>
        public virtual bool Contains(string Name)
        {
            return propertys.ContainsKey(Name);
        }


        /// <summary>
        /// �����ѳ�ʼ���Ը���,���������������������
        /// </summary>
        public virtual int GetInitializePropertyCount()
        {
            return propertys.Count;
        }

        /// <summary>
        /// ��ȡָ������ַ�����ʽ
        /// </summary>
        /// <param name="name">������</param>
        public virtual string GetString(string name)
        {
            return this.Get<string>(name);
        }

        /// <summary>
        /// ��ȡָ����������,��δ�ҵ�ֵ,����׳��쳣��
        /// </summary>
        /// <param name="Name">������</param>
        public virtual object Get(string Name)
        {
            object value;
            if (!propertys.TryGetValue(Name, out value))
            {
                return null;
            }

            if (null != value)
                return DBNull.Value.Equals(value) ? null : value;

            return value;
        }

        /// <summary>
        /// ��ȡָ��������ֵ��
        /// </summary>
        /// <param name="name">������</param>
        public virtual T Get<T>(string name)
        {
            return this.Get<T>(name, default(T));
        }

        /// <summary>
        /// ��ȡָ��������ֵ��
        /// </summary>
        /// <param name="name">������</param>
        /// <param name="defaultValue">Ϊnullʱ��δ����ʱ��Ĭ��ֵ</param>
        public virtual T Get<T>(string name, T defaultValue)
        {
            var value = this.Get(name);

            if (value == null)
                return defaultValue;

            if (value is T)
                return (T)value;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// �ж�ָ���������Ƿ�Ϊnullֵ
        /// </summary>
        /// <param name="name">��������</param>
        public virtual bool IsNull(string name)
        {
            return this.Get(name) == null;
        }

        #endregion


        /// <summary>
        /// ��������������õ��ֵ
        /// </summary>
        public virtual Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return propertys.GetEnumerator();
        }

        /// <summary>
        /// �����������������Ƽ���
        /// </summary>
        public virtual Dictionary<string, object>.KeyCollection GetPropertyNames()
        {
            return this.propertys.Keys;
        }

        /// <summary>
        /// �����������ݸ��Ƶ�ָ���Ķ���
        /// </summary>
        /// <param name="target">�������ݵĶ���</param>
        public virtual void CopyTo(DbEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            foreach (KeyValuePair<string, object> kvp in this.propertys)
            {
                target.propertys[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Initialize(IDbReader reader)
        {
            foreach (var ni in reader.NameIndex)
            {
                //this.Set(ni.Key, reader.Get(ni.Key));
                this.propertys.Add(ni.Key, reader.Get(ni.Key));
            }
        }

        /// <summary>
        /// ��ȡ��ǰʵ���Ӧ�ı�����
        /// </summary>
        /// <returns></returns>
        public virtual string GetTableName()
        {
            if (this._name == null)
                this._name = this.GetType().Name;
            return this._name;
        }


        /// <summary>
        /// ���õ�ǰʵ���Ӧ�ı�����
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual void SetTableName(string name)
        {
            this._name = name;
        }
    }
}
