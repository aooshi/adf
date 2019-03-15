using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class HashSetDemoTest
    {
        public void Test()
        {
            var hashSet = new HashSetDemo<string>();


            hashSet.Add("a");
            hashSet.Add("a");
            hashSet.Add("a");

            //

            hashSet.Add("b");
            hashSet.Add("c");

            hashSet.Contains("b");

            hashSet.Add("d");
            hashSet.Add("e");
            hashSet.Add("f");

            hashSet.Remove("f");
            hashSet.Remove("d");

            hashSet.Add("f");

        }
    }
}
