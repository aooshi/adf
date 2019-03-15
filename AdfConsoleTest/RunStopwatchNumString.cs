using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class RunStopwatchNumString
    {
        const int RUN_COUNT = 2000 * 10000;

        public void Test()
        {
            var total = 0L;
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            byte[] bytes = new byte[16];
            long n1 = 100;

            var charsLength = 16;
            var chars = new char[] {
                'g',
                'h',
                't',
                'v',
                'p',
                'k',
                's',
                'u',
                'm',
                'n',
                'q',
                'r',
                'x',
                'y',
                'w',
                'z'
            };
            
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

                //var v0 = Guid.NewGuid().ToString("N");
                

                //var v3 = v2.ToString();
                //Console.WriteLine(v3);

                //var v4 = v2.ToString("F4");
                //Console.WriteLine(v4);

                //var v5 = Adf.NumberHelper.BytesToHex(bytes);
                //Console.WriteLine(v5);

                //var v6 = n1.ToString("x") + "." + total.ToString("x");
                //var v6 = n1.ToString("x") + chars[total % charsLength] + total.ToString("x");
                var v6 = string.Concat(n1.ToString("x"), chars[total % charsLength], total.ToString("x"));
                Console.WriteLine(v6);


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
