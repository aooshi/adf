using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;

namespace AdfConsoleTest
{
    public class ConfigTest
    {
        public void Test()
        {
            var waitHandle = new System.Threading.ManualResetEvent(false);

            var userconfig = new Adf.Config.NameValue("users/users.config");
            var size = 0;

            ConfigWatcher.INTERVAL_MILLISECONDS = 10000;

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Global:\t{0},\t\twatcher:{1}", GlobalConfig.Instance.Count, GlobalConfig.Instance.IsWatcher);
            Console.WriteLine("App:\t{0}, \t\twatcher:{1}", AppConfig.Instance.Count, AppConfig.Instance.IsWatcher);
            Console.WriteLine("Log:\t{0}, \t\twatcher:{1}", LogConfig.Instance.Count, LogConfig.Instance.IsWatcher);
            Console.WriteLine("Users:\t{0}, \t\twatcher:{1}", userconfig.Count, userconfig.IsWatcher);
            Console.ForegroundColor = color;

            System.Threading.ThreadPool.QueueUserWorkItem(obj =>
            {

                while (true)
                {
                    Console.WriteLine("Global:\t{0},\t\tkey1:{1}", GlobalConfig.Instance.Count, GlobalConfig.Instance["key1"]);
                    Console.WriteLine("App:\t{0}, \t\tkey1:{1}", AppConfig.Instance.Count, AppConfig.Instance["key1"]);
                    Console.WriteLine("Log:\t{0}, \t\tkey1:{1}", LogConfig.Instance.Count, LogConfig.Instance["key1"]);

                    Console.WriteLine("Users:\t{0}, \t\tkey1:{1}", userconfig.Count, userconfig["key1"]);

                    Console.WriteLine("wait 5s for next " + size ++);
                    System.Threading.Thread.Sleep(5000);
                }

            });

            GlobalConfig.Instance.Changed += new EventHandler(Instance_Changed);
            AppConfig.Instance.Changed += new EventHandler(Instance_Changed);
            userconfig.Changed += new EventHandler(Instance_Changed);





            waitHandle.WaitOne();
        }

        static void Instance_Changed(object sender, EventArgs e)
        {
            if (sender is GlobalConfig)
                Console.WriteLine("global changed");
            else if (sender is AppConfig)
                Console.WriteLine("app changed");
            else
                Console.WriteLine(((IConfig)sender).FileName + " changed");

        }
    }
}