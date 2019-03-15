using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class RunStopwatchTest
    {
        const int RUN_COUNT = 2000 * 10000;

        public void Test()
        {
            var total = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            while (++total != RUN_COUNT)
            {
                //var a = DateTime.Now;
                //var b = DateTime.Now.Subtract(a).TotalSeconds;
                //var a = Environment.TickCount;
                //var a = System.Diagnostics.Stopwatch.StartNew();
                //var a = new System.Diagnostics.Stopwatch();
                //a.Start();
                //a.Stop();
                //var b = a.Elapsed.TotalSeconds;

            }
            stopwatch.Stop();
            Console.WriteLine("total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );
        }
    }
}
