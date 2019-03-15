using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AdfConsoleTest
{
    public class LogManagerTest2
    {
        public void Test()
        {
            var logManager = new Adf.LogManager("LogManagerTest2", "c:\\logs\\test\\");
            //logManager.SetFlushInterval(5);
            logManager.ToConsole = true;


            Console.WriteLine("flush interval: 5");


            logManager.Message.Flushed += this.FlushCompleted;
            logManager.Warning.Flushed += this.FlushCompleted;
            logManager.Error.Flushed += this.FlushCompleted;

            var custom = logManager.GetWriter("custom");
            custom.Flushed += this.FlushCompleted;

            while (true)
            {
                logManager.Message.WriteTimeLine("one message");
                logManager.Warning.WriteTimeLine("one warning");
                logManager.Error.WriteTimeLine("one error");
                custom.WriteTimeLine("on custom");

                Console.WriteLine();

                System.Threading.Thread.Sleep(2000);
            }

        }

        private void FlushCompleted(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(((Adf.LogWriter)sender).Name + " flushed");
        }
    }
}