using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace AdfConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = Adf.Arguments.Parse(args);
            var testname = "";
            parser.TryGetValue("name", out testname);

            while (true)
            {              
                //
                if (testname == "")
                {
                    Console.WriteLine("key in test name");
                    testname = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("RUN " + testname);
                }


                var type = typeof(AdfConsoleTest.Program).Assembly.GetType("AdfConsoleTest." + testname, false,true);
                if (type == null)
                {
                    Console.WriteLine("no find test class AdfConsoleTest." + testname);
                    continue;
                }

                var method = type.GetMethod("test", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method == null)
                {
                    Console.WriteLine("no find test method AdfConsoleTest." + testname + ".test");
                    continue;
                }

                try
                {
                    method.Invoke(Activator.CreateInstance(type), new object[0]);
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    Console.WriteLine(exception.GetBaseException().ToString());
                }
            }
            
            //Skip32Test.Test();

            //ListSort.SIZE = 100 * 10000;
            //ListSort.TestEntity();
            //ListSort.TestDouble();
            //ListSort.TestQuickSort();
            //ListSort.TestQuickSort2();
            //ListSort.TestQuickSort3();
            //ListSort.TestHeapSort();

            //LogManagerTest.Test();

            //ConfigTest.Init();

            //SerializeTest.BaseDataSerializableTest();
            //DataSerializableTest.Test();
            //RunStopwatch.Test();
            //PoolTest.Test();
            //SmtpTest.Test();

            //new HttpServerTest2();
            //new HttpServerTest();
            //Console.Read();

            //ConsistentHashingTest.Test();

            //
            //RedisClientTest.Test("192.168.199.53", 6379);
            //Console.Read();


            //UUIDEncoder.init();
            //Console.Read();

            //UniqueID.init();

            //var smtp = new Adf.Smtp();
            //smtp.Host = "smtp.163.com";
            //smtp.Account = "fafffsfda.com.cn";
            //smtp.Password = "Mail2fr";

            ////smtp.SSLEnabled = true;
            ////smtp.Port = 465;

            //string message;

            //Console.WriteLine(smtp.CheckAuth(out message) + " " + message);
            //Console.Read();


            //test mq
            //new Mq();

            //test json
            //JsonTest.Test();


            //PathHelperTest.NumberPath();

            //server test
            //new HttpServerTest();

            //TestDnsHelper.T();

            //WhoisTest.Test();

            //StringFormatTest.Test();
        }
    }
}