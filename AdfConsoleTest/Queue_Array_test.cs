using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    class Queue_Array_test
    {
        public bool A;

        public static void t()
        {
            var queue = new System.Collections.Queue();
            var arr = new Queue_Array_test[500];
            for (var i = 0; i < 500; i++)
            {
                queue.Enqueue(new Queue_Array_test());
                arr[i] = new Queue_Array_test();
            }
            var start = new Stopwatch();
            start.Start();

            object o;
            for (var i = 0; i < 1000000; i++)
            {
                o = queue.Dequeue();
                queue.Enqueue(o);
            }
            start.Stop();

            Console.WriteLine(start.ElapsedMilliseconds);

            start.Reset();
            start.Start();

            for (var i = 0; i < 1000000; i++)
            {
                var b = i % 500;

                if (arr[b].A)
                {
                }
                //arr[b].A = true;
                //arr[b].A = false;
            }
            start.Stop();

            Console.WriteLine(start.ElapsedMilliseconds);
        }
    }
}
