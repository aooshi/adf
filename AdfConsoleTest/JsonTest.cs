using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace AdfConsoleTest
{
    public class JsonTest2
    {
        //public DateTime now { get; set; }

        public JsonTest2()
        {
            this.flo = new List<char>();
        }

        public List<char> flo { get; set; }
    }

    public class JsonTest
    {
        public string a { get; set; }

        public string b { get; private set; }

        public int c { get;set;}

        public JsonTest d { get; set; }

        public Hashtable e { get; set; }

        public DateTime now { get; set; }

        public float[] f { get; set; }

        public override string ToString()
        {
            return Adf.JsonHelper.Serialize(this);
        }

        public static JsonTest FromJson(string json)
        {
            return (JsonTest)Adf.JsonHelper.Deserialize(typeof(JsonTest), json);
        }

        public void Test()
        {
            var  j = new JsonTest()
            {
                a = "中",
                c = 1
            };
            j.d = new JsonTest() { 
                a ="中2",
                c = 2
            };
            j.e = new Hashtable();
            j.e.Add("ea", 1);
            j.e.Add("eb", "a");
            j.now = DateTime.Now;
           j.f = new float[] { 1.0f,2.1f };


            var json = Adf.JsonHelper.Serialize(j);
            Console.WriteLine(json);

            var jdecode1 = Adf.JsonHelper.Deserialize(json);
            var jdecode2 = Adf.JsonHelper.Deserialize<JsonTest>(json);

            var o = Adf.JsonHelper.Deserialize(typeof(JsonTest), json);


            json = Adf.JsonHelper.Serialize(new JsonTest2() { flo = {'a','b'} });
            o = Adf.JsonHelper.Deserialize(typeof(JsonTest2), json);


            Console.WriteLine(o == null ? "fail" : "success");
        }
    }
}
