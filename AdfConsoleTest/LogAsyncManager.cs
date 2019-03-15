using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AdfConsoleTest
{
    public class LogAsyncManager
    {
        public void Test()
        {
            var logManager = new Adf.LogManager("LogAsyncManager", "c:\\logs\\test\\");
            logManager.ToConsole = true;

            var custom = logManager.CreateAsyncWriter("custom");
            custom.Flushed += this.FlushCompleted;


            Console.WriteLine("buffer size: " + custom.BufferSize);
            custom.BufferSize = 256;
            Console.WriteLine("buffer size: " + custom.BufferSize);
            
            for(int i=0;i<20;i++)
            {
                custom.WriteTimeLine("on custom {0}/{1}" , i,20 );


                Console.WriteLine("buffer size: " + custom.BufferSize);
                Console.WriteLine("buffer count: " + custom.BufferCount);

                Console.WriteLine();

                System.Threading.Thread.Sleep(2000);
            }

            custom.Dispose();
            Console.WriteLine("log writer disposed");

            logManager.Dispose();

            Console.WriteLine("Comploeted");

        }

        private void FlushCompleted(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(((Adf.LogWriter)sender).Name + " flushed");
        }
    }
}