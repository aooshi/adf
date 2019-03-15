using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class ThreadSlatTest
    {
        const int RUN_COUNT = 1000 * 10000;

        public void Test()
        {
            LocalDataStoreSlot slot = System.Threading.Thread.AllocateNamedDataSlot("ThreadSlatTest");


            var total = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            while (++total != RUN_COUNT)
            {
                var obj = new object();
                System.Threading.Thread.SetData(slot, obj);
                var obj2 = System.Threading.Thread.GetData(slot);

                if (obj != obj2)
                {
                    Console.WriteLine("no match");
                    Console.ReadLine();
                }
            }
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
