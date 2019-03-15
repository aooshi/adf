using System;
using System.Collections.Generic;
using System.Text;
using Adf;

namespace AdfConsoleTest
{
    public static class SerializeTest
    {
        public static void BaseDataSerializableTest()
        {
            //int64
            var a1 = BaseDataConverter.ToBytes(long.MaxValue);
            var a2 = BaseDataConverter.ToInt64(a1);
            Console.WriteLine("int64:{0} {1}", a2 == long.MaxValue, sizeof(long));

            var a3 = BaseDataConverter.ToBytes(long.MinValue);
            var a4 = BaseDataConverter.ToInt64(a3);
            Console.WriteLine("int64:{0} {1}", a4 == long.MinValue, sizeof(long));

            //int32
            var b1 = BaseDataConverter.ToBytes(int.MaxValue);
            var b2 = BaseDataConverter.ToInt32(b1);
            Console.WriteLine("int32:{0} {1}", b2 == int.MaxValue, sizeof(int));

            var b3 = BaseDataConverter.ToBytes(int.MinValue);
            var b4 = BaseDataConverter.ToInt32(b3);
            Console.WriteLine("int32:{0} {1}", b4 == int.MinValue, sizeof(int));

            //int16
            var c1 = BaseDataConverter.ToBytes(short.MaxValue);
            var c2 = BaseDataConverter.ToInt16(c1);
            Console.WriteLine("int16:{0} {1}", c2 == short.MaxValue, sizeof(short));

            var c3 = BaseDataConverter.ToBytes(short.MinValue);
            var c4 = BaseDataConverter.ToInt16(c3);
            Console.WriteLine("int16:{0} {1}", c4 == short.MinValue, sizeof(short));

            //float
            var d1 = BaseDataConverter.ToBytes(float.MaxValue);
            var d2 = BaseDataConverter.ToSingle(d1);
            Console.WriteLine("single:{0} {1}", d2 == float.MaxValue, sizeof(float));

            var d3 = BaseDataConverter.ToBytes(float.MinValue);
            var d4 = BaseDataConverter.ToSingle(d3);
            Console.WriteLine("single:{0} {1}", d4 == float.MinValue, sizeof(float));

            //double
            var e1 = BaseDataConverter.ToBytes(double.MaxValue);
            var e2 = BaseDataConverter.ToDouble(e1);
            Console.WriteLine("double:{0} {1}", e2 == double.MaxValue, sizeof(double));

            var e3 = BaseDataConverter.ToBytes(double.MinValue);
            var e4 = BaseDataConverter.ToDouble(e3);
            Console.WriteLine("double:{0} {1}", e4 == double.MinValue, sizeof(double));

            //decimal
            var f1 = BaseDataConverter.ToBytes(decimal.MaxValue);
            var f2 = BaseDataConverter.ToDecimal(f1);
            Console.WriteLine("decimal:{0} {1}", f2 == decimal.MaxValue, sizeof(decimal));

            var f3 = BaseDataConverter.ToBytes(decimal.MinValue);
            var f4 = BaseDataConverter.ToDecimal(f3);
            Console.WriteLine("decimal:{0} {1}", f4 == decimal.MinValue, sizeof(decimal));

            var f5 = BaseDataConverter.ToBytes(decimal.MinusOne);
            var f6 = BaseDataConverter.ToDecimal(f5);
            Console.WriteLine("decimal:{0} {1}", f6 == decimal.MinusOne, sizeof(decimal));

            var f7 = BaseDataConverter.ToBytes(decimal.One);
            var f8 = BaseDataConverter.ToDecimal(f7);
            Console.WriteLine("decimal:{0} {1}", f8 == decimal.One, sizeof(decimal));            

            //uint16
            var g1 = BaseDataConverter.ToBytes(UInt16.MaxValue);
            var g2 = BaseDataConverter.ToUInt16(g1);
            var g3 = BaseDataConverter.ToBytes(UInt16.MinValue);
            var g4 = BaseDataConverter.ToUInt16(g3);
            Console.WriteLine("uint16:{0} {1}", g2 == UInt16.MaxValue, sizeof(UInt16));
            Console.WriteLine("uint16:{0} {1}", g4 == UInt16.MinValue, sizeof(UInt16));

            //uint32
            var h1 = BaseDataConverter.ToBytes(UInt32.MaxValue);
            var h2 = BaseDataConverter.ToUInt32(h1);
            var h3 = BaseDataConverter.ToBytes(UInt32.MinValue);
            var h4 = BaseDataConverter.ToUInt32(h3);
            Console.WriteLine("uint32:{0} {1}", h2 == UInt32.MaxValue, sizeof(UInt32));
            Console.WriteLine("uint32:{0} {1}", h4 == UInt32.MinValue, sizeof(UInt32));

            //uint64
            var i1 = BaseDataConverter.ToBytes(UInt64.MaxValue);
            var i2 = BaseDataConverter.ToUInt64(i1);
            var i3 = BaseDataConverter.ToBytes(UInt64.MinValue);
            var i4 = BaseDataConverter.ToUInt64(i3);
            Console.WriteLine("uint64:{0} {1}", i2 == UInt64.MaxValue, sizeof(UInt64));
            Console.WriteLine("uint64:{0} {1}", i4 == UInt64.MinValue, sizeof(UInt64));

            //char
            var j1 = BaseDataConverter.ToBytes(Char.MaxValue);
            var j2 = BaseDataConverter.ToChar(j1);
            var j3 = BaseDataConverter.ToBytes(Char.MinValue);
            var j4 = BaseDataConverter.ToChar(j3);
            Console.WriteLine("char:{0} {1}", j2 == Char.MaxValue, sizeof(Char));
            Console.WriteLine("char:{0} {1}", j4 == Char.MinValue, sizeof(Char));

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}