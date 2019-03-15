using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;

namespace AdfConsoleTest
{
    /// <summary>
    /// time id generate
    /// </summary>
    public class TimeIdGeneratorTest
    {
        const int RUN_COUNT = 40 * 10000;

        public void Test()
        {
            var generate = new Adf.TimeIdGenerator(Adf.RandomHelper.Letter(16), 1);
            var id1 = "";
            var id2 = "";
            id1 = generate.GenerateId();
            id2 = generate.GenerateHexId();
            Console.WriteLine("{0}:\t{1}", id1.Length,id1);
            Console.WriteLine("{0}:\t{1}", id2.Length,id2);

            var total = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            while (++total != RUN_COUNT)
            {
                generate.GenerateId();
            }
            stopwatch.Stop();
            Console.WriteLine("base58 total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
            //
            total = 0;
            stopwatch.Reset();
            stopwatch.Start();
            while (++total != RUN_COUNT)
            {
                generate.GenerateHexId();
            }
            stopwatch.Stop();
            Console.WriteLine("hex total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );

            while (true)
            {
                System.Threading.Thread.Sleep(260);

                id1 = generate.GenerateId();
                id2 = generate.GenerateHexId();
                Console.WriteLine("{0}:\t{1}", id1.Length, id1);
                Console.WriteLine("{0}:\t{1}", id2.Length, id2);
            }

            Console.Read();
        }
    }
}