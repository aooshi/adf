using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Runtime.InteropServices;

namespace Adf
{
    /// <summary>
    /// 数据序列化助手
    /// </summary>
    /// <remarks>本类默认支持系统原生数据类型,数组，自定义实体类对象 </remarks>
    public class DataSerializable : IBinarySerializable
    {
        /// <summary>
        /// BYTE LENGTH FOR BOOLEAN
        /// </summary>
        const int SIZE_BOOLEAN = 1;
        /// <summary>
        /// BYTE LENGTH FOR BYTE
        /// </summary>
        const int SIZE_BYTE = 1;
        /// <summary>
        /// BYTE LENGTH FOR SBYTE
        /// </summary>
        const int SIZE_SBYTE = 1;
        /// <summary>
        /// BYTE LENGTH FOR INT16
        /// </summary>
        const int SIZE_CHAR = 2;
        /// <summary>
        /// BYTE LENGTH FOR
        /// </summary>
        const int SIZE_INT16 = 2;
        /// <summary>
        /// BYTE LENGTH FOR UINT16
        /// </summary>
        const int SIZE_UINT16 = 2;
        /// <summary>
        /// BYTE LENGTH FOR INT32
        /// </summary>
        const int SIZE_INT32 = 4;
        /// <summary>
        /// BYTE LENGTH FOR UINT32
        /// </summary>
        const int SIZE_UINT32 = 4;
        /// <summary>
        /// BYTE LENGTH FOR SINGLE
        /// </summary>
        const int SIZE_SINGLE = 4;
        /// <summary>
        /// BYTE LENGTH FOR DATETIME
        /// </summary>
        const int SIZE_DATETIME = 8;
        /// <summary>
        /// BYTE LENGTH FOR DOUBLE
        /// </summary>
        const int SIZE_DOUBLE = 8;
        /// <summary>
        /// BYTE LENGTH FOR INT64
        /// </summary>
        const int SIZE_INT64 = 8;
        /// <summary>
        /// BYTE LENGTH FOR UINT64
        /// </summary>
        const int SIZE_UINT64 = 8;
        /// <summary>
        /// BYTE LENGTH FOR DECIMAL
        /// </summary>
        const int SIZE_DECIMAL = 16;


        /// <summary>
        /// TYPE  FOR BOOLEAN
        /// </summary>
        static readonly Type TYPE_BOOLEAN = typeof(Boolean);
        /// <summary>
        /// TYPE  FOR BYTE
        /// </summary>
        static readonly Type TYPE_BYTE = typeof(Byte);
        /// <summary>
        /// TYPE  FOR SBYTE
        /// </summary>
        static readonly Type TYPE_SBYTE = typeof(SByte);
        /// <summary>
        /// TYPE  FOR INT16
        /// </summary>
        static readonly Type TYPE_CHAR = typeof(Char);
        /// <summary>
        /// TYPE  FOR
        /// </summary>
        static readonly Type TYPE_INT16 = typeof(Int16);
        /// <summary>
        /// TYPE  FOR UINT16
        /// </summary>
        static readonly Type TYPE_UINT16 = typeof(UInt16);
        /// <summary>
        /// TYPE  FOR INT32
        /// </summary>
        static readonly Type TYPE_INT32 = typeof(Int32);
        /// <summary>
        /// TYPE  FOR UINT32
        /// </summary>
        static readonly Type TYPE_UINT32 = typeof(UInt32);
        /// <summary>
        /// TYPE  FOR SINGLE
        /// </summary>
        static readonly Type TYPE_SINGLE = typeof(Single);
        /// <summary>
        /// TYPE  FOR STRING
        /// </summary>
        static readonly Type TYPE_STRING = typeof(String);
        /// <summary>
        /// TYPE  FOR DATETIME
        /// </summary>
        static readonly Type TYPE_DATETIME = typeof(DateTime);
        /// <summary>
        /// TYPE  FOR DOUBLE
        /// </summary>
        static readonly Type TYPE_DOUBLE = typeof(Double);
        /// <summary>
        /// TYPE  FOR INT64
        /// </summary>
        static readonly Type TYPE_INT64 = typeof(Int64);
        /// <summary>
        /// TYPE  FOR UINT64
        /// </summary>
        static readonly Type TYPE_UINT64 = typeof(UInt64);
        /// <summary>
        /// TYPE  FOR DECIMAL
        /// </summary>
        static readonly Type TYPE_DECIMAL = typeof(Decimal);
        /// <summary>
        /// 获取默认实例对象
        /// </summary>
        public static readonly DataSerializable DefaultInstance = new DataSerializable();

        /// <summary>
        /// 序列化数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (DBNull.Value.Equals(value))
            {
                return null;
            }

            if (value is string)
            {
                return Encoding.UTF8.GetBytes((string)value);
            }


            if (value is DateTime)
            {
                //return BaseDataConverter.ToBytes(((DateTime)value).ToUniversalTime().Ticks);
                return BaseDataConverter.ToBytes(((DateTime)value).Ticks);
            }

            if (value is Boolean)
            {
                var buffer = new byte[1];
                buffer[0] = (byte)((Boolean)value ? 1 : 0);
                return buffer;
            }

            if (value is Byte)
            {
                var buffer = new byte[1];
                buffer[0] = (byte)value;
                return buffer;
            }

            if (value is SByte)
            {
                var buffer = new byte[1];
                buffer[0] = (byte)((sbyte)value);
                return buffer;
            }

            if (value is Int16)
            {
                return BaseDataConverter.ToBytes((Int16)value);
            }

            if (value is UInt16)
            {
                return BaseDataConverter.ToBytes((UInt16)value);
            }

            if (value is Int32)
            {
                return BaseDataConverter.ToBytes((Int32)value);
            }

            if (value is UInt32)
            {
                return BaseDataConverter.ToBytes((UInt32)value);
            }

            if (value is Int64)
            {
                return BaseDataConverter.ToBytes((Int64)value);
            }

            if (value is UInt64)
            {
                return BaseDataConverter.ToBytes((UInt64)value);
            }

            if (value is Char)
            {
                return BaseDataConverter.ToBytes((Char)value);
            }

            if (value is Double)
            {
                return BaseDataConverter.ToBytes((Double)value);
            }

            if (value is Single)
            {
                return BaseDataConverter.ToBytes((Single)value);
            }

            if (value is Decimal)
            {
                return BaseDataConverter.ToBytes((Decimal)value);
            }

            if (value is Array)
            {
                return this.SerializeArray(value);
            }

            return this.SerializeObject(value);
        }

        /// <summary>
        /// SerializeArray
        /// </summary>
        /// <param name="array"></param>
        private byte[] SerializeArray(object array)
        {
            //item length,    item 1, item 2, item N
            if (array is string[])
            {
                var stringArray = (string[])array;
                byte[] itemBuffer, lengthBuffer, sizeBuffer;
                int length;
                using (var m = new MemoryStream())
                {
                    //item length,  item size1, item 1, item size 2, item 2, item size N, item N

                    //write item length
                    lengthBuffer = BaseDataConverter.ToBytes(stringArray.Length);
                    m.Write(lengthBuffer, 0, SIZE_INT32);
                    //
                    foreach (string item in stringArray)
                    {
                        if (item == null)
                        {
                            sizeBuffer = BaseDataConverter.ToBytes(-1);
                            //write size
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //null item no body
                        }
                        else if ("".Equals(item))
                        {
                            sizeBuffer = BaseDataConverter.ToBytes(0);
                            //write size
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //empty item no body
                        }
                        else
                        {
                            itemBuffer = Encoding.UTF8.GetBytes(item);
                            length = itemBuffer.Length;
                            sizeBuffer = BaseDataConverter.ToBytes(length);
                            //write size
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //item
                            m.Write(itemBuffer, 0, length);
                        }
                    }
                    return m.ToArray();
                }
            }

            if (array is DateTime[])
            {
                var workArray = (DateTime[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_DATETIME];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    //BaseDataConverter.ToBytes(workArray[i].ToUniversalTime().Ticks, buffer, offset);
                    BaseDataConverter.ToBytes(workArray[i].Ticks, buffer, offset);
                    offset += SIZE_INT64;
                }
                return buffer;
            }

            if (array is Boolean[])
            {
                var workArray = (Boolean[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                for (int i = 0; i < arrayLength; i++)
                {
                    buffer[SIZE_INT32 + i] = (byte)(workArray[i] ? 1 : 0);
                }
                return buffer;
            }

            if (array is Byte[])
            {
                var workArray = (Byte[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                Array.Copy(workArray, 0, buffer, SIZE_INT32, arrayLength);
                return buffer;
            }

            if (array is SByte[])
            {
                var workArray = (SByte[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                for (int i = 0; i < arrayLength; i++)
                {
                    buffer[SIZE_INT32 + i] = (byte)workArray[i];
                }
                return buffer;
            }

            if (array is Int16[])
            {
                var workArray = (Int16[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_INT16];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_INT16;
                }
                return buffer;
            }

            if (array is UInt16[])
            {
                var workArray = (UInt16[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_UINT16];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_UINT16;
                }
                return buffer;
            }

            if (array is Int32[])
            {
                var workArray = (Int32[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_INT32];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_INT32;
                }
                return buffer;
            }

            if (array is UInt32)
            {
                var workArray = (UInt32[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_UINT32];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_UINT32;
                }
                return buffer;
            }

            if (array is Int64[])
            {
                var workArray = (Int64[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_INT64];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_INT64;
                }
                return buffer;
            }

            if (array is UInt64[])
            {
                var workArray = (UInt64[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_UINT64];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_UINT64;
                }
                return buffer;
            }

            if (array is Char[])
            {
                var workArray = (Char[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_CHAR];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_CHAR;
                }
                return buffer;
            }

            if (array is Double[])
            {
                var workArray = (Double[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_DOUBLE];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_DOUBLE;
                }
                return buffer;
            }

            if (array is Single[])
            {
                var workArray = (Single[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_SINGLE];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_SINGLE;
                }
                return buffer;
            }

            if (array is Decimal[])
            {
                var workArray = (Decimal[])array;
                var arrayLength = workArray.Length;
                var buffer = new byte[SIZE_INT32 + arrayLength * SIZE_DECIMAL];
                BaseDataConverter.ToBytes(arrayLength, buffer, 0);
                var offset = SIZE_INT32;
                for (int i = 0; i < arrayLength; i++)
                {
                    BaseDataConverter.ToBytes(workArray[i], buffer, offset);
                    offset += SIZE_DECIMAL;
                }
                return buffer;
            }
            else
            {
                byte[] itemBuffer, lengthBuffer, sizeBuffer;
                var workArray = (Array)array;
                var arrayLength = workArray.Length;
                int length;
                using (var m = new MemoryStream())
                {
                    //write length
                    lengthBuffer = BaseDataConverter.ToBytes(arrayLength);
                    m.Write(lengthBuffer, 0, SIZE_INT32);

                    //item length,  item size1, item 1, item size 2, item 2, item size N, item N
                    foreach (var item in workArray)
                    {
                        if (item == null || DBNull.Value.Equals(item))
                        {
                            sizeBuffer = BaseDataConverter.ToBytes(-1);
                            //write size
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //null item no body
                        }
                        else if (item.GetType().IsArray)
                        {
                            itemBuffer = this.SerializeArray(item);
                            length = itemBuffer.Length;
                            //write size
                            sizeBuffer = BaseDataConverter.ToBytes(length);
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //item
                            m.Write(itemBuffer, 0, length);
                        }
                        else
                        {
                            itemBuffer = SerializeObject(item);
                            length = itemBuffer.Length;
                            //write size
                            sizeBuffer = BaseDataConverter.ToBytes(length);
                            m.Write(sizeBuffer, 0, SIZE_INT32);
                            //item
                            m.Write(itemBuffer, 0, length);
                        }
                    }
                    return m.ToArray();
                }
            }
        }


        /// <summary>
        /// 返回指定类型的数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] buffer)
        {
            //default
            if (buffer == null)
            {
                if (type.IsValueType)
                    return System.Activator.CreateInstance(type);
                return null;
            }

            if (TYPE_BOOLEAN.Equals(type))
                return buffer[0] == 1;

            if (TYPE_BYTE.Equals(type))
                return buffer[0];

            if (TYPE_SBYTE.Equals(type))
                return (sbyte)buffer[0];

            if (TYPE_CHAR.Equals(type))
                return BaseDataConverter.ToChar(buffer);

            if (TYPE_INT16.Equals(type))
                return BaseDataConverter.ToInt16(buffer);

            if (TYPE_UINT16.Equals(type))
                return BaseDataConverter.ToUInt16(buffer);

            if (TYPE_INT32.Equals(type))
                return BaseDataConverter.ToInt32(buffer);

            if (TYPE_SINGLE.Equals(type))
                return BaseDataConverter.ToSingle(buffer);

            if (TYPE_UINT32.Equals(type))
                return BaseDataConverter.ToUInt32(buffer);

            if (TYPE_DATETIME.Equals(type))
                return new DateTime(BaseDataConverter.ToInt64(buffer));
                //return new DateTime(BaseDataConverter.ToInt64(buffer), DateTimeKind.Utc);

            if (TYPE_DOUBLE.Equals(type))
                return BaseDataConverter.ToDouble(buffer);

            if (TYPE_INT64.Equals(type))
                return BaseDataConverter.ToInt64(buffer);

            if (TYPE_UINT64.Equals(type))
                return BaseDataConverter.ToUInt64(buffer);

            if (TYPE_DECIMAL.Equals(type))
                return BaseDataConverter.ToDecimal(buffer);

            if (TYPE_STRING.Equals(type))
                return Encoding.UTF8.GetString(buffer);

            if (type.IsArray)
                return this.DeserializeArray(type, buffer);

            return this.DeserializeObject(type, buffer);
        }

        /// <summary>
        /// 解析指定类型数组对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private object DeserializeArray(Type type, byte[] buffer)
        {
            //legnth
            var arrayLength = BaseDataConverter.ToInt32(buffer);
            var offset = SIZE_INT32;
            var elementType = type.GetElementType();

            //
            if (TYPE_STRING.Equals(elementType))
            {
                //item length,  item size1, item 1, item size 2, item 2, item size N, item N
                var stringArray = new string[arrayLength];
                var itemSize = 0;
                for (int i = 0; i < arrayLength; i++)
                {
                    itemSize = BaseDataConverter.ToInt32(buffer, offset);
                    offset += SIZE_INT32;

                    if (itemSize == -1)
                    {
                        stringArray[i] = null;
                    }
                    else if (itemSize == 0)
                    {
                        stringArray[i] = "";
                    }
                    else
                    {
                        stringArray[i] = Encoding.UTF8.GetString(buffer, offset, itemSize);
                        offset += itemSize;
                    }
                }
                return stringArray;
            }

            if (TYPE_DATETIME.Equals(elementType))
            {
                var workArray = new DateTime[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    //workArray[i] = new DateTime(BaseDataConverter.ToInt64(buffer, offset), DateTimeKind.Utc);
                    workArray[i] = new DateTime(BaseDataConverter.ToInt64(buffer, offset));
                    offset += SIZE_DATETIME;
                }
                return workArray;
            }

            if (TYPE_BOOLEAN.Equals(elementType))
            {
                var workArray = new Boolean[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = buffer[offset] == 1;
                    offset += SIZE_BOOLEAN;
                }
                return workArray;
            }

            if (TYPE_BYTE.Equals(elementType))
            {
                var workArray = new Byte[arrayLength];
                Array.Copy(buffer, offset, workArray, 0, arrayLength);
                return workArray;
            }

            if (TYPE_SBYTE.Equals(elementType))
            {
                var workArray = new SByte[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = (SByte)buffer[offset];
                    offset += SIZE_SBYTE;
                }
                return workArray;
            }

            if (TYPE_INT16.Equals(elementType))
            {
                var workArray = new Int16[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToInt16(buffer, offset);
                    offset += SIZE_INT16;
                }
                return workArray;
            }

            if (TYPE_UINT16.Equals(elementType))
            {
                var workArray = new UInt16[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToUInt16(buffer, offset);
                    offset += SIZE_UINT16;
                }
                return workArray;
            }

            if (TYPE_INT32.Equals(elementType))
            {
                var workArray = new Int32[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToInt32(buffer, offset);
                    offset += SIZE_INT32;
                }
                return workArray;
            }

            if (TYPE_UINT32.Equals(elementType))
            {
                var workArray = new UInt32[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToUInt32(buffer, offset);
                    offset += SIZE_UINT32;
                }
                return workArray;
            }

            if (TYPE_INT64.Equals(elementType))
            {
                var workArray = new Int64[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToInt64(buffer, offset);
                    offset += SIZE_INT64;
                }
                return workArray;
            }

            if (TYPE_UINT64.Equals(elementType))
            {
                var workArray = new UInt64[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToUInt64(buffer, offset);
                    offset += SIZE_UINT64;
                }
                return workArray;
            }

            if (TYPE_CHAR.Equals(elementType))
            {
                var workArray = new Char[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToChar(buffer, offset);
                    offset += SIZE_CHAR;
                }
                return workArray;
            }

            if (TYPE_DOUBLE.Equals(elementType))
            {
                var workArray = new Double[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToDouble(buffer, offset);
                    offset += SIZE_DOUBLE;
                }
                return workArray;
            }

            if (TYPE_SINGLE.Equals(elementType))
            {
                var workArray = new Single[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToSingle(buffer, offset);
                    offset += SIZE_SINGLE;
                }
                return workArray;
            }

            if (TYPE_DECIMAL.Equals(elementType))
            {
                var workArray = new Decimal[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    workArray[i] = BaseDataConverter.ToDecimal(buffer, offset);
                    offset += SIZE_DECIMAL;
                }
                return workArray;
            }
            else
            {
                //item length,  item size1, item 1, item size 2, item 2, item size N, item N

                var workArray = (IList)Array.CreateInstance(elementType, arrayLength);
                int itemSize;
                byte[] itemBuffer;
                for (int i = 0; i < arrayLength; i++)
                {
                    itemSize = BaseDataConverter.ToInt32(buffer, offset);
                    offset += SIZE_INT32;

                    if (itemSize == -1)
                    {
                        workArray[i] = null;
                    }
                    else if (elementType.IsArray)
                    {
                        itemBuffer = new byte[itemSize];
                        Array.Copy(buffer, offset, itemBuffer, 0, itemSize);
                        workArray[i] = this.DeserializeArray(elementType, itemBuffer);
                        offset += itemSize;
                    }
                    else
                    {
                        itemBuffer = new byte[itemSize];
                        Array.Copy(buffer, offset, itemBuffer, 0, itemSize);
                        workArray[i] = this.DeserializeObject(elementType, itemBuffer);
                        offset += itemSize;
                    }
                }
                return workArray;
            }
        }
        
        /// <summary>
        /// 将流字节数组序列化为指定类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected virtual object DeserializeObject(Type type, byte[] buffer)
        {
            //all length
            //, item 1 name size, item 1 name data, item 1 value size, item 1 value data
            //, item 2 name size,  item 2 name data, item 2 value size, item 2 value data
            //, item N name size,  item N name data, item N value size, item N value data
            var instance = Activator.CreateInstance(type);
            object itemValue;
            string itemName;
            int itemSize;
            byte[] itemBuffer;
            System.Reflection.PropertyInfo propertyInfo;

            //all length
            var psLength = BaseDataConverter.ToInt32(buffer);
            var offset = SIZE_INT32;
            //
            for (int i = 0; i < psLength; i++)
            {
                //name
                itemSize = BaseDataConverter.ToInt32(buffer, offset);
                offset += SIZE_INT32;
                itemName = Encoding.UTF8.GetString(buffer, offset, itemSize);
                offset += itemSize;

                //value
                itemSize = BaseDataConverter.ToInt32(buffer, offset);
                offset += SIZE_INT32;

                if (itemSize == -1)
                {
                    //set
                    propertyInfo = PropertyAccessor.FindProperty(instance, itemName);
                    if (propertyInfo != null)
                    {
                        PropertyAccessor.SetValue(instance, itemName, null);
                    }
                }
                else
                {
                    itemBuffer = new byte[itemSize];
                    Array.Copy(buffer, offset, itemBuffer, 0, itemSize);
                    offset += itemSize;

                    //set
                    propertyInfo = PropertyAccessor.FindProperty(instance, itemName);
                    itemValue = this.Deserialize(propertyInfo.PropertyType, itemBuffer);
                    if (propertyInfo != null)
                    {
                        PropertyAccessor.SetValue(instance, itemName, itemValue);
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// 将对象序列化成字节数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual byte[] SerializeObject(object value)
        {
            //all length
            //, item 1 name size, item 1 name data, item 1 value size, item 1 value data
            //, item 2 name size,  item 2 name data, item 2 value size, item 2 value data
            //, item N name size,  item N name data, item N value size, item N value data
            //
            var ps = PropertyAccessor.GetGets(value.GetType());
            object itemValue;
            //
            byte[] nameBuffer, valueBuffer, sizeBuffer;
            int length = 0;
            int psLength = ps.Length;
            using (var m = new MemoryStream())
            {
                //all length
                sizeBuffer = BaseDataConverter.ToBytes(psLength);
                m.Write(sizeBuffer, 0, SIZE_INT32);
                //
                for (int i = 0; i < psLength; i++)
                {
                    //name
                    nameBuffer = Encoding.UTF8.GetBytes(ps[i].PropertyInfo.Name);
                    length = nameBuffer.Length;
                    sizeBuffer = BaseDataConverter.ToBytes(length);

                    m.Write(sizeBuffer, 0, SIZE_INT32);
                    m.Write(nameBuffer, 0, length);

                    //value
                    itemValue = PropertyAccessor.GetValue(value, ps[i].PropertyInfo.Name);
                    valueBuffer = this.Serialize(itemValue);
                    if (valueBuffer == null)
                    {
                        sizeBuffer = BaseDataConverter.ToBytes(-1);
                        m.Write(sizeBuffer, 0, SIZE_INT32);
                        //no body
                    }
                    else
                    {
                        length = valueBuffer.Length;
                        sizeBuffer = BaseDataConverter.ToBytes(length);
                        m.Write(sizeBuffer, 0, SIZE_INT32);
                        m.Write(valueBuffer, 0, length);
                    }
                }

                return m.ToArray();
            }
        }
    }
}