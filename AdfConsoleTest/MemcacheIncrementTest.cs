using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace AdfConsoleTest
{
    class MemcacheIncrementTest
    {
        public void Test()
        {
            const int threadCount = 10;
            const int size = 10000;

            var host = "127.0.0.1";

            host = "192.168.199.14";

            var port = 201;

            port = 6224;

            Console.WriteLine("initialize 10000 cache item");

            var cacheItems = new List<string>(size);

            bool run = true;
            var eventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            using (var memcache = new Adf.Memcache(host, port))
            {
                memcache.ReadTimeout = 2000;

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

                Console.WriteLine("add " + (size + 1) + " item;");
                for (int i = 0; i <= size; i++)
                {
                    var key = "test" + i;
                    var value = i;
                    cacheItems.Add(key);

                    memcache.Set(key, value, 10);
                }

                new Thread(us =>
                {

                    Console.WriteLine("rand set begin");
                    var memcache2 = new Adf.Memcache(host, port);
                    while (run)
                    {
                        eventHandle.WaitOne(2000);
                        memcache2.Set("testrand" + DateTime.Now.Ticks, "10");
                    }

                    Console.WriteLine("rand set end");

                }).Start();


                Console.WriteLine();
                Console.WriteLine();


                Console.WriteLine("rand " + threadCount + " thread incr cache");
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
                        for (int j = 0; j < int.MaxValue && run; j = j == size ? 0 : j + 1)
                        {
                            key = "test" + j;
                            if (memcache2.Increment(key) == -1)
                            {
                                Console.WriteLine("no find " + key);
                            }
                            total++;
                        }
                        memcache2.Dispose();
                        stopwatch.Stop();

                        Console.WriteLine("Increment {3}-> total:{0}, seconds:{1}, {2} set/s"
                            , total
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , i
                            );

                    }).Start(i);
                }

                Console.WriteLine("rand " + threadCount + " thread decr cache");
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
                        for (int j = size; j > -1 && run; j = j == 0 ? size : j - 1)
                        {
                            key = "test" + j;
                            if (memcache2.Decrement(key) == -1)
                            {
                                Console.WriteLine("no find " + key);
                            }
                            //value = memcache2.Get(key);
                            //if (value != key)
                            //{
                            //    Console.WriteLine("{0} <> {1}", key, value);
                            //}

                            total++;
                        }
                        memcache2.Dispose();
                        stopwatch.Stop();

                        Console.WriteLine("Decrement {3}-> total:{0}, seconds:{1}, {2} get/s"
                            , total
                            , (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                            , i
                            );

                    }).Start(i);
                }


                Console.WriteLine("series incr");
                new Thread(userstate =>
                {
                    var memcache2 = new Adf.Memcache(host, port);
                    var key = "";
                    var total = 0L;
                    var value = 0L;
                    //
                    if (string.IsNullOrEmpty(memcache2.Get("testseries")))
                    {
                        memcache2.Set("testseries", "1");
                    }
                    //
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    for (int j = 0; j < int.MaxValue && run; j++)
                    {
                        key = "testseries";
                        value = memcache2.Increment(key);
                        if (value == -1)
                        {
                            Console.WriteLine("no find " + key);
                        }
                        else
                        {
                            Console.WriteLine(key + " => " + value);
                        }
                        total++;
                    }
                    memcache2.Dispose();
                    stopwatch.Stop();

                    Console.WriteLine("series incr -> total:{0}, seconds:{1}, {2} set/s"
                        , total
                        , (double)(stopwatch.ElapsedMilliseconds / 1000)
                        , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                        );

                }).Start();
            }

            Console.WriteLine("key entry to stop");
            Console.ReadLine();
            eventHandle.Set();
            run = false;

        }

        //public static void Test2()
        //{
        //    using (var socket = new Socket(AddressFamily.InterNetwork
        //      , SocketType.Stream
        //      , ProtocolType.Tcp))
        //    {
        //        socket.NoDelay = true;
        //        socket.Connect("192.168.199.14", 226);
        //        using (var stream = new NetworkStream(socket, false))
        //        {
        //            var command = "delete test1\r\n";
        //            var buffer = Encoding.ASCII.GetBytes(command);

        //            stream.Write(buffer, 0, buffer.Length);
        //            //read
        //            string line = Adf.StreamHelper.ReadLine(stream, Encoding.ASCII);
        //            Console.WriteLine(line);


        //            //add test1 0 0 1\r\n
        //            command = "add test1 0 0 1\r\n";
        //            buffer = Encoding.ASCII.GetBytes(command);
        //            stream.Write(buffer, 0, buffer.Length);

        //            buffer = Encoding.ASCII.GetBytes("1");
        //            stream.Write(buffer, 0, buffer.Length);

        //            buffer = Encoding.ASCII.GetBytes("\r\n");
        //            stream.Write(buffer, 0, buffer.Length);

        //            line = Adf.StreamHelper.ReadLine(stream, Encoding.ASCII);
        //            Console.WriteLine(line);


        //            //set test1 0 0 1\r\n
        //            command = "set test1 0 0 1\r\n";
        //            buffer = Encoding.ASCII.GetBytes(command);
        //            stream.Write(buffer, 0, buffer.Length);

        //            buffer = Encoding.ASCII.GetBytes("1");
        //            stream.Write(buffer, 0, buffer.Length);

        //            buffer = Encoding.ASCII.GetBytes("\r\n");
        //            stream.Write(buffer, 0, buffer.Length);

        //            line = Adf.StreamHelper.ReadLine(stream, Encoding.ASCII);
        //            Console.WriteLine(line);

        //            //get test1\r\n
        //            command = "get test1\r\n";
        //            buffer = Encoding.ASCII.GetBytes(command);
        //            stream.Write(buffer, 0, buffer.Length);

        //            line = Adf.StreamHelper.ReadLine(stream, Encoding.ASCII);
        //            Console.WriteLine(line);
        //        }
        //    }
        //}


        class Member : IDisposable
        {
            public long id;

            public Member(long id)
            {
                this.id = id;
            }

            public void Write()
            {
                Console.WriteLine("M: " + this.id);
            }

            public void Dispose()
            {

            }
        }
    }
}