using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    class BaseDataSerializableTest
    {
        public void Test()
        {
            //big-endian

            var un = new string[10];
            var ua = new byte[10][];
            var us = new byte[10][];

            un[0] = "sbyte";
            ua[0] = new byte[] { (byte)sbyte.MaxValue }; // Adf.BaseDataConverter.ToBytes(sbyte.MaxValue);
            us[0] = new byte[] { (byte)sbyte.MaxValue }; //System.BitConverter.GetBytes(sbyte.MaxValue);
            //if (System.BitConverter.IsLittleEndian) Array.Reverse(us[0]);


            un[1] = "byte";
            //ua[1] = Adf.BaseDataConverter.ToBytes((byte)(byte.MaxValue - 1));
            //us[1] = System.BitConverter.GetBytes((byte)(byte.MaxValue - 1));
            ua[1] = new byte[] { (byte)(byte.MaxValue - 1) };
            us[1] = new byte[] { (byte)(byte.MaxValue - 1) };
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[1]);


            un[2] = "int16";
            ua[2] = Adf.BaseDataConverter.ToBytes(Int16.MaxValue);
            us[2] = System.BitConverter.GetBytes(Int16.MaxValue);
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[2]);


            un[3] = "uint16";
            ua[3] = Adf.BaseDataConverter.ToBytes((UInt16)(UInt16.MaxValue - 1));
            us[3] = System.BitConverter.GetBytes((UInt16)(UInt16.MaxValue - 1));
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[3]);

            un[4] = "int32";
            ua[4] = Adf.BaseDataConverter.ToBytes(Int32.MaxValue);
            us[4] = System.BitConverter.GetBytes(Int32.MaxValue);
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[4]);


            un[5] = "uint32";
            ua[5] = Adf.BaseDataConverter.ToBytes((UInt32)(UInt32.MaxValue - 1));
            us[5] = System.BitConverter.GetBytes((UInt32)(UInt32.MaxValue - 1));
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[5]);

            un[6] = "int64";
            ua[6] = Adf.BaseDataConverter.ToBytes(Int64.MaxValue);
            us[6] = System.BitConverter.GetBytes(Int64.MaxValue);
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[6]);


            un[7] = "uint64";
            ua[7] = Adf.BaseDataConverter.ToBytes((UInt64)(UInt64.MaxValue - 1));
            us[7] = System.BitConverter.GetBytes((UInt64)(UInt64.MaxValue - 1));
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[7]);

            un[8] = "float";
            ua[8] = Adf.BaseDataConverter.ToBytes((float)float.MaxValue);
            us[8] = System.BitConverter.GetBytes((float)float.MaxValue);
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[8]);


            un[9] = "double";
            ua[9] = Adf.BaseDataConverter.ToBytes((double)(double.MaxValue - 1));
            us[9] = System.BitConverter.GetBytes((double)(double.MaxValue - 1));
            if (System.BitConverter.IsLittleEndian) Array.Reverse(us[9]);




            for (int i = 0; i < us.Length; i++)
            {
                var n = un[i];
                var a = ua[i];
                var s = us[i];


                var a1 = Adf.NumberHelper.BytesToHex(a);
                var s1 = Adf.NumberHelper.BytesToHex(s);

                Console.WriteLine("adf:{0}:{1}", n, a1);
                Console.WriteLine("sys:{0}:{1}", n, s1);
                Console.WriteLine("diff:" + this.Differ(a, s));

                Console.WriteLine();
                Console.WriteLine();


            }



            //var d1 = Adf.BaseDataConverter.ToBytes((int)32769);
            //var d2 = Adf.BaseDataConverter.ToBytes((uint)32769);
            //var d3 = Adf.BaseDataConverter.ToBytes((long)32769);
            //var d4 = Adf.BaseDataConverter.ToBytes((ulong)32769);
            //var d5 = Adf.BaseDataConverter.ToBytes((short)32767);
            //var d6 = Adf.BaseDataConverter.ToBytes((ushort)32769);

            //var b1 = System.BitConverter.IsLittleEndian;
            //var b2 = System.BitConverter.GetBytes((int)32769);
            //var b3 = System.BitConverter.GetBytes((uint)32769);
            //var b4 = System.BitConverter.GetBytes((long)32769);
            //var b5 = System.BitConverter.GetBytes((uint)32769);


            //Console.WriteLine("{0},{1},{2},{3}", d1[0], d1[1], d1[2], d1[3]);
            //Console.WriteLine("{0},{1},{2},{3}", d2[0], d2[1], d2[2], d2[3]);

            Console.ReadLine();
        }

        private string Differ(byte[] a, byte[] s)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != s[i])
                {
                    return "no / adf: " + a[i].ToString("X2") + "/ sys: " + s[i].ToString("X2") + " / index: " + i;
                }
            }

            return "yes";
        }
    }
}
