using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class DictionarySortTest
    {

        public void Test()
        {
            var dictionary = new Dictionary<string, int>();
            var random = new Random();
            for (int i = 0; i < 10; i++)
            {
                dictionary.Add(i.ToString(), random.Next(10000, 99999));
            }


            var keys = new string[dictionary.Count];
            var values = new int[dictionary.Count];

            dictionary.Keys.CopyTo(keys, 0);
            dictionary.Values.CopyTo(values, 0);

            for (var i = 0; i < dictionary.Count; i++)
            {
                Console.WriteLine("{0}:{1}", keys[i], values[i]);
            }

            Array.Sort<int, string>(values, keys);

            Console.WriteLine("---------------------------------");

            for (var i = 0; i < dictionary.Count; i++)
            {
                Console.WriteLine("{0}:{1}", keys[i], values[i]);
            }

        }


    }
}