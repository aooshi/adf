using System;
using System.Collections.Generic;
using System.Text;
using Adf;
using System.Diagnostics;

namespace AdfConsoleTest
{
   public class ConsistentHashingTest
    {
       const int TEST_COUNT = 1000 * 10000;

       public void Test()
       {
           var items = new item[10];
           for (int i = 0; i < items.Length; i++)
           {
               items[i] = new item() { index = i };
           }

           var hashing = new ConsistentHashing<item>(items);

           Console.WriteLine("hashing init completed");

           var rand = new Random();
           var rands = new double[TEST_COUNT];
           for (int i = 0; i < TEST_COUNT; i++)
               rands[i] = rand.NextDouble();
           Console.WriteLine("hashing rands completed");

           var stopwatch = Stopwatch.StartNew();
           for (var i = 0; i < TEST_COUNT; i++)
           {
               hashing.GetPrimary(rands[i].ToString()).count++;
           }
           stopwatch.Stop();
                      
           foreach (var item in items)
           {
               Console.WriteLine("node:{0}, run:{1}, {2}%",item.index, item.count,(int)( (double)item.count / TEST_COUNT*100) );
           }
           Console.WriteLine("Elapsed:Milliseconds:{0}, {1}get/s", stopwatch.ElapsedMilliseconds, TEST_COUNT / (stopwatch.ElapsedMilliseconds/1000));

           Console.ReadLine();

       }

       class item : Adf.IConsistentHashingNode
       {
           public int index;
           public int count;
           
           public string GetHashingIdentity()
           {
               return string.Concat(index, '-', count);
           }
       }
    }
}
