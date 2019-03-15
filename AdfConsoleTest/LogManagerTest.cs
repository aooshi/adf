using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AdfConsoleTest
{
    public static class LogManagerTest
    {
        const int RUN_COUNT = 2000 * 10000;

        public static void Test()
        {
            bool run = true;
            var total = 0L;
            while (true)
            {
                total = 0;
                run = true;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Thread.Sleep(5000);
                    run = false;
                }).Start();

                var log = new Adf.LogManager("test", "c:\\logs\test\\");
                //log.Message.FlushInterval = 5;
                log.SetFlushInterval(5);

                var stopwatch = new Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();
                while (run)
                {
                    total++;
                    log.Message.Write("total:{0}, seconds:{1}, {2} loop");
                }
                stopwatch.Stop();
                Console.WriteLine("LogManager.Message.Write total:{0}, seconds:{1}, {2} loop/s"
                    , total
                    , (double)(stopwatch.ElapsedMilliseconds / 1000)
                    , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                    );

                total = 0;
                run = true;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Thread.Sleep(5000);
                    run = false;
                }).Start();
                stopwatch.Reset();
                stopwatch.Start();
                while (run)
                {
                    total++;
                    log.Message.WriteLine("total:{0}, seconds:{1}, {2} loop");
                }
                stopwatch.Stop();
                Console.WriteLine("LogManager.Message.WriteLine total:{0}, seconds:{1}, {2} loop/s"
                    , total
                    , (double)(stopwatch.ElapsedMilliseconds / 1000)
                    , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                    );

                total = 0;
                run = true;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Thread.Sleep(5000);
                    run = false;
                }).Start();
                stopwatch.Reset();
                stopwatch.Start();
                while (run)
                {
                    total++;
                    log.Message.WriteTimeLine("total:{0}, seconds:{1}, {2} loop");
                }
                stopwatch.Stop();
                Console.WriteLine("LogManager.Message.WriteTimeLine total:{0}, seconds:{1}, {2} loop/s"
                    , total
                    , (double)(stopwatch.ElapsedMilliseconds / 1000)
                    , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                    );

                Console.WriteLine();
                Console.WriteLine();
            }
            Console.Read();
        }
    }
}
