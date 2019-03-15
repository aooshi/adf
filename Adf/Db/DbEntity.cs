using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections;

namespace Adf.Db
{
    /// <summary>
    /// 对象基类
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
        /// 初始化一实例
        /// </summary>
        protected DbEntity(string name)
            : this()
        {
            this.SetTableName(name);
        }

        /// <summary>
        /// 初始化一实例
        /// </summary>
        protected DbEntity()
        {
            propertys = new Dictionary<string, object>(10,StringComparer.OrdinalIgnoreCase);
            this.SetWhereRelation(WhereRelation.AND);
            this.SetQuerySize(0);
        }
        
        /// <summary>
        /// 设置查询影响行数
        /// </summary>
        /// <param name="size">0则不限</param>
        public DbEntity SetQuerySize(int size)
        {
            this.querySize = size;
            return this;
        }

        /// <summary>
        /// 获取已设置的查询影响行数
        /// </summary>
        /// <returns></returns>
        public int GetQuerySize()
        {
            return this.querySize;
        }        
        
        /// <summary>
        /// 设置查询时的条件关系，默认为AND关系
        /// </summary>
        /// <param name="whereRelation"></param>
        public DbEntity SetWhereRelation(WhereRelation whereRelation)
        {
            this.whereRelation = whereRelation;
            return this;
        }

        /// <summary>
        /// 获取已设置的查询条件
        /// </summary>
        /// <returns></returns>
        public WhereRelation GetWhereRelation()
        {
            return this.whereRelation;
        }

        #region Property Auto method

        /// <summary>
        /// 添加或设置指定的属性
        /// </summary>
        /// <param name="Name">参数名称</param>
        /// <param name="Value">属性值</param>
        public virtual void Set(string Name, object Value)
        {
            propertys[Name] = Value;
        }

        /// <summary>
        /// 移除指定的已设置属性
        /// </summary>
        /// <param name="Name">属性名称</param>
        public virtual void Remove(string Name)
        {
            propertys.Remove(Name);
        }

        /// <summary>
        /// 返回一个值,表示是否已初始或设置过指定名称的属性值
        /// </summary>
        /// <param name="Name">属性名称</param>
        public virtual bool Contains(string Name)
        {
            return propertys.ContainsKey(Name);
        }


        /// <summary>
        /// 返回已初始属性个数,包念标量定义与隐含定义
        /// </summary>
        public virtual int GetInitializePropertyCount()
        {
            return propertys.Count;
        }

        /// <summary>
        /// 获取指定项的字符串格式
        /// </summary>
        /// <param name="name">属性名</param>
        public virtual string GetString(string name)
        {
            return this.Get<string>(name);
        }

        /// <summary>
        /// 获取指定的属性名,如未找到值,则会抛出异常。
        /// </summary>
        /// <param name="Name">属性名</param>
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
        /// 获取指定的属性值。
        /// </summary>
        /// <param name="name">属性名</param>
        public virtual T Get<T>(string name)
        {
            return this.Get<T>(name, default(T));
        }

        /// <summary>
        /// 获取指定的属性值。
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="defaultValue">为null时或未设置时的默认值</param>
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
        /// 判断指定的属性是否为null值
        /// </summary>
        /// <param name="name">属性名称</param>
        public virtual bool IsNull(string name)
        {
            return this.Get(name) == null;
        }

        #endregion


        /// <summary>
        /// 返回已设置属性玫举值
        /// </summary>
        public virtual Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return propertys.GetEnumerator();
        }

        /// <summary>
        /// 返回已设置属性名称集合
        /// </summary>
        public virtual Dictionary<string, object>.KeyCollection GetPropertyNames()
        {
            return this.propertys.Keys;
        }

        /// <summary>
        /// 将本对象数据复制到指定的对象
        /// </summary>
        /// <param name="target">接收数据的对象</param>
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
        /// 初始化数据
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
        /// 获取当前实体对应的表名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetTableName()
        {
            if (this._name == null)
                this._name = this.GetType().Name;
            return this._name;
        }


        /// <summary>
        /// 设置当前实体对应的表名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual void SetTableName(string name)
        {
            this._name = name;
        }
    }
}
