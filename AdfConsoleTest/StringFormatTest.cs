using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace AdfConsoleTest
{
    public class StringFormatTest
    {
        const int RUN_COUNT = 100 * 10000;

        public static void Test()
        {
            var total = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            var input = "my name is {name}, and my birthday {birthday}";

            var dictionary = new Dictionary<string, string>();
            dictionary.Add("name", "NAME");
            dictionary.Add("birthday", "2010/01/01");

            var value = "";
            Console.WriteLine("data:");
            foreach (var item in dictionary)
            {
                Console.WriteLine("\t{0}:{1}", item.Key, item.Value);
            }
            Console.WriteLine("source:" + input);
            while (++total != RUN_COUNT)
            {
                value = Adf.StringHelper.Format(input, dictionary);
            }
            Console.WriteLine("format:" + value);

            stopwatch.Stop();
            Console.WriteLine("total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
            Console.Read();
        }
    }
}
