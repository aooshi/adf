using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class PoolTest
    {
        public void Test()
        {
            var hashVNode = 100;

            //4 member
            var pool = new Adf.Pool<PoolInstance>(800, new PoolMember[] {
                new PoolMember(),
                new PoolMember(),
                new PoolMember(),
                new PoolMember(),
            },hashVNode);

            bool run = true;

            int threadCount = 1;
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                int index = 0;
                while (run)
                {
                    Console.WriteLine("Available Instance: " + pool.ActiveCount + "");
                    Console.WriteLine("Runing Instance: " + pool.RuningCount + "");
                    System.Threading.Thread.Sleep(1000);

                    if (index++ == 5)
                        run = false;
                }
            });


            string hashKey = null;

            Console.WriteLine("" + threadCount + " thread single member");
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new System.Threading.Thread((threadIndex) =>
                {
                    var total = 0L;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (run)
                    {
                        pool.Call(instance => {

                            //instance.Test();
                            instance.TestValue((int)total);

                        },hashKey,null);
                        total++;
                    }
                    stopwatch.Stop();

                    Console.WriteLine("{3}-> total:{0}, seconds:{1}, {2} call/s"
                        , total
                        , (double)(stopwatch.ElapsedMilliseconds / 1000)
                        , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                        , threadIndex
                        );

                });
                thread.IsBackground = true;
                thread.Start(i);
            }

            //Console.WriteLine("any key stop");
            //Console.ReadLine();

        }




        public class PoolMember : Adf.IPoolMember
        {
            public bool PoolActive
            {
                get;
                set;
            }
            string id = Guid.NewGuid().ToString();
            public string PoolMemberId
            {
                get
                {
                    return this.id;
                }
            }

            public Adf.IPoolInstance CreatePoolInstance()
            {
                return new PoolInstance();
            }
        }

        public class PoolInstance : Adf.IPoolInstance
        {
            public void Test()
            {

            }

            public int TestValue(int v)
            {
                return v;
            }

            public bool PoolAbandon
            {
                get;
                set;
            }

            public void Dispose()
            {
            }
        }

    }
}
