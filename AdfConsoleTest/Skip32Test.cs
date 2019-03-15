using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class Skip32Test
    {
        //public void Test()
        //{
        //    var cipher1 = new Adf.Skip32Cipher("uIN1qoCy8k");
        //    var cipher2 = new Adf.Skip32Cipher("zVOkhWWbsX");

        //    var dictionary = new Dictionary<uint, uint>();

        //    for (int i = 0; i < 10000; i++)
        //    {
        //        var a = (uint)cipher1.Encrypt(i);
        //        var b = (uint)cipher2.Encrypt(i);

        //        dictionary.Add(a, a);
        //        dictionary.Add(b, b);

        //        Console.WriteLine("A:" + a.ToString().PadLeft(10, '0'));
        //        Console.WriteLine("B:" + b.ToString().PadLeft(10, '0'));
        //    }

        //}
        public void Test()
        {
            var cipher = new Adf.Skip32Cipher("uIN1qoCy8k");
            var r = 0L;

            r = cipher.Encrypt(14294967295L);
            Console.WriteLine("14294967295:" + r);
            r = cipher.Encrypt(24294967295L);
            Console.WriteLine("24294967295:" + r);
            r = cipher.Encrypt(34294967295L);
            Console.WriteLine("34294967295:" + r);
            r = cipher.Encrypt(44294967295L);
            Console.WriteLine("44294967295:" + r);

            Console.WriteLine("=====================  ");

            /*
            var i = 14294967295;
            */
            long i = 10000000000;
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                r = cipher.Encrypt(++i);
                Console.WriteLine(i.ToString() + ":" + r);
            }

            Console.ReadLine();
        }
        //public void Test()
        //{
        //    var cipher1 = new Adf.Skip32Cipher("uIN1qoCy8k");
        //    var cipher2 = new Adf.Skip32Cipher("zVOkhWWbsX");

        //    var dictionary = new Dictionary<long, long>();

        //    for (long i = 0; i < 10000; i++)
        //    {
        //        var a = (long)cipher1.Encrypt(i);
        //        var b = (long)cipher2.Encrypt(i);

        //        dictionary.Add(a, a);
        //        dictionary.Add(b, b);

        //        Console.WriteLine("A:" + a.ToString().PadLeft(10, '0'));
        //        Console.WriteLine("B:" + b.ToString().PadLeft(10, '0'));
        //    }

        //}
    }
}
