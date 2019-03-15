using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    class WebSocketPerformanceTest
    {
        public void Test()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(wi =>
            {
                TestMethod();
            });
            System.Threading.ThreadPool.QueueUserWorkItem(wi =>
            {
                TestMethod();
            });
            System.Threading.ThreadPool.QueueUserWorkItem(wi =>
            {
                TestMethod();
            });

            Console.ReadLine();
        }

        private void TestMethod()
        {
            long s = 0;
            long r = 0;

            var tick1 = Environment.TickCount;
            System.Threading.ThreadPool.QueueUserWorkItem(wi =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (s == 0)
                    {
                        continue;
                    }
                    //
                    var tick2 = Environment.TickCount;
                    var tick = tick2 - tick1;

                    Console.WriteLine();
                    Console.WriteLine("send " + Math.Round((double)(s / (tick / 1000)), 3));
                    Console.WriteLine("read " + Math.Round((double)(r / (tick / 1000)), 3));
                }

            });

            using (var client = new Adf.WebSocketClient("127.0.0.1", 888))
            {
                client.Message += (object sender, Adf.WebSocketMessageEventArgs e) =>
                {
                    System.Threading.Interlocked.Increment(ref r);
                };
                client.Error += (object sender, Adf.WebSocketErrorEventArgs e) =>
                {
                    Console.WriteLine(e.Exception.Message);
                };

                var buffer = Guid.NewGuid().ToByteArray();

                while (true)
                {
                    if (client.IsConnectioned == false)
                    {
                        client.Connection();
                        Console.WriteLine("connect");
                        System.Threading.Thread.Sleep(2000);
                    }
                    
                    try
                    {
                        client.Send(buffer);
                    }
                    catch
                    {
                        Console.WriteLine("is closed");
                    }
                    System.Threading.Interlocked.Increment(ref s);
                }

                //Console.WriteLine("Close");
            }
        }
    }
}