namespace AdfConsoleTest
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Adf;
using System.Threading;

    public class RedisClientTest
    {
        public string A
        {
            get;
            set;
        }
        public string B
        {
            get;
            set;
        }

        public static void Test(string host,int port)
        {
            RedisClient r;
                r = new RedisClient(host, port);

              pubsub(r, new RedisClient(host, port));
              return;

                r.Set("ab", new RedisClientTest() { A = "a",  B ="b"});
                var ab = r.Get<RedisClientTest>("ab");
                if (ab == null)
                    Console.WriteLine("error: set object");


                r.Set("foo", "bar");
                r.FlushAll();
            
                if (r.GetKeys().Length > 0)
                    Console.WriteLine("error: there should be no keys but there were {0}", r.GetKeys().Length);
                r.Set("foo", "bar");
                if (r.GetKeys().Length < 1)
                    Console.WriteLine("error: there should be at least one key");
                if (r.GetKeys("f*").Length < 1)
                    Console.WriteLine("error: there should be at least one key");

                if (r.TypeOf("foo") != RedisKeyType.String)
                    Console.WriteLine("error: type is not string");
                r.Set("bar", "foo");

                var arr = r.GetKeys("foo", "bar");
                if (arr.Length != 2)
                    Console.WriteLine("error, expected 2 values");
                if (arr[0].Length != 3)
                    Console.WriteLine("error, expected foo to be 3");
                if (arr[1].Length != 3)
                    Console.WriteLine("error, expected bar to be 3");


                r.Set("one", "world");
                if (r.GetSet("one", "newvalue") != "world")
                    Console.WriteLine("error: Getset failed");
                if (!r.Rename("one", "two"))
                    Console.WriteLine("error: failed to rename");
                //if (r.Rename("one", "one"))
                //    Console.WriteLine("error: should have sent an error on rename");

                if (r.Select(10) == false)
                    Console.WriteLine("error: select error ,db index 10");

                r.Set("foo", "diez");
                if (r.Get("foo") != "diez")
                {
                    Console.WriteLine("error: got {0}", r.Get("foo"));
                }
                if (!r.Remove("foo"))
                    Console.WriteLine("error: Could not remove foo");

                r.Select(0);
                if (r.Get("foo") != "bar")
                    Console.WriteLine("error, foo was not bar:" + r.Get("foo"));
                //if (!r.ContainsKey("foo"))
                //    Console.WriteLine("error, there is no foo");
                //if (r.Remove("foo", "bar") != 2)
                //    Console.WriteLine("error: did not remove two keys");
                //if (r.ContainsKey("foo"))
                //    Console.WriteLine("error, foo should be gone.");
                //r.Save();
                //r.BackgroundSave();
                //Console.WriteLine("Last save: {0}", r.LastSave);
                //r.Shutdown ();

                var info = r.GetInfo();
                foreach (var k in info.Keys)
                {
                    Console.WriteLine("{0} -> {1}", k, info[k]);
                }

                var dict = new Dictionary<string, string>();
                dict["hello"] = "world";
                dict["goodbye"] = "my dear";
                r.Set (dict);
                assert(r.Get("hello") == "world" && r.Get("goodbye") == "my dear", "errir: set dict error");


                assert( r.Lists.RPush("alist", "avalue") == 1,"List.RPush Error");
                assert(r.Lists.RPush("alist", "another value") == 2,"List.RPush Error");
                assert(r.Lists.LLen("alist") == 2, "List length should have been 2");

                var value = r.Lists.LIndex("alist", 1);
                if (!value.Equals("another value"))
                    Console.WriteLine("error: Received {0} and should have been 'another value'", value);
                value = r.Lists.LPop("alist");
                if (!value.Equals("avalue"))
                    Console.WriteLine("error: Received {0} and should have been 'avalue'", value);
                if (r.Lists.LLen("alist") != 1)
                    Console.WriteLine("error: List should have one element after pop");
                r.Lists.RPush("alist", "yet another value");
                var values = r.Lists.LRange("alist", 0, 1);
                if (!values[0].Equals("another value"))
                    Console.WriteLine("error: Range did not return the right values");

                assert(r.Sets.SAdd("FOO", "BAR"), "Problem adding to set");
                assert(r.Sets.SAdd("FOO", "BAZ"), "Problem adding to set");
                assert(r.Sets.SAdd("FOO", "Hoge"), "Problem adding string to set");
                assert(r.Sets.SCARD("FOO") == 3, "Cardinality should have been 3 after adding 3 items to set");
                assert(r.Sets.SISMEMBER("FOO", "BAR"), "BAR should have been in the set");
                assert(r.Sets.SISMEMBER("FOO", "BAR"), "BAR should have been in the set");
                var members = r.Sets.SMEMBERS("FOO");
                assert(members.Length == 3, "Set should have had 3 members");

                assert(r.Sets.REM("FOO", "Hoge"), "Should have removed Hoge from set");
                assert(!r.Sets.REM("FOO", "Hoge"), "Hoge should not have existed to be removed");
                assert(2 == r.Sets.SMEMBERS("FOO").Length, "Set should have 2 members after removing Hoge");

                assert(r.Sets.SAdd("BAR", "BAR"), "Problem adding to set");
                assert(r.Sets.SAdd("BAR", "ITEM1"), "Problem adding to set");
                assert(r.Sets.SAdd("BAR", "ITEM2"), "Problem adding string to set");

                assert(r.Sets.SUNION("FOO", "BAR").Length == 4, "Resulting union should have 4 items");
                assert(1 == r.Sets.SINTER("FOO", "BAR").Length, "Resulting intersection should have 1 item");
                assert(1 == r.Sets.SDIFF("FOO", "BAR").Length, "Resulting difference should have 1 item");
                assert(2 == r.Sets.SDIFF("BAR", "FOO").Length, "Resulting difference should have 2 items");

                string itm = r.Sets.SRANDMEMBER("FOO");
                assert(null != itm, "GetRandomMemberOfSet should have returned an item");
                string[] itms = r.Sets.SRANDMEMBER("FOO",2);
                assert(null != itms && itms.Length ==2, "GetRandomMemberOfSet should have returned an item");
                assert(r.Sets.SMOVE("FOO", "BAR", itm), "Data within itm should have been moved to set BAR");

                r.Hashs.HSET("hset", "k1", "v1");
                r.Hashs.HSET("hset", "k2", "v2");
                r.Hashs.HSET("hset", "k3", "v4");
                assert(r.Hashs.HLEN("hset") == 3, "error: hset");

                assert(r.Hashs.HGET("hset", "k1") == "v1", "error: hget");
                assert(r.Hashs.HGETALL("hset").Count == 3, "error: hgetall");




                r.FlushDb();

                if (r.GetKeys().Length > 0)
                    Console.WriteLine("error: there should be no keys but there were {0}", r.GetKeys().Length);


        }

        private static void pubsub(RedisClient sub, RedisClient pub)
        {


            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
            Console.WriteLine("publish test start");

                    var v = DateTime.Now.Ticks.ToString();
                    v = Adf.JsonHelper.Serialize(sub);
                    pub.PubSub.Publish("test", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(v)));

                    Console.WriteLine("publish:{0}", v);
                }
            }).Start();

            sub.PubSub.Subscribe((RedisSubscribeResult result) => {

                if (result.Type == "message")
                {

                    Console.WriteLine("new message:");
                    Console.WriteLine("\t{0}:{1}", result.Channel, result.Message);
                }
                else
                {
                    Console.WriteLine(result.Type + ":");
                    Console.WriteLine("\t{0}:{1}", result.Channel, result.SubscribeCount);
                }
                
            
            }
            , "test");
        }

        static void assert(bool condition, string message)
        {
            if (!condition)
            {
                Console.WriteLine("error: {0}", message);
            }
        }
    }
}
