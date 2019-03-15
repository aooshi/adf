using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace AdfConsoleTest
{
    class MemcacheTest
    {
        public void Test()
        {
            const int threadCount = 1;

            var host = "127.0.0.1";

            //host = "192.168.199.13";

            var port = 201;

            //port = 11211;

            Console.WriteLine("initialize 10000 cache item");

            var cacheItems = new List<string>(10000);

            bool run = true;
            using (var memcache = new Adf.Memcache(host, port))
            {
                Console.WriteLine("del test1:" + memcache.Delete("test1"));
                Console.WriteLine("add test1:" + memcache.Add("test1", "1"));
                Console.WriteLine("set test1:" + memcache.Set("test1", "2"));
                Console.WriteLine("get test1:" + memcache.Get("test1"));
                var test1 = memcache.Increment("test1");
                Console.WriteLine("inc test1:" + test1);
                test1 = memcache.Decrement("test1");
                Console.WriteLine("dec test1:" + test1);
                Console.WriteLine("del test1:" + memcache.Delete("test1"));


                //Console.WriteLine("key enter continue;");
                //Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine();

                var stats = memcache.Stats();
                var enumerator = stats.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var k = string.Concat(enumerator.Key).PadRight(30, ' ');
                    Console.WriteLine("{0}:{1}", k, enumerator.Value);
                }


                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("add " + cacheItems.Capacity + " item;");
                for (int i = 0; i < cacheItems.Capacity; i++)
                {
                    var key = "k" + i;
                    var value = "value" + key;
                    cacheItems.Add(key);

                    memcache.Set(key, value, 10);
                }


                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("clean:" + memcache.FlushAll());

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("rand " + threadCount + " thread set cache");
                for (int i = 0; i < threadCount; i++)
                {
                    new Thread(userstate =>
                    {
                        var index = (int)userstate;
                        var memcache2 = new Adf.Memcache(host, port);
                        var key = "";
                        var total = 0L;
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        for (int j = 0; j < int.MaxValue && run; j = j > 10000 ? 0 : j + 1)
                        {
                            key = "item" + j;
                            memcache2.Set(key, key, 100);
                            total++;
                        }
                        memcache2.Dispose();
                        stopwatch.Stop();

                        Console.WriteLine("set {3}-> total:{0}, seconds:{1}, {2} set/s"
                            , total
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , i
                            );

                    }).Start(i);
                }

                Console.WriteLine("rand " + threadCount + " thread get cache");
                for (int i = 0; i < threadCount; i++)
                {
                    new Thread(userstate =>
                    {
                        var index = (int)userstate;
                        var memcache2 = new Adf.Memcache(host, port);
                        var key = "";
                        var value = "";
                        var total = 0L;
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        for (int j = 0; j < int.MaxValue && run; j = j > 10000 ? 0 : j + 1)
                        {
                            key = "item" + j;
                            value = memcache2.Get(key);
                            if (value != key)
                            {
                                Console.WriteLine("{0} <> {1}", key, value);
                            }

                            total++;
                        }
                        memcache2.Dispose();
                        stopwatch.Stop();

                        Console.WriteLine("get {3}-> total:{0}, seconds:{1}, {2} get/s"
                            , total
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , i
                            );

                    }).Start(i);
                }

            }
            Console.WriteLine("key entry stop");
            Console.ReadLine();

            run = false;
        }
    }
}