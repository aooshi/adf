using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 属性访问器
    /// </summary>
    public class PropertyAccessor
    {
        static Type accessType = typeof(Accessor<,>);

        static Dictionary<string, IAccessor> accessorCache = new Dictionary<string, IAccessor>(32);
        static Dictionary<string, PropertyAccessorItem[]> propertyCache = new Dictionary<string, PropertyAccessorItem[]>();

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static object GetValue(object instance, string memberName)
        {
            return FindAccessor(instance, memberName).GetValue(instance);
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <param name="newValue"></param>
        public static void SetValue(object instance, string memberName, object newValue)
        {
            FindAccessor(instance, memberName).SetValue(instance, newValue);
        }

        /// <summary>
        /// 找到实例属性
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static PropertyInfo FindProperty(object instance, string memberName)
        {
            return FindAccessor(instance, memberName).PropertyInfo;
        }
        
        /// <summary>
        /// 获取实例所有可读属性列表
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static PropertyAccessorItem[] GetGets(object instance)
        {
            if (instance == null)
                return new PropertyAccessorItem[0];
            return GetGets(instance.GetType());
        }
        /// <summary>
        /// 获取目标类型所有可读属性列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyAccessorItem[] GetGets(Type type)
        {
            string key = string.Concat(type.FullName, "|get");
            PropertyAccessorItem[] propertys;
            propertyCache.TryGetValue(key, out propertys);
            if (propertys == null)
            {
                lock (propertyCache)
                {
                    propertyCache.TryGetValue(key, out propertys);
                    if (propertys == null)
                    {
                        var propertyies = type.GetProperties();
                        var propertyList = new List<PropertyAccessorItem>(propertyies.Length);
                        var match = false;
                        PropertyAccessorItem ap;
                        for (int i = 0, l = propertyies.Length; i < l; i++)
                        {
                            ap = new PropertyAccessorItem(propertyies[i], propertyies[i].GetGetMethod());

                            if (ap.PropertyInfo.Name.Equals("Item"))
                                match = propertyies[i].CanRead && ap.Method != null && propertyies[i].GetIndexParameters().Length == 0;
                            else
                                match = propertyies[i].CanRead && ap.Method != null;

                            if (match)
                                propertyList.Add(ap);
                        }
                        propertys = propertyList.ToArray();
                        propertyCache.Add(key, propertys);
                    }
                }
            }
            return propertys;
        }

        /// <summary>
        /// 获取实例所有可写属性列表
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static PropertyAccessorItem[] GetSets(object instance)
        {
            if (instance == null)
                return new PropertyAccessorItem[0];

            return GetSets(instance.GetType());
        }
        /// <summary>
        /// 获取目标类型所有可写属性列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyAccessorItem[] GetSets(Type type)
        {
            string key = string.Concat(type.FullName, "|set");
            PropertyAccessorItem[] propertys;
            propertyCache.TryGetValue(key, out propertys);
            if (propertys == null)
            {
                lock (propertyCache)
                {
                    propertyCache.TryGetValue(key, out propertys);
                    if (propertys == null)
                    {
                        var propertyies = type.GetProperties();
                        var propertyList = new List<PropertyAccessorItem>(propertyies.Length);
                        var match = false;
                        PropertyAccessorItem ap;
                        for (int i = 0, l = propertyies.Length; i < l; i++)
                        {
                            ap = new PropertyAccessorItem(propertyies[i], propertyies[i].GetSetMethod());

                            if (ap.PropertyInfo.Name.Equals("Item"))
                                match = propertyies[i].CanWrite && ap.Method != null && propertyies[i].GetIndexParameters().Length == 0;
                            else
                                match = propertyies[i].CanWrite && ap.Method != null;

                            if (match)
                                propertyList.Add(ap);
                        }
                        propertys = propertyList.ToArray();
                        propertyCache.Add(key, propertys);
                    }
                }
            }
            return propertys;
        }

        private static IAccessor FindAccessor(object instance, string memberName)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            var type = instance.GetType();
            var key = string.Concat(type.FullName, memberName);
            IAccessor accessor; ;
            accessorCache.TryGetValue(key, out accessor);
            lock (accessorCache)
            {
                accessorCache.TryGetValue(key, out accessor);
                if (accessor == null)
                {
                    var propertyInfo = type.GetProperty(memberName);
                    if (propertyInfo == null)
                        throw new System.Reflection.TargetException(type.FullName + " not have property " + memberName);
                    accessor = Activator.CreateInstance(accessType.MakeGenericType(type, propertyInfo.PropertyType), type, memberName) as IAccessor;
                    accessorCache.Add(key, accessor);
                }
            }
            return accessor;
        }
        
        /// <summary>
        /// 属性访问项
        /// </summary>
        public class PropertyAccessorItem
        {
            internal PropertyAccessorItem(PropertyInfo propertyInfo, MethodInfo method)
            {
                this.PropertyInfo = propertyInfo;
                this.Method = method;
            }
            /// <summary>
            /// 属性
            /// </summary>
            public PropertyInfo PropertyInfo { get; private set; }
            /// <summary>
            /// 相当方法
            /// </summary>
            public MethodInfo Method { get; private set; }
        }

        interface IAccessor
        {
            object GetValue(object instance);
            PropertyInfo PropertyInfo { get; }
            void SetValue(object instance, object newValue);
        }
        class Accessor<T, P> : IAccessor
        {
            delegate TP PFunc<TT, TP>(TT t);
            delegate void PAction<TT, TP>(TT t, TP p);

            private PFunc<T, P> GetValueDelegate;
            private PAction<T, P> SetValueDelegate;
            static Type PFuncType = typeof(PFunc<T, P>);
            static Type PActionType = typeof(PAction<T, P>);
            private PropertyInfo propertyInfo;
            public Accessor(Type type, string propertyName)
            {
                var propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var gm = propertyInfo.GetGetMethod();
                    var sm = propertyInfo.GetSetMethod();

                    if (propertyInfo.CanRead  && gm != null)
                        GetValueDelegate = (PFunc<T, P>)Delegate.CreateDelegate(PFuncType, gm);

                    if (propertyInfo.CanWrite && sm != null)
                        SetValueDelegate = (PAction<T, P>)Delegate.CreateDelegate(PActionType, sm);
                }
                this.propertyInfo = propertyInfo;
            }

            public PropertyInfo PropertyInfo
            {
                get { return this.propertyInfo; }
            }

            public object GetValue(object instance)
            {
                return GetValueDelegate((T)instance);
            }

            public void SetValue(object instance, object newValue)
            {
                SetValueDelegate((T)instance, (P)newValue);
            }
        }
    }
}