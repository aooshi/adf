using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// ������������
    /// </summary>
    public class TypeHelper
    {
        /// <summary>
        /// CHAR����
        /// </summary>
        public static Type CHAR = typeof(char);
        /// <summary>
        /// Byte����
        /// </summary>
        public static Type BYTE = typeof(Byte);
        /// <summary>
        /// SByte����
        /// </summary>
        public static Type SBYTE = typeof(SByte);
        /// <summary>
        /// Boolean����
        /// </summary>
        public static Type BOOLEAN = typeof(Boolean);
        /// <summary>
        /// INT16����
        /// </summary>
        public static Type INT16 = typeof(Int16);
        /// <summary>
        /// INT32����
        /// </summary>
        public static Type INT32 = typeof(Int32);
        /// <summary>
        /// INT64����
        /// </summary>
        public static Type INT64 = typeof(Int64);
        /// <summary>
        /// UINT16����
        /// </summary>
        public static Type UINT16 = typeof(UInt16);
        /// <summary>
        /// UINT32����
        /// </summary>
        public static Type UINT32 = typeof(UInt32);
        /// <summary>
        /// UINT64����
        /// </summary>
        public static Type UINT64 = typeof(UInt64);
        /// <summary>
        /// Double����
        /// </summary>
        public static Type DOUBLE = typeof(Double);
        /// <summary>
        /// Single����
        /// </summary>
        public static Type SINGLE = typeof(Single);
        /// <summary>
        /// DateTime����
        /// </summary>
        public static Type DATETIME = typeof(DateTime);
        /// <summary>
        /// String����
        /// </summary>
        public static Type STRING = typeof(string);
        /// <summary>
        /// GUID����
        /// </summary>
        public static Type GUID = typeof(Guid);
        /// <summary>
        /// õ����, IEnumerable
        /// </summary>
        public static Type IENUMERABLE = typeof(IEnumerable);
        /// <summary>
        /// �б���, ILIST
        /// </summary>
        public static Type ILIST = typeof(IList);
        /// <summary>
        /// �����IDictionary
        /// </summary>
        public static Type IDICTIONARY = typeof(IDictionary);

        /// <summary>
        /// ��ȡ���ͼ��ϵ�Ԫ������
        /// </summary>
        /// <param name="type"></param>
        /// <returns>�Ƿ��ͽ�����NULL</returns>
        public static Type GetGenericCollectionType(Type type)
        {
            if (type.IsGenericType && IENUMERABLE.IsAssignableFrom(type))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// ��ȡ�����ֵ�Ԫ������
        /// </summary>
        /// <param name="type"></param>
        /// <returns>��������,0:key,1:value, �Ƿ����ֵ䷵��null</returns>
        public static Type[] GetGenericDictionaryTypes(Type type)
        {
            if (!type.IsGenericType || !IDICTIONARY.IsAssignableFrom(type))
            {
                return null;
            }

            Type[] arguments = type.GetGenericArguments();

            return new Type[] { 
                    arguments[0]
                    ,arguments[1]
                };
        }

        /// <summary>
        /// �Ƿ�Ϊ��ֵ
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumber(object obj)
        {
            if (obj == null)
                return false;

            return IsNumber(obj.GetType());
        }

        /// <summary>
        /// �Ƿ�Ϊ��ֵ
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumber(Type type)
        {
            //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            var code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }


        public static int MaxByte = 255;
        public static int MinByte = 0;

        public static int MaxSByte = 127;
        public static int MinSByte = -128;

        public static int MaxInt16 = 32767;
        public static int MinInt16 = -32768;

        public static int MaxUInt16 = 65535;
        public static int MinUInt16 = 0;

        public static int MaxInt32 = 2147483647;
        public static int MinInt32 = -2147483648;

        public static long MaxUInt32 = 4294967295L;
        public static long MinUInt32 = 0;

        public static long MaxInt64 = 9223372036854775807L;
        public static long MinInt64 = -9223372036854775808L;

        public static ulong MaxUInt64 = 18446744073709551615UL;
        public static ulong MinUInt64 = 0;

        public static float MaxFloat = 3.40282e+038f;
        public static float MinFloat = -3.40282e+038f;

        public static double MaxDouble = 1.79769e+308;
        public static double MinDouble = -1.79769e+308;
    }
}
