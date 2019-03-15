using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AdfConsoleTest
{
    public class LogWriterTest
    {
        public void Test()
        {
            var logWriter = new Adf.LogWriter("logwriter-test", "c:\\logs\\");
            logWriter.Flushed += (s, o) => {
                //Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tflushed");
            };

            Console.WriteLine("buffer size: " + logWriter.BufferSize);
            Console.WriteLine("buffer count: " + logWriter.BufferCount);
            Console.WriteLine("writer 10 line log begin");
            for (int i = 0; i < 10; i++)
            {
                logWriter.WriteTimeLine(i.ToString("x") + " line");
            }
            Console.WriteLine("writer 10 line log end");

            logWriter.BufferSize = 100;
            Console.WriteLine("buffer size: " + logWriter.BufferSize);
            Console.WriteLine("buffer count: " + logWriter.BufferCount);


            Console.WriteLine("writer 10 line log");
            for (int i = 0; i < 10; i++)
            {
                logWriter.WriteTimeLine(i.ToString("x").PadRight(1024) + " line");
            }
            Console.WriteLine("writer 10 line log end");


            Console.WriteLine("wait 3s");
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("buffer size: " + logWriter.BufferSize);
            Console.WriteLine("buffer count: " + logWriter.BufferCount);
            
            logWriter.Dispose();

            Console.WriteLine("writer disabled");

            Console.Read();
        }
    }
}