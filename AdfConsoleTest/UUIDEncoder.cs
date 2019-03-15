using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Data;

namespace AdfConsoleTest
{
    class UUIDEncoder
    {
        public static void init()
        {
            //length = 21
            //var a = Adf.UUIDEncoder.Encode("04385181-749b-4692-8a74-94b37a611f78");
            //var b = Adf.UUIDEncoder.Decode(a);


            for (int i = 0; i < 20; i++)
            {
                var g = Guid.NewGuid();
                var v = Adf.UUIDEncoder.Encode(g);
                Console.WriteLine("{0}:{1}", v.Length, v);
            }

        }
    }
}
