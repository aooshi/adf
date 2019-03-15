using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class PathHelperTest
    {

        public static void NumberPath()
        {
            
            Console.WriteLine(Adf.PathHelper.NumberPath(long.MaxValue,"/",100));
            //Console.WriteLine(Adf.PathHelper.NumberPath(1000));
            //Console.WriteLine(Adf.PathHelper.NumberPath(1001));
            //Console.WriteLine(Adf.PathHelper.NumberPath(1999));
            //Console.WriteLine(Adf.PathHelper.NumberPath(2000));
            //Console.WriteLine(Adf.PathHelper.NumberPath(2001));
            //Console.WriteLine(Adf.PathHelper.NumberPath(1001001));
            //Console.WriteLine(Adf.PathHelper.NumberPath(1000001));
            //Console.WriteLine(Adf.PathHelper.NumberPath(2001001));
            Console.Read();
            int i = 998;
            while (true)
            {
                Console.WriteLine("{0}:{1}", i, Adf.PathHelper.NumberPath(i++));
                if (i % 1000 == 0)
                    System.Threading.Thread.Sleep(200);
            }
        }
    }
}
