using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 对象转换器
    /// </summary>
    public class ObjectConverter
    {
        /// <summary>
        /// 定义对象转换器转换对象值时的回调，以便转换具有嵌套值时的自处理
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public delegate bool ConvertValueCallback(object value, Type objectType, out object result);

        /// <summary>
        /// 将目标对象属性转换为键值列表
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="recursion"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(object targetObject, bool recursion = true)
        {
            if (targetObject == null)
                return null;
            //
            var type = targetObject.GetType();
            var ps = PropertyAccessor.GetGets(targetObject);
            var dictionary = new Dictionary<string, object>(ps.Length);
            object value;
            for (int i = 0, l = ps.Length; i < l; i++)
            {
                value = PropertyAccessor.GetValue(targetObject, ps[i].PropertyInfo.Name);
                if (value == null || !recursion || value is ValueType || value is string || value is IDictionary || value is ICollection)
                {
                    dictionary.Add(ps[i].PropertyInfo.Name, value);
                }
                else
                {
                    dictionary.Add(ps[i].PropertyInfo.Name, ToDictionary(value, recursion));
                }
            }

            return dictionary;
        }

        /// <summary>
        /// 将键值表值转换为对象
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static object ToObject(Type objectType, IDictionary dictionary)
        {
            return ToObject(objectType, dictionary, null);
        }
        
        /// <summary>
        /// 将键值表值转换为对象
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="objectType"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private static object ToObject(Type objectType, IDictionary dictionary, ConvertValueCallback callback)
        {
            if (dictionary == null)
                return ConvertValue(null, objectType);

            var ps = PropertyAccessor.GetSets(objectType);
            var obj = Activator.CreateInstance(objectType);
            string name = string.Empty;
            object value;
            for (int i = 0, l = ps.Length; i < l; i++)
            {
                value = dictionary[ps[i].PropertyInfo.Name];
                PropertyAccessor.SetValue(obj, ps[i].PropertyInfo.Name, ConvertValue(value, ps[i].PropertyInfo.PropertyType, callback));
            }
            return obj;
        }

        
        /// <summary>
        /// 转换一个对象值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static object ConvertValue(object value, Type objectType)
        {
            return ConvertValue(value, objectType, null);
        }

        /// <summary>
        /// 转换一个对象值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectType"></param>
        /// <param name="callback">定义转换回调以及嵌套调用时自处理,返回值为true时，输出参数有效</param>
        /// <returns></returns>
        public static object ConvertValue(object value, Type objectType,ConvertValueCallback callback)
        {
            //处理回调
            if (callback != null)
            {
                object callbackResult;
                if (callback(value, objectType, out callbackResult))
                {
                    return callbackResult;
                }
            }

            //null
            if (value == null)
            {
                if (objectType.IsValueType)
                    return Activator.CreateInstance(objectType);
                return null;
            }

            //same
            var valueType = value.GetType();
            if (valueType.Equals(objectType))
                return value;

            //string
            if (objectType.Equals(TypeHelper.STRING))
                return Convert.ToString(value);

            //primitive
            if (objectType.IsPrimitive)
                return Convert.ChangeType(value, objectType);
            
            //inherit
            if (objectType.IsAssignableFrom(valueType))
                return value;

            //dictiory
            if (TypeHelper.IDICTIONARY.IsAssignableFrom(objectType))
            {
                var dict = value as IDictionary;
                if (dict == null)
                    throw new InvalidCastException(string.Format("{0} not to {1}", valueType.FullName, objectType.FullName));

                var kvType = TypeHelper.GetGenericDictionaryTypes(objectType);
                if (kvType != null)
                {
                    //Generic
                    if (!kvType[0].Equals(TypeHelper.STRING))
                    {
                        throw new InvalidCastException(string.Format("{0} not to {1}", valueType.FullName, objectType.FullName));
                    }

                    var objval = (IDictionary)Activator.CreateInstance(objectType);
                    var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        objval.Add(e.Key, ConvertValue(e.Value, kvType[1]));
                    }
                    return objval;
                }
                else
                {
                    //object
                    var objval = Activator.CreateInstance(objectType) as IDictionary;
                    var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        objval.Add(e.Key, e.Value);
                    }
                    return objval;
                }
            }

            //array
            if (objectType.IsArray)
            {
                var dict = value as IEnumerable;
                if (dict == null)
                    throw new InvalidCastException(string.Format("{0} not to {1}", valueType.FullName, objectType.FullName));

                var eletype = objectType.GetElementType();
                var objlist = new ArrayList(5);
                if (eletype != null)
                {
                    var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        objlist.Add( ConvertValue( e.Current ,eletype) );
                    }
                }
                return objlist.ToArray(eletype);
            }

            //list
            if (TypeHelper.ILIST.IsAssignableFrom(objectType))
            {
                var dict = value as IEnumerable;
                if (dict == null)
                    throw new InvalidCastException(string.Format("{0} not to {1}", valueType.FullName, objectType.FullName));

                var eletype = TypeHelper.GetGenericCollectionType(objectType);
                var objlist = (IList)Activator.CreateInstance(objectType);
                if (eletype != null)
                {
                    //Generic
                    var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        objlist.Add(ConvertValue(e.Current, eletype));
                    }
                }
                else
                {
                    //object 
                    var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        objlist.Add(e.Current);
                    }
                }
                return objlist;
            }

            //to object
            if (objectType.IsClass && value is IDictionary)
            {
                var obj = Activator.CreateInstance(objectType);
                return ToObject(objectType, (IDictionary)value, callback);
            }

            return value;
        }
    }
}
