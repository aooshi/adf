using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Data;

namespace AdfConsoleTest
{
    class UniqueID
    {
        static Random random = new Random();

        public static void init()
        {
            for (int i = 0; i < 20; i++)
            {
                var v = Adf.UniqueID.Generate();
                Console.WriteLine("{0}:{1}",v.Length,v);
            }


            Console.WriteLine(Adf.UniqueID.Generate("fdjkslaf1", ""));
            Console.WriteLine(Adf.UniqueID.Generate("fdjkslaf1", "").Length);
            Console.WriteLine(Adf.UniqueID.Generate("", "dsad2"));
            Console.WriteLine(Adf.UniqueID.Generate("", "dsad2").Length);

            //
            var time = (new DateTime(2010, 1, 1).ToUniversalTime().Ticks - new DateTime(1900, 1, 1).ToUniversalTime().Ticks) / 10000000;
            Console.WriteLine(Adf.UniqueID.Encoder.Encode(time));
            time = (new DateTime(3010, 1, 1).ToUniversalTime().Ticks - new DateTime(1900, 1, 1).ToUniversalTime().Ticks) / 10000000;
            Console.WriteLine(Adf.UniqueID.Encoder.Encode(time));



            //var nbe = new Adf.NumberBaseEncode("0123456789ABCDEFGHIJKLMNOPQRSTUVWXY");

            //Console.WriteLine(nbe.Encode(uint.MaxValue));
            //Console.WriteLine(nbe.Encode(int.MaxValue));
            //Console.WriteLine(nbe.Encode(46656));
            //Console.WriteLine(nbe.Encode(1679616));
            //Console.WriteLine(nbe.Encode(35));
            //Console.WriteLine(nbe.Encode(36));
            //Console.WriteLine(nbe.Encode(37));
            //Console.WriteLine(nbe.Decode("10"));
            //Console.WriteLine(nbe.Decode("100"));
            //Console.WriteLine(nbe.Decode("1000"));
            //Console.WriteLine(nbe.Decode("10000"));
            //Console.WriteLine(nbe.Decode("100000"));


            
            //for (int i = 0; i < 100; i++)
            //{

            //    new Thread(() => {

            //        while (true)
            //        {
            //            var id = Adf.UniqueID.Generate();

            //            var conn = new System.Data.SqlClient.SqlConnection("server=10.10.10.20;user id=sa;password=developer;database=RijndaelId;");
            //            conn.Open();
            //            var cmd = conn.CreateCommand();
            //            cmd.CommandText = "insert into uidtable values ('" + id + "')";
            //            cmd.ExecuteNonQuery();
            //            conn.Close();
            //        }
                
            //    }).Start();
            //}

            //Console.WriteLine( "start 100 thread insert table" );


            //Console.WriteLine( Adf.UniqueID.Encoder.Encode(int.MaxValue) );



            Console.Read();
        }
        
        //public static void init1()
        //{
        //    //time      tick
        //    //hostname  crc32
        //    //rand-4    rand            

        //    var length = 36;
        //    //
        //    var unixstamp = Adf.UnixTimestampHelper.ToTimestamp();
        //    var microsecond = int.Parse(DateTime.Now.ToString("fffffff"));
        //    var hostname = Math.Abs( Adf.CRC32Helper.Encode(System.Net.Dns.GetHostName()) );
        //    var rand = random.Next(100000, 999999);
        //    //
        //    var eunixstamp = Adf.Base62Helper.Encode(unixstamp, length);
        //    var emicrosecond = Adf.Base62Helper.Encode(microsecond, length);
        //    var ehostname = Adf.Base62Helper.Encode(hostname, length);
        //    var erand = Adf.Base62Helper.Encode(rand, length);
        //    //
        //    var dtimetick = Adf.Base62Helper.Decode(eunixstamp, length);
        //    var dmicrosecond = Adf.Base62Helper.Decode(emicrosecond, length);
        //    var dhostname = Adf.Base62Helper.Decode(ehostname, length);
        //    var drand = Adf.Base62Helper.Decode(erand, length);
        //    //
        //    var build = new StringBuilder();
        //    build.Append(eunixstamp);
        //    build.Append(emicrosecond);
        //    build.Append(ehostname);
        //    build.Append(erand);
        //    //
        //    var id = build.ToString();
        //    //
        //    Console.WriteLine(DateTime.Now.ToString("fffffff"));
        //    Console.WriteLine(DateTime.Now.ToString("fffffff"));
        //    //
        //    Console.WriteLine("Unix Stamp: {0}", unixstamp);
        //    Console.WriteLine("Microsecond: {0}", microsecond);
        //    Console.WriteLine("Host Name: {0}", hostname);
        //    Console.WriteLine("Rand Num#: {0}", rand);
        //    Console.WriteLine();
        //    Console.WriteLine("Time Tick: {0},{1}", eunixstamp, dtimetick);
        //    Console.WriteLine("Microsecond: {0},{1}", emicrosecond, dmicrosecond);
        //    Console.WriteLine("Host Name: {0},{1}", ehostname, dhostname);
        //    Console.WriteLine("Rand Num : {0},{1}", erand, drand);
        //    Console.WriteLine();
        //    Console.WriteLine("ID       : {0}", id);
        //    Console.WriteLine("ID Length: {0}", id.Length);
        //    Console.WriteLine();

        //    for (int i = 0; i < 100; i++)
        //    {
        //        UniqueID.encode();
        //    }

        //    Console.Read();

        //}

        //private static void encode()
        //{
        //    var length = 36;
        //    //
        //    var unixstamp = Adf.UnixTimestampHelper.ToTimestamp();
        //    var microsecond = int.Parse(DateTime.Now.ToString("fffffff"));
        //    var hostname = Math.Abs( Adf.CRC32Helper.Encode(System.Net.Dns.GetHostName()) );
        //    var rand = random.Next(1, Int32.MaxValue);
        //    //
        //    var eunixstamp = Adf.Base62Helper.Encode(unixstamp, length);
        //    var emicrosecond = Adf.Base62Helper.Encode(microsecond, length);
        //    var ehostname = Adf.Base62Helper.Encode(hostname, length);
        //    var erand = Adf.Base62Helper.Encode(rand, length);
        //    //
        //    var build = new StringBuilder();
        //    build.Append(eunixstamp);
        //    build.Append(emicrosecond);
        //    build.Append(ehostname);
        //    build.Append(erand);
        //    //
        //    var id = build.ToString();
        //    Console.WriteLine(id);

        //}
    }
}
