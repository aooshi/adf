using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
   public class ListSort
    {
        public class entity
        {
            public string a;
            public double s;
        }

        public static int SIZE = 100 * 10000;

        public static void TestEntity()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = entityList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                //MsdnAA.QuickSortApp.QuickSort(arr, 0, arr.Length - 1);

                //Array.Sort(arr);
                //Array.Sort<double>(arr, Compare);
                Array.Sort<entity>(arr, Compare);


                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }

        public static void TestDouble()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = scoreList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                //MsdnAA.QuickSortApp.QuickSort(arr, 0, arr.Length - 1);

               Array.Sort(arr);
               // Array.Sort<double>(arr, Compare);


                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }

        public static void TestQuickSort()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = scoreList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                MsdnAA.QuickSortApp.QuickSort(arr, 0, arr.Length - 1);
                

                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }

        public static void TestQuickSort2()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = scoreList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                QuickSort2.Qsort(arr, 0, arr.Length - 1);


                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }


        public static void TestQuickSort3()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = scoreList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                QuickSort3.Sort(arr);


                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }

        public static void TestHeapSort()
        {
            var rand = new Random();

            var scoreList = new List<double>();
            var entityList = new List<entity>();
            for (int i = SIZE; i > 0; i--)
            {
                var d = rand.NextDouble();

                scoreList.Add(d);
                entityList.Add(new entity()
                {
                    a = Guid.NewGuid().ToString("N"),
                    s = d
                });
            }

            var stopwatch = new Stopwatch();
            while (true)
            {
                var arr = scoreList.ToArray();

                stopwatch.Reset();
                stopwatch.Start();

                HeapSort.Sort(arr);


                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.Read();
            Environment.Exit(0);
        }


        private static int Compare(entity a, entity b)
        {
            return a.s == b.s ? 0 : (a.s < b.s ? -1 : 1);
        }

        private static int Compare(double a, double b)
        {
            return a == b ? 0 : (a < b ? -1 : 1);
        }


        //public static void TestCompareTo()
        //{
        //    double a, b;
        //    a = 2.2;
        //    b = 4.4;
        //    Console.WriteLine(a.CompareTo(b));
        //    Console.WriteLine(a == b ? 0 : (a < b ? -1 : 1));
        //    Console.Read();
        //    Environment.Exit(0);
        //}
    }
}
